# Launches the backend (.NET) and frontend (Vite) for local development.
# Press Ctrl+C to stop both processes.

$root = $PSScriptRoot

Write-Host "Starting backend..." -ForegroundColor Cyan
$backend = Start-Process -FilePath "dotnet" `
    -ArgumentList "run", "--project", "DustRacing2D.Server" `
    -WorkingDirectory "$root\backend" `
    -PassThru -NoNewWindow

Write-Host "Starting frontend..." -ForegroundColor Cyan
$frontend = Start-Process -FilePath "npm" `
    -ArgumentList "run", "dev" `
    -WorkingDirectory "$root\frontend" `
    -PassThru -NoNewWindow

Write-Host ""
Write-Host "Both processes running. Press Ctrl+C to stop." -ForegroundColor Green
Write-Host "  Backend : http://localhost:5000" -ForegroundColor Yellow
Write-Host "  Frontend: http://localhost:5173" -ForegroundColor Yellow
Write-Host ""

try {
    while ($true) { Start-Sleep -Seconds 1 }
} finally {
    Write-Host "`nStopping processes..." -ForegroundColor Red
    if (-not $backend.HasExited)  { Stop-Process -Id $backend.Id  -Force }
    if (-not $frontend.HasExited) { Stop-Process -Id $frontend.Id -Force }
}
