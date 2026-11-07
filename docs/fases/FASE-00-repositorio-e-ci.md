# Fase 0 — Repositório, solução e CI

**Objetivo:** estrutura do projeto pronta, compilando localmente e no GitHub Actions, sem nenhuma função ainda.

## Tarefas

- [x] Criar `C:\dev\orcamentos-ponto-do-gesso\PontoDoGesso.sln`
- [x] Criar `C:\dev\orcamentos-ponto-do-gesso\src\Directory.Build.props` com:
  - `<Version>0.0.1</Version>`, `<LangVersion>latest</LangVersion>`, `<Nullable>disable</Nullable>`
  - `<Product>Ponto do Gesso</Product>`, `<Company>Ponto do Gesso</Company>`
  - referência a `Microsoft.NETFramework.ReferenceAssemblies` (PrivateAssets=all)
- [x] Projeto `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\PontoDoGesso.csproj` — SDK, `net48`, `WinExe`, `UseWindowsForms`, `PlatformTarget x86`, `ApplicationIcon` (provisório)
- [x] Projeto `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso.Atualizador\PontoDoGesso.Atualizador.csproj` — `net48`, `WinExe`, x86 (só `Program.cs` vazio por enquanto)
- [x] Projeto `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso.Testes\PontoDoGesso.Testes.csproj` — `net48`, x86, xUnit, com 1 teste de exemplo
- [x] Conferir `C:\dev\orcamentos-ponto-do-gesso\LICENSE` (MIT, já incluído no kit)
- [x] `C:\dev\orcamentos-ponto-do-gesso\.gitignore` para .NET (bin, obj, .vs, *.user, publicacao/)
- [x] `C:\dev\orcamentos-ponto-do-gesso\.github\workflows\build.yml`: em push/PR na `main`, `runs-on: windows-latest`, `dotnet build -c Release` e `dotnet test`
- [x] Criar as pastas vazias com `.gitkeep`: `Forms`, `Controles`, `Dominio`, `Dados\Migracoes`, `Servicos`, `Exportacao`
- [x] `dotnet build` e `dotnet test` passando localmente
- [x] Push e CI verde no GitHub

## Pronto quando

- `dotnet build C:\dev\orcamentos-ponto-do-gesso\PontoDoGesso.sln -c Release` sem erros nem avisos de alvo
- Selo do CI verde (adicionar o badge no `README.md`)

## Observações


- Concluída em 06/11/2026. Build local com .NET SDK 10.0.403 sem avisos; CI verde no GitHub (`windows-latest`, SDK 10.0.x).
- A solução só tem a plataforma `x86` (definida em `C:\dev\orcamentos-ponto-do-gesso\src\Directory.Build.props`), por isso a saída fica em `bin\x86\Release\net48`, como a Fase 1 espera. O SDK 10 cria `.slnx` por padrão; o `PontoDoGesso.sln` foi mantido no formato clássico.
- Ícone provisório em `C:\dev\orcamentos-ponto-do-gesso\assets\icone-app.ico` (quadrado laranja com "PG"), a trocar quando houver o definitivo.
