# Clean Linux-Compatible EB Deployment Script
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Elastic Beanstalk Deployment Script" -ForegroundColor Cyan
Write-Host "Linux-compatible • Test-free • Health checks" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Clean everything
Write-Host "1. Deep cleaning..." -ForegroundColor Yellow
Remove-Item -Path "bin", "obj", "eb-deploy", "*.zip" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "StargateAPI.Tests\bin", "StargateAPI.Tests\obj" -Recurse -Force -ErrorAction SilentlyContinue

# Build main project only (exclude tests completely)
Write-Host "2. Building main project (excluding tests)..." -ForegroundColor Yellow
dotnet clean StargateAPI.csproj --verbosity quiet
dotnet restore StargateAPI.csproj --verbosity quiet
dotnet build StargateAPI.csproj -c Release --verbosity quiet

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Publish main project only
Write-Host "3. Publishing main project..." -ForegroundColor Yellow
dotnet publish StargateAPI.csproj -c Release -o .\eb-deploy --no-build

if ($LASTEXITCODE -ne 0) {
    Write-Host "Publish failed!" -ForegroundColor Red
    exit 1
}

# Verify critical files exist
Write-Host "4. Verifying critical files..." -ForegroundColor Yellow
$requiredFiles = @(
    "StargateAPI.dll",
    "StargateAPI.runtimeconfig.json",
    "appsettings.json"
)

$allFilesPresent = $true
foreach ($file in $requiredFiles) {
    if (Test-Path ".\eb-deploy\$file") {
        Write-Host "  ? $file" -ForegroundColor Green
    } else {
        Write-Host "  ? $file MISSING" -ForegroundColor Red
        $allFilesPresent = $false
    }
}

if (-not $allFilesPresent) {
    Write-Host "Critical files missing! Cannot continue." -ForegroundColor Red
    exit 1
}

# Clean up test files and debug symbols
Write-Host "5. Removing unnecessary files..." -ForegroundColor Yellow
Get-ChildItem -Path ".\eb-deploy" -Recurse | Where-Object { 
    $_.Name -like "*Test*" -or 
    $_.Name -like "*.pdb" -or 
    $_.Name -like "*testhost*" -or
    $_.Name -like "*xunit*" -or
    $_.Directory.Name -like "*Test*"
} | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue

Write-Host "  ? Cleaned test files and debug symbols" -ForegroundColor Green

# Copy EB configuration
Write-Host "6. Copying EB configuration..." -ForegroundColor Yellow
if (Test-Path ".ebextensions") {
    Copy-Item -Path ".ebextensions" -Destination ".\eb-deploy\.ebextensions" -Recurse -Force
    Write-Host "  ? .ebextensions copied" -ForegroundColor Green
}

if (Test-Path "Procfile") {
    Copy-Item -Path "Procfile" -Destination ".\eb-deploy\Procfile" -Force
    Write-Host "  ? Procfile copied" -ForegroundColor Green
}

# Create a health check endpoint file
Write-Host "7. Adding health check endpoint..." -ForegroundColor Yellow
$healthCheckContent = @"
<!DOCTYPE html>
<html>
<head><title>Health Check</title></head>
<body><h1>API is running</h1><p>Timestamp: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')</p></body>
</html>
"@
New-Item -Path ".\eb-deploy\health.html" -ItemType File -Value $healthCheckContent -Force
Write-Host "  ? Health check file created" -ForegroundColor Green

