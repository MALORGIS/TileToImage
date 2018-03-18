using System;
using System.Collections.Generic;

namespace TileUtil
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
    /// 1pxあたりの距離 [m] EPSG:3857投影時
    /// 地球の円周長 (2×半径×π)を 256px としたのがL0
    /// L0から四分木
    /// </summary>
    /// <returns></returns>
    private static readonly Dictionary<int, double> RESOLUTION = new Dictionary<int, double>() {
      { 0, 2 * GEO_R * Math.PI / PX_COL / Math.Pow(2,  0) },
      { 1, 2 * GEO_R * Math.PI / PX_COL / Math.Pow(2,  1) },
      { 2, 2 * GEO_R * Math.PI / PX_COL / Math.Pow(2,  2) },
      { 3, 2 * GEO_R * Math.PI / PX_COL / Math.Pow(2,  3) },
      { 4, 2 * GEO_R * Math.PI / PX_COL / Math.Pow(2,  4) },
      { 5, 2 * GEO_R * Math.PI / PX_COL / Math.Pow(2,  5) },
      { 6, 2 * GEO_R * Math.PI / PX_COL / Math.Pow(2,  6) },
      { 7, 2 * GEO_R * Math.PI / PX_COL / Math.Pow(2,  7) },
      { 8, 2 * GEO_R * Math.PI / PX_COL / Math.Pow(2,  8) },
      { 9, 2 * GEO_R * Math.PI / PX_COL / Math.Pow(2,  9) },
      {10, 2 * GEO_R * Math.PI / PX_COL / Math.Pow(2, 10) },
      {11, 2 * GEO_R * Math.PI / PX_COL / Math.Pow(2, 11) },
      {12, 2 * GEO_R * Math.PI / PX_COL / Math.Pow(2, 12) },
      {13, 2 * GEO_R * Math.PI / PX_COL / Math.Pow(2, 13) },
      {14, 2 * GEO_R * Math.PI / PX_COL / Math.Pow(2, 14) },
      {15, 2 * GEO_R * Math.PI / PX_COL / Math.Pow(2, 15) },
      {16, 2 * GEO_R * Math.PI / PX_COL / Math.Pow(2, 16) },
      {17, 2 * GEO_R * Math.PI / PX_COL / Math.Pow(2, 17) },
      {18, 2 * GEO_R * Math.PI / PX_COL / Math.Pow(2, 18) },
      {19, 2 * GEO_R * Math.PI / PX_COL / Math.Pow(2, 19) },
      {20, 2 * GEO_R * Math.PI / PX_COL / Math.Pow(2, 20) },
      {21, 2 * GEO_R * Math.PI / PX_COL / Math.Pow(2, 21) },
      {22, 2 * GEO_R * Math.PI / PX_COL / Math.Pow(2, 22) },
      {23, 2 * GEO_R * Math.PI / PX_COL / Math.Pow(2, 23) },
      {24, 2 * GEO_R * Math.PI / PX_COL / Math.Pow(2, 24) },
      {25, 2 * GEO_R * Math.PI / PX_COL / Math.Pow(2, 25) },
      {26, 2 * GEO_R * Math.PI / PX_COL / Math.Pow(2, 26) },
    };

    /// <summary>
    /// 原点 ( タイル0,0 )のX座標
    /// </summary>
    private const double ORG_X = -20037508.342787;

    /// <summary>
    /// 原点 ( タイル0,0 )のY座標
    /// </summary>
    private const double ORG_Y = 20037508.342787;

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
    public IEnumerable<TIleInfo> ExtentToTileInfo(int level, Extent mapExtent)
    {
      var tileUrl = this._tileUrl;

      var resolution = RESOLUTION[level];

      var unitH = PX_ROW * resolution;
      var unitW = PX_COL * resolution;

      //ex: 原点と矩形の距離差 / (256px×解像度 = 1タイルあたりの距離) = 行列
      var minxtile = (int)Math.Floor((mapExtent.XMin - ORG_X) / unitW);
      var minytile = (int)Math.Floor((ORG_Y - mapExtent.YMax) / unitH);
      var maxxtile = (int)Math.Ceiling((mapExtent.XMax - ORG_X) / unitW);
      var maxytile = (int)Math.Ceiling((ORG_Y - mapExtent.YMin) / unitH);
      //縦横のタイルで回す
      for (var iCol = minxtile; iCol < maxxtile + 1; iCol++)
      {
        for (var iRow = minytile; iRow < maxytile + 1; iRow++)
        {
          var tileInfo = new TIleInfo();
          tileInfo.Level = level;
          tileInfo.Col = iCol;
          tileInfo.Row = iRow;

          tileInfo.Url = tileUrl.GetUrl(level, iRow, iCol);

          var mapX = ORG_X + (unitH * iCol);
          var mapY = ORG_Y - (unitW * iRow);
          //四隅座標をセット
          tileInfo.WebMercatorExtent = new Extent(mapX,  mapY,
                                                  mapX + unitW,
                                                  mapY + unitH);
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
        var resolution = RESOLUTION[level];
        var width = (int)Math.Ceiling(mapExtent.Width / resolution);
        var height = (int)Math.Ceiling(mapExtent.Height / resolution);

        return new Size(width, height);
    }//end method


  }//end class

}//end namespace
