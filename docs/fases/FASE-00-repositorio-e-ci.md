# Fase 0 — Repositório, solução e CI

**Objetivo:** estrutura do projeto pronta, compilando localmente e no GitHub Actions, sem nenhuma função ainda.

## Tarefas

- [ ] Criar `C:\dev\orcamentos-ponto-do-gesso\PontoDoGesso.sln`
- [ ] Criar `C:\dev\orcamentos-ponto-do-gesso\src\Directory.Build.props` com:
  - `<Version>0.0.1</Version>`, `<LangVersion>latest</LangVersion>`, `<Nullable>disable</Nullable>`
  - `<Product>Ponto do Gesso</Product>`, `<Company>Ponto do Gesso</Company>`
  - referência a `Microsoft.NETFramework.ReferenceAssemblies` (PrivateAssets=all)
- [ ] Projeto `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\PontoDoGesso.csproj` — SDK, `net48`, `WinExe`, `UseWindowsForms`, `PlatformTarget x86`, `ApplicationIcon` (provisório)
- [ ] Projeto `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso.Atualizador\PontoDoGesso.Atualizador.csproj` — `net48`, `WinExe`, x86 (só `Program.cs` vazio por enquanto)
- [ ] Projeto `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso.Testes\PontoDoGesso.Testes.csproj` — `net48`, x86, xUnit, com 1 teste de exemplo
- [ ] Conferir `C:\dev\orcamentos-ponto-do-gesso\LICENSE` (MIT, já incluído no kit)
- [ ] `C:\dev\orcamentos-ponto-do-gesso\.gitignore` para .NET (bin, obj, .vs, *.user, publicacao/)
- [ ] `C:\dev\orcamentos-ponto-do-gesso\.github\workflows\build.yml`: em push/PR na `main`, `runs-on: windows-latest`, `dotnet build -c Release` e `dotnet test`
- [ ] Criar as pastas vazias com `.gitkeep`: `Forms`, `Controles`, `Dominio`, `Dados\Migracoes`, `Servicos`, `Exportacao`
- [ ] `dotnet build` e `dotnet test` passando localmente
- [ ] Push e CI verde no GitHub

## Pronto quando

- `dotnet build C:\dev\orcamentos-ponto-do-gesso\PontoDoGesso.sln -c Release` sem erros nem avisos de alvo
- Selo do CI verde (adicionar o badge no `README.md`)

## Observações

