

namespace TileUtil
{

  /// <summary>
  /// マップタイルURLの生成用
  /// https://ほげほげ//{level}/{col}/{row}.jpgのようなURL生成
  /// </summary>
  public class TileUrl
  {
    /// <summary>
    /// 前置詞
    /// </summary>
    public string PreFix { get; set; } = string.Empty;

    /// <summary>
    /// 後置詞
    /// </summary>
    public string PostFix { get; set; } = string.Empty;

    /// <summary>
    /// level-col-rowの並びの際=TRUE/level-row-colの際=FALSE
    /// </summary>
    public bool NeedsColRow { get; set; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="prefix">前置詞</param>
    /// <param name="postFix">後置詞</param>
    public TileUrl(string prefix, string postFix)
    {
      if (!string.IsNullOrWhiteSpace(prefix) &&
          !prefix.EndsWith("/"))
      {
        prefix = prefix + "/";
      }//end if

      this.PreFix = prefix;
      this.PostFix = postFix;
    }//end method

    /// <summary>
    /// URL取得メソッド
    /// </summary>
    /// <param name="level">レベル</param>
    /// <param name="row">行</param>
    /// <param name="col">列</param>
    /// <returns>URL文字列</returns>
    public virtual string GetUrl(int level, int row, int col)
    {
      if (this.NeedsColRow)
        return $"{this.PreFix}{level}/{col}/{row}{this.PostFix}";
      return $"{this.PreFix}{level}/{row}/{col}{this.PostFix}";
    }//end method


  }//end class

}//end class