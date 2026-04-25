FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8088
EXPOSE 8089

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Clothy.Services/Clothy.ReviewService/Clothy.ReviewService.API/Clothy.ReviewService.API.csproj", "Clothy.Services/Clothy.ReviewService/Clothy.ReviewService.API/"]
COPY ["Clothy.ServiceDefaults/Clothy.ServiceDefaults.csproj", "Clothy.ServiceDefaults/"]
COPY ["Clothy.Shared/Clothy.Shared.csproj", "Clothy.Shared/"]
COPY ["Clothy.Services/Clothy.ReviewService/Clothy.ReviewService.Application/Clothy.ReviewService.Application.csproj", "Clothy.Services/Clothy.ReviewService/Clothy.ReviewService.Application/"]
COPY ["Clothy.Services/Clothy.ReviewService/Clothy.ReviewService.Domain/Clothy.ReviewService.Domain.csproj", "Clothy.Services/Clothy.ReviewService/Clothy.ReviewService.Domain/"]
COPY ["Clothy.Services/Clothy.ReviewService/Clothy.ReviewService.gRPC.Client/Clothy.ReviewService.gRPC.Client.csproj", "Clothy.Services/Clothy.ReviewService/Clothy.ReviewService.gRPC.Client/"]
COPY ["Clothy.Services/Clothy.ReviewService/Clothy.ReviewService.gRPC.Server/Clothy.ReviewService.gRPC.Server.csproj", "Clothy.Services/Clothy.ReviewService/Clothy.ReviewService.gRPC.Server/"]
COPY ["Clothy.Services/Clothy.ReviewService/Clothy.ReviewService.Infrastructure/Clothy.ReviewService.Infrastructure.csproj", "Clothy.Services/Clothy.ReviewService/Clothy.ReviewService.Infrastructure/"]
RUN dotnet restore "Clothy.Services/Clothy.ReviewService/Clothy.ReviewService.API/Clothy.ReviewService.API.csproj"
COPY . .
WORKDIR "/src/Clothy.Services/Clothy.ReviewService/Clothy.ReviewService.API"
RUN dotnet build "./Clothy.ReviewService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Clothy.ReviewService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Clothy.ReviewService.API.dll"]
