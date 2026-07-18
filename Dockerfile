FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/Inventory.Api/Inventory.Api.csproj", "src/Inventory.Api/"]
RUN dotnet restore "src/Inventory.Api/Inventory.Api.csproj"

COPY . .
WORKDIR "/src/src/Inventory.Api"
RUN dotnet publish "Inventory.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM runtime AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Inventory.Api.dll"]
