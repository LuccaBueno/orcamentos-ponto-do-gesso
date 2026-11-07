# CLAUDE.md — instruções para o Claude Code

Projeto: **Ponto do Gesso** — app desktop de orçamentos da loja Ponto do Gesso (Gesso e Ferramentas).
Repositório: `LuccaBueno/orcamentos-ponto-do-gesso` (público). Pasta local: `C:\dev\orcamentos-ponto-do-gesso`.

## Antes de qualquer tarefa

1. Ler `docs/fases/README.md` para saber a fase atual.
2. Ler o arquivo da fase em `docs/fases/` e trabalhar **só no escopo dela**.
3. Consultar `docs/ARQUITETURA.md` e, para exportação, `docs/LAYOUT-ORCAMENTO.md`.
4. Ao terminar a fase: marcar os itens concluídos no arquivo da fase, atualizar o status em `docs/fases/README.md` e o `CHANGELOG.md`.

## Restrições que não podem ser quebradas

- **Alvo: .NET Framework 4.8, WinForms, PlatformTarget x86.** Precisa rodar no Windows 7 SP1 32 e 64 bits.
  - Não usar .NET 5/6/7/8 como runtime do app, nem WPF, WebView2, Electron, Flutter ou qualquer coisa que exija Windows 10.
  - Não usar APIs que não existem no Windows 7 (ex.: `Windows.*`/WinRT, notificações toast, `SetProcessDpiAwarenessContext` sem fallback).
  - Toda dependência NuGet precisa ter alvo `net48`, `net46x` ou `netstandard2.0` e ser testada no Windows 7.
- **HTTPS:** antes de qualquer chamada de rede, definir `ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;` (o Windows 7 não ativa TLS 1.2 por padrão no .NET).
- **Dinheiro em centavos (`long`)** no banco e nos cálculos. Nunca `double`/`float` para valores. Quantidades em `decimal`.
- **Cultura pt-BR** fixa na formatação (`R$ 1.234,56`, datas `dd/MM/yyyy`), independente da configuração do Windows.
- **Orçamento guarda cópia (snapshot) dos dados do cliente e dos itens.** Editar um cliente ou produto nunca altera orçamentos antigos.
- Dados do usuário ficam em `%APPDATA%\PontoDoGesso\` — nunca na pasta do executável (a pasta do app é substituída nas atualizações).
- Sem telemetria e sem enviar dados para lugar nenhum. A única chamada de rede é a verificação de atualização no GitHub.

## Convenções

- Interface, mensagens, comentários, commits e documentação em **português (pt-BR)**.
- Nomes de domínio em português sem acento: `Orcamento`, `ItemOrcamento`, `Cliente`, `Produto`, `Frete`.
- Commits no formato `tipo: descrição` (`feat:`, `fix:`, `docs:`, `refactor:`, `test:`, `build:`).
- Lógica fora dos formulários: formulários (`Forms/`) só chamam serviços (`Servicos/`) e repositórios (`Dados/`).
- Toda regra de cálculo tem teste no projeto `PontoDoGesso.Testes`.
- Sempre escrever caminhos completos nas instruções para o Lucca (ex.: `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\FormPrincipal.cs`), nunca "o caminho já citado".

## Comandos

```powershell
cd C:\dev\orcamentos-ponto-do-gesso
dotnet build PontoDoGesso.sln -c Debug
dotnet test  PontoDoGesso.sln
dotnet build PontoDoGesso.sln -c Release
powershell -ExecutionPolicy Bypass -File C:\dev\orcamentos-ponto-do-gesso\scripts\empacotar.ps1   # gera o .zip da release (Fase 2)
```
