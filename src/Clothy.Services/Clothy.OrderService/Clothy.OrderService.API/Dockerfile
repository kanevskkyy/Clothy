FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8086
EXPOSE 8087

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Clothy.Services/Clothy.OrderService/Clothy.OrderService.API/Clothy.OrderService.API.csproj", "Clothy.Services/Clothy.OrderService/Clothy.OrderService.API/"]
COPY ["Clothy.ServiceDefaults/Clothy.ServiceDefaults.csproj", "Clothy.ServiceDefaults/"]
COPY ["Clothy.Shared/Clothy.Shared.csproj", "Clothy.Shared/"]
COPY ["Clothy.Services/Clothy.OrderService/Clothy.OrderService.BLL/Clothy.OrderService.BLL.csproj", "Clothy.Services/Clothy.OrderService/Clothy.OrderService.BLL/"]
COPY ["Clothy.Services/Clothy.BasketService/Clothy.BasketService.gRPC.Server/Clothy.BasketService.gRPC.Server.csproj", "Clothy.Services/Clothy.BasketService/Clothy.BasketService.gRPC.Server/"]
COPY ["Clothy.Services/Clothy.BasketService/Clothy.BasketService.BLL/Clothy.BasketService.BLL.csproj", "Clothy.Services/Clothy.BasketService/Clothy.BasketService.BLL/"]
COPY ["Clothy.Services/Clothy.OrderService/Clothy.OrderService.gRPC.Client/Clothy.OrderService.gRPC.Client.csproj", "Clothy.Services/Clothy.OrderService/Clothy.OrderService.gRPC.Client/"]
COPY ["Clothy.Services/Clothy.BasketService/Clothy.BasketService.DAL/Clothy.BasketService.DAL.csproj", "Clothy.Services/Clothy.BasketService/Clothy.BasketService.DAL/"]
COPY ["Clothy.Services/Clothy.BasketService/Clothy.BaskteService.Domain/Clothy.BaskteService.Domain.csproj", "Clothy.Services/Clothy.BasketService/Clothy.BaskteService.Domain/"]
COPY ["Clothy.Services/Clothy.BasketService/Clothy.BasketService.gRPC.Client/Clothy.BasketService.gRPC.Client.csproj", "Clothy.Services/Clothy.BasketService/Clothy.BasketService.gRPC.Client/"]
COPY ["Clothy.Services/Clothy.OrderService/Clothy.OrderService.DAL/Clothy.OrderService.DAL.csproj", "Clothy.Services/Clothy.OrderService/Clothy.OrderService.DAL/"]
COPY ["Clothy.Services/Clothy.OrderService/Clothy.OrderService.Domain/Clothy.OrderService.Domain.csproj", "Clothy.Services/Clothy.OrderService/Clothy.OrderService.Domain/"]
COPY ["Clothy.Services/Clothy.OrderService/Clothy.OrderService.gRPC.Server/Clothy.OrderService.gRPC.Server.csproj", "Clothy.Services/Clothy.OrderService/Clothy.OrderService.gRPC.Server/"]
RUN dotnet restore "Clothy.Services/Clothy.OrderService/Clothy.OrderService.API/Clothy.OrderService.API.csproj"
COPY . .
WORKDIR "/src/Clothy.Services/Clothy.OrderService/Clothy.OrderService.API"
RUN dotnet build "./Clothy.OrderService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Clothy.OrderService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Clothy.OrderService.API.dll"]
