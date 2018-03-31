using System.Drawing.Imaging;
using System.IO;
using TileLib;
using System.Linq;

using CommandLine;
using System.Collections.Generic;
using System;
using Csv;
using System.Drawing;

namespace TileToImage
{
  class Program
  {
    static void Main(string[] args)
    {
      
      Parser.Default.ParseArguments<CmdOptions>(args)
       .WithParsed<CmdOptions>(opts => Program.runProcess(opts))
       .WithNotParsed<CmdOptions>((errs) => Program.errorHandle(errs)); ;
      
      
      ////標準地図時
      //var tileUrl = new TileUrl("https://cyberjapandata.gsi.go.jp/xyz/std/", ".png");
      //////航空写真時
      ////var tileUrl = new TileUrl("https://cyberjapandata.gsi.go.jp/xyz/seamlessphoto/", ".jpg");
      //tileUrl.NeedsColRow = true;
      //var tileUtil = new TileUtil(tileUrl);


      //var level = 17;
      //var ext = new Extent(15556632, 4259789, 15557460, 4260364);
      ////var outFile = @"C:\temp\mytileimgtest.jpg";
      //var outFile = @"C:\temp\mytileimgtest2.png";
      //var imgFormat = ImageFormat.Png;
      //if (outFile.ToLower().EndsWith("jpg"))
      //{
      //  imgFormat = ImageFormat.Jpeg;
      //}

      

      //var imgUtil = new ImgUtil(ext);
      ////var ret = imgUtil.Export(tileUtil, level);
      //var ret = imgUtil.Export8bppIndexed(tileUtil, level);

      //using (var img = ret.ExportImage)
      //{
      //  img.Save(outFile, imgFormat);
      //}//end img
      //File.WriteAllText(outFile + "w", ret.EsriWorldfile);
      
    }//end method


    


    private static void runProcess(CmdOptions opts)
    {
      try
      {
        //ファイルの存在確認
        if (!File.Exists(opts.InputCsv))
        {
          Console.WriteLine($"File Not Found:{opts.InputCsv}");
          Environment.Exit(1);
        }//end if

        //ディレクトリの存在確認
        if (!Directory.Exists(opts.OutDir))
        {
          Console.WriteLine($"Directory Not Found:{opts.OutDir}");
          Environment.Exit(1);
        }

        //4隅座標カラムの件数確認
        var extCols = opts.ExtentColumn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                      .Select(s => s.Trim()).ToList();
        
        if (extCols.Count < 4)
        {
          Console.WriteLine("extent-column needs 4:ex minx,miny,maxx,maxy");
          Console.WriteLine($"input:{opts.ExtentColumn}");
          Environment.Exit(1);
        }

        //タイル関連準備
        var tileUrl = new TileUrl(opts.PreUrl, opts.PostUrl);
        tileUrl.NeedsColRow = true;
        var tileUtil = new TileUtil(tileUrl);
        var imgUtil = new ImgUtil(tileUtil);

        var level = (int)opts.Level;
        

        bool checkHeader = false;

        //カラーファイルがあれば読み込み
        var colorFile = Path.Combine(opts.OutDir, "colors.clr");
        List<Color> colors = null;
        if (File.Exists(colorFile))
        {
          colors = Program.readColors(colorFile);
        }
        imgUtil.Colors = colors;
         

        using (var fs = new FileStream(opts.InputCsv, FileMode.Open, FileAccess.Read))
        using (var wfs = new FileStream(Path.Combine(opts.OutDir, "list.csv"), FileMode.Create, FileAccess.Write))
        using (var sw = new StreamWriter(wfs))
        {
          sw.WriteLine($"{opts.UniqueColumn},img");

          foreach (var line in CsvReader.ReadFromStream(fs))
          {
            //ヘッダのチェック
            if (!checkHeader)
            {
              if (!line.Headers.Contains(opts.UniqueColumn))
              {
                Console.WriteLine($"Unique Column Not Found:{opts.UniqueColumn}");
                Environment.Exit(1);
              }
              if (!line.Headers.Contains(extCols[0]) ||
                  !line.Headers.Contains(extCols[1]) ||
                  !line.Headers.Contains(extCols[2]) ||
                  !line.Headers.Contains(extCols[3]))
              {
                Console.WriteLine($"extent-column Not Found:{opts.ExtentColumn}");
                Environment.Exit(1);
              }

              checkHeader = true;
            }//end if

            var uniqueId = line[opts.UniqueColumn];
            double minx, miny, maxx, maxy;
            if (!double.TryParse(line[extCols[0]], out minx) ||
                !double.TryParse(line[extCols[1]], out miny) ||
                !double.TryParse(line[extCols[2]], out maxx) ||
                !double.TryParse(line[extCols[3]], out maxy))
            {
              Console.WriteLine($"unique id:{uniqueId} extent cannot read.");
              continue;
            }

            var outFile = Path.Combine(opts.OutDir, $"img{uniqueId}{opts.ImageFormat}");
            sw.WriteLine($"{uniqueId},{outFile}");//とりあえずリスト出力

            //ファイルあれば戻す
            if (File.Exists(outFile))
              continue;
            
            var imgFormat = ImageFormat.Png;
            if (outFile.ToLower().EndsWith("jpg"))
            {
              imgFormat = ImageFormat.Jpeg;
            }

            //範囲
            var ext = new Extent(minx, miny, maxx, maxy);

            ExportResult ret = null;
            try
            {
              if (!opts.Bit8)
              {
                ret = imgUtil.Export(ext, level);
              }
              else
              {
                ret = imgUtil.Export8bppIndexed(ext, level);
              }
              ret.ExportImage.Save(outFile, imgFormat);
              File.WriteAllText(outFile + "w", ret.EsriWorldfile);
              if (imgUtil.Colors != null)
              {
                Program.writeColors(colorFile, imgUtil.Colors);
              }
              
            }
            catch (Exception ex)
            {
              Console.WriteLine($"unique id:{uniqueId} err.");
              Console.WriteLine(ex.Message);
              Console.WriteLine(ex.StackTrace);
            }
            finally
            {
              if (ret != null && ret.ExportImage != null)
              {
                ret.ExportImage.Dispose();
              }
            }

          }//end csv loop
        }//end file



      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        Console.WriteLine(ex.StackTrace);
        Environment.Exit(99);

      }//end try
    }//end method

    private static List<Color> readColors(string file)
    {
      List<Color> ret = new List<Color>();

      using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
      using (var sr = new StreamReader(fs))
      {
        string line = null;
        while ((line = sr.ReadLine()) != null)
        {
          var irgb = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select( s => Convert.ToInt32(s)).ToList();
          if (irgb.Count < 4)
            continue;
          //インデックスは無視注意
          var color = Color.FromArgb(irgb[1], irgb[2], irgb[3]);
          ret.Add(color);
        }
      }
      return ret;
    }

    private static void writeColors(string file, List<Color> colors)
    {
      using (var fs = new FileStream(file, FileMode.Create, FileAccess.Write))
      using (var sw = new StreamWriter(fs))
      {
        for (int i = 0; i < colors.Count; i++)
        {
          var c = colors[i];
          sw.WriteLine($"{i} {c.R} {c.G} {c.B}");
        }//end loop color
      }//end stream
    }//end method

    
    private static void errorHandle( IEnumerable<Error> errors )
    {
      
    }



  }//end class
}//end namespace
