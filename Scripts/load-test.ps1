$apiUrl = "http://localhost:5107/api/PricingPackages" 
$concurrentRequests = 10
$iterations = 5

Write-Host "Running load test on $apiUrl"
Write-Host "Concurrent requests: $concurrentRequests"
Write-Host "Iterations: $iterations"
Write-Host ""


function Invoke-ApiRequest {
    param (
        [string]$url
    )

    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $response = Invoke-WebRequest -Uri $url -Method Get -UseBasicParsing
        $statusCode = $response.StatusCode
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
    }
    $stopwatch.Stop()

    return @{
        Time       = $stopwatch.ElapsedMilliseconds
        StatusCode = $statusCode
    }
}

# Run the load test
$allResults = @()

for ($i = 1; $i -le $iterations; $i++) {
    Write-Host "Iteration $i of $iterations"

    # Create jobs for concurrent requests
    $jobs = @()
    for ($j = 1; $j -le $concurrentRequests; $j++) {
        $jobs += Start-Job -ScriptBlock {
            param($url)

            $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
            try {
                $response = Invoke-WebRequest -Uri $url -Method Get -UseBasicParsing
                $statusCode = $response.StatusCode
            }
            catch {
                $statusCode = $_.Exception.Response.StatusCode.value__
            }
            $stopwatch.Stop()

            return @{
                Time       = $stopwatch.ElapsedMilliseconds
                StatusCode = $statusCode
            }
        } -ArgumentList $apiUrl
    }

    # Wait for all jobs to complete
    $results = $jobs | Wait-Job | Receive-Job
    $jobs | Remove-Job

    # Process results
    $iterationResults = @{
        Min          = ($results | Measure-Object -Property Time -Minimum).Minimum
        Max          = ($results | Measure-Object -Property Time -Maximum).Maximum
        Average      = ($results | Measure-Object -Property Time -Average).Average
        SuccessCount = ($results | Where-Object { $_.StatusCode -eq 200 }).Count
        FailureCount = ($results | Where-Object { $_.StatusCode -ne 200 }).Count
    }

    $allResults += $iterationResults

    # Display iteration results
    Write-Host "  Min response time: $($iterationResults.Min) ms"
    Write-Host "  Max response time: $($iterationResults.Max) ms"
    Write-Host "  Average response time: $([Math]::Round($iterationResults.Average, 2)) ms"
    Write-Host "  Success rate: $($iterationResults.SuccessCount)/$concurrentRequests"
    Write-Host ""

    # Small delay between iterations
    Start-Sleep -Seconds 1
}

# Calculate overall statistics
$overallAvg = ($allResults | Measure-Object -Property Average -Average).Average
$overallMin = ($allResults | Measure-Object -Property Min -Minimum).Minimum
$overallMax = ($allResults | Measure-Object -Property Max -Maximum).Maximum
$totalRequests = $concurrentRequests * $iterations
$totalSuccesses = ($allResults | Measure-Object -Property SuccessCount -Sum).Sum

# Display overall results
Write-Host "Load Test Summary:"
Write-Host "Total requests: $totalRequests"
Write-Host "Successful requests: $totalSuccesses"
Write-Host "Success rate: $([Math]::Round(($totalSuccesses / $totalRequests) * 100, 2))%"
Write-Host "Overall min response time: $overallMin ms"
Write-Host "Overall max response time: $overallMax ms"
Write-Host "Overall average response time: $([Math]::Round($overallAvg, 2)) ms"

# Performance assessment
if ($overallAvg -lt 100) {
    Write-Host "Performance assessment: Excellent" -ForegroundColor Green
}
elseif ($overallAvg -lt 300) {
    Write-Host "Performance assessment: Good" -ForegroundColor Green
}
elseif ($overallAvg -lt 1000) {
    Write-Host "Performance assessment: Acceptable" -ForegroundColor Yellow
}
else {
    Write-Host "Performance assessment: Poor - Optimization needed" -ForegroundColor Red
}
