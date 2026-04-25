FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Clothy.Services/Clothy.CatalogService/Clothy.CatalogService.SeedData/Clothy.CatalogService.SeedData.csproj", "Clothy.Services/Clothy.CatalogService/Clothy.CatalogService.SeedData/"]
COPY ["Clothy.Services/Clothy.CatalogService/Clothy.CatalogService.DAL/Clothy.CatalogService.DAL.csproj", "Clothy.Services/Clothy.CatalogService/Clothy.CatalogService.DAL/"]
COPY ["Clothy.Services/Clothy.CatalogService/Clothy.CatalogService.Domain/Clothy.CatalogService.Domain.csproj", "Clothy.Services/Clothy.CatalogService/Clothy.CatalogService.Domain/"]
RUN dotnet restore "Clothy.Services/Clothy.CatalogService/Clothy.CatalogService.SeedData/Clothy.CatalogService.SeedData.csproj"
COPY . .
WORKDIR "/src/Clothy.Services/Clothy.CatalogService/Clothy.CatalogService.SeedData"
RUN dotnet publish "./Clothy.CatalogService.SeedData.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Clothy.CatalogService.SeedData.dll"]