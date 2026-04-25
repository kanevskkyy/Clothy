FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Clothy.Services/Clothy.OrderService/Clothy.OrderService.SeedData/Clothy.OrderService.SeedData.csproj", "Clothy.Services/Clothy.OrderService/Clothy.OrderService.SeedData/"]
COPY ["Clothy.Services/Clothy.OrderService/Clothy.OrderService.BLL/Clothy.OrderService.BLL.csproj", "Clothy.Services/Clothy.OrderService/Clothy.OrderService.BLL/"]
COPY ["Clothy.Services/Clothy.OrderService/Clothy.OrderService.DAL/Clothy.OrderService.DAL.csproj", "Clothy.Services/Clothy.OrderService/Clothy.OrderService.DAL/"]
COPY ["Clothy.Services/Clothy.OrderService/Clothy.OrderService.Domain/Clothy.OrderService.Domain.csproj", "Clothy.Services/Clothy.OrderService/Clothy.OrderService.Domain/"]
RUN dotnet restore "Clothy.Services/Clothy.OrderService/Clothy.OrderService.SeedData/Clothy.OrderService.SeedData.csproj"
COPY . .
WORKDIR "/src/Clothy.Services/Clothy.OrderService/Clothy.OrderService.SeedData"
RUN dotnet publish "./Clothy.OrderService.SeedData.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Clothy.OrderService.SeedData.dll"]