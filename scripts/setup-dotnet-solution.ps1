$ErrorActionPreference = "Stop"

$root = Join-Path $PSScriptRoot "..\dotnet-solution"
New-Item -ItemType Directory -Force -Path $root | Out-Null
$root = (Resolve-Path $root).Path

Set-Location $root

Write-Host "Creating .NET solution at $root"

dotnet new sln -n BookstoreRefactor

dotnet new classlib -n BookstoreRefactor.Domain -f net10.0 --force
dotnet new classlib -n BookstoreRefactor.Application -f net10.0 --force
dotnet new classlib -n BookstoreRefactor.Infrastructure -f net10.0 --force
dotnet new webapi -n BookstoreRefactor.Api -f net10.0 --force
dotnet new xunit -n BookstoreRefactor.Tests -f net10.0 --force

dotnet sln BookstoreRefactor.sln add `
  .\BookstoreRefactor.Domain\BookstoreRefactor.Domain.csproj `
  .\BookstoreRefactor.Application\BookstoreRefactor.Application.csproj `
  .\BookstoreRefactor.Infrastructure\BookstoreRefactor.Infrastructure.csproj `
  .\BookstoreRefactor.Api\BookstoreRefactor.Api.csproj `
  .\BookstoreRefactor.Tests\BookstoreRefactor.Tests.csproj

# References: inward flow toward Domain

dotnet add .\BookstoreRefactor.Application\BookstoreRefactor.Application.csproj reference .\BookstoreRefactor.Domain\BookstoreRefactor.Domain.csproj
dotnet add .\BookstoreRefactor.Infrastructure\BookstoreRefactor.Infrastructure.csproj reference .\BookstoreRefactor.Domain\BookstoreRefactor.Domain.csproj
dotnet add .\BookstoreRefactor.Infrastructure\BookstoreRefactor.Infrastructure.csproj reference .\BookstoreRefactor.Application\BookstoreRefactor.Application.csproj
dotnet add .\BookstoreRefactor.Api\BookstoreRefactor.Api.csproj reference .\BookstoreRefactor.Application\BookstoreRefactor.Application.csproj
dotnet add .\BookstoreRefactor.Api\BookstoreRefactor.Api.csproj reference .\BookstoreRefactor.Infrastructure\BookstoreRefactor.Infrastructure.csproj
dotnet add .\BookstoreRefactor.Tests\BookstoreRefactor.Tests.csproj reference .\BookstoreRefactor.Domain\BookstoreRefactor.Domain.csproj
dotnet add .\BookstoreRefactor.Tests\BookstoreRefactor.Tests.csproj reference .\BookstoreRefactor.Application\BookstoreRefactor.Application.csproj
dotnet add .\BookstoreRefactor.Tests\BookstoreRefactor.Tests.csproj reference .\BookstoreRefactor.Infrastructure\BookstoreRefactor.Infrastructure.csproj
dotnet add .\BookstoreRefactor.Tests\BookstoreRefactor.Tests.csproj reference .\BookstoreRefactor.Api\BookstoreRefactor.Api.csproj

Write-Host "Solution scaffold created successfully."
