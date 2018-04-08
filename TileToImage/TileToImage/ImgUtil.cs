
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using TileLib;

namespace TileToImage
{
  /// <summary>
  /// 画像用Util
  /// </summary>
  public class ImgUtil
  {
    
    /// <summary>
    /// タイル定義
    /// </summary>
    private TileUtil _tileUtil;


    /// <summary>
    /// 範囲のタイル列挙
    /// </summary>
    private IEnumerable<TileInfo> _infos = null;

    /// <summary>
    /// 範囲のPXサイズ
    /// </summary>
    private TileLib.Size _size = null;



    /// <summary>
    /// png用カラーパターン
    /// </summary>
    public List<Color> Colors { get; set; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="mapExtent">出力範囲(Webメルカトル)</param>
    public ImgUtil(TileUtil tileUtil)
    {
      this._tileUtil = tileUtil;  
    }//end method




    /// <summary>
    /// 画像出力
    /// </summary>
    /// <param name="tileUtil"></param>
    /// <returns></returns>
    public ExportResult Export(Extent ext, int level)
    {
      var tileUtil = this._tileUtil;
      
      var infos = tileUtil.ExtentToTileInfo(level, ext);
      var size = tileUtil.CalcPxSize(level, ext);
      this._infos = infos;
      this._size = size;

      PixelFormat pxFormat = PixelFormat.Format32bppArgb;
      ColorPalette palette = null;
      //一旦最初の画像を取ってピクセルフォーマットを取得
      using (WebClient client = new WebClient())
      using (var mem = new MemoryStream(client.DownloadData(infos.FirstOrDefault().Url)))
      using (var tile = Bitmap.FromStream(mem))
      {
        pxFormat = tile.PixelFormat;
        palette = tile.Palette;
      }//end io
      //インデックス系だとGraphicsが動作しないので仕方なし
      //インデックス系を扱う場合はExportIndexを使用
      if (pxFormat == PixelFormat.Indexed || pxFormat == PixelFormat.Format8bppIndexed)
      {
        pxFormat = PixelFormat.Format32bppArgb;
      }

      var img = new Bitmap(size.Width, size.Height, pxFormat);

      using (var gra = Graphics.FromImage(img))
      {
        var esriWorldFile = this.exportImage(ext, (tileParam) => {

          var tile = tileParam.Tile;
          var x = tileParam.X;
          var y = tileParam.Y;
          gra.DrawImageUnscaled(tile, x, y);
        });
        return new ExportResult (img, esriWorldFile);
      }//end graphics
    }//end method


    /// <summary>
    /// インデックスカラー画像の出力
    /// </summary>
    /// <param name="ext"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public ExportResult Export8bppIndexed(Extent ext, int level)
    {
      var tileUtil = this._tileUtil;
      var infos = tileUtil.ExtentToTileInfo(level, ext);
      var size = tileUtil.CalcPxSize(level, ext);
      this._infos = infos;
      this._size = size;

      
      PixelFormat pxFormat = PixelFormat.Format8bppIndexed;
      ColorPalette palette = null;

      //一旦最初の画像を取ってピクセルフォーマットを取得
      using (WebClient client = new WebClient())
      using (var mem = new MemoryStream(client.DownloadData(infos.FirstOrDefault().Url)))
      using (var tile = Bitmap.FromStream(mem))
      {
        pxFormat = tile.PixelFormat;
        palette = tile.Palette;
      }//end io

      //パレットにセットするための色のリストを準備
      List<Color> colors = null;
      if (this.Colors == null)
      {
        colors = new List<Color>(palette.Entries);
        this.Colors = colors;
      }
      else
      {
        colors = new List<Color>(this.Colors);
      }//end if


      //返却画像の用意
      var img = new Bitmap(size.Width, size.Height, PixelFormat.Format8bppIndexed);
      string esriWorldFile = null;

      try
      {
        
        //ロックする
        var data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.WriteOnly, img.PixelFormat);

        //バイト配列の取得
        var bytes = new byte[data.Height * data.Stride];
        Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);


        esriWorldFile = this.exportImage(ext, (tileParams) =>
        {
          var tile = tileParams.Tile;
          var x = tileParams.X;
          var y = tileParams.Y;

          //全体サイズよりXYがでかい場合戻す
          if (size.Width < x)//(size.Height < y || size.Width < x)
            return;

          try
          {

            var pal = tile.Palette;
            Dictionary<int, int> colorMap = new Dictionary<int, int>();

            
            //Debug.WriteLine("---------------------------------------------");
            for (int i = 0; i < pal.Entries.Length; i++)
            {
              
              var c = pal.Entries[i];
              //Debug.WriteLine($"{i}:RGB:{String.Format("#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B)}");

              var colorIndex = colors.IndexOf(c);
              //ない場合
              if (colorIndex < 0)
              {
                colorMap.Add(i, colors.Count);
                colors.Add(c);
              }
              else
              {
                colorMap.Add(i, colorIndex);
              }//end if
            }//end loop
            //Debug.WriteLine("---------------------------------------------");

            var tw = tile.Width;
            var th = tile.Height;

            //はみ出た分を切るよう
            var tileX = x < 0 ? Math.Abs(x) : 0;
            var tileY = y < 0 ? Math.Abs(y) : 0;

            var tileW = size.Width < x + tw ? tw - ((x + tw) - size.Width) : tw;
            var tileH = size.Height < y + th ?th - ((y + th) - size.Height) : th;
            
            //はみ出ているばあいはけずる
            tileW = tileW - tileX;
            tileH = tileH - tileY;

            //ロックする
            try
            {
              //タイルデータの取得
              var tileBytes = this.getTileData(tile, colorMap, colors);
              var tstride = tw;


              //幅単位で回る
              //for (int i = 0; i < tiledata.Width; i++)

              //タイルから転送先画像のインデックスを足すよう
              var cnt = 0;
              var setX = x < 0 ? 0 : x;
              var setY = y < 0 ? 0 : y;

              for (int i = tileY; i < tileY + tileH; i++)
              {
                var start = i * tstride + tileX;

                //var start = i * tiledata.Height;
                //var copyBytes = new byte[1 * tiledata.Height];
                var copyBytes = new byte[1 * tileW];
                
                var imgStart = (setY + cnt) * data.Stride + setX;
                cnt++;

                try
                {
                  Array.Copy(tileBytes, start, copyBytes, 0, copyBytes.Length);
                  Array.Copy(copyBytes, 0, bytes, imgStart, copyBytes.Length);
                }
                catch (Exception ex)
                {
                  Console.Error.WriteLine(ex.Message);
                  Console.Error.WriteLine(ex.StackTrace);
                  Console.Error.WriteLine($"Start:{start} Length:{copyBytes.Length}");
                  Console.Error.WriteLine($"image:{imgStart}");
                }
              }

              ////アンロック
              //tile.UnlockBits(tiledata);
            }
            catch(Exception ex)
            {
              Console.Error.WriteLine(ex.Message);
              Console.Error.WriteLine(ex.StackTrace);
              Console.Error.WriteLine($"TH:{tileH} TW:{tileW} X:{tileX} Y:{tileY}");

            }
            
          }catch (Exception ex)
          {
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine(ex.StackTrace);

          }

        });//end tile action

        var newPalette = img.Palette;
        //色数が多い際は切り捨て
        var colorCount = colors.Count;
        if (newPalette.Entries.Length < colors.Count)
        {
          Console.WriteLine("color over");
          colorCount = newPalette.Entries.Length;
        }

        Array.Copy(colors.ToArray(), newPalette.Entries, colorCount);
        img.Palette = newPalette;
        


        //バイト配列の書き込み
        Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);

