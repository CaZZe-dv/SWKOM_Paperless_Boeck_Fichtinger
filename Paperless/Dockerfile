FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Paperless.csproj", "./"]
RUN dotnet restore "./Paperless.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "Paperless.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Paperless.csproj" -c Release -o /app/publish

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Paperless.dll"]
