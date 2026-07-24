# Pheracies, 7/23/26
# Dockerfile built from within the src/AnalyticEngine.Api directory
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /App

# Copy the API project files
COPY . ./

# Restore dependencies
RUN dotnet restore AnalyticEngine.Api.csproj

# Compile and publish release binaries
RUN dotnet publish AnalyticEngine.Api.csproj -c Release -o out

# Build the slim runtime container
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /App
COPY --from=build-env /App/out .

# Bind the server to port 5000
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "AnalyticEngine.Api.dll"]
