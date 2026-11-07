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
 5. Confere o SHA-256 — se não bater, cancela
 6. Faz backup de dados.db em %APPDATA%\PontoDoGesso\backups\
 7. Copia PontoDoGesso.Atualizador.exe para a pasta temporária e o executa com:
    --pid <id do processo> --zip <caminho do zip> --destino <pasta do app>
 8. Fecha

PontoDoGesso.Atualizador.exe
 1. Espera o processo <pid> terminar (até 30 s)
 2. Extrai o zip numa pasta temporária
 3. Renomeia a pasta do app atual para <pasta>.anterior (rollback)
 4. Move os arquivos novos para a pasta do app
 5. Se algo falhar: restaura <pasta>.anterior e avisa
 6. Abre o PontoDoGesso.exe novo e se encerra
```

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
4. O workflow `C:\dev\orcamentos-ponto-do-gesso\.github\workflows\release.yml` compila em Release, roda os testes, executa `scripts\empacotar.ps1` e publica a Release com:
   - `PontoDoGesso-v1.4.0.zip`
   - `PontoDoGesso-v1.4.0.zip.sha256`
   - texto da Release = seção da versão no CHANGELOG.

Versionamento: `MAJOR.MINOR.PATCH` — PATCH para correções, MINOR para funções novas, MAJOR para mudanças grandes no banco ou na forma de usar.

## Conteúdo do zip

```
PontoDoGesso-v1.4.0.zip
└─ PontoDoGesso\
   ├─ PontoDoGesso.exe
   ├─ PontoDoGesso.exe.config
   ├─ PontoDoGesso.Atualizador.exe
   ├─ *.dll                       (Dapper, PDFsharp, System.Data.SQLite...)
   ├─ x86\SQLite.Interop.dll
   └─ assets\
```
