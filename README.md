# MMTTLVParser

C#で書かれたMMT/TLVパーサ

## サンプルプログラムの使用例
`MMTTLVParser.Sample`を次のように実行すると、データ放送と字幕データ(TTML文章)が、入力ファイルパスと同じディレクトリに抽出されます。オプションを指定せずに実行した場合、TLVパケットの各種別のカウント情報を表示し終了します。

```
MMTTLVParser.Sample.exe -i <入力ファイルパス> -d -c
```
