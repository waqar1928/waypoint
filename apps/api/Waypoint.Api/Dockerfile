FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Waypoint.sln .
COPY src/ src/
COPY apps/api/Waypoint.Api/ apps/api/Waypoint.Api/

RUN dotnet publish apps/api/Waypoint.Api/Waypoint.Api.csproj -c Release -o /app --no-self-contained

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Waypoint.Api.dll"]
