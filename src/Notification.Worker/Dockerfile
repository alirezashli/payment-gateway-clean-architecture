FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["NuGet.Config", "."]
COPY ["src/Notification.Worker/Notification.Worker.csproj", "src/Notification.Worker/"]
RUN dotnet restore "src/Notification.Worker/Notification.Worker.csproj"
COPY ["src/Notification.Worker/", "src/Notification.Worker/"]
RUN dotnet publish "src/Notification.Worker/Notification.Worker.csproj" -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app .
USER app
ENTRYPOINT ["dotnet", "Notification.Worker.dll"]
