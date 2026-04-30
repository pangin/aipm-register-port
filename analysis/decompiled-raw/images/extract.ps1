#requires -Version 5.1
# Extracts every image-typed resource from AIPM_Register.frmMain.resx into
# this folder. Run on Windows PowerShell 5.1 (it ships with .NET Framework
# 4.8 + the BinaryFormatter that ImageListStreamer needs to deserialize).
# The .resx ships 4 binary entries:
#   * $this.Icon                  -> form icon            (.ico)
#   * picLogo.BackgroundImage     -> top-left logo        (.png)
#   * picPcKey.BackgroundImage    -> "PC key" hint art    (.png)
#   * imageList1.ImageStream      -> 13 product photos    (.png each, key=model code)

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Design

$here  = Split-Path -Parent $PSCommandPath
$resx  = Join-Path (Split-Path -Parent $here) 'AIPM_Register.frmMain.resx'

if (-not (Test-Path $resx)) {
    throw "resx not found: $resx"
}

$reader = New-Object System.Resources.ResXResourceReader($resx)
try {
    foreach ($entry in $reader) {
        $name  = $entry.Key
        $value = $entry.Value
        if ($null -eq $value) { continue }

        switch ($value) {
            { $_ -is [System.Drawing.Icon] } {
                $path = Join-Path $here ("{0}.ico" -f ($name -replace '[^A-Za-z0-9._-]', '_'))
                $fs = [System.IO.File]::Open($path, 'Create')
                try { $value.Save($fs) } finally { $fs.Dispose() }
                Write-Host "icon  -> $path"
                continue
            }
            { $_ -is [System.Windows.Forms.ImageListStreamer] } {
                $il = New-Object System.Windows.Forms.ImageList
                $il.ImageStream = $value
                for ($i = 0; $i -lt $il.Images.Count; $i++) {
                    $key  = $il.Images.Keys[$i]
                    if ([string]::IsNullOrEmpty($key)) { $key = "img$i" }
                    $img  = $il.Images[$i]
                    $path = Join-Path $here ("{0}.png" -f $key)
                    $img.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
                    Write-Host "imageList[$i]=$key -> $path"
                }
                continue
            }
            { $_ -is [System.Drawing.Image] } {
                $safe = $name -replace '[^A-Za-z0-9._-]', '_'
                $path = Join-Path $here ("{0}.png" -f $safe)
                $value.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
                Write-Host "image -> $path"
                continue
            }
        }
    }
}
finally {
    $reader.Close()
}