        img.UnlockBits(data);

      }
      catch (Exception ex)
      {
        Debug.Print(ex.Message);
        Debug.Print(ex.StackTrace);

        if (img != null)
        {
          img.Dispose();
          img = null;
        }
        throw;
      }//end try
      return new ExportResult( img, esriWorldFile);
    }//end method

    /// <summary>
    /// タイル カラー インデックス配列の取得
    /// </summary>
    /// <param name="tile">タイル画像</param>
    /// <param name="colorMap">カラーマップ</param>
    /// <param name="colors">カラー配列</param>
    /// <returns>画像データ配列</returns>
    private byte[] getTileData(Bitmap tile, Dictionary<int, int> colorMap, List<Color> colors)
    {

      var tw = tile.Width;
      var th = tile.Height;

      var tilePx = tile.PixelFormat;

      //RectでカットしてもScan0は同配列の模様
      //var tiledata = tile.LockBits(new Rectangle(tileX, tileY, tileW, tileH), ImageLockMode.ReadOnly, pxFormat);
      var tiledata = tile.LockBits(new Rectangle(0, 0, tw, th), ImageLockMode.ReadOnly, tilePx);
      //アンロック
      tile.UnlockBits(tiledata);

      //インデックス定型の際はこのまま戻しても..
      //インデックス不定の際は下記

      //byte[] tileBytes = new byte[tiledata.Height * tiledata.Width];
      byte[] tileBytes = new byte[tiledata.Height * tiledata.Stride];
      Marshal.Copy(tiledata.Scan0, tileBytes, 0, tileBytes.Length);

      var tstride = tiledata.Stride;

      //カラーマップに応じた書き換え
      if (tilePx == PixelFormat.Format8bppIndexed)
      {
        tileBytes = Array.ConvertAll<byte, byte>(tileBytes, new Converter<byte, byte>(i =>
        {
          return (byte)colorMap[(int)i];
        }));
      }
      //4bit indexの場合は分割
      else if (tilePx == PixelFormat.Format4bppIndexed)
      {
        tstride = tstride * 2;
        List<byte> newData = new List<byte>(tiledata.Height * tstride);
        for (int i = 0; i < tileBytes.Length; i++)
        {
          var b = tileBytes[i];
          var high_bits = b >> 4;
          var low_bits = b & 15;
          high_bits = colorMap[high_bits];
          low_bits = colorMap[low_bits];

          newData.Add((byte)high_bits);
          newData.Add((byte)low_bits);
        }
        tileBytes = newData.ToArray();
      }//end if
       //2bit inndexは32bppArgbになってしまう
      else if (tilePx == PixelFormat.Format32bppArgb)
      {
        var step = tstride / tiledata.Width;
        tstride = tiledata.Width;

        List<byte> newData = new List<byte>(tiledata.Height * tiledata.Width);
        for (int i = 0; i < tileBytes.Length; i = i + step)
        {
          var a = (int)tileBytes[i + 3];
          var r = (int)tileBytes[i + 2];
          var g = (int)tileBytes[i + 1];
          var b = (int)tileBytes[i + 0];

          var c = Color.FromArgb(a, r, g, b);

          var colorIndex = colors.IndexOf(c);
          if (colorIndex < 0)
          {
            colorIndex = colors.Count;
            colors.Add(c);
          }//end if
          newData.Add((byte)colorIndex);
        }//end loop
        tileBytes = newData.ToArray();
      }
      else if (tilePx == PixelFormat.Format1bppIndexed)
      {
        tstride = tiledata.Width;
        List<byte> newData = new List<byte>(tiledata.Height * tstride);
        for (int i = 0; i < tileBytes.Length; i++)
        {
          var b = tileBytes[i];

          for (int bi = 0; bi < 8; bi++)
          {
            if ((b & (1 << bi)) != 0)
            {
              newData.Add((byte)colorMap[1]);
            }
            else
            {
              newData.Add((byte)colorMap[0]);
            }
          }
        }
        tileBytes = newData.ToArray();

      }
      else
      {
        Debug.Print(tilePx.ToString("G"));
      }

      return tileBytes;
    }//end method

    
    /// <summary>
    /// タイル出力処理の引数
    /// </summary>
    private class ExportTile
    {
      public Bitmap Tile {get;set;}
      public int X {get; set;}
      public int Y {get; set;}
    }

    /// <summary>
    /// タイルのループ 共通処理
    /// </summary>
    /// <param name="ext">範囲</param>
    /// <param name="tileAction">タイル読み込み後の処理</param>
    /// <returns>WorldFile文字列</returns>
    private string exportImage(Extent ext, Action<ExportTile> tileAction)
    {
      var tileUtil = this._tileUtil;
      var infos = this._infos;
      var size = this._size;

      var dx = size.Width / ext.Width;
      var dy = size.Height / ext.Height;

      var esriWorldFile = $"{ext.Width / size.Width}{Environment.NewLine}" +
                            $"0{Environment.NewLine}" +
                            $"0{Environment.NewLine}" +
                            $"{(ext.Height / size.Height) * -1}{Environment.NewLine}" +
                            $"{ext.XMin}{Environment.NewLine}" +
                            $"{ext.YMax}";

      var cList = infos.Select(i => i.Col).Distinct().ToList();
      var rList = infos.Select(i => i.Row).Distinct().ToList();

      var col = cList[cList.Count / 2];
      var row = rList[rList.Count / 2];

      var crinfo = infos.Where(i => i.Col == col && i.Row == row).FirstOrDefault();
      
      var cx = (int)Math.Floor((crinfo.WebMercatorExtent.XMin - ext.XMin) * dx);
      var cy = (int)Math.Floor((ext.YMax - crinfo.WebMercatorExtent.YMin) * dy);

        using (WebClient client = new WebClient())
        {
          //タイルのループ          
          foreach (var info in infos)
          {
            var x = cx + (256 * (info.Col - crinfo.Col));
            var y = cy + (256 * (info.Row - crinfo.Row));

            using (var mem = new MemoryStream(client.DownloadData(info.Url)))
            using (var tile = (Bitmap)Bitmap.FromStream(mem))
            {
              var tileParams = new ExportTile()
              {
                Tile = tile,
                X = x,
                Y = y
              };
              //個別処理実行
              tileAction(tileParams);
            }//end tile

          }//end loop tile info
        }//end web client

        return esriWorldFile;
    }//end method




  }//end class
}//end namespace
