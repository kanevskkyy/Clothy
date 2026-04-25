FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8091

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Clothy.Services/Clothy.NotificationService/Clothy.NotificationService.API/Clothy.NotificationService.API.csproj", "Clothy.Services/Clothy.NotificationService/Clothy.NotificationService.API/"]
COPY ["Clothy.ServiceDefaults/Clothy.ServiceDefaults.csproj", "Clothy.ServiceDefaults/"]
COPY ["Clothy.Shared/Clothy.Shared.csproj", "Clothy.Shared/"]
COPY ["Clothy.Services/Clothy.NotificationService/Clothy.NotificationService.BLL/Clothy.NotificationService.BLL.csproj", "Clothy.Services/Clothy.NotificationService/Clothy.NotificationService.BLL/"]
RUN dotnet restore "Clothy.Services/Clothy.NotificationService/Clothy.NotificationService.API/Clothy.NotificationService.API.csproj"
COPY . .
WORKDIR "/src/Clothy.Services/Clothy.NotificationService/Clothy.NotificationService.API"
RUN dotnet build "./Clothy.NotificationService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Clothy.NotificationService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Clothy.NotificationService.API.dll"]
