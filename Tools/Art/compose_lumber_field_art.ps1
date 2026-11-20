$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$root=Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$doc=Join-Path $root 'Docs/Art/Settlement/LumberFieldStorefront'
$items=(Get-Content (Join-Path $root 'Assets/_Game/Art/Settlement/LumberField/LumberFieldArtManifest.json') -Raw | ConvertFrom-Json).Assemblies
function Board($names,$columns,$cell,$filename){
 $rows=[int][Math]::Ceiling($names.Count/$columns);$bmp=[System.Drawing.Bitmap]::new($columns*$cell,$rows*($cell+26));$g=[System.Drawing.Graphics]::FromImage($bmp);$font=[System.Drawing.Font]::new('Arial',9)
 try{$g.Clear([System.Drawing.Color]::FromArgb(44,51,54));for($i=0;$i -lt $names.Count;$i++){
  $x=($i%$columns)*$cell;$y=[Math]::Floor($i/$columns)*($cell+26);$im=[System.Drawing.Image]::FromFile((Join-Path $doc ($names[$i]+'.png')))
  try{$scale=[Math]::Min($cell/$im.Width,$cell/$im.Height);$w=[int]($im.Width*$scale);$h=[int]($im.Height*$scale);$g.DrawImage($im,[System.Drawing.Rectangle]::new($x+($cell-$w)/2,$y+($cell-$h)/2,$w,$h))}finally{$im.Dispose()}
  $g.DrawString($names[$i],$font,[System.Drawing.Brushes]::White,[single]($x+4),[single]($y+$cell+4))
 };$bmp.Save((Join-Path $doc $filename),[System.Drawing.Imaging.ImageFormat]::Png)}finally{$g.Dispose();$bmp.Dispose();$font.Dispose()}
}
$names=@();foreach($item in $items){foreach($view in @('front','back','left','right','top','study')){$names+=($item.Name+'-'+$view)}}
Board $names 6 220 'native-six-view-review.png'
$names=@();foreach($item in $items){foreach($view in @('unity-front','unity-study')){$names+=($item.Name+'-'+$view)}}
Board $names 4 280 'unity-review.png'
Board @('counter-door-closed','counter-door-open','sign-close','ledger-close','LumberZoneT1-with-trees','LumberZoneT2-with-trees','LumberZoneT3-with-trees','LumberZoneT4-with-trees','street-tailor-lumber-plot3') 3 400 'integration-review.png'
# Flat raster companion to the editable SVG. No fonts or runtime text.
$bmp=[System.Drawing.Bitmap]::new(512,384);$g=[System.Drawing.Graphics]::FromImage($bmp)
function Brush($hex){[System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml($hex))}
$brushes=@();try{
 foreach($hex in @('#5a3d27','#9a7552','#e4dcc8','#7d5a3c','#8a8e90')){$brushes+=,(Brush $hex)}
 $g.Clear([System.Drawing.Color]::Transparent);$g.FillRectangle($brushes[0],20,20,472,344);$g.FillRectangle($brushes[1],40,40,432,304)
 $g.TranslateTransform(256,192);$g.RotateTransform(-29);$g.FillRectangle($brushes[2],-122,-36,244,72);$g.FillEllipse($brushes[2],-150,-36,56,72);$g.FillEllipse($brushes[2],94,-36,56,72);$g.FillEllipse($brushes[3],-143,-30,42,60)
 $g.ResetTransform();$g.TranslateTransform(256,192);$g.RotateTransform(30);$g.FillRectangle($brushes[0],-11,-140,22,290)
 $g.FillPolygon($brushes[4],[System.Drawing.Point[]]@([System.Drawing.Point]::new(-14,-123),[System.Drawing.Point]::new(110,-139),[System.Drawing.Point]::new(105,-64),[System.Drawing.Point]::new(-14,-72)))
 $bmp.Save((Join-Path $doc 'LumberSign-flat.png'),[System.Drawing.Imaging.ImageFormat]::Png)
}finally{foreach($b in $brushes){$b.Dispose()};$g.Dispose();$bmp.Dispose()}
