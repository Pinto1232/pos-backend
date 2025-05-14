# PowerShell script to test API caching performance

# Configuration
$apiUrl = "http://localhost:5107/api/PricingPackages" # Update with your actual API endpoint
$iterations = 10
$clearCacheUrl = "http://localhost:5107/api/Cache/clear" # Update with your actual cache clear endpoint

Write-Host "Testing API caching performance for $apiUrl"
Write-Host "Running $iterations iterations..."
Write-Host ""

# Function to test API performance
function Test-ApiPerformance {
    param (
        [string]$testName,
        [string]$url,
        [int]$iterations,
        [switch]$clearCache
    )
    
    Write-Host "=== $testName ==="
    
    if ($clearCache) {
        try {
            Write-Host "Clearing cache..."
            Invoke-WebRequest -Uri $clearCacheUrl -Method Post -Headers @{"Authorization" = "Bearer $token"} | Out-Null
        }
        catch {
            Write-Host "Failed to clear cache: $_"
        }
    }
    
    $times = @()
    
    for ($i = 1; $i -le $iterations; $i++) {
        Write-Host "Request $i of $iterations..."
        $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
        
        try {
            $response = Invoke-WebRequest -Uri $url -Method Get
            $stopwatch.Stop()
            $time = $stopwatch.ElapsedMilliseconds
            $times += $time
            
            Write-Host "  Response time: $time ms"
            Write-Host "  Status code: $($response.StatusCode)"
        }
        catch {
            $stopwatch.Stop()
            Write-Host "  Error: $_"
        }
    }
    
    # Calculate statistics
    if ($times.Count -gt 0) {
        $avg = ($times | Measure-Object -Average).Average
        $min = ($times | Measure-Object -Minimum).Minimum
        $max = ($times | Measure-Object -Maximum).Maximum
        
        Write-Host ""
        Write-Host "Results:"
        Write-Host "  Average response time: $avg ms"
        Write-Host "  Minimum response time: $min ms"
        Write-Host "  Maximum response time: $max ms"
        Write-Host ""
        
        return @{
            Average = $avg
            Minimum = $min
            Maximum = $max
            Times = $times
        }
    }
    
    return $null
}

# Get an authentication token (if needed)
# $token = Get-AuthToken

# Test 1: Cold cache (first request)
$coldCacheResults = Test-ApiPerformance -testName "Cold Cache Test" -url $apiUrl -iterations $iterations -clearCache

# Test 2: Warm cache (subsequent requests)
$warmCacheResults = Test-ApiPerformance -testName "Warm Cache Test" -url $apiUrl -iterations $iterations

# Compare results
if ($coldCacheResults -and $warmCacheResults) {
    $improvement = 100 - (($warmCacheResults.Average / $coldCacheResults.Average) * 100)
    
    Write-Host "Performance Improvement:"
    Write-Host "  Cold cache average: $($coldCacheResults.Average) ms"
    Write-Host "  Warm cache average: $($warmCacheResults.Average) ms"
    Write-Host "  Improvement: $improvement%"
}
