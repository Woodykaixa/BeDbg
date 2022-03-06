function CopyDll {
    param (
        [string]$source,
        [string]$clientPath,
        [string]$testPath
    )
    
    Copy-Item $source -Destination $clientPath;
    Copy-Item $source -Destination $testPath;
}

CopyDll $args[0] $args[1] $args[2]
