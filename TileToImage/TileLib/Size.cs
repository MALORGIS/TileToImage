

namespace TileLib
{

  /// <summary>
  /// サイズ定義保持用クラス
  /// </summary>
  public class Size
  {
    /// <summary>
    /// コンストラクタ 
    /// </summary>
    public Size() {}

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="width">幅</param>
    /// <param name="height">高さ</param>
    public Size(int width, int height)
    {
      this.Width = width;
      this.Height = height;
    }

    /// <summary>
    /// 幅
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// 高さ
    /// </summary>
    public int Height { get; set; }

  }//end class

}//end namespace
