# 設定基礎映像檔
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# 使用 SDK 映像檔進行建置
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 複製專案檔並還原 NuGet 套件
COPY ["NailArtHub.csproj", "./"]
RUN dotnet restore "NailArtHub.csproj"

# 複製所有原始碼並進行編譯
COPY . .
RUN dotnet build "NailArtHub.csproj" -c Release -o /app/build

# 發佈專案
FROM build AS publish
RUN dotnet publish "NailArtHub.csproj" -c Release -o /app/publish

# 最後執行映像檔
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NailArtHub.dll"]