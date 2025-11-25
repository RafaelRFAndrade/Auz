# Auz - Sistema de Gestão de Atendimentos Médicos

API REST desenvolvida em .NET 8.0 para gestão completa de atendimentos médicos, agendamentos, pacientes e médicos. Sistema desenvolvido especialmente para clínicas pequenas, oferecendo uma solução completa e acessível para o gerenciamento do dia a dia clínico.

## Índice

- [Características](#características)
- [Tecnologias](#tecnologias)
- [Arquitetura](#arquitetura)
- [Pré-requisitos](#pré-requisitos)
- [Configuração](#configuração)
- [Executando o Projeto](#executando-o-projeto)
- [Documentação da API](#documentação-da-api)
- [Estrutura do Projeto](#estrutura-do-projeto)

## Características

- **Autenticação JWT** - Sistema seguro de autenticação e autorização
- **Gestão de Usuários** - Controle de acesso e permissões
- **Gestão de Médicos** - Cadastro e gerenciamento de profissionais
- **Gestão de Pacientes** - Cadastro completo de pacientes
- **Agendamentos** - Sistema de agendamento de consultas
- **Atendimentos** - Registro e acompanhamento de atendimentos
- **Documentos** - Upload e gerenciamento de documentos (AWS S3)
- **Parceiros** - Gestão de parceiros e relacionamentos
- **Logging** - Integração com Loki para logs centralizados
- **Docker** - Containerização pronta para deploy

## Tecnologias

- **.NET 8.0** - Framework principal
- **Entity Framework Core** - ORM para acesso a dados
- **SQL Server** - Banco de dados
- **JWT Bearer** - Autenticação
- **AutoMapper** - Mapeamento de objetos
- **Swagger/OpenAPI** - Documentação da API
- **AWS SDK S3** - Armazenamento de documentos
- **Polly** - Resiliência e retry policies
- **Loki** - Sistema de logging

## Arquitetura

O projeto segue uma arquitetura em camadas (Clean Architecture):

```
┌─────────────────┐
│   Auz (API)     │  ← Controllers, Middleware, Configurações
├─────────────────┤
│  Application    │  ← Services, Interfaces, DTOs
├─────────────────┤
│     Domain      │  ← Entidades, Enums
├─────────────────┤
│     Infra       │  ← Repositories, Helpers
└─────────────────┘
```

## Pré-requisitos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server) ou SQL Server Express
- [Docker](https://www.docker.com/) (opcional, para containerização)
- Conta AWS com S3 configurado (para upload de documentos)

## Configuração

1. **Clone o repositório**
   ```bash
   git clone <url-do-repositorio>
   cd Auz
   ```

2. **Configure as variáveis de ambiente**

   Copie o arquivo `env.example` e configure as variáveis necessárias:

   ```bash
   # Banco de Dados
   CONNECTION_STRING=Server=SEU_SERVIDOR;Database=SEU_DATABASE;User Id=SEU_USUARIO;Password=SUA_SENHA;TrustServerCertificate=True
   
   # JWT
   JWT_KEY=SUA_CHAVE_JWT_DEVE_TER_PELO_MENOS_32_CARACTERES
   
   # AWS S3
   AWS_ACCESS_KEY=SUA_AWS_ACCESS_KEY
   AWS_SECRET_KEY=SUA_AWS_SECRET_KEY
   AWS_REGION=sa-east-1
   AWS_BUCKET=seu-bucket-s3
   ```

3. **Execute o script SQL**

   Execute o arquivo `create_tables.sql` no seu banco de dados SQL Server para criar as tabelas necessárias.

## Executando o Projeto

### Desenvolvimento Local

```bash
# Restaurar dependências
dotnet restore

# Executar a aplicação
cd Auz
dotnet run
```

A API estará disponível em:
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5001`
- **Swagger UI**: `https://localhost:5001/swagger`

### Docker

```bash
# Build da imagem
docker build -t auz-api .

# Executar container
docker run -p 8080:8080 --env-file .env auz-api
```

## Documentação da API

A documentação interativa da API está disponível através do Swagger UI quando a aplicação está em execução:

- **Swagger UI**: `https://localhost:5001/swagger`
- **Swagger JSON**: `https://localhost:5001/swagger/v1/swagger.json`

### Endpoints Principais

- `POST /Usuario/Login` - Autenticação de usuário
- `GET /Medico/Listar` - Listar médicos
- `GET /Paciente/Listar` - Listar pacientes
- `POST /Agendamento/Cadastrar` - Criar agendamento
- `GET /Atendimento/Listar` - Listar atendimentos
- `POST /Documento/Upload` - Upload de documento

> A maioria dos endpoints requer autenticação JWT. Use o token retornado no login no header `Authorization: Bearer {token}`

## Estrutura do Projeto

```
Auz/
├── Auz/                    # Camada de apresentação (API)
│   ├── Controllers/        # Controllers da API
│   ├── Middleware/         # Middlewares customizados
│   └── Program.cs         # Configuração da aplicação
├── Application/            # Camada de aplicação
│   ├── Interfaces/        # Contratos dos serviços
│   ├── Services/          # Implementação dos serviços
│   └── Messaging/         # DTOs (Request/Response)
├── Domain/                 # Camada de domínio
│   ├── Entidades/         # Entidades do domínio
│   └── Enums/             # Enumerações
├── Infra/                  # Camada de infraestrutura
│   ├── Repositories/      # Repositórios de dados
│   └── Helper/            # Helpers utilitários
└── Test/                   # Testes unitários
```

## Segurança

- Autenticação JWT obrigatória para endpoints protegidos
- Validação de permissões por usuário
- CORS configurado para origens específicas
- Upload de arquivos com limite de tamanho (100MB)

## Licença

Este projeto é proprietário e confidencial.

---

**Desenvolvido usando .NET 8.0**

