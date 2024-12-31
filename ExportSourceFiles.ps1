# Define source and destination directories
$source = "C:\Users\doola\OneDrive\Documents\Github\Basis Foundation\Basis Unity\Basis Server"
$destination = "C:\Users\doola\OneDrive\Documents\Github\Basis Foundation\Basis Unity\Basis\Packages\com.basis.server"

# Remove all .cs files in the destination directory
Get-ChildItem -Path $destination -Recurse -Include *.cs | Remove-Item -Force

# Get all files from the source, excluding .dll files, .asmdef files, and obj folders
Get-ChildItem -Path $source -Recurse | Where-Object { 
    # Exclude .dll files, .asmdef files, and obj directories
    $_.Extension -notin @('.dll', '.asmdef') -and $_.FullName -notmatch '\\obj\\'
} | ForEach-Object {
    # Calculate the destination file path
    $destinationPath = $_.FullName -replace [regex]::Escape($source), $destination

    # Ensure the destination folder exists
    $destinationFolder = [System.IO.Path]::GetDirectoryName($destinationPath)
    if (-not (Test-Path -Path $destinationFolder)) {
        New-Item -ItemType Directory -Path $destinationFolder -Force
    }

    # Copy the file to the destination
    if (-not $_.PSIsContainer) { # Ensure it's not a directory
        Copy-Item -Path $_.FullName -Destination $destinationPath -Force
    }
}