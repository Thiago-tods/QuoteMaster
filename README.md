# QuoteMaster

Sistema web para cadastro de clientes, catálogo de produtos e serviços e gerenciamento de orçamentos comerciais.

## Stack

- ASP.NET Core MVC com Razor Views
- C# / .NET 10
- Entity Framework Core e SQLite
- Solução organizada em Domain, Application, Infrastructure e Web

## Estrutura

```text
src/
  QuoteMaster.Domain/          Entidades e enums
  QuoteMaster.Application/     Casos de uso e serviços
  QuoteMaster.Infrastructure/  EF Core e persistência
  QuoteMaster.Web/             Aplicação ASP.NET Core MVC
```

## Requisitos

- .NET 10 SDK
- Git
- Visual Studio Code com a extensão C# Dev Kit (recomendado)

## Executar localmente

```powershell
dotnet restore
dotnet tool restore
dotnet run --project src/QuoteMaster.Web
```

Para compilar e executar a suíte automatizada:

```powershell
dotnet build
dotnet test
```

O banco SQLite será armazenado localmente como `quotemaster-day2.db` na pasta de execução. A aplicação aplica as migrations pendentes ao iniciar. Esse arquivo está ignorado pelo Git.

## Estado do projeto

Camada de negócio inicial concluída: validação de clientes e catálogo, cálculo financeiro, transições de status, serviços de clientes/produtos/orçamentos, numeração anual atômica, migrations e testes unitários/de integração. O banco é atualizado automaticamente ao iniciar a aplicação.
