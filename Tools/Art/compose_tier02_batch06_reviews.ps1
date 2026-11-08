$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$reviewRoot = Join-Path (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent) 'Docs/Art/Tier02/Batch06'
$assetNames = @('FlaxCloth','FlaxCord','FlaxStuffing','FineFlaxCloth','CattailCord')
$reviewViews = @('front','back','left','right','top','study')
$sheet = New-Object System.Drawing.Bitmap(1560,1450)
$canvas = [System.Drawing.Graphics]::FromImage($sheet)
$font = New-Object System.Drawing.Font('Arial',10)
$brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(239,238,222))
try {
    $canvas.Clear([System.Drawing.Color]::FromArgb(44,51,54))
    for ($row=0; $row -lt 5; $row++) {
        for ($col=0; $col -lt 6; $col++) {
            $file = Join-Path $reviewRoot ($assetNames[$row]+'-blender-'+$reviewViews[$col]+'.png')
            $frame = [System.Drawing.Image]::FromFile($file)
            try { $canvas.DrawImage($frame,[System.Drawing.Rectangle]::new($col*260,$row*290,256,256)) }
            finally { $frame.Dispose() }
            $canvas.DrawString(($assetNames[$row]+' / '+$reviewViews[$col]),$font,$brush,([single]($col*260+5)),([single]($row*290+262)))
        }
    }
    $sheet.Save((Join-Path $reviewRoot 'batch06-six-view-review.png'),[System.Drawing.Imaging.ImageFormat]::Png)
} finally { $canvas.Dispose(); $sheet.Dispose() }
$hero = New-Object System.Drawing.Bitmap(1440,1590)
$heroCanvas = [System.Drawing.Graphics]::FromImage($hero)
try {
    $heroCanvas.Clear([System.Drawing.Color]::FromArgb(51,59,61))
    for ($index=0; $index -lt 5; $index++) {
        $frame = [System.Drawing.Image]::FromFile((Join-Path $reviewRoot ($assetNames[$index]+'-unity-study.png')))
        try { $heroCanvas.DrawImage($frame,[System.Drawing.Rectangle]::new(($index%2)*720+120,([int][math]::Floor($index/2))*530,480,480)) }
        finally { $frame.Dispose() }
        $heroCanvas.DrawString($assetNames[$index],$font,$brush,([single](($index%2)*720+25)),([single](([int][math]::Floor($index/2))*530+490)))
    }
    $hero.Save((Join-Path $reviewRoot 'batch06-unity-overview.png'),[System.Drawing.Imaging.ImageFormat]::Png)
} finally { $heroCanvas.Dispose(); $hero.Dispose(); $font.Dispose(); $brush.Dispose() }
Write-Output 'Saved six-view sheet and actual Unity-prefab overview.'
