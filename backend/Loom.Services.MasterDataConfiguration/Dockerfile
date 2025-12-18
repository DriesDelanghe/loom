FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/Loom.Services.MasterDataConfiguration.Api/Loom.Services.MasterDataConfiguration.Api.csproj src/Loom.Services.MasterDataConfiguration.Api/
COPY src/Loom.Services.MasterDataConfiguration.Core/Loom.Services.MasterDataConfiguration.Core.csproj src/Loom.Services.MasterDataConfiguration.Core/
COPY src/Loom.Services.MasterDataConfiguration.Domain/Loom.Services.MasterDataConfiguration.Domain.csproj src/Loom.Services.MasterDataConfiguration.Domain/

RUN dotnet restore "src/Loom.Services.MasterDataConfiguration.Api/Loom.Services.MasterDataConfiguration.Api.csproj"

COPY . .
WORKDIR /src/src/Loom.Services.MasterDataConfiguration.Api
RUN dotnet build "Loom.Services.MasterDataConfiguration.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Loom.Services.MasterDataConfiguration.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Loom.Services.MasterDataConfiguration.Api.dll"]

