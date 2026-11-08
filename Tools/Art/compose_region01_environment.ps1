Add-Type -AssemblyName System.Drawing
$projectRoot=Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$artPath=Join-Path $projectRoot 'Docs/Art/Environment/Region01'
$names=@(Get-Content -LiteralPath (Join-Path $artPath 'labels.txt'))
$bitmap=New-Object System.Drawing.Bitmap(1600,1230)
$graphics=[System.Drawing.Graphics]::FromImage($bitmap)
$font=New-Object System.Drawing.Font('Segoe UI',14)
$title=New-Object System.Drawing.Font('Segoe UI',22,[System.Drawing.FontStyle]::Bold)
$brush=New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(239,224,187))
try {
    $graphics.Clear([System.Drawing.Color]::FromArgb(38,46,47))
    $graphics.DrawString('DAWN PLAINS / REUSABLE ENVIRONMENT KIT',$title,$brush,22,16)
    for($index=0;$index -lt $names.Count;$index++) {
        $x=($index%4)*400+20
        $y=[int][Math]::Floor($index/4)*380+80
        $image=[System.Drawing.Image]::FromFile((Join-Path $artPath ($names[$index]+'.png')))
        try { $graphics.DrawImage($image,$x,$y,360,320) } finally { $image.Dispose() }
        $label=$names[$index] -creplace '([a-z])([A-Z])','$1 $2'
        $graphics.DrawString($label,$font,$brush,$x,$y+330)
    }
    $bitmap.Save((Join-Path $artPath 'environment-kit-gallery.png'),[System.Drawing.Imaging.ImageFormat]::Png)
} finally { $graphics.Dispose();$bitmap.Dispose();$font.Dispose();$title.Dispose();$brush.Dispose() }
