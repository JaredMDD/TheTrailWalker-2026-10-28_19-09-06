param([Parameter(Mandatory=$true)][int]$Batch)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$projectRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$code = $Batch.ToString('D2')
$reviewRoot = Join-Path $projectRoot ('Docs/Art/Tier02/Batch'+$code)
$manifest = Get-Content (Join-Path $projectRoot ('Assets/_Game/Art/Crafting/Tier02/Tier02Batch'+$code+'Manifest.json')) -Raw | ConvertFrom-Json
$assetNames = @($manifest.Items | ForEach-Object { $_.Model })
$reviewViews = @('front','back','left','right','top','study')
$font = New-Object System.Drawing.Font('Arial',10)
$brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(239,238,222))
function Save-Board($suffix,$columns,$views,$cellWidth,$cellHeight,$imageSize) {
    $cells=$assetNames.Count*$views.Count
    $rows=[int][math]::Ceiling($cells/$columns)
    $board=New-Object System.Drawing.Bitmap(($columns*$cellWidth),($rows*$cellHeight))
    $canvas=[System.Drawing.Graphics]::FromImage($board)
    try {
        $canvas.Clear([System.Drawing.Color]::FromArgb(44,51,54))
        $i=0
        foreach ($name in $assetNames) {
            foreach ($view in $views) {
                $x=($i%$columns)*$cellWidth
                $y=([int][math]::Floor($i/$columns))*$cellHeight
                $path=Join-Path $reviewRoot ($name+'-'+$view+'.png')
                $frame=[System.Drawing.Image]::FromFile($path)
                try { $canvas.DrawImage($frame,[System.Drawing.Rectangle]::new(($x+[int](($cellWidth-$imageSize)/2)),$y,$imageSize,$imageSize)) }
                finally { $frame.Dispose() }
                $canvas.DrawString(($name+' / '+$view),$font,$brush,[single]($x+5),[single]($y+$imageSize+6))
                $i++
            }
        }
        $board.Save((Join-Path $reviewRoot ('batch'+$code+'-'+$suffix+'.png')),[System.Drawing.Imaging.ImageFormat]::Png)
    } finally { $canvas.Dispose(); $board.Dispose() }
}
try {
    Save-Board 'six-view-review' 6 @('blender-front','blender-back','blender-left','blender-right','blender-top','blender-study') 260 290 256
    Save-Board 'unity-overview' 2 @('unity-study') 720 530 480
    Save-Board 'unity-front-review' 3 @('unity-front') 410 440 400
} finally { $font.Dispose(); $brush.Dispose() }
Write-Output ('Saved batch '+$code+' native six-view, Unity study and Unity front boards.')
