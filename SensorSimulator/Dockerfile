FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Shared/Shared.csproj Shared/
COPY SensorSimulator/SensorSimulator.csproj SensorSimulator/
RUN dotnet restore SensorSimulator/SensorSimulator.csproj

COPY Shared/ Shared/
COPY SensorSimulator/ SensorSimulator/
COPY keys/private/ keys/private/
COPY keys/public/server-public.pem keys/public/
RUN dotnet publish SensorSimulator/SensorSimulator.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "SensorSimulator.dll"]