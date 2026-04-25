FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8094

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Clothy.Gateway/Clothy.Gateway.csproj", "Clothy.Gateway/"]
COPY ["Clothy.ServiceDefaults/Clothy.ServiceDefaults.csproj", "Clothy.ServiceDefaults/"]
COPY ["Clothy.Shared/Clothy.Shared.csproj", "Clothy.Shared/"]
RUN dotnet restore "Clothy.Gateway/Clothy.Gateway.csproj"
COPY . .
WORKDIR "/src/Clothy.Gateway"
RUN dotnet build "./Clothy.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Clothy.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Clothy.Gateway.dll"]
