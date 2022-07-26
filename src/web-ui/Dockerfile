FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ajiva-home-automation.web-ui/ajiva-home-automation.web-ui.csproj", "ajiva-home-automation.web-ui/"]
RUN dotnet restore "ajiva-home-automation.web-ui/ajiva-home-automation.web-ui.csproj"
COPY . .
WORKDIR "/src/ajiva-home-automation.web-ui"
RUN dotnet build "ajiva-home-automation.web-ui.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ajiva-home-automation.web-ui.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ajiva-home-automation.web-ui.dll"]
