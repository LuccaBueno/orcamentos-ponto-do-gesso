# Atualização pelo app

## Para o usuário (na loja)

1. Tela principal → rodapé → **Verificar atualizações**.
2. Se houver versão nova, o app mostra o número e o que mudou (texto da Release).
3. **Atualizar agora** → baixa, confere o arquivo, faz backup do banco, fecha, troca os arquivos e abre de novo sozinho.
4. Sem internet ou com erro: mensagem clara e botão **Abrir página de downloads** (atualização manual).

O app também verifica sozinho ao abrir, no máximo uma vez por dia, sem travar a tela (só mostra um aviso discreto no rodapé).

## Como funciona por dentro

```
PontoDoGesso.exe
 1. TLS 1.2 ligado (ServicePointManager)
 2. GET https://api.github.com/repos/LuccaBueno/orcamentos-ponto-do-gesso/releases/latest
    (User-Agent: PontoDoGesso/<versão>)
 3. Compara tag_name (v1.4.0) com a versão do exe (Directory.Build.props)
 4. Baixa PontoDoGesso-v1.4.0.zip e PontoDoGesso-v1.4.0.zip.sha256
    para %APPDATA%\PontoDoGesso\atualizacao\
 5. Confere o SHA-256 — se não bater, apaga o zip e cancela (nada é alterado)
 6. Faz backup de dados.db em %APPDATA%\PontoDoGesso\backups\dados_AAAA-MM-DD_HHmm_antes-de-atualizar.db
    (se o backup falhar, cancela)
 7. Copia PontoDoGesso.Atualizador.exe para %TEMP%\PontoDoGesso-atualizador\<código>\ e o executa com:
    --pid <id do processo> --zip <caminho do zip> --destino <pasta do app> --logs <pasta de logs>
    (se o Atualizador sumiu ou foi bloqueado: avisa e abre a página de downloads)
 8. Fecha

PontoDoGesso.Atualizador.exe
 1. Espera o processo <pid> terminar (até 30 s)
 2. Extrai o zip em <pasta>.nova (ao lado da pasta do app, ex.: C:\PontoDoGesso.nova)
 3. Renomeia a pasta do app atual para <pasta>.anterior (rollback)
 4. Renomeia a pasta nova para a pasta do app
    (cada passo tenta 10 vezes, 1 s entre elas, caso o antivírus esteja lendo algum arquivo)
 5. Se algo falhar: restaura <pasta>.anterior, reabre o app antigo e avisa
 6. Abre o PontoDoGesso.exe novo; se ele não abrir, volta para <pasta>.anterior e avisa
 7. Apaga o zip baixado e se encerra
 Log: %APPDATA%\PontoDoGesso\logs\atualizacao_AAAA-MM.log
```

- A `<pasta>.anterior` (ex.: `C:\PontoDoGesso.anterior`) fica guardada até a próxima atualização. Para voltar à versão anterior à mão: fechar o app, renomear `C:\PontoDoGesso` para `C:\PontoDoGesso.ruim` e `C:\PontoDoGesso.anterior` para `C:\PontoDoGesso`.
- Por isso o usuário também precisa poder criar pastas ao lado da pasta do app. Em `C:\` isso já é permitido por padrão no Windows 7.

- A pasta do app precisa ter permissão de escrita para o usuário. Por isso a instalação recomendada é `C:\PontoDoGesso\` (e não `C:\Arquivos de Programas\`).
- Os dados ficam em `%APPDATA%\PontoDoGesso\`, fora da pasta do app, então nunca são apagados pela atualização.
- Migrações do banco rodam na primeira abertura da versão nova (com backup antes).
- **Risco a testar na Fase 1:** a conexão HTTPS com o GitHub a partir do Windows 7. Se o Windows 7 da loja não conseguir negociar com o GitHub mesmo com TLS 1.2 ligado (falta de atualizações do Windows), o botão cai no modo manual (abre a página de downloads no navegador) e o `docs/INSTALACAO-WINDOWS7.md` explica quais atualizações instalar.

## Para o desenvolvedor: publicar uma versão

1. Atualizar `<Version>` em `C:\dev\orcamentos-ponto-do-gesso\src\Directory.Build.props` (ex.: `1.4.0`).
2. Atualizar `C:\dev\orcamentos-ponto-do-gesso\CHANGELOG.md`.
3. Commit e tag:
   ```powershell
   cd C:\dev\orcamentos-ponto-do-gesso
   git add -A
   git commit -m "build: versão 1.4.0"
   git tag v1.4.0
   git push origin main --tags
   ```
4. O workflow `C:\dev\orcamentos-ponto-do-gesso\.github\workflows\release.yml` compila em Release, roda os testes, executa `scripts\empacotar.ps1` (com o autoteste da pasta montada), instala o Inno Setup 6.7.4 (`scripts\instalar-inno-ci.ps1`, SHA-256 conferido), executa `scripts\gerar-instalador.ps1` e publica a Release com:
   - `PontoDoGesso-v1.4.0.zip`
   - `PontoDoGesso-v1.4.0.zip.sha256`
   - `PontoDoGesso.zip` — cópia idêntica com nome fixo, para instalação manual (opção avançada do README). O atualizador **não** usa esse arquivo: só procura `PontoDoGesso-vX.Y.Z.zip` e o `.sha256`.
   - `PontoDoGesso-Instalador-v1.4.0.exe` — instalador (Inno Setup, `instalador\PontoDoGesso.iss`).
   - `PontoDoGesso-Instalador.exe` — cópia com nome fixo, para o link `releases/latest/download/PontoDoGesso-Instalador.exe` do README.
   - texto da Release = aviso de instalação (baixar o `PontoDoGesso-Instalador.exe`; no Windows 7, antes o .NET 4.8) + seção da versão no CHANGELOG.

   O workflow `build.yml` também empacota e gera o instalador a cada push no `main`, sem publicar nada, para pegar erro de empacotamento antes da release.

## Instalador

`instalador\PontoDoGesso.iss` (Inno Setup 6; o 6.x gera instaladores que rodam no Windows 7 SP1):

- instala sempre em `C:\PontoDoGesso\`, sem pedir pasta e **sem pedir administrador** (a atualização pelo botão precisa trocar essa pasta com o usuário comum);
- confere o .NET Framework 4.8 (registro `Release` ≥ 528040). Se faltar, avisa em português — "Instale primeiro o .NET Framework 4.8 (versão Runtime, NÃO a 4.8.1 nem o Developer Pack)" — com o link direto https://go.microsoft.com/fwlink/?linkid=2088631 (oferece abrir) e **não instala**;
- cria atalhos na Área de Trabalho e no Menu Iniciar do usuário, com o ícone do app, e no fim oferece "Abrir o Ponto do Gesso";
- por cima de uma instalação antiga (inclusive a v0.9.2 descompactada do zip, com DLLs na raiz), apaga as DLLs da raiz e a pasta `x86\` antes de copiar; os dados em `%APPDATA%\PontoDoGesso\` não são tocados;
- o desinstalador (`unins000.exe`) fica em `C:\PontoDoGesso\`; o Atualizador leva esses arquivos para a pasta nova a cada atualização pelo botão. Desinstalar remove `C:\PontoDoGesso\` e `C:\PontoDoGesso.anterior\`, mas não os dados.

Teste real (tudo em `.tmp\`, sem atalhos nem registro, com os parâmetros só de teste `/TESTE` e `/SIMULARSEMDOTNET`): `C:\dev\orcamentos-ponto-do-gesso\scripts\teste-atualizacao\teste-instalador.ps1 -ZipV092 <PontoDoGesso-v0.9.2.zip>`.

Versionamento: `MAJOR.MINOR.PATCH` — PATCH para correções, MINOR para funções novas, MAJOR para mudanças grandes no banco ou na forma de usar.

## Conteúdo do zip

```
PontoDoGesso-v1.4.0.zip
└─ PontoDoGesso\
   ├─ PontoDoGesso.exe
   ├─ PontoDoGesso.exe.config     (tem <probing privatePath="lib"/>)
   ├─ PontoDoGesso.Atualizador.exe
   ├─ PontoDoGesso.Atualizador.exe.config
   ├─ lib\
   │  ├─ *.dll                    (Dapper, PDFsharp, System.Data.SQLite...)
   │  └─ x86\SQLite.Interop.dll
   └─ assets\
```

Até a v0.9.2 as DLLs ficavam na raiz e o `SQLite.Interop.dll` em `x86\`. A atualização da v0.9.2 para uma versão com `lib\` funciona sem nada especial: o Atualizador troca a **pasta inteira** (a antiga vai para `PontoDoGesso.anterior`), então as DLLs antigas da raiz somem junto e os dados, que ficam em `%APPDATA%\PontoDoGesso\`, não são tocados.

O `scripts\empacotar.ps1` roda `PontoDoGesso.exe --autoteste` na pasta montada (com dados numa pasta descartável) e falha se o app não carregar o SQLite e o PDFsharp de `lib\`, não criar o banco ou não gerar PDF e PNG.
