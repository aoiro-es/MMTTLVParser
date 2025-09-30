using CommandLine;

namespace MMTTLVParser.Sample;

public class Options
{
    [Option('i', "input", Required = true, HelpText = "入力ファイルパス")]
    public string FileName { get; init; } = string.Empty;

    [Option('d', "datacasting", Required = false, HelpText = "データ放送の抽出を有効にする")]
    public bool EnableParseDatacasting { get; init; } = false;

    [Option('c', "cc", Required = false, HelpText = "TTML文章の抽出を有効にする")]
    public bool EnableParseCC { get; init; } = false;
}
