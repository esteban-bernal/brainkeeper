#!/usr/bin/env bash
# Crea la solución .NET 10 de Brainkeeper. Ejecutar una vez, desde la raíz del repo.
set -euo pipefail
dotnet --version >/dev/null || { echo "Instala el SDK de .NET 10: https://dotnet.microsoft.com/download"; exit 1; }

dotnet new sln -n Brainkeeper --force
for p in Vault Indexer Retriever Promoter Linter Mcp; do
  dotnet new classlib -n Brainkeeper.$p -o src/Brainkeeper.$p --framework net10.0 --force
  dotnet sln add src/Brainkeeper.$p
done
dotnet new worker -n Brainkeeper.Api -o src/Brainkeeper.Api --framework net10.0 --force
dotnet sln add src/Brainkeeper.Api
dotnet new xunit -n Brainkeeper.Tests -o tests/Brainkeeper.Tests --framework net10.0 --force
dotnet sln add tests/Brainkeeper.Tests

# Dependencias entre proyectos
dotnet add src/Brainkeeper.Indexer   reference src/Brainkeeper.Vault
dotnet add src/Brainkeeper.Retriever reference src/Brainkeeper.Indexer src/Brainkeeper.Vault
dotnet add src/Brainkeeper.Promoter  reference src/Brainkeeper.Vault src/Brainkeeper.Indexer
dotnet add src/Brainkeeper.Linter    reference src/Brainkeeper.Vault src/Brainkeeper.Indexer
dotnet add src/Brainkeeper.Mcp       reference src/Brainkeeper.Vault src/Brainkeeper.Indexer src/Brainkeeper.Retriever src/Brainkeeper.Promoter src/Brainkeeper.Linter
dotnet add src/Brainkeeper.Api       reference src/Brainkeeper.Mcp
dotnet add tests/Brainkeeper.Tests   reference src/Brainkeeper.Vault src/Brainkeeper.Indexer src/Brainkeeper.Retriever src/Brainkeeper.Promoter src/Brainkeeper.Linter

# Paquetes base
dotnet add src/Brainkeeper.Vault package YamlDotNet
dotnet add src/Brainkeeper.Vault package JsonSchema.Net
dotnet add src/Brainkeeper.Indexer package Microsoft.Data.Sqlite
dotnet add src/Brainkeeper.Mcp package ModelContextProtocol --prerelease
dotnet add src/Brainkeeper.Api package Microsoft.Extensions.AI --prerelease
dotnet add tests/Brainkeeper.Tests package FluentAssertions

# Quitar los Class1.cs de plantilla; el código real está en src/*/ (ya incluido en el repo)
find src -name Class1.cs -delete
find tests -name UnitTest1.cs -delete

dotnet build
dotnet test
echo "OK: solución creada. Haz commit."
