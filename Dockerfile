# Pheracies, 7/23/26
# Dockerfile to compile and run the C# backend on AWS App Runner
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /App

# Copy the entire workspace
COPY . ./

# Restore dependencies
RUN dotnet restore src/AnalyticEngine.Api/AnalyticEngine.Api.csproj

# Compile and publish release binaries
RUN dotnet publish src/AnalyticEngine.Api/AnalyticEngine.Api.csproj -c Release -o out

# Build the slim runtime container
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /App
COPY --from=build-env /App/out .

# Bind the server to port 5000
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "AnalyticEngine.Api.dll"]
