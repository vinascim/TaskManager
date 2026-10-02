FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/TaskManager.Domain/TaskManager.Domain.csproj src/TaskManager.Domain/
COPY src/TaskManager.Application/TaskManager.Application.csproj src/TaskManager.Application/
COPY src/TaskManager.Infrastructure/TaskManager.Infrastructure.csproj src/TaskManager.Infrastructure/
COPY src/TaskManager.Api/TaskManager.Api.csproj src/TaskManager.Api/
RUN dotnet restore src/TaskManager.Api/TaskManager.Api.csproj

COPY src/ src/
RUN dotnet publish src/TaskManager.Api/TaskManager.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS final
WORKDIR /app

COPY --from=build /app/publish .

USER $APP_UID
EXPOSE 8080

ENTRYPOINT ["dotnet", "TaskManager.Api.dll"]
