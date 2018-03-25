
using System;
using System.Collections.Generic;
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



    public List<Color> GetColors(Extent ext, int level)
    {
      var tileUtil = this._tileUtil;
      var infos = tileUtil.ExtentToTileInfo(level, ext);

      var colors = new List<Color>();

      using (WebClient client = new WebClient())
      {
        foreach (var inf in infos)
        {
          using (var mem = new MemoryStream(client.DownloadData(infos.FirstOrDefault().Url)))
          using (var tile = Bitmap.FromStream(mem))
          {
            var pal = tile.Palette;
            //Debug.WriteLine("---------------------------------------------");
            for (int i = 0; i < pal.Entries.Length; i++)
            {
              var c = pal.Entries[i];
              //Debug.WriteLine($"{i}:RGB:{String.Format("#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B)}");

              var colorIndex = colors.IndexOf(c);
              //ない場合
              if (colorIndex < 0)
              {
                colors.Add(c);
              }
              else
              {
              }//end if
            }//end loop

          }//end io
        }//end loop
      }//end web
      return colors;
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
      

      PixelFormat pxFormat = PixelFormat.Format32bppArgb;
      ColorPalette palette = null;

      var size = tileUtil.CalcPxSize(level, ext);

      var dx = size.Width / ext.Width;
      var dy = size.Height / ext.Height;

      var esriWorldFile = $"{ext.Width / size.Width}{Environment.NewLine}" +
                            $"0{Environment.NewLine}" +
                            $"0{Environment.NewLine}" +
                            $"{(ext.Height / size.Height) * -1}{Environment.NewLine}" +
                            $"{ext.XMin}{Environment.NewLine}" +
                            $"{ext.YMax}";


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

      try
      {
        using (var gra = Graphics.FromImage(img))
        using (WebClient client = new WebClient())
        {
          foreach (var info in infos)
          {
            //Console.WriteLine(info.Url);


            ////サーバに負荷を掛けないよう待機
            //Thread.Sleep(1000);


            var tileExt = info.WebMercatorExtent;
            using (var mem = new MemoryStream(client.DownloadData(info.Url)))
            using (var tile = Bitmap.FromStream(mem))
            {


              var x = (int)Math.Floor((tileExt.XMin - ext.XMin) * dx);
              var y = (int)Math.Floor((ext.YMax - tileExt.YMin) * dy);
              gra.DrawImageUnscaled(tile, x, y);
            }//end tile

          }
        }//end graphics

        
        
        

      }
      catch
      {
        if (img != null)
        {
          img.Dispose();
          img = null;
        }
        throw;
      }

      return new ExportResult (img, esriWorldFile);
    }//end method


    public ExportResult Export8bppIndexed(Extent ext, int level)
    {
      var tileUtil = this._tileUtil;
      var infos = tileUtil.ExtentToTileInfo(level, ext);


      PixelFormat pxFormat = PixelFormat.Format8bppIndexed;
      ColorPalette palette = null;

      var size = tileUtil.CalcPxSize(level, ext);

      var dx = size.Width / ext.Width;
      var dy = size.Height / ext.Height;

      var esriWorldFile = $"{ext.Width / size.Width}{Environment.NewLine}" +
                            $"0{Environment.NewLine}" +
                            $"0{Environment.NewLine}" +
                            $"{(ext.Height / size.Height) * -1}{Environment.NewLine}" +
                            $"{ext.XMin}{Environment.NewLine}" +
                            $"{ext.YMax}";


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
      if (pxFormat != PixelFormat.Format8bppIndexed && pxFormat != PixelFormat.Format4bppIndexed)
      {
        throw new Exception("Not 4/8bppIndexed");
      }

      var img = new Bitmap(size.Width, size.Height, PixelFormat.Format8bppIndexed);

      //パレットにセットするための色のリストを準備
      var colors = this.Colors;
      if (colors == null)
      {
        colors = new List<Color>(palette.Entries);
        this.Colors = colors;
      }//end if

      try
      {
        //パレットセット
        //img.Palette = palette;
        
        
        //ロックする
        var data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.WriteOnly, img.PixelFormat);

        //バイト配列の取得
        byte[] bytes = new byte[data.Height * data.Stride];
        Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);

        using (WebClient client = new WebClient())
        {
          //タイルのループ          
          foreach (var info in infos)
          {
            //Console.WriteLine(info.Url);

            ////サーバに負荷を掛けないよう待機
            //Thread.Sleep(1000);

            var tileExt = info.WebMercatorExtent;
            var x = (int)Math.Floor((tileExt.XMin - ext.XMin) * dx);
            var y = (int)Math.Floor((ext.YMax - tileExt.YMin) * dy);

            //全体サイズよりXYがでかい場合戻す
            if (size.Width < x)//(size.Height < y || size.Width < x)
              continue;

            try
            {
              using (var mem = new MemoryStream(client.DownloadData(info.Url)))
              using (var tile = (Bitmap)Bitmap.FromStream(mem))
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
                // var tileH = y < 0 ? th + y :th;
                // var tileY = y < 0 ? 0 : y;

                //ロックする
                try
                {
                  //RectでカットしてもScan0は同配列の模様
                  //var tiledata = tile.LockBits(new Rectangle(tileX, tileY, tileW, tileH), ImageLockMode.ReadOnly, pxFormat);
                  var tiledata = tile.LockBits(new Rectangle(0, 0, tw, th), ImageLockMode.ReadOnly, tile.PixelFormat);

                  //byte[] tileBytes = new byte[tiledata.Height * tiledata.Width];
                  byte[] tileBytes = new byte[tiledata.Height * tiledata.Stride];
                  Marshal.Copy(tiledata.Scan0, tileBytes, 0, tileBytes.Length);

                  //カラーマップに応じた書き換え
                  if (tile.PixelFormat == PixelFormat.Format8bppIndexed)
                  {
                    tileBytes = Array.ConvertAll<byte, byte>(tileBytes, new Converter<byte, byte>(i =>
                     {
                       return (byte)colorMap[(int)i];
                     }));
                  }//end if

                  var tstride = tiledata.Stride;

                  //4bit indexの場合は分割
                  if (tile.PixelFormat == PixelFormat.Format4bppIndexed)
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

                  //for (int i = 0; i < tileBytes.Length; i++)
                  //{
                  //  var b = (int)tileBytes[i];
                  //  tileBytes[i] = (byte)colorMap[b];
                  //}//end loop


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
                      Debug.Print(ex.Message);
                      Debug.Print(ex.StackTrace);
                      Debug.Print($"Start:{start} Length:{copyBytes.Length}");
                      Debug.Print($"image:{imgStart}");
                    }
                  }

                  //アンロック
                  tile.UnlockBits(tiledata);
                }
                catch(Exception ex)
                {
                  Debug.WriteLine(ex.Message);
                  Debug.WriteLine(ex.StackTrace);
                  Debug.WriteLine($"TH:{tileH} TW:{tileW} X:{tileX} Y:{tileY}");

                }
              }//end tille
            }catch (Exception ex)
            {
              Debug.WriteLine(ex.Message);
              Debug.WriteLine(ex.StackTrace);

            }

          }//end loop


          var newPalette = img.Palette;
          Array.Copy(colors.ToArray(), newPalette.Entries, colors.Count);
          img.Palette = newPalette;
          

        }//end web client


        //バイト配列の書き込み
        Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);

        img.UnlockBits(data);

      }
      catch
      {
        if (img != null)
        {
          img.Dispose();
          img = null;
        }
        throw;
      }//end try
      return new ExportResult( img, esriWorldFile);
    }//end method


  }//end class
}//end namespace
