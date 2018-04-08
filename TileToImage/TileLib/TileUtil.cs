using System;
using System.Collections.Generic;

namespace TileLib
{
  /// <summary>
  /// メッシュ集約処理の呼び出し用クラス
  /// </summary>
  public class TileUtil
  {

    //--------------------
    #region 定数

    /// <summary>
    /// WGS84 地球半径
    /// </summary>
    const double GEO_R = 6378137;
    

    /// <summary>
    /// 原点 ( タイル0,0 )のX座標
    /// </summary>
    private static readonly double ORG_X = -1 * (2 * GEO_R * Math.PI / 2);

    /// <summary>
    /// 原点 ( タイル0,0 )のY座標
    /// </summary>
    private static readonly double ORG_Y = (2 * GEO_R * Math.PI / 2);

    /// <summary>
    /// タイル画像の縦px数
    /// </summary>
    private const int PX_ROW = 256;

    /// <summary>
    /// タイル画像の横px数
    /// </summary>
    private const int PX_COL = 256;

    #endregion 定数
    //--------------------

    /// <summary>
    /// タイルURL定義
    /// </summary>
    private TileUrl _tileUrl = null;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="tileurl">タイルURL定義</param>
    public TileUtil(TileUrl tileurl)
    {
      this._tileUrl = tileurl;
    }//end method

    /// <summary>
    /// 範囲を指定してタイル情報を取得する
    /// </summary>
    /// <param name="level">指定レベル</param>
    /// <param name="extent">Webメルカトル範囲</param>
    /// <returns>タイル情報列挙</returns>
    public IEnumerable<TileInfo> ExtentToTileInfo(int level, Extent mapExtent)
    {
      var tileUrl = this._tileUrl;

      var resolution = this.getResolution(level);

      var unitH = PX_ROW * resolution;
      var unitW = PX_COL * resolution;

      //ex: 原点と矩形の距離差 / (256px×解像度 = 1タイルあたりの距離) = 行列
      var minxtile = (int)Math.Floor((mapExtent.XMin - ORG_X) / unitW);
      var minytile = (int)Math.Floor((ORG_Y - mapExtent.YMax) / unitH);
      var maxxtile = (int)Math.Ceiling((mapExtent.XMax - ORG_X) / unitW);
      var maxytile = (int)Math.Ceiling((ORG_Y - mapExtent.YMin) / unitH);
      if (minxtile == maxxtile)
        maxxtile++;
      if (minytile == maxytile)
        maxytile++;
        
      //縦横のタイルで回す
      for (var iCol = minxtile; iCol < maxxtile; iCol++)
      {
        for (var iRow = minytile; iRow < maxytile; iRow++)
        {
          var tileInfo = new TileInfo();
          tileInfo.Level = level;
          tileInfo.Col = iCol;
          tileInfo.Row = iRow;

          tileInfo.Url = tileUrl.GetUrl(level, iRow, iCol);

          var mapX = ORG_X + (unitH * iCol);
          var mapY = ORG_Y - (unitW * iRow);
          //四隅座標をセット
          tileInfo.WebMercatorExtent = new Extent(mapX,  mapY - unitH,
                                                  mapX + unitW,
                                                  mapY);
          //タイル情報の返却
          yield return tileInfo;
        }//end loop row
      }//end loop col

      yield break;
    }//end method

    /// <summary>
    /// PXサイズ計算
    /// </summary>
    /// <param name="level">レベル</param>
    /// <param name="mapExtent">地図範囲(Webメルカトル)</param>
    /// <returns>PXサイズ</returns>
    public Size CalcPxSize(int level, Extent mapExtent)
    {
        var resolution = this.getResolution(level);
        var width = (int)Math.Ceiling(mapExtent.Width / resolution);
        var height = (int)Math.Ceiling(mapExtent.Height / resolution);

        return new Size(width, height);
    }//end method

    /// <summary>
    /// pxあたりのm数返却
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    private double getResolution(int level)
    {
      return 2 * GEO_R * Math.PI / PX_COL / Math.Pow(2, level);
    }//end method


  }//end class

}//end namespace
