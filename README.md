
### overview  (概要)
Convert tiles specified by CSV to images with world files.
CSVで指定されたタイルをワールドファイル付の画像に変換します

### usage (使用法)
```
  -r, --preUrl           Required. Input url prefix.
  -o, --postUrl          Required. Input url postfix.
  -f, --inputfile        Required. Input csv file.
  -u, --unique-column    Required. CSV Unique col name.
  -e, --extent-column    Required. (Default: minX,minY,maxX,maxY) minX,minY,maxX,maxY [WebMecator Coords].
  -l, --level            Required. zoom level
  -d, --outdir           Required. output directory.
  --format               (Default: .png) .png/.jpg image format.
  -b, --bit8             (Default: false) 8bit png.
  --help                 Display this help screen.
  --version              Display version information.
```

Option example.(オプション例)
```
-r https://cyberjapandata.gsi.go.jp/xyz/std/ -o .png -f "C:\temp\TileImg.csv" -u OBJECTID -e "MINX,MINY,MAXX,MAXY" -l 16 -d "E:\gisdatas\testtileout" -b
```