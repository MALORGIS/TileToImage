using System;

using TileUtil;

namespace TileToImage
{
  class Program
  {
    static void Main(string[] args)
    {

      var tileUrl = new TileUrl("https://cyberjapandata.gsi.go.jp/xyz/seamlessphoto/", ".jpg");
      tileUrl.NeedsColRow = true;

      var tileUtil = new TileUtil.TileUtil(tileUrl);

      var ext = new Extent(15556632, 4259789, 15557460, 4260364);
      var infos = tileUtil.ExtentToTileInfo(17, ext);
      foreach (var info in infos)
      {
        Console.WriteLine(info.Url);
      }



    }
  }
}
