FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app
COPY / ./

RUN dotnet restore &&\
    dotnet publish /app/src/api/API/API.csproj -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update &&\
    apt-get install -y curl &&\
    rm -rf /var/lib/apt/lists/*

COPY --from=build /out ./

ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "API.dll"]
