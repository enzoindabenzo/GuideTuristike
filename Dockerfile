# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copy solution and project files
COPY *.sln .
COPY GuideTuristike/*.csproj ./GuideTuristike/

# Restore dependencies
RUN dotnet restore

# Copy all source code
COPY . .

# Publish the app in Release mode to /app/out
WORKDIR /app/GuideTuristike
RUN dotnet publish -c Release -o out

# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/GuideTuristike/out ./

# Set the entrypoint to run your app
ENTRYPOINT ["dotnet", "GuideTuristike.dll"]
