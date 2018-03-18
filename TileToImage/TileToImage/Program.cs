using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading;
using TileUtil;

namespace TileToImage
{
  class Program
  {
    static void Main(string[] args)
    {

      var level = 17;
      var outFile = @"C:\temp\mytileimgtest.jpg";
      var imgFormat = ImageFormat.Png;
      if (outFile.ToLower().EndsWith("jpg"))
      {
        imgFormat = ImageFormat.Jpeg;
      }

      var tileUrl = new TileUrl("https://cyberjapandata.gsi.go.jp/xyz/seamlessphoto/", ".jpg");
      tileUrl.NeedsColRow = true;

      var tileUtil = new TileUtil.TileUtil(tileUrl);

      var ext = new Extent(15556632, 4259789, 15557460, 4260364);
      var infos = tileUtil.ExtentToTileInfo(level, ext);
      var size = tileUtil.CalcPxSize(level, ext);

      var dx = size.Width / ext.Width;
      var dy = size.Height / ext.Height;

      using (var img = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb))
      {
        using (var gra = Graphics.FromImage(img))
        using (WebClient client = new WebClient())
        {
          foreach (var info in infos)
          {
            Console.WriteLine(info.Url);


            //サーバに負荷を掛けないよう待機
            Thread.Sleep(1000);


            var tileExt = info.WebMercatorExtent;
            using (var mem = new MemoryStream(client.DownloadData(info.Url)))
            using (var tile = Bitmap.FromStream(mem))
            {
           
              
              var x = (int)Math.Floor((tileExt.XMin-ext.XMin) * dx);
              var y = (int)Math.Floor((ext.YMax- tileExt.YMin) * dy);
              gra.DrawImageUnscaled(tile, x, y);
            }//end tile

          }
        }//end graphics

        img.Save(outFile, imgFormat);

        var esriWorldFile = $"{ext.Width / size.Width}{Environment.NewLine}" +
                            $"0{Environment.NewLine}" +
                            $"0{Environment.NewLine}" +
                            $"{(ext.Height / size.Height) * -1}{Environment.NewLine}" +
                            $"{ext.XMin}{Environment.NewLine}" +
                            $"{ext.YMax}";
        File.WriteAllText(outFile + "w", esriWorldFile);


      }//end image



          
        


    }//end method
  }//end class
}//end namespace
