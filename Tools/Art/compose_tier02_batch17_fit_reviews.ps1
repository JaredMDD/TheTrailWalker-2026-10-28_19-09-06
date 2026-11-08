$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$reviewRoot=Join-Path (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent) 'Docs/Art/Tier02/Batch17'
$font=New-Object System.Drawing.Font('Arial',10)
$brush=New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(239,238,222))
function Board($name,$files,$columns,$w,$h) {
    $rows=[int][math]::Ceiling($files.Count/$columns)
    $board=New-Object System.Drawing.Bitmap(($columns*$w),($rows*($h+28)))
    $canvas=[System.Drawing.Graphics]::FromImage($board)
    try {
        $canvas.Clear([System.Drawing.Color]::FromArgb(44,51,54))
        for($i=0;$i -lt $files.Count;$i++) {
            $x=($i%$columns)*$w;$y=([int][math]::Floor($i/$columns))*($h+28)
            $frame=[System.Drawing.Image]::FromFile((Join-Path $reviewRoot ($files[$i]+'.png')))
            try{$canvas.DrawImage($frame,[System.Drawing.Rectangle]::new($x,$y,$w,$h))}finally{$frame.Dispose()}
            $canvas.DrawString($files[$i],$font,$brush,[single]($x+3),[single]($y+$h+3))
        }
        $board.Save((Join-Path $reviewRoot ($name+'.png')),[System.Drawing.Imaging.ImageFormat]::Png)
    }finally{$canvas.Dispose();$board.Dispose()}
}
try {
    foreach($body in @('Male','Female')) {
        $files=@();foreach($pose in @('standing','seated','stride','reach')){foreach($yaw in @(0,90,180,35)){$files+=($body+'-fit-'+$pose+'-'+$yaw)}}
        Board ($body+'-pose-review') $files 4 240 270
        foreach($part in @('shirt','pants')) {
            $files=@();foreach($yaw in @(0,90,180,35)){$files+=($body+'-'+$part+'-close-'+$yaw)}
            Board ($body+'-'+$part+'-review') $files 4 300 338
        }
    }
}finally{$font.Dispose();$brush.Dispose()}
Write-Output 'Saved all 48 static fit/garment views in six review boards.'

