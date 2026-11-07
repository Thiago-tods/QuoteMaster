# Distribuição local do QuoteMaster

## Pacote para Windows x64

Na raiz do repositório, execute `powershell -ExecutionPolicy Bypass -File scripts/Build-Distribution.ps1`. O script publica o servidor e o launcher em modo Release, self-contained para `win-x64`, compila o instalador Inno Setup, cria um ZIP portátil e calcula SHA-256 dos dois arquivos. É necessário ter o .NET 10 SDK para compilar e o Inno Setup 7 para criar o instalador. O cliente que executa o pacote não precisa do SDK nem do runtime .NET instalado.

Saídas geradas (ignoradas pelo Git):

- `dist/installer/QuoteMaster-Setup-1.0.0-win-x64.exe`
- `dist/QuoteMaster-1.0.0-win-x64-portable.zip`
- `dist/SHA256SUMS.txt`

O instalador é por usuário e instala em `%LOCALAPPDATA%\Programs\QuoteMaster`. A desinstalação remove os arquivos do programa e preserva os dados. Atualizações devem fechar o launcher primeiro; faça backup e mantenha o instalador anterior até confirmar a nova instalação.

## Armazenamento e ciclo de vida

O launcher inicia o processo Web com uma porta local disponível e limita o Kestrel a `127.0.0.1`. Ele aguarda `/health`, abre o navegador e mantém uma pequena janela com controles para abrir novamente ou encerrar o servidor. Uma mutex evita duas instâncias do launcher; outra mutex por banco bloqueia inicializações concorrentes do servidor. Ao fechar o launcher, o processo Web e seus descendentes são encerrados.

Os dados reais ficam em `%LOCALAPPDATA%\QuoteMaster\`:

```text
database\quotemaster.db
backups\pre-migration-*.db
uploads\logos\
logs\
```

O banco e os logotipos não ficam na pasta de instalação. Se o aplicativo detectar migrations pendentes em um banco existente, cria um backup SQLite consistente antes de migrá-lo. Mesmo assim, mantenha cópias externas regulares: o backup pré-migration não substitui rotina de backup.

O atalho “QuoteMaster - Demonstração” usa `%LOCALAPPDATA%\QuoteMaster\Demo\`, exibe um aviso visível e recria os dados fictícios sempre que o modo é iniciado. Alterações feitas nesse ambiente de demonstração são descartadas ao fechar e abrir novamente esse atalho. A base real permanece separada. O seed cria cinco clientes, dez itens e dez cotações: duas em rascunho, três enviadas, três aprovadas, uma recusada e uma expirada.

## Checklist de validação manual antes de entregar

1. Em uma máquina Windows x64 sem SDK .NET, instalar o setup e abrir o atalho.
2. Conferir que a URL usa `127.0.0.1` e que o navegador abre o dashboard.
3. No atalho demo, conferir o aviso, cinco clientes, dez itens e os estados de orçamento.
4. Criar um orçamento com vários itens e descontos, gerar/abrir o PDF e aprovar.
5. Encerrar e abrir o modo demo novamente; verificar que os dados voltaram ao estado original.
6. No atalho normal, criar um registro, encerrar e abrir novamente; confirmar persistência.
7. Fazer backup do banco e da pasta `uploads\logos`; testar restauração em uma cópia separada.
8. Instalar a versão seguinte por cima e confirmar que o banco e os logotipos permanecem.
9. Desinstalar e confirmar que os arquivos de `%LOCALAPPDATA%\QuoteMaster\` foram preservados.

O build automatizado valida compilação e testes, mas não substitui esses passos em uma máquina limpa. Não marque o instalador como release 1.0.0 público enquanto essa validação não tiver sido registrada.

## Licenças

O pacote está configurado como QuestPDF `Evaluation`, adequado ao trabalho de desenvolvimento e avaliação. Não use essa configuração em produção. Antes de uso com cliente, confirme a licença aplicável com base na entidade que usará ou se beneficiará do sistema e configure `QuestPdf__LicenseType` de acordo. O pacote Inno Setup não assinado pode gerar aviso do Windows; não publique um instalador assinado até validar certificado, assinatura e checksum.
