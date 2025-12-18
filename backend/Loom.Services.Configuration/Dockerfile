FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 50051

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/Loom.Services.Configuration.Api/Loom.Services.Configuration.Api.csproj", "src/Loom.Services.Configuration.Api/"]
COPY ["src/Loom.Services.Configuration.Core/Loom.Services.Configuration.Core.csproj", "src/Loom.Services.Configuration.Core/"]
COPY ["src/Loom.Services.Configuration.Domain/Loom.Services.Configuration.Domain.csproj", "src/Loom.Services.Configuration.Domain/"]
COPY ["src/Loom.Services.Configuration.Contracts/Loom.Services.Configuration.Contracts.csproj", "src/Loom.Services.Configuration.Contracts/"]

RUN dotnet restore "src/Loom.Services.Configuration.Api/Loom.Services.Configuration.Api.csproj"

COPY . .
WORKDIR "/src/src/Loom.Services.Configuration.Api"
RUN dotnet build "Loom.Services.Configuration.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Loom.Services.Configuration.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Loom.Services.Configuration.Api.dll"]

