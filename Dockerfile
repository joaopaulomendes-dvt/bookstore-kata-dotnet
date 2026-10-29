FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Bookstore.sln Directory.Build.props Directory.Packages.props ./
COPY src/Bookstore.Api/Bookstore.Api.csproj src/Bookstore.Api/
COPY src/Bookstore.Application/Bookstore.Application.csproj src/Bookstore.Application/
COPY src/Bookstore.Domain/Bookstore.Domain.csproj src/Bookstore.Domain/
COPY src/Bookstore.Infrastructure/Bookstore.Infrastructure.csproj src/Bookstore.Infrastructure/
COPY tests/Bookstore.Tests/Bookstore.Tests.csproj tests/Bookstore.Tests/
COPY tools/Bookstore.Migration/Bookstore.Migration.csproj tools/Bookstore.Migration/
RUN dotnet restore Bookstore.sln

COPY src/ src/
COPY tests/ tests/
COPY tools/ tools/
RUN dotnet publish src/Bookstore.Api/Bookstore.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
USER app
ENTRYPOINT ["dotnet", "Bookstore.Api.dll"]
