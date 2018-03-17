
namespace TileUtil
{
  /// <summary>
  /// タイル情報の保持用クラス
  /// </summary>
  public class TIleInfo
  {
    /// <summary>
    /// 縮尺レベル
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// 行 (Y方向)
    /// </summary>
    public int Row { get; set; }

    /// <summary>
    /// 列　（X方向)
    /// </summary>
    public int Col { get; set; }

    /// <summary>
    /// Webメルカトル座標の範囲
    /// </summary>
    public Extent WebMercatorExtent { get; set; }

    /// <summary>
    /// URL文字列
    /// </summary>
    public string Url { get; set; }


  }//end class
}//end namespace