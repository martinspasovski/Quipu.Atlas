FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Quipu.Atlas.Api/Quipu.Atlas.Api.csproj", "Quipu.Atlas.Api/"]
RUN dotnet restore "Quipu.Atlas.Api/Quipu.Atlas.Api.csproj"
COPY . .
WORKDIR "/src/Quipu.Atlas.Api"
RUN dotnet build "./Quipu.Atlas.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Quipu.Atlas.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Quipu.Atlas.Api.dll"]
