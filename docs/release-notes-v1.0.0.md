# QuoteMaster 1.0.0 — rascunho de release

> **Status: candidata local, não publicar ainda.** Falta instalação e atualização em máquina limpa, teste de backup/restauração e confirmação da licença QuestPDF adequada à entidade cliente. A CI foi definida no repositório, mas não executada em um remoto.

## Incluído

- Dashboard, cadastro de clientes e catálogo de produtos/serviços.
- Criação e transições de estado de orçamentos, descontos validados no servidor e numeração anual sequencial em SQLite.
- Geração de PDF com configurações da empresa, logotipo, valores em pt-BR e paginação.
- SQLite e imagens sob `%LOCALAPPDATA%\QuoteMaster`, separados dos binários de instalação.
- Launcher local, atalho de demonstração com dados fictícios, instalador Windows x64 e pacote portátil.

## Requisitos

- Windows 10/11 x64 para a distribuição preparada.
- O pacote self-contained não requer SDK/runtime .NET instalado.
- O modo normal é local e de usuário único; rede, sincronização, multiusuário e SaaS não estão incluídos.

## Antes da publicação

1. Build e testes Release aprovados.
2. Instalação, atualização, desinstalação e preservação dos dados verificadas em Windows limpo.
3. Backup/restauração testados com SQLite e diretório de logotipos.
4. Licença de produção confirmada e configurada.
5. Capturas e vídeo reais adicionados, sem dados de clientes.
6. Workflow remoto verde; instalador e hashes anexados à release.
