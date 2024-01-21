FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SuperGigaToad/SuperGigaToad.csproj", "SuperGigaToad/"]
RUN dotnet restore "SuperGigaToad/SuperGigaToad.csproj"
COPY . .
WORKDIR "/src/SuperGigaToad"
RUN dotnet build "SuperGigaToad.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SuperGigaToad.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SuperGigaToad.dll"]
