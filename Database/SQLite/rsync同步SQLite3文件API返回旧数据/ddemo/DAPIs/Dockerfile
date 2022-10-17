FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /source

COPY . .
RUN dotnet restore DAPIs/DAPIs.csproj -s https://nuget.cdn.azure.cn/v3/index.json
RUN dotnet publish DAPIs/DAPIs.csproj -c release -o /app --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "DAPIs.dll"]