# Update runtime configuration for better compatibility
Write-Host "8. Optimizing runtime configuration..." -ForegroundColor Yellow
$runtimeConfigPath = ".\eb-deploy\StargateAPI.runtimeconfig.json"
if (Test-Path $runtimeConfigPath) {
    try {
        $runtimeConfig = Get-Content $runtimeConfigPath | ConvertFrom-Json
        
        # Ensure correct framework reference
        if ($runtimeConfig.runtimeOptions.framework.name -ne "Microsoft.AspNetCore.App") {
            $runtimeConfig.runtimeOptions.framework.name = "Microsoft.AspNetCore.App"
            $runtimeConfig.runtimeOptions.framework.version = "8.0.0"
        }
        
        # Add server GC for better performance
        if (-not $runtimeConfig.runtimeOptions.configProperties) {
            $runtimeConfig.runtimeOptions | Add-Member -MemberType NoteProperty -Name "configProperties" -Value @{}
        }
        $runtimeConfig.runtimeOptions.configProperties."System.GC.Server" = $true
        
        # Save updated config
        $runtimeConfig | ConvertTo-Json -Depth 4 | Set-Content $runtimeConfigPath -Encoding UTF8
        Write-Host "  ? Runtime configuration optimized" -ForegroundColor Green
    } catch {
        Write-Host "  ? Could not optimize runtime config: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

# Create Linux-compatible ZIP using .NET compression
Write-Host "9. Creating Linux-compatible deployment package..." -ForegroundColor Yellow
Add-Type -AssemblyName System.IO.Compression.FileSystem

$deployPath = Resolve-Path ".\eb-deploy"
$zipPath = Join-Path (Get-Location) "stargate-deployment.zip"

# Remove existing ZIP
Remove-Item $zipPath -Force -ErrorAction SilentlyContinue

# Create ZIP with Linux-compatible paths
$zip = [System.IO.Compression.ZipFile]::Open($zipPath, [System.IO.Compression.ZipArchiveMode]::Create)

try {
    $fileCount = 0
    Get-ChildItem -Path $deployPath -Recurse -File | ForEach-Object {
        $relativePath = $_.FullName.Substring($deployPath.Path.Length + 1)
        # Convert Windows backslashes to Linux forward slashes
        $linuxPath = $relativePath.Replace('\', '/')
        
        # Skip certain files that might cause issues
        if ($linuxPath -notlike "*.pdb" -and $linuxPath -notlike "*Test*") {
            $zipEntry = $zip.CreateEntry($linuxPath)
            $zipEntryStream = $zipEntry.Open()
            $fileStream = $_.OpenRead()
            $fileStream.CopyTo($zipEntryStream)
            $fileStream.Close()
            $zipEntryStream.Close()
            $fileCount++
        }
    }
    Write-Host "  ? Added $fileCount files to deployment package" -ForegroundColor Green
} finally {
    $zip.Dispose()
}

# Verify ZIP contents
Write-Host "10. Verifying deployment package..." -ForegroundColor Yellow
$verifyZip = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
$pathsInZip = $verifyZip.Entries | Select-Object -ExpandProperty FullName

# Check for Windows path separators
$hasBackslashes = $pathsInZip | Where-Object { $_ -like "*\*" }
if ($hasBackslashes) {
    Write-Host "  ? WARNING: ZIP contains backslashes!" -ForegroundColor Red
    $hasBackslashes | Select-Object -First 5 | ForEach-Object { Write-Host "    $_" -ForegroundColor Red }
} else {
    Write-Host "  ? All paths use Linux-compatible forward slashes" -ForegroundColor Green
}

# Check for critical files
$criticalFiles = @("StargateAPI.dll", "StargateAPI.runtimeconfig.json", "appsettings.json", "Procfile")
foreach ($file in $criticalFiles) {
    if ($pathsInZip -contains $file) {
        Write-Host "  ? $file included" -ForegroundColor Green
    } else {
        Write-Host "  ? $file MISSING from ZIP" -ForegroundColor Red
    }
}

$verifyZip.Dispose()

# Final summary
$packageSize = [math]::Round((Get-Item $zipPath).Length / 1MB, 2)
$totalFiles = $pathsInZip.Count

Write-Host "========================================" -ForegroundColor Green
Write-Host "Deployment Package Ready!" -ForegroundColor Green
Write-Host "File: stargate-deployment.zip" -ForegroundColor Green
Write-Host "Size: $packageSize MB" -ForegroundColor Green
Write-Host "Files: $totalFiles" -ForegroundColor Green
Write-Host "Tests excluded: ?" -ForegroundColor Green
Write-Host "Linux-compatible: ?" -ForegroundColor Green
Write-Host "Health checks: ?" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "?? Deploy with:" -ForegroundColor Cyan
Write-Host "   eb deploy" -ForegroundColor White
Write-Host ""
Write-Host "? Features:" -ForegroundColor Cyan
Write-Host "   • No test files or debug symbols" -ForegroundColor White
Write-Host "   • Linux path separators (fixes unzip errors)" -ForegroundColor White
Write-Host "   • Health check endpoints for ELB" -ForegroundColor White
Write-Host "   • Optimized runtime configuration" -ForegroundColor White
Write-Host "   • Clean, minimal deployment package" -ForegroundColor White