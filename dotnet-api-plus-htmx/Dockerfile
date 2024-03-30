FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["dotnet-api-plus-htmx/dotnet-api-plus-htmx.csproj", "dotnet-api-plus-htmx/"]
RUN dotnet restore "dotnet-api-plus-htmx/dotnet-api-plus-htmx.csproj"
COPY . .
WORKDIR "/src/dotnet-api-plus-htmx"
RUN dotnet build "dotnet-api-plus-htmx.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "dotnet-api-plus-htmx.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "dotnet-api-plus-htmx.dll"]
