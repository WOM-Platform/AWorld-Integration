# ---
# First stage (build)
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copy solution as distinct layer
COPY WomAWorldIntegration.sln .
COPY WomAWorldIntegration/WomAWorldIntegration.csproj ./WomAWorldIntegration/
COPY WomAWorldIntegration/WomAWorldIntegration.csproj ./WomAWorldIntegration/
RUN dotnet restore

# Copy everything else and build
COPY WomAWorldIntegration/. ./WomAWorldIntegration/
WORKDIR /app/WomAWorldIntegration
RUN dotnet publish -c Release -o out

# ---
# Second stage (execution)
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime

WORKDIR /app
COPY --from=build /app/WomAWorldIntegration/out ./

# Fix console logging
ENV Logging__Console__FormatterName=

# Run on localhost:8080
ENV ASPNETCORE_URLS http://+:8080
EXPOSE 8080

# Drop privileges
USER 1000

ENTRYPOINT ["dotnet", "WomAWorldIntegration.dll"]
