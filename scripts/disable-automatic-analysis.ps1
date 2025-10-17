# Script PowerShell para desabilitar análise automática no SonarCloud
# Execute este script se o workflow não conseguir desabilitar automaticamente

param(
    [string]$ProjectKey = "RafaelRFAndrade_Auz",
    [string]$SonarToken = $env:SONAR_TOKEN
)

Write-Host "🔧 Desabilitando análise automática no SonarCloud..." -ForegroundColor Blue

if (-not $SonarToken) {
    Write-Host "❌ SONAR_TOKEN não definido. Configure a variável de ambiente." -ForegroundColor Red
    exit 1
}

try {
    # Desabilitar análise automática
    $response = Invoke-RestMethod -Uri "https://sonarcloud.io/api/settings/set" `
        -Method Post `
        -Headers @{Authorization = "Basic $([Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("$SonarToken`:")))"} `
        -Body @{
            key = "sonar.ci.automaticAnalysis"
            value = "false"
            component = $ProjectKey
        }
    
    Write-Host "✅ Análise automática desabilitada com sucesso!" -ForegroundColor Green
} catch {
    Write-Host "❌ Falha ao desabilitar análise automática: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Verificar configuração
Write-Host "🔍 Verificando configuração..." -ForegroundColor Blue
try {
    $config = Invoke-RestMethod -Uri "https://sonarcloud.io/api/settings/values?component=$ProjectKey" `
        -Headers @{Authorization = "Basic $([Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("$SonarToken`:")))"}
    
    $autoAnalysis = $config.settings | Where-Object { $_.key -eq "sonar.ci.automaticAnalysis" }
    if ($autoAnalysis) {
        Write-Host "📋 Configuração encontrada: $($autoAnalysis.value)" -ForegroundColor Cyan
    } else {
        Write-Host "⚠️  Configuração não encontrada" -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠️  Não foi possível verificar a configuração" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "🎉 Configuração concluída!" -ForegroundColor Green
Write-Host "📋 Próximos passos:" -ForegroundColor Cyan
Write-Host "   1. Execute o workflow do GitHub Actions novamente" -ForegroundColor White
Write-Host "   2. Verifique se a análise executa sem erros" -ForegroundColor White
Write-Host "   3. Confirme que o Quality Gate funciona" -ForegroundColor White
