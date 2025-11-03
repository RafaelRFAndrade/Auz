FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Instalar ferramenta dotnet-depends para verificar dependências
RUN dotnet tool install -g dotnet-depends

WORKDIR /app

# Criar um projeto temporário para extrair todas as DLLs relacionadas ao JWT
RUN dotnet new console -o /temp-jwt
WORKDIR /temp-jwt
RUN dotnet add package System.IdentityModel.Tokens.Jwt --version 7.1.2
RUN dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.10
RUN dotnet add package Microsoft.IdentityModel.Protocols --version 7.1.2
RUN dotnet add package Microsoft.IdentityModel.Protocols.OpenIdConnect --version 7.1.2
RUN dotnet add package Microsoft.IdentityModel.JsonWebTokens --version 7.1.2
RUN dotnet add package Microsoft.IdentityModel.Tokens --version 7.1.2
RUN dotnet add package Microsoft.IdentityModel.Logging --version 7.1.2
RUN dotnet restore
RUN dotnet build

# Extrair todas as DLLs relacionadas ao Identity para uma pasta
RUN mkdir -p /jwt-libs
RUN find /root/.nuget/packages/ -path "*/7.1.2/lib/net8.0/*.dll" -exec cp {} /jwt-libs/ \;
RUN find /root/.nuget/packages/microsoft.aspnetcore.authentication.jwtbearer -path "*/8.0.10/lib/net8.0/*.dll" -exec cp {} /jwt-libs/ \;
RUN ls -la /jwt-libs

WORKDIR /app

# Copiar arquivos de projeto
COPY *.sln .
COPY Auz/*.csproj ./Auz/
COPY Domain/*.csproj ./Domain/
COPY Application/*.csproj ./Application/
COPY Infra/*.csproj ./Infra/
COPY Test/*.csproj ./Test/

# Restaurar pacotes
RUN dotnet restore

# Copiar todo o código fonte
COPY . .

# Compilar e publicar com todas as dependências
RUN dotnet publish -c Release -o out /p:CopyLocalLockFileAssemblies=true

# Verificar as dependências geradas
RUN dotnet-depends out/Web.dll | grep -i identity || echo "Nenhuma dependência Identity encontrada"

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copiar binários publicados
COPY --from=build /app/out ./

# Copiar todas as DLLs JWT extraídas
COPY --from=build /jwt-libs/*.dll ./

# Listar todos os arquivos .dll no diretório da aplicação
RUN ls -la *.dll

EXPOSE 8080

# Iniciar aplicação
ENTRYPOINT ["dotnet", "Web.dll"] 