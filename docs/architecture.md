# Arquitetura

O QuoteMaster usa uma solução .NET em quatro projetos:

- **Domain** contém entidades e enums e não depende de outras camadas.
- **Application** contém os casos de uso e depende de Domain.
- **Infrastructure** implementa persistência e depende de Application e Domain.
- **Web** contém a aplicação ASP.NET Core MVC e conecta Application a Infrastructure.

O Entity Framework Core usa SQLite no MVP. Controllers devem encaminhar operações para serviços da camada Application, mantendo regras de negócio fora da interface web.
