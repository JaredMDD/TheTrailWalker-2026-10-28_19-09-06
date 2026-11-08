Add-Type -AssemblyName System.Drawing
$projectRoot=Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$root=Join-Path $projectRoot 'Docs/Art/Environment/Tier01'
$names=@('Dawn Plains','Windward Hills','Mistwood','Fern Hollow','Cloudstep Mountains','Reedwater Delta')
$font=New-Object System.Drawing.Font('Segoe UI',12)
$title=New-Object System.Drawing.Font('Segoe UI',22,[System.Drawing.FontStyle]::Bold)
$brush=New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(240,226,192))
try {
    for($region=1;$region -le 6;$region++) {
        $path=Join-Path $root ('Region{0:00}' -f $region)
        $labels=@(Get-Content -LiteralPath (Join-Path $path 'labels.txt'))
        $bitmap=New-Object System.Drawing.Bitmap(1800,1400)
        $graphics=[System.Drawing.Graphics]::FromImage($bitmap)
        try {
            $graphics.Clear([System.Drawing.Color]::FromArgb(38,46,47))
            $graphics.DrawString(('{0} / {1} / 20 MESHES' -f $region,$names[$region-1]),$title,$brush,20,15)
            for($i=0;$i -lt $labels.Count;$i++) {
                $x=($i%5)*360+20;$y=[int][Math]::Floor($i/5)*325+75
                $cell=[System.Drawing.Image]::FromFile((Join-Path $path ($labels[$i]+'.png')))
                try { $graphics.DrawImage($cell,$x,$y,320,280) } finally { $cell.Dispose() }
                $label=$labels[$i] -creplace '([a-z])([A-Z])','$1 $2'
                $labelRect=New-Object System.Drawing.RectangleF($x,($y+286),320,38)
                $graphics.DrawString($label,$font,$brush,$labelRect)
            }
            $bitmap.Save((Join-Path $path 'asset-gallery.png'),[System.Drawing.Imaging.ImageFormat]::Png)
        } finally { $graphics.Dispose();$bitmap.Dispose() }
    }
    $overview=New-Object System.Drawing.Bitmap(2100,1340)
    $graphics=[System.Drawing.Graphics]::FromImage($overview)
    try {
        $graphics.Clear([System.Drawing.Color]::FromArgb(38,46,47))
        $graphics.DrawString('TIER 1 / SIX REGION ENVIRONMENT STUDIES',$title,$brush,25,12)
        for($i=0;$i -lt 6;$i++) {
            $x=($i%3)*700;$y=[int][Math]::Floor($i/3)*630+70
            $path=Join-Path $root ('Region{0:00}/biome-art-study.png' -f ($i+1))
            $cell=[System.Drawing.Image]::FromFile($path)
            try { $graphics.DrawImage($cell,$x,$y,700,500) } finally { $cell.Dispose() }
            $graphics.DrawString(('{0} / {1}' -f ($i+1),$names[$i]),$title,$brush,$x+20,$y+520)
            $graphics.DrawString('Asset composition only; no playable segment layout.',$font,$brush,$x+20,$y+560)
        }
        $overview.Save((Join-Path $root 'six-biome-overview.png'),[System.Drawing.Imaging.ImageFormat]::Png)
    } finally { $graphics.Dispose();$overview.Dispose() }
} finally { $font.Dispose();$title.Dispose();$brush.Dispose() }
