# Base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["Paperless.csproj", "./"]
RUN dotnet restore "./Paperless.csproj"

# Copy the entire project and build it
COPY . .
RUN dotnet build "Paperless.csproj" -c Release -o /app/build

# Install the EF CLI tool to apply migrations (Ensure PATH is set)
# ENV PATH="$PATH:/root/.dotnet/tools"
# RUN dotnet tool install --global dotnet-ef

# Run migrations during the build stage (optional for production)
# Uncomment the line below if you want migrations run at this stage
# RUN dotnet ef database update --project Paperless.csproj --startup-project Paperless.csproj

# Publish stage
FROM build AS publish
RUN dotnet publish "Paperless.csproj" -c Release -o /app/publish

# Hier wird das log4net.config in den Container kopiert
COPY log4net.config /app/log4net.config

# Erstelle das Logs-Verzeichnis, falls es nicht existiert
RUN mkdir -p /app/Logs


# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set the PATH for the EF tool in case it's needed during runtime
ENV PATH="$PATH:/root/.dotnet/tools"

# Entry point for the application
ENTRYPOINT ["dotnet", "Paperless.dll"]
