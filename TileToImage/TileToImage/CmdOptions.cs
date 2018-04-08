using CommandLine;

namespace TileToImage
{

  /// <summary>
  /// コマンドラインオプション
  /// </summary>
  class CmdOptions
  {
    /// <summary>
    /// URLの前部分
    /// </summary>
    [Option('r', "preUrl", Required = true, HelpText = "Input url prefix.")]
    public string PreUrl { get; set; }

    /// <summary>
    /// URLの後半部分
    /// </summary>
    [Option('o' , "postUrl", Required = true, HelpText = "Input url postfix.")]
    public string PostUrl { get; set; }


    /// <summary>
    /// 入力ファイル
    /// </summary>
    [Option('f', "inputfile", Required =true, HelpText = "Input csv file.")]
    public string InputCsv { get; set; }

    /// <summary>
    /// 一意識別
    /// </summary>
    [Option('u', "unique-column", Required = true, HelpText = "CSV Unique col name.")]
    public string UniqueColumn { get; set; }

    /// <summary>
    /// 4隅座標カラム
    /// </summary>
    [Option('e', "extent-column", Required = true, Default = "minX,minY,maxX,maxY", HelpText = "minX,minY,maxX,maxY [WebMecator Coords].")]
    public string ExtentColumn { get; set; }


    /// <summary>
    /// 縮尺レベル
    /// </summary>
    [Option('l', "level", Required =true, HelpText =  "zoom level")]
    public byte Level { get; set; }

    
    /// <summary>
    /// 出力ディレクトリ
    /// </summary>
    [Option('d', "outdir", Required = true, HelpText = "output directory.")]
    public string OutDir { get; set; }

    /// <summary>
    /// 出力フォーマット
    /// </summary>
    [Option("format", Required = false, Default =".png", HelpText = ".png/.jpg image format.")]
    public string ImageFormat { get; set; }

    /// <summary>
    /// 8bit pngとして出力
    /// </summary>
    [Option('b',"bit8", Required = false, Default = false, HelpText = "8bit png.")]
    public bool Bit8 { get; set; }


    
  }//end class

}//end namespace
