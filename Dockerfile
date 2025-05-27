# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /app

# Copy solution file and project files
COPY *.sln .
COPY GuideTuristike/*.csproj ./GuideTuristike/

# Restore dependencies
RUN dotnet restore

# Copy everything else and build
COPY . .

RUN dotnet publish -c Release -o out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime

WORKDIR /app

COPY --from=build /app/out ./

ENTRYPOINT ["dotnet", "GuideTuristike.dll"]
