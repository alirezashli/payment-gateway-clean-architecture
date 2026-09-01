FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["NuGet.Config", "."]
COPY ["src/Gateway.Core/Gateway.Core.csproj", "src/Gateway.Core/"]
COPY ["src/Gateway.Infrastructure/Gateway.Infrastructure.csproj", "src/Gateway.Infrastructure/"]
COPY ["src/Gateway.Web/Gateway.Web.csproj", "src/Gateway.Web/"]
RUN dotnet restore "src/Gateway.Web/Gateway.Web.csproj"
COPY ["src/Gateway.Core/", "src/Gateway.Core/"]
COPY ["src/Gateway.Infrastructure/", "src/Gateway.Infrastructure/"]
COPY ["src/Gateway.Web/", "src/Gateway.Web/"]
RUN dotnet publish "src/Gateway.Web/Gateway.Web.csproj" -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app .
USER app
ENTRYPOINT ["dotnet", "Gateway.Web.dll"]
