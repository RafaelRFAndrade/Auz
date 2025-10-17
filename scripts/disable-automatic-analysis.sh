#!/bin/bash

# Script para desabilitar análise automática no SonarCloud
# Execute este script se o workflow não conseguir desabilitar automaticamente

set -e

# Configurações
PROJECT_KEY="RafaelRFAndrade_Auz"
SONAR_TOKEN="${SONAR_TOKEN}"

if [ -z "$SONAR_TOKEN" ]; then
    echo "❌ SONAR_TOKEN não definido. Configure a variável de ambiente."
    exit 1
fi

echo "🔧 Desabilitando análise automática no SonarCloud..."

# Desabilitar análise automática
curl -u "$SONAR_TOKEN": -X POST \
  "https://sonarcloud.io/api/settings/set" \
  -d "key=sonar.ci.automaticAnalysis&value=false&component=$PROJECT_KEY"

if [ $? -eq 0 ]; then
    echo "✅ Análise automática desabilitada com sucesso!"
else
    echo "❌ Falha ao desabilitar análise automática"
    exit 1
fi

# Verificar configuração
echo "🔍 Verificando configuração..."
curl -u "$SONAR_TOKEN": \
  "https://sonarcloud.io/api/settings/values?component=$PROJECT_KEY" | \
  grep -i "automaticAnalysis" || echo "Configuração não encontrada"

echo ""
echo "🎉 Configuração concluída!"
echo "📋 Próximos passos:"
echo "   1. Execute o workflow do GitHub Actions novamente"
echo "   2. Verifique se a análise executa sem erros"
echo "   3. Confirme que o Quality Gate funciona"
