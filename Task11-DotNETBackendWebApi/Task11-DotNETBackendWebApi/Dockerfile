# 1. Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Task11-DotNETBackendWebApi.csproj", "./"]
RUN dotnet restore "Task11-DotNETBackendWebApi.csproj"

COPY . .
RUN dotnet publish "Task11-DotNETBackendWebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Task11-DotNETBackendWebApi.dll"]
