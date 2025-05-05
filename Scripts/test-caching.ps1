# PowerShell script to test API caching performance

# Configuration
$apiUrl = "http://localhost:5107/api/PricingPackages" # Update with your actual API endpoint
$iterations = 5

Write-Host "Testing API caching performance for $apiUrl"
Write-Host "Running $iterations iterations..."
Write-Host ""

# First request (cold cache)
Write-Host "First request (cold cache):"
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
$response = Invoke-WebRequest -Uri $apiUrl -Method Get
$stopwatch.Stop()
$firstRequestTime = $stopwatch.ElapsedMilliseconds
Write-Host "Response time: $firstRequestTime ms"
Write-Host "Status code: $($response.StatusCode)"
Write-Host ""

# Subsequent requests (should use cache)
$totalTime = 0
Write-Host "Subsequent requests (should use cache):"
for ($i = 1; $i -le $iterations; $i++) {
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    $response = Invoke-WebRequest -Uri $apiUrl -Method Get
    $stopwatch.Stop()
    $requestTime = $stopwatch.ElapsedMilliseconds
    $totalTime += $requestTime
    Write-Host "Request $i - Response time: $requestTime ms"
}

$averageTime = $totalTime / $iterations
$improvementPercentage = [Math]::Round((($firstRequestTime - $averageTime) / $firstRequestTime) * 100, 2)

Write-Host ""
Write-Host "Performance Summary:"
Write-Host "First request (cold cache): $firstRequestTime ms"
Write-Host "Average of subsequent requests: $averageTime ms"
Write-Host "Performance improvement: $improvementPercentage%"

if ($improvementPercentage -gt 30) {
    Write-Host "Caching is working effectively!" -ForegroundColor Green
} elseif ($improvementPercentage -gt 10) {
    Write-Host "Caching is working, but could be improved." -ForegroundColor Yellow
} else {
    Write-Host "Caching may not be working correctly. Check your configuration." -ForegroundColor Red
}
