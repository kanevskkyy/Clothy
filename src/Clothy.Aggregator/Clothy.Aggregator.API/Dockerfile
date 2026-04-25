FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8093

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Clothy.Aggregator/Clothy.Aggregator.API/Clothy.Aggregator.API.csproj", "Clothy.Aggregator/Clothy.Aggregator.API/"]
COPY ["Clothy.ServiceDefaults/Clothy.ServiceDefaults.csproj", "Clothy.ServiceDefaults/"]
COPY ["Clothy.Shared/Clothy.Shared.csproj", "Clothy.Shared/"]
COPY ["Clothy.Aggregator/Clothy.Aggregator.Aggregate/Clothy.Aggregator.Aggregate.csproj", "Clothy.Aggregator/Clothy.Aggregator.Aggregate/"]
RUN dotnet restore "Clothy.Aggregator/Clothy.Aggregator.API/Clothy.Aggregator.API.csproj"
COPY . .
WORKDIR "/src/Clothy.Aggregator/Clothy.Aggregator.API"
RUN dotnet build "./Clothy.Aggregator.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Clothy.Aggregator.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Clothy.Aggregator.API.dll"]
