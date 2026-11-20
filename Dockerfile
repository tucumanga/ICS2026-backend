# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# 1. Cachear dependencias NuGet copiando primero los archivos .csproj y .sln
COPY *.sln .
COPY src/Dsw2025Tpi.Api/*.csproj src/Dsw2025Tpi.Api/
COPY src/Dsw2025Tpi.Application/*.csproj src/Dsw2025Tpi.Application/
COPY src/Dsw2025Tpi.Data/*.csproj src/Dsw2025Tpi.Data/
COPY src/Dsw2025Tpi.Domain/*.csproj src/Dsw2025Tpi.Domain/
COPY tests/ECommerce.UnitTests/*.csproj tests/ECommerce.UnitTests/
COPY tests/ECommerce.IntegrationTests/*.csproj tests/ECommerce.IntegrationTests/

RUN dotnet restore

# 2. Copiar el resto del código y compilar la API
COPY . .
RUN dotnet publish src/Dsw2025Tpi.Api/Dsw2025Tpi.Api.csproj -c Release --no-restore -o /app/publish

# 3. Imagen de ejecución final optimizada (.NET 8 runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Crear directorio de logs con permisos para el usuario 'app'
RUN mkdir -p /app/Logs && chown -R app:app /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 8080

COPY --from=build --chown=app:app /app/publish .

USER app
ENTRYPOINT ["dotnet", "Dsw2025Tpi.Api.dll"]
