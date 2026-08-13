# ── Etapa 1: Build ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias (aprovecha cache de capas)
COPY ["AppDrugsV2.Api/AppDrugsV2.Api.csproj",             "AppDrugsV2.Api/"]
COPY ["AppDrugsV2.Application/AppDrugsV2.Application.csproj", "AppDrugsV2.Application/"]
COPY ["AppDrugsV2.Domain/AppDrugsV2.Domain.csproj",       "AppDrugsV2.Domain/"]
COPY ["AppDrugsV2.Infrastructure/AppDrugsV2.Infrastructure.csproj", "AppDrugsV2.Infrastructure/"]

RUN dotnet restore "AppDrugsV2.Api/AppDrugsV2.Api.csproj"

# Copiar el resto del código
COPY . .

# Publicar en modo Release
WORKDIR "/src/AppDrugsV2.Api"
RUN dotnet publish "AppDrugsV2.Api.csproj" -c Release -o /app/publish --no-restore

# ── Etapa 2: Runtime ────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Crear directorio de logs
RUN mkdir -p /app/logs

# Copiar los binarios publicados
COPY --from=build /app/publish .

# Variables de entorno requeridas (sobreescribir al desplegar)
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "AppDrugsV2.Api.dll"]
