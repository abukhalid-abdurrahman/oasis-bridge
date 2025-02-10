# Используем официальный образ .NET SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Копируем и восстанавливаем зависимости
COPY . ./
RUN dotnet restore

# Компилируем и публикуем приложение
RUN dotnet publish -c Release -o /out

# Используем официальный образ ASP.NET Core Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /out ./

# Настраиваем порты для ASP.NET Core
ENV ASPNETCORE_URLS=http://+:80

# Запускаем приложение
ENTRYPOINT ["dotnet", "WebAPI.dll"]
