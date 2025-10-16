# 🔍 Configuração do SonarCloud - Auz API

Este guia explica como configurar o [SonarCloud](https://sonarcloud.io/) para análise automática de qualidade de código a cada commit.

## 🚀 Configuração Inicial

### 1. Criar Conta no SonarCloud

1. Acesse [https://sonarcloud.io/](https://sonarcloud.io/)
2. Clique em "Start for free"
3. Faça login com sua conta GitHub
4. Autorize o SonarCloud a acessar seus repositórios

### 2. Configurar Projeto

1. No dashboard do SonarCloud, clique em "Analyze new project"
2. Selecione "GitHub" como fonte
3. Escolha seu repositório "Auz"
4. Configure as seguintes informações:
   - **Project Key**: `RafaelRFAndrade_Auz`
   - **Organization**: `rafaelrfandrade`
   - **Project Name**: `Auz API`

### 3. Obter Token de Acesso

1. No SonarCloud, vá em **Account** → **Security**
2. Gere um novo token:
   - **Name**: `GitHub Actions`
   - **Type**: `Global Analysis Token`
3. **Copie o token** (você só verá uma vez!)

## 🔧 Configuração no GitHub

### 1. Adicionar Secrets

No seu repositório GitHub:

1. Vá em **Settings** → **Secrets and variables** → **Actions**
2. Clique em **New repository secret**
3. Adicione os seguintes secrets:

```
SONAR_TOKEN = seu_token_do_sonarcloud
```

### 2. Configurar Branch Protection (Opcional)

Para garantir que apenas código de qualidade seja mergeado:

1. Vá em **Settings** → **Branches**
2. Clique em **Add rule**
3. Configure:
   - **Branch name pattern**: `main`
   - ✅ **Require status checks to pass before merging**
   - ✅ **Require branches to be up to date before merging**
   - Selecione: `SonarCloud Analysis`

## 📋 Arquivos Criados

### 1. Configuração via GitHub Actions
Configurações específicas do projeto para análise (via argumentos do workflow):
- Caminhos do código fonte
- Exclusões de arquivos
- Configurações de qualidade
- Configurações de segurança
- **Desabilitação da análise automática** para evitar conflito com CI

### 2. `.github/workflows/sonarcloud.yml`
Workflow do GitHub Actions que:
- Executa a cada push/PR
- Faz build e testes
- Gera relatório de cobertura
- Executa análise do SonarCloud
- Verifica Quality Gate
- **Configurado para evitar conflito com análise automática**

### 3. `.github/workflows/security-scan.yml`
Workflow adicional para:
- Análise de segurança
- Verificação de dependências vulneráveis
- Relatórios de segurança

## 🎯 Como Funciona

### Fluxo Automático:

1. **Push/PR** → GitHub Actions é acionado
2. **Build** → Compila o projeto .NET
3. **Testes** → Executa testes e gera cobertura
4. **Análise** → SonarCloud analisa o código
5. **Quality Gate** → Verifica se atende critérios
6. **Relatório** → Gera relatório de qualidade

### Triggers:
- ✅ Push para `main` ou `develop`
- ✅ Push para branches `feature/*`
- ✅ Pull Requests para `main` ou `develop`
- ✅ Agendamento semanal (segunda-feira 2h)

## 📊 Métricas Analisadas

### Qualidade de Código:
- **Bugs**: Erros no código
- **Vulnerabilities**: Problemas de segurança
- **Code Smells**: Problemas de manutenibilidade
- **Duplications**: Código duplicado
- **Coverage**: Cobertura de testes

### Segurança:
- **Security Hotspots**: Pontos de atenção de segurança
- **OWASP Top 10**: Vulnerabilidades conhecidas
- **CWE**: Common Weakness Enumeration
- **Secrets Detection**: Detecção de credenciais expostas

## 🛡️ Quality Gates

### Critérios Padrão:
- **Bugs**: 0 novos bugs
- **Vulnerabilities**: 0 novas vulnerabilidades
- **Security Hotspots**: 0 novos hotspots
- **Coverage**: ≥ 80% de cobertura
- **Duplications**: ≤ 3% de duplicação

### Personalização:
1. No SonarCloud, vá em **Quality Gates**
2. Crie um novo Quality Gate personalizado
3. Configure critérios específicos para seu projeto

## 🔍 Visualização dos Resultados

### No GitHub:
- **Pull Requests**: Comentários automáticos com issues
- **Actions Tab**: Logs detalhados da análise
- **Security Tab**: Vulnerabilidades encontradas

### No SonarCloud:
- **Dashboard**: Visão geral da qualidade
- **Issues**: Lista detalhada de problemas
- **Measures**: Métricas e tendências
- **Security**: Análise de segurança

## 🚨 Alertas e Notificações

### Configurar Notificações:
1. No SonarCloud, vá em **Account** → **Notifications**
2. Configure:
   - **Email**: Para novos issues
   - **Slack**: Integração com Slack
   - **Webhooks**: Para sistemas externos

### Integração com Slack:
```json
{
  "webhook_url": "https://hooks.slack.com/services/...",
  "channels": ["#dev-team"],
  "events": ["new_issues", "quality_gate_failures"]
}
```

## 🔧 Troubleshooting

### Problemas Comuns:

#### 1. Token Inválido
```
Error: Invalid token
```
**Solução**: Verifique se o `SONAR_TOKEN` está correto nos secrets do GitHub

#### 2. Projeto Não Encontrado
```
Error: Project not found
```
**Solução**: Verifique se o `sonar.projectKey` está correto

#### 3. Falha no Quality Gate
```
Quality Gate failed
```
**Solução**: Corrija os issues reportados ou ajuste os critérios

#### 4. Conflito com Análise Automática
```
ERROR: You are running CI analysis while Automatic Analysis is enabled
```
**Solução**: ✅ **CORRIGIDO** - Adicionado `sonar.ci.automaticAnalysis=false`

#### 5. Arquivo sonar-project.properties não reconhecido
```
sonar-project.properties files are not understood by the SonarScanner for .NET
```
**Solução**: ✅ **CORRIGIDO** - Removido arquivo e movidas configurações para argumentos do workflow

### Logs Úteis:
```bash
# Ver logs do GitHub Actions
gh run list --repo seu-usuario/auz
gh run view --repo seu-usuario/auz [run-id]

# Ver status do SonarCloud
curl -u $SONAR_TOKEN: https://sonarcloud.io/api/qualitygates/project_status?projectKey=RafaelRFAndrade_Auz
```

## 📈 Melhorias Contínuas

### 1. Configurar Integração com IDE
- Instale **SonarLint** no Visual Studio
- Configure para usar o SonarCloud
- Receba feedback em tempo real

### 2. Configurar Relatórios
- Configure relatórios automáticos
- Integre com ferramentas de BI
- Configure dashboards executivos

### 3. Configurar Integração com Jira
- Conecte issues do SonarCloud com Jira
- Automatize criação de tickets
- Configure workflows de correção

## 🎉 Resultado Final

Após a configuração, você terá:

- ✅ **Análise automática** a cada commit
- ✅ **Relatórios de qualidade** em tempo real
- ✅ **Detecção de vulnerabilidades** automática
- ✅ **Quality Gates** que impedem código ruim
- ✅ **Integração completa** com GitHub
- ✅ **Monitoramento contínuo** da qualidade
- ✅ **Sem conflitos** entre análise CI e automática

## 📞 Suporte

- **Documentação**: [SonarCloud Docs](https://docs.sonarcloud.io/)
- **Community**: [Sonar Community](https://community.sonarsource.com/)
- **GitHub Issues**: Para problemas específicos
- **Support**: Através do dashboard do SonarCloud

---

**🎯 Agora seu código será analisado automaticamente a cada commit, garantindo alta qualidade e segurança!**
