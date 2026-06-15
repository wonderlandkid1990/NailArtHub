FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["NailArtHub.csproj", "./"]
RUN dotnet restore "NailArtHub.csproj"

COPY . .
RUN dotnet build "NailArtHub.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NailArtHub.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NailArtHub.dll"]