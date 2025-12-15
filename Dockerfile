# 1 этап - сборка
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем файлы решения и проектов для восстановления зависимостей
COPY ["Neur.Server.Net.sln", "./"]
COPY ["Neur.Server.Net.Core/Neur.Server.Net.Core.csproj", "Neur.Server.Net.Core/"]
COPY ["Neur.Server.Net.Postgres/Neur.Server.Net.Postgres.csproj", "Neur.Server.Net.Postgres/"]
COPY ["Neur.Server.Net.Infrastructure/Neur.Server.Net.Infrastructure.csproj", "Neur.Server.Net.Infrastructure/"]
COPY ["Neur.Server.Net.Application/Neur.Server.Net.Application.csproj", "Neur.Server.Net.Application/"]
COPY ["Neur.Server.Net.API/Neur.Server.Net.API.csproj", "Neur.Server.Net.API/"]

# Восстанавливаем зависимости
RUN dotnet restore "Neur.Server.Net.API/Neur.Server.Net.API.csproj"
# Копируем весь исходный код
COPY . .

# Публикуем проект
WORKDIR "/src/Neur.Server.Net.API"
RUN dotnet publish -c Release -o /app/publish

# 2 Этап - запуск
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Neur.Server.Net.API.dll"]
