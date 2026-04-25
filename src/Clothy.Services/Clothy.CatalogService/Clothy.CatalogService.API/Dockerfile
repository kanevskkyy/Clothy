FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8082
EXPOSE 8083

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Clothy.Services/Clothy.CatalogService/Clothy.CatalogService.API/Clothy.CatalogService.API.csproj", "Clothy.Services/Clothy.CatalogService/Clothy.CatalogService.API/"]
COPY ["Clothy.Aggregator/Clothy.Aggregator.Aggregate/Clothy.Aggregator.Aggregate.csproj", "Clothy.Aggregator/Clothy.Aggregator.Aggregate/"]
COPY ["Clothy.Shared/Clothy.Shared.csproj", "Clothy.Shared/"]
COPY ["Clothy.Services/Clothy.CatalogService/Clothy.CatalogService.BLL/Clothy.CatalogService.BLL.csproj", "Clothy.Services/Clothy.CatalogService/Clothy.CatalogService.BLL/"]
COPY ["Clothy.Services/Clothy.CatalogService/Clothy.CatalogService.DAL/Clothy.CatalogService.DAL.csproj", "Clothy.Services/Clothy.CatalogService/Clothy.CatalogService.DAL/"]
COPY ["Clothy.Services/Clothy.CatalogService/Clothy.CatalogService.Domain/Clothy.CatalogService.Domain.csproj", "Clothy.Services/Clothy.CatalogService/Clothy.CatalogService.Domain/"]
COPY ["Clothy.ServiceDefaults/Clothy.ServiceDefaults.csproj", "Clothy.ServiceDefaults/"]
COPY ["Clothy.Services/Clothy.CatalogService/Clothy.CatalogService.gRPC.Server/Clothy.CatalogService.gRPC.Server.csproj", "Clothy.Services/Clothy.CatalogService/Clothy.CatalogService.gRPC.Server/"]
RUN dotnet restore "Clothy.Services/Clothy.CatalogService/Clothy.CatalogService.API/Clothy.CatalogService.API.csproj"
COPY . .
WORKDIR "/src/Clothy.Services/Clothy.CatalogService/Clothy.CatalogService.API"
RUN dotnet build "./Clothy.CatalogService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Clothy.CatalogService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Clothy.CatalogService.API.dll"]
