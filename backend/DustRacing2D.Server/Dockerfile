FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend/DustRacing2D.Game/DustRacing2D.Game.csproj backend/DustRacing2D.Game/
COPY backend/DustRacing2D.Server/DustRacing2D.Server.csproj backend/DustRacing2D.Server/

RUN dotnet restore backend/DustRacing2D.Server/DustRacing2D.Server.csproj

COPY backend/ backend/
COPY shared/ shared/

RUN dotnet publish backend/DustRacing2D.Server/DustRacing2D.Server.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV TRACKS_DIR=/app/shared/tracks

COPY --from=build /app/publish/ ./
COPY --from=build /src/shared/tracks ./shared/tracks

EXPOSE 8080

ENTRYPOINT ["dotnet", "DustRacing2D.Server.dll"]
