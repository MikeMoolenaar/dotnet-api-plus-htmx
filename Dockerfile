FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine3.18 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

WORKDIR /src
COPY ["dotnet-api-plus-htmx/dotnet-api-plus-htmx.csproj", "dotnet-api-plus-htmx/"]
RUN dotnet restore "dotnet-api-plus-htmx/dotnet-api-plus-htmx.csproj"

COPY . .
WORKDIR "/src/dotnet-api-plus-htmx"
RUN dotnet ef database update
RUN dotnet build "dotnet-api-plus-htmx.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "dotnet-api-plus-htmx.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .
COPY --from=build /src/dotnet-api-plus-htmx/data.db ./data.db
COPY --from=build /src/dotnet-api-plus-htmx/views ./views
COPY --from=build /src/dotnet-api-plus-htmx/static ./static
ENTRYPOINT ["dotnet", "dotnet-api-plus-htmx.dll"]
