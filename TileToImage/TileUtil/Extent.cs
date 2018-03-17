
namespace TileUtil
{
  /// <summary>
  /// 4隅座標の保持用クラス
  /// </summary>
  public class Extent
  {
    /// <summary>
    /// 最小X
    /// </summary>
    public double XMin { get; set; } = double.NaN;

    /// <summary>
    /// 最小Y
    /// </summary>
    public double YMin { get; set; } = double.NaN;

    /// <summary>
    /// 最大X
    /// </summary>
    public double XMax { get; set; } = double.NaN;

    /// <summary>
    /// 最大Y
    /// </summary>
    public double YMax { get; set; } = double.NaN;

    /// <summary>
    /// 幅
    /// </summary>
    public double Width
    {
      get { return this.XMax - this.XMin; }
    }

    /// <summary>
    /// 高さ
    /// </summary>
    /// <returns></returns>
    public double Height
    {
      get { return this.YMax - this.YMin; }
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public Extent()
    {
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="minX">最小X</param>
    /// <param name="minY">最小Y</param>
    /// <param name="maxX">最大X</param>
    /// <param name="maxY">最大Y</param>
    public Extent(double minX, double minY, double maxX, double maxY)
    {
      this.PutCoords(minX, minY, maxX, maxY);
    }//end method

    /// <summary>
    /// 座標値の設定
    /// </summary>
    /// <param name="minX">最小X</param>
    /// <param name="minY">最小Y</param>
    /// <param name="maxX">最大X</param>
    /// <param name="maxY">最大Y</param>
    public void PutCoords(double minX, double minY, double maxX, double maxY)
    {
      this.XMin = minX;
      this.YMin = minY;
      this.XMax = maxX;
      this.YMax = maxY;
    }//end method

  }//end class

}//end namespace

