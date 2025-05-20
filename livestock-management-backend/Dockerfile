# Stage 1: Build the solution
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the solution file and project files
COPY *.sln .  
COPY LivestockManagementSystemAPI/LivestockManagementSystemAPI.csproj LivestockManagementSystemAPI/
COPY DataAccess/DataAccess.csproj DataAccess/
COPY BusinessObjects/BusinessObjects.csproj BusinessObjects/

# Copy everything to ensure dependencies exist before restore
COPY . .

# Restore dependencies
RUN dotnet restore LivestockManagementSystemAPI/LivestockManagementSystemAPI.csproj

# Build and publish the API project
WORKDIR /src/LivestockManagementSystemAPI
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose correct port
EXPOSE 5000

# Run the Web API
ENTRYPOINT ["dotnet", "LivestockManagementSystemAPI.dll"]
