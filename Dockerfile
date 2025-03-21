FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY *.sln .
COPY Auz/*.csproj ./Auz/
COPY Domain/*.csproj ./Domain/
COPY Application/*.csproj ./Application/
COPY Infra/*.csproj ./Infra/
RUN dotnet restore

COPY . .

RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

EXPOSE 80

# Iniciar a aplicação
ENTRYPOINT ["dotnet", "Auz.dll"]