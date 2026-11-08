# Label actual Unity mesh renders. Windows System.Drawing; no Python packages required.
Add-Type -AssemblyName System.Drawing
$ErrorActionPreference = 'Stop'
$houseDocs = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../Docs/Art/Settlement/Houses'))
$houseManifest = Get-Content (Join-Path $PSScriptRoot '../../Assets/_Game/Art/Settlement/Houses/HouseManifest.json') -Raw | ConvertFrom-Json
$houseInk = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#302E29'))
$houseQuiet = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#666253'))
$houseTitle = [Drawing.Font]::new('Segoe UI', 43, [Drawing.FontStyle]::Bold, [Drawing.GraphicsUnit]::Pixel)
$houseLabel = [Drawing.Font]::new('Segoe UI', 25, [Drawing.FontStyle]::Bold, [Drawing.GraphicsUnit]::Pixel)
$houseBody = [Drawing.Font]::new('Segoe UI', 22, [Drawing.FontStyle]::Regular, [Drawing.GraphicsUnit]::Pixel)

function Write-HouseGallery([string]$Suffix, [string]$Output, [string]$Title, [string]$Subtitle) {
    $canvas = [Drawing.Bitmap]::new(2240, 1550)
    $graphics = [Drawing.Graphics]::FromImage($canvas)
    try {
        $graphics.Clear([Drawing.ColorTranslator]::FromHtml('#E8E2D5'))
        $graphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.TextRenderingHint = [Drawing.Text.TextRenderingHint]::AntiAliasGridFit
        $graphics.DrawString($Title, $houseTitle, $houseInk, 34, 20)
        $graphics.DrawString($Subtitle, $houseBody, $houseQuiet, 36, 84)
        for ($i = 0; $i -lt 8; $i++) {
            $spec = $houseManifest.Houses[$i]
            $x = 30 + ($i % 4) * 555
            $y = 155 + [Math]::Floor($i / 4) * 665
            $filename = 'house-l{0:00}-{1}.png' -f $spec.Level, $Suffix
            $picture = [Drawing.Image]::FromFile((Join-Path $houseDocs $filename))
            try {
                $scale = [Math]::Min(530.0 / $picture.Width, 530.0 / $picture.Height)
                $w = [int]($picture.Width * $scale); $h = [int]($picture.Height * $scale)
                $graphics.DrawImage($picture, [int]($x+(530-$w)/2), [int]$y, $w, $h)
            } finally { $picture.Dispose() }
            $graphics.DrawString(('L{0}  {1}' -f $spec.Level, $spec.Name), $houseLabel, $houseInk, [single]$x, [single]($y+542))
            $graphics.DrawString(('{0} / tier {1}   |   {2} x {3} interior' -f $spec.Wood,$spec.Tier,$spec.GridWidth,$spec.GridDepth), $houseBody, $houseQuiet, [single]$x, [single]($y+581))
        }
        $graphics.DrawString('Generated mesh studies only. Gameplay and device testing remain pending.', $houseBody, $houseQuiet, 35, 1504)
        $canvas.Save((Join-Path $houseDocs $Output), [Drawing.Imaging.ImageFormat]::Png)
    } finally { $graphics.Dispose(); $canvas.Dispose() }
}

try {
    Write-HouseGallery 'three-quarter' 'house-design-overview.png' 'Eight stages of the player home' 'Shared player / neighbour exteriors, one storey. Width stops growing at level 3; every level grows deeper toward the back.'
    Write-HouseGallery 'interior-grid' 'house-interior-overview.png' 'Matched cutaway interiors' 'Floor grids grow toward the back with each level (width x depth). Guides are optional; trim does not consume placement capacity.'
    Write-HouseGallery 'furnished-study' 'house-furniture-studies.png' 'Six new furniture types per level - proxy layouts' 'Optional composition studies only. Homes receive no free furniture or buffs from these previews.'
    $lineup = [Drawing.Image]::FromFile((Join-Path $houseDocs 'eight-level-lineup.png'))
    $canvas = [Drawing.Bitmap]::new($lineup.Width, $lineup.Height + 250)
    $graphics = [Drawing.Graphics]::FromImage($canvas)
    try {
        $graphics.Clear([Drawing.ColorTranslator]::FromHtml('#E8E2D5'))
        $graphics.DrawImage($lineup, 0, 125, $lineup.Width, $lineup.Height)
        $graphics.DrawString('Small shed to finished homestead - one shared house family', $houseTitle, $houseInk, 45, 25)
        for ($i = 0; $i -lt 8; $i++) {
            $spec = $houseManifest.Houses[$i]; $x = 40 + $i * 600
            $graphics.DrawString(('L{0}  {1}' -f $spec.Level,$spec.Name),$houseLabel,$houseInk,[single]$x,[single]($lineup.Height+145))
            $graphics.DrawString(('{0} x {1} interior' -f $spec.GridWidth,$spec.GridDepth),$houseBody,$houseQuiet,[single]$x,[single]($lineup.Height+188))
        }
        $canvas.Save((Join-Path $houseDocs 'eight-level-lineup-labeled.png'),[Drawing.Imaging.ImageFormat]::Png)
    } finally { $graphics.Dispose(); $canvas.Dispose(); $lineup.Dispose() }
    Write-Output 'Composed four labeled review sheets from Unity mesh renders.'
} finally {
    $houseInk.Dispose(); $houseQuiet.Dispose(); $houseTitle.Dispose(); $houseLabel.Dispose(); $houseBody.Dispose()
}
