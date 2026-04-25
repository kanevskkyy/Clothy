FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8092

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Clothy.Services/Clothy.PaymentService/Clothy.PaymentService.API/Clothy.PaymentService.API.csproj", "Clothy.Services/Clothy.PaymentService/Clothy.PaymentService.API/"]
COPY ["Clothy.ServiceDefaults/Clothy.ServiceDefaults.csproj", "Clothy.ServiceDefaults/"]
COPY ["Clothy.Shared/Clothy.Shared.csproj", "Clothy.Shared/"]
COPY ["Clothy.Services/Clothy.PaymentService/Clothy.PaymentService.BLL/Clothy.PaymentService.BLL.csproj", "Clothy.Services/Clothy.PaymentService/Clothy.PaymentService.BLL/"]
COPY ["Clothy.Services/Clothy.PaymentService/Clothy.PaymentService.DAL/Clothy.PaymentService.DAL.csproj", "Clothy.Services/Clothy.PaymentService/Clothy.PaymentService.DAL/"]
COPY ["Clothy.Services/Clothy.PaymentService/Clothy.PaymentService.Domain/Clothy.PaymentService.Domain.csproj", "Clothy.Services/Clothy.PaymentService/Clothy.PaymentService.Domain/"]
COPY ["Clothy.Services/Clothy.PaymentService/Clothy.PaymentService.gRPC.Client/Clothy.PaymentService.gRPC.Client.csproj", "Clothy.Services/Clothy.PaymentService/Clothy.PaymentService.gRPC.Client/"]
RUN dotnet restore "Clothy.Services/Clothy.PaymentService/Clothy.PaymentService.API/Clothy.PaymentService.API.csproj"
COPY . .
WORKDIR "/src/Clothy.Services/Clothy.PaymentService/Clothy.PaymentService.API"
RUN dotnet build "./Clothy.PaymentService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Clothy.PaymentService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Clothy.PaymentService.API.dll"]
