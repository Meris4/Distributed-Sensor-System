FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Shared/Shared.csproj Shared/
COPY ConsensusService/ConsensusService.csproj ConsensusService/
RUN dotnet restore ConsensusService/ConsensusService.csproj

COPY Shared/ Shared/
COPY ConsensusService/ ConsensusService/
RUN dotnet publish ConsensusService/ConsensusService.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "ConsensusService.dll"]