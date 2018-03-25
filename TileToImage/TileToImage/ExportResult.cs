using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace TileToImage
{
  /// <summary>
  /// 出力定義
  /// </summary>
  public class ExportResult
  {
    /// <summary>
    /// 出力画像
    /// </summary>
    public Image ExportImage { get; private set; }

    /// <summary>
    /// ワールドファイル
    /// </summary>
    public string EsriWorldfile { get; private set; }
    
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="img"></param>
    /// <param name="esriWorldfile"></param>
    public ExportResult(Image img, string esriWorldfile)
    {
      this.ExportImage = img;
      this.EsriWorldfile = esriWorldfile;

    }//end mehtod
  }//end class
}//end namespace
