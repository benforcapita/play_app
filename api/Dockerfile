# Use the official .NET 9.0 SDK image as the base image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set the working directory
WORKDIR /src

# Copy the project file and restore dependencies
COPY play_app_api/play_app_api.csproj ./play_app_api/
RUN dotnet restore play_app_api/play_app_api.csproj

# Copy the rest of the source code
COPY play_app_api/ ./play_app_api/

# Build the application
WORKDIR /src/play_app_api
RUN dotnet build play_app_api.csproj -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish play_app_api.csproj -c Release -o /app/publish

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy the published application
COPY --from=publish /app/publish .

# Expose port 80
EXPOSE 80

# Set environment variable to listen on port 80
ENV ASPNETCORE_HTTP_PORTS=80

# Set the entry point
ENTRYPOINT ["dotnet", "play_app_api.dll"] 