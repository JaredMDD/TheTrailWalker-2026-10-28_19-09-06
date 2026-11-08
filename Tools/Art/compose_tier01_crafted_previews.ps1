Add-Type -AssemblyName System.Drawing
$projectRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$artPath = Join-Path $projectRoot 'Docs/Art/Tier01/Crafted'
$font = New-Object System.Drawing.Font('Segoe UI',12)
$titleFont = New-Object System.Drawing.Font('Segoe UI',19,[System.Drawing.FontStyle]::Bold)
$ink = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(244,231,201))
try {
    foreach ($category in @('Material','Tool','Wearable','Food','Misc','ShopInputs')) {
        $labels = @(Get-Content -LiteralPath (Join-Path $artPath "$category-labels.txt"))
        $columns = [Math]::Min(4,$labels.Count)
        $rows = [int][Math]::Ceiling($labels.Count / $columns)
        $bitmap = New-Object System.Drawing.Bitmap(($columns*352),($rows*332+72))
        $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
        try {
            $graphics.Clear([System.Drawing.Color]::FromArgb(39,45,50))
            $graphics.DrawString("TIER 1 / $category",$titleFont,$ink,20,16)
            for ($index=0; $index -lt $labels.Count; $index++) {
                $x = ($index % $columns)*352+16
                $y = [int][Math]::Floor($index/$columns)*332+72
                $cell = Join-Path $artPath ('{0}-{1:00}.png' -f $category,$index)
                $image = [System.Drawing.Image]::FromFile($cell)
                try { $graphics.DrawImage($image,$x,$y,320,280) } finally { $image.Dispose() }
                $graphics.DrawString($labels[$index],$font,$ink,$x,$y+288)
            }
            $bitmap.Save((Join-Path $artPath "$category-gallery.png"),[System.Drawing.Imaging.ImageFormat]::Png)
        } finally { $graphics.Dispose(); $bitmap.Dispose() }
    }
} finally { $font.Dispose(); $titleFont.Dispose(); $ink.Dispose() }
