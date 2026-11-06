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
dotnet run --project src/QuoteMaster.Web
```

O banco SQLite será armazenado localmente como `quotemaster.db` na pasta de execução. Esse arquivo está ignorado pelo Git.

## Estado do projeto

Fundação inicial criada: solução, quatro projetos, entidades centrais, enums, contexto EF Core e configuração SQLite. A primeira migration e a validação do build dependem da instalação do SDK .NET neste ambiente.
