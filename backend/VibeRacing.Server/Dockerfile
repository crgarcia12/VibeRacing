FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend/VibeRacing.Game/VibeRacing.Game.csproj backend/VibeRacing.Game/
COPY backend/VibeRacing.Server/VibeRacing.Server.csproj backend/VibeRacing.Server/

RUN dotnet restore backend/VibeRacing.Server/VibeRacing.Server.csproj

COPY backend/ backend/
COPY shared/ shared/

RUN dotnet publish backend/VibeRacing.Server/VibeRacing.Server.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV TRACKS_DIR=/app/shared/tracks

COPY --from=build /app/publish/ ./
COPY --from=build /src/shared/tracks ./shared/tracks

EXPOSE 8080

ENTRYPOINT ["dotnet", "VibeRacing.Server.dll"]
