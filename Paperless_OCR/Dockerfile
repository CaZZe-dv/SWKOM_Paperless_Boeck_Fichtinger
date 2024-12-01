# Verwende das .NET SDK als Basis-Image
FROM mcr.microsoft.com/dotnet/sdk:8.0-nanoserver-1809 AS build



# Setze das Arbeitsverzeichnis
WORKDIR /src

# Kopiere die .csproj-Datei von Paperless_OCR ins Container-Verzeichnis
COPY Paperless_OCR.csproj ./

# Stelle sicher, dass alle Abhängigkeiten installiert werden
RUN dotnet restore Paperless_OCR.csproj

# Kopiere den gesamten Code in das Image
COPY . .

# Baue das Projekt
RUN dotnet build "Paperless_OCR.csproj" -c Release -o /app/build

# Definiere das Laufzeit-Image
FROM mcr.microsoft.com/dotnet/runtime:8.0-nanoserver-1809

WORKDIR /app

# Kopiere die Anwendung von Build-Stage
COPY --from=build /app/build .

# Definiere den Startbefehl
ENTRYPOINT ["dotnet", "Paperless_OCR.dll"]