# ---- Estágio 1: build (compila o código) ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia só os .csproj primeiro — arquivo que lista as dependências, não o código em si
COPY src/TechDebtHub.Api/TechDebtHub.Api.csproj src/TechDebtHub.Api/
COPY src/TechDebtHub.Application/TechDebtHub.Application.csproj src/TechDebtHub.Application/
COPY src/TechDebtHub.Domain/TechDebtHub.Domain.csproj src/TechDebtHub.Domain/
COPY src/TechDebtHub.Infrastructure/TechDebtHub.Infrastructure.csproj src/TechDebtHub.Infrastructure/

# Baixa os pacotes NuGet com base só nos .csproj
RUN dotnet restore src/TechDebtHub.Api/TechDebtHub.Api.csproj

# Agora sim copia todo o código-fonte
COPY src/ src/

# Compila e publica em modo Release (otimizado, sem símbolos de debug)
RUN dotnet publish src/TechDebtHub.Api/TechDebtHub.Api.csproj -c Release -o /app/publish --no-restore

# ---- Estágio 2: runtime (só roda o que já foi compilado) ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Traz só o resultado publicado do estágio anterior — nada de SDK, código-fonte ou cache do NuGet
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "TechDebtHub.Api.dll"]
