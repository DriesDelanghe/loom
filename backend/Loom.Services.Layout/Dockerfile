FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/Loom.Services.Layout.Api/Loom.Services.Layout.Api.csproj src/Loom.Services.Layout.Api/
COPY src/Loom.Services.Layout.Core/Loom.Services.Layout.Core.csproj src/Loom.Services.Layout.Core/
COPY src/Loom.Services.Layout.Domain/Loom.Services.Layout.Domain.csproj src/Loom.Services.Layout.Domain/

RUN dotnet restore "src/Loom.Services.Layout.Api/Loom.Services.Layout.Api.csproj"

COPY . .
WORKDIR /src/src/Loom.Services.Layout.Api
RUN dotnet build "Loom.Services.Layout.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Loom.Services.Layout.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Loom.Services.Layout.Api.dll"]

