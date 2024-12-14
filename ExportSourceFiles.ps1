# Define source and destination directories
$source = "C:\Users\doola\OneDrive\Documents\Github\Basis Foundation\Basis Unity\Basis Server"
$destination = "C:\Users\doola\OneDrive\Documents\Github\Basis Foundation\Basis Unity\Basis\Packages\Basis Server\"

# Remove the destination folder and recreate it
Remove-Item -Recurse -Force $destination
New-Item -ItemType Directory -Path $destination

# Get all files from the source, excluding .dll files and obj folders
Get-ChildItem -Path $source -Recurse | Where-Object { 
    # Exclude .dll files and obj directories
    $_.Extension -ne '.dll' -and $_.FullName -notmatch '\\obj\\'
} | ForEach-Object {
    # Calculate the destination file path
    $destinationPath = $_.FullName.Replace($source, $destination)

    # Create destination folder if it doesn't exist
    $destinationFolder = [System.IO.Path]::GetDirectoryName($destinationPath)
    if (-not (Test-Path -Path $destinationFolder)) {
        New-Item -ItemType Directory -Path $destinationFolder
    }

    # Copy the file to the destination
    Copy-Item -Path $_.FullName -Destination $destinationPath -Force
}