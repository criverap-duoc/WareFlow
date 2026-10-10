# Multi-stage build — WareFlow.API (.NET 10)

# ---------- Etapa 1: build / publish ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar solución y csproj primero para aprovechar el cache de restore
COPY *.slnx .
COPY WareFlow.API/*.csproj WareFlow.API/
COPY WareFlow.Core/*.csproj WareFlow.Core/
COPY WareFlow.Infrastructure/*.csproj WareFlow.Infrastructure/
RUN dotnet restore WareFlow.API/WareFlow.API.csproj

# Copiar el resto del código y publicar en Release
COPY . .
RUN dotnet publish WareFlow.API/WareFlow.API.csproj -c Release -o /app/publish

# ---------- Etapa 2: runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# wget para el healthcheck de docker-compose (la imagen aspnet no incluye wget/curl)
RUN apt-get update \
    && apt-get install -y --no-install-recommends wget \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "WareFlow.API.dll"]
