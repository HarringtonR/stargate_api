#!/usr/bin/env pwsh

Write-Host "=== STARGATE API COVERAGE REPORT ===" -ForegroundColor Cyan
Write-Host ""

# Run tests with the built-in coverlet configuration
Write-Host "Running tests with coverage collection..." -ForegroundColor Green
$testResult = dotnet test 2>&1

# Display test results
$testResult | ForEach-Object {
    $line = $_.ToString()
    if ($line -match "Test Run Successful|Test summary.*succeeded") {
        Write-Host $line -ForegroundColor Green
    } elseif ($line -match "Failed|Error" -and $line -notmatch "Failed to log to database") {
        Write-Host $line -ForegroundColor Red
    } elseif ($line -match "Total tests:|Passed:|Failed:|Skipped:") {
        Write-Host $line -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "Generating detailed coverage report..." -ForegroundColor Green

# Look for coverage files from both methods
$coverageFiles = @()
$searchPaths = @("TestResults", "StargateAPI.Tests\TestResults", ".")
$searchPatterns = @("coverage.cobertura.xml", "*.xml")

foreach ($path in $searchPaths) {
    foreach ($pattern in $searchPatterns) {
        $files = Get-ChildItem -Path $path -Filter $pattern -Recurse -ErrorAction SilentlyContinue | 
                 Where-Object { $_.Name -like "*coverage*" -or $_.Name -like "*cobertura*" }
        $coverageFiles += $files
    }
}

if ($coverageFiles.Count -gt 0) {
    $latestCoverage = $coverageFiles | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    
    Write-Host "Processing: $($latestCoverage.Name)" -ForegroundColor Yellow
    
    # Generate reports
    $reportOutput = reportgenerator -reports:"$($latestCoverage.FullName)" -targetdir:"TestResults/CoverageReport" -reporttypes:"TextSummary;Html" 2>&1
    
    # Display summary
    if (Test-Path "TestResults/CoverageReport/Summary.txt") {
        Write-Host ""
        Write-Host "=== COVERAGE SUMMARY ===" -ForegroundColor Cyan
        
        $summary = Get-Content "TestResults/CoverageReport/Summary.txt"
        $inSummarySection = $false
        
        foreach ($line in $summary) {
            if ($line -match "^Summary" -or $line -match "^StargateAPI") {
                $inSummarySection = $true
            }
            
            if ($inSummarySection) {
                if ($line -match "Line coverage:|Branch coverage:|Method coverage:") {
                    Write-Host $line -ForegroundColor Green -NoNewline
                    # Extract and highlight the percentage
                    if ($line -match "(\d+\.\d+%)") {
                        Write-Host " ? $($matches[1])" -ForegroundColor Yellow
                    } else {
                        Write-Host ""
                    }
                } elseif ($line -match "^\s*StargateAPI\s+(\d+\.\d+%)") {
                    Write-Host "Overall Project Coverage: " -NoNewline -ForegroundColor Cyan
                    Write-Host $matches[1] -ForegroundColor Yellow
                    Write-Host $line
                } else {
                    Write-Host $line
                }
            }
        }
    }
    
    Write-Host ""
    Write-Host "?? Full HTML Report: TestResults/CoverageReport/index.html" -ForegroundColor Green
    Write-Host ""
    
    # Show key metrics from the summary
    if (Test-Path "TestResults/CoverageReport/Summary.txt") {
        $content = Get-Content "TestResults/CoverageReport/Summary.txt" -Raw
        if ($content -match "Line coverage: (\d+\.\d+%)") {
            Write-Host "?? Current Line Coverage: $($matches[1])" -ForegroundColor Yellow
        }
        if ($content -match "Method coverage: (\d+\.\d+%)") {
            Write-Host "?? Method Coverage: $($matches[1])" -ForegroundColor Yellow  
        }
    }
    
} else {
    Write-Host "??  No coverage files found. Make sure tests ran successfully." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== QUICK COMMANDS ===" -ForegroundColor Cyan
Write-Host "• Full coverage: .\run-coverage.ps1" -ForegroundColor White
Write-Host "• Quick test: dotnet test" -ForegroundColor White  
Write-Host "• View report: start TestResults\CoverageReport\index.html" -ForegroundColor White
Write-Host ""