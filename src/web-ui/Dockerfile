FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["web-ui/web-ui.csproj", "web-ui/"]
RUN dotnet restore "web-ui/web-ui.csproj"
COPY . .
WORKDIR "/src/web-ui"
RUN dotnet build "web-ui.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "web-ui.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "web-ui.dll"]
