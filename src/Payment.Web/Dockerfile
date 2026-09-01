FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["NuGet.Config", "."]
COPY ["src/Payment.Core/Payment.Core.csproj", "src/Payment.Core/"]
COPY ["src/Payment.Infrastructure/Payment.Infrastructure.csproj", "src/Payment.Infrastructure/"]
COPY ["src/Payment.Web/Payment.Web.csproj", "src/Payment.Web/"]
RUN dotnet restore "src/Payment.Web/Payment.Web.csproj"
COPY ["src/Payment.Core/", "src/Payment.Core/"]
COPY ["src/Payment.Infrastructure/", "src/Payment.Infrastructure/"]
COPY ["src/Payment.Web/", "src/Payment.Web/"]
RUN dotnet publish "src/Payment.Web/Payment.Web.csproj" -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app .
USER app
ENTRYPOINT ["dotnet", "Payment.Web.dll"]
