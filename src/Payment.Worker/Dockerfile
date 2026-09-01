FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["NuGet.Config", "."]
COPY ["src/Payment.Worker/Payment.Worker.csproj", "src/Payment.Worker/"]
RUN dotnet restore "src/Payment.Worker/Payment.Worker.csproj"
COPY ["src/Payment.Worker/", "src/Payment.Worker/"]
RUN dotnet publish "src/Payment.Worker/Payment.Worker.csproj" -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app .
USER app
ENTRYPOINT ["dotnet", "Payment.Worker.dll"]
