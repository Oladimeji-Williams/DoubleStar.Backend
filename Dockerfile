# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY global.json Directory.Build.props Directory.Build.targets Directory.Packages.props ./
COPY src/ src/
RUN dotnet restore src/Host/DoubleStar.API/DoubleStar.API.csproj
RUN dotnet publish src/Host/DoubleStar.API/DoubleStar.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
# Behind a proxy the API must trust X-Forwarded-For/Proto, otherwise the login rate limit
# is shared by every user (they all appear to come from the proxy's IP).
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "DoubleStar.API.dll"]