# Basis-Image für das Laufzeitumfeld
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Installiere Ghostscript, Tesseract und Leptonica
USER root
RUN apt-get update && apt-get install -y \
    ghostscript \
    tesseract-ocr \
    libleptonica-dev \
    && apt-get clean

# Setze zurück auf einen nicht-root Benutzer (empfohlen für Sicherheit)
USER app

# Build-Stage für den OCR-Worker
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Kopiere die Projektdatei und führe `restore` aus
COPY ["Paperless_OCR.csproj", "./"]
RUN dotnet restore "./Paperless_OCR.csproj"

# Kopiere den gesamten Code in das Image und baue das Projekt
COPY . .
RUN dotnet build "./Paperless_OCR.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish-Stage: Veröffentliche die Anwendung
FROM build AS publish
RUN dotnet publish "./Paperless_OCR.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final-Stage: Laufzeit-Image für die Anwendung
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Definiere den Startbefehl
ENTRYPOINT ["dotnet", "Paperless_OCR.dll"]