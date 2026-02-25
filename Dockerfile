# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

COPY RealTimeMonitor/RealTimeMonitor.csproj RealTimeMonitor/
RUN dotnet restore RealTimeMonitor/RealTimeMonitor.csproj

COPY RealTimeMonitor/ RealTimeMonitor/
RUN dotnet publish RealTimeMonitor/RealTimeMonitor.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "RealTimeMonitor.dll"]
