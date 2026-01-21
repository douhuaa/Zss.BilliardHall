# Pre-push hook for architecture validation (Windows PowerShell)
# Install: Copy to .git/hooks/pre-push (no extension) and ensure Git can execute PowerShell

Write-Host "🏗️  Running architecture validation before push..." -ForegroundColor Cyan
Write-Host ""

# Build the solution
Write-Host "📦 Building solution..." -ForegroundColor Yellow
$buildResult = & dotnet build --configuration Release 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed" -ForegroundColor Red
    Write-Host "   Fix build errors before pushing" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Build successful" -ForegroundColor Green
Write-Host ""

# Run architecture tests
Write-Host "🔍 Running architecture tests..." -ForegroundColor Yellow
$testResult = & dotnet test src/tests/ArchitectureTests --no-build --configuration Release --verbosity quiet 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Architecture tests failed" -ForegroundColor Red
    Write-Host ""
    Write-Host "Test output:" -ForegroundColor Yellow
    Write-Host $testResult
    Write-Host ""
    Write-Host "Please fix architecture violations before pushing." -ForegroundColor Red
    Write-Host "See docs/adr/ADR-0000-architecture-tests.md for guidance." -ForegroundColor Yellow
    exit 1
}
Write-Host "✅ Architecture tests passed" -ForegroundColor Green
Write-Host ""

# Check for analyzer warnings (optional, non-blocking)
Write-Host "🔬 Checking for analyzer warnings..." -ForegroundColor Yellow
$analyzerWarnings = & dotnet build --no-restore --configuration Release 2>&1 | Select-String "warning ADR"
$warningCount = ($analyzerWarnings | Measure-Object).Count
if ($warningCount -gt 0) {
    Write-Host "⚠️  Found $warningCount architecture analyzer warning(s)" -ForegroundColor Yellow
    Write-Host "   Consider reviewing these before pushing (non-blocking)" -ForegroundColor Yellow
} else {
    Write-Host "✅ No analyzer warnings" -ForegroundColor Green
}
Write-Host ""

Write-Host "🎉 All checks passed! Push allowed." -ForegroundColor Green
exit 0
