# Como começar (PC de desenvolvimento)

O desenvolvimento é feito no seu PC (Windows 10/11) com o Claude Code. O app gerado roda no Windows 7 da loja.

## 1. Instalar as ferramentas (uma vez)

| Ferramenta | Para quê | Como conferir |
|---|---|---|
| Git | versionar | `git --version` |
| GitHub CLI (`gh`) | criar repo e releases | `gh --version` e `gh auth status` |
| .NET SDK 8 (ou mais novo) | `dotnet build` / `dotnet test` | `dotnet --version` |
| .NET Framework 4.8 Developer Pack | rodar os testes e o app net48 no seu PC | já vem no Win10/11 o runtime; o pacote de referência vem pelo NuGet |
| Claude Code | desenvolver | `claude --version` |

Opcional: Visual Studio 2022 Community com a carga "Desenvolvimento para desktop com .NET", se quiser usar o designer visual do WinForms.

## 2. Criar a pasta e o repositório

```powershell
mkdir C:\dev\orcamentos-ponto-do-gesso
cd C:\dev\orcamentos-ponto-do-gesso
git init -b main
```

Copiar todos os arquivos deste kit para `C:\dev\orcamentos-ponto-do-gesso\` mantendo as pastas (o resultado deve ter `C:\dev\orcamentos-ponto-do-gesso\CLAUDE.md` e `C:\dev\orcamentos-ponto-do-gesso\docs\fases\README.md`).

Colocar o ícone do WhatsApp usado na folha atual em `C:\dev\orcamentos-ponto-do-gesso\assets\icone-whatsapp.png`.

```powershell
cd C:\dev\orcamentos-ponto-do-gesso
git add -A
git commit -m "docs: kit inicial do projeto e plano de fases"
gh repo create LuccaBueno/orcamentos-ponto-do-gesso --public --source C:\dev\orcamentos-ponto-do-gesso --push --description "Sistema desktop de orçamentos para loja de materiais de construção — C#, WinForms, SQLite, PDF/PNG, atualização automática via GitHub Releases. Compatível com Windows 7."
gh repo edit LuccaBueno/orcamentos-ponto-do-gesso --add-topic csharp,winforms,dotnet-framework,sqlite,pdf-generation,desktop-app,windows-7,auto-update
```

## 3. Começar a Fase 0 com o Claude Code

```powershell
cd C:\dev\orcamentos-ponto-do-gesso
claude
```

Primeira mensagem sugerida:

> Leia C:\dev\orcamentos-ponto-do-gesso\CLAUDE.md e C:\dev\orcamentos-ponto-do-gesso\docs\fases\FASE-00-repositorio-e-ci.md e execute a Fase 0. Ao terminar, marque os itens concluídos, atualize C:\dev\orcamentos-ponto-do-gesso\docs\fases\README.md e me diga o que testar.

Para cada fase seguinte, a mesma mensagem trocando o arquivo da fase.

## 4. Fluxo de cada fase

1. Claude Code implementa a fase e roda `dotnet build` e `dotnet test`.
2. Você testa no seu PC.
3. A partir da Fase 2, gerar uma versão (tag) e atualizar o PC da loja pelo próprio botão do app.
4. Anotar problemas encontrados na seção "Observações" do arquivo da fase.
