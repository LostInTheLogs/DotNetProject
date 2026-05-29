# Base image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore
COPY ["ClinicManager/ClinicManager.csproj", "ClinicManager/"]
RUN dotnet restore "ClinicManager/ClinicManager.csproj" -p:AllowMissingPrunePackageData=true

# Copy everything else and build
COPY . .
WORKDIR "/src/ClinicManager"
RUN dotnet build "ClinicManager.csproj" -c Release -o /app/build -p:AllowMissingPrunePackageData=true

# Publish the application
FROM build AS publish
RUN dotnet publish "ClinicManager.csproj" -c Release -o /app/publish /p:UseAppHost=false -p:AllowMissingPrunePackageData=true

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClinicManager.dll"]
