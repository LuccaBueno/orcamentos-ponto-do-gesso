# Ponto do Gesso — app de orçamentos

App desktop que substituiu o talão de papel nos orçamentos da loja **Ponto do Gesso** (Gesso e Ferramentas — Pindamonhangaba/SP). Roda até no **Windows 7** do PC da loja.

<h2>⬇️ Baixar</h2>

<p align="center">
  <a href="https://github.com/LuccaBueno/orcamentos-ponto-do-gesso/releases/latest/download/PontoDoGesso.zip">
    <img alt="Baixar o Ponto do Gesso" src="https://img.shields.io/badge/Baixar_o_Ponto_do_Gesso-Windows_7%2C_10_e_11-2ea44f?style=for-the-badge">
  </a>
</p>

<ol>
  <li>Clique no botão acima para baixar o <code>PontoDoGesso.zip</code>.</li>
  <li>Clique com o botão direito no arquivo baixado → <b>Extrair tudo...</b> → digite <code>C:\</code> e clique em <b>Extrair</b>.</li>
  <li>Abra <code>C:\PontoDoGesso\PontoDoGesso.exe</code>. Se aparecer <b>"O Windows protegeu o computador"</b>, clique em <b>Mais informações › Executar assim mesmo</b> (o app ainda não tem assinatura digital).</li>
</ol>

<blockquote>
  ⚠️ Não baixe <b>"Source code (zip)"</b> nem <b>"Source code (tar.gz)"</b>: isso é o código-fonte para programadores, não o app.<br>
  No Windows 7, instale antes o .NET Framework 4.8 — veja <a href="docs/INSTALACAO-WINDOWS7.md">docs/INSTALACAO-WINDOWS7.md</a>.
</blockquote>

[![build](https://github.com/LuccaBueno/orcamentos-ponto-do-gesso/actions/workflows/build.yml/badge.svg)](https://github.com/LuccaBueno/orcamentos-ponto-do-gesso/actions/workflows/build.yml)
[![última versão](https://img.shields.io/github/v/release/LuccaBueno/orcamentos-ponto-do-gesso?label=%C3%BAltima%20vers%C3%A3o)](https://github.com/LuccaBueno/orcamentos-ponto-do-gesso/releases/latest)
[![licença](https://img.shields.io/github/license/LuccaBueno/orcamentos-ponto-do-gesso?label=licen%C3%A7a)](LICENSE)
![Windows 7 a 11](https://img.shields.io/badge/Windows-7%20a%2011-0078D6)
![.NET Framework 4.8](https://img.shields.io/badge/.NET%20Framework-4.8-512BD4)

<p align="center">
  <img src="docs/imagens/demo.gif" alt="Fazendo um orçamento do zero até copiar a imagem para o WhatsApp" width="673">
  <br><sub>Orçamento do zero até a imagem pronta para o WhatsApp (dados fictícios do modo de demonstração).</sub>
</p>

## O problema

Os orçamentos eram escritos à mão num talão de papel: a conta era feita na calculadora, para mandar pelo WhatsApp só restava fotografar a folha e nada ficava guardado para consultar depois. Achar um orçamento antigo ou refazer um parecido para outra obra significava folhear talões.

## A solução

Um app simples de balcão, pensado para quem não é da área de tecnologia:

- o orçamento é feito pelo teclado, com o cliente e os produtos sugeridos enquanto se digita, e as contas são feitas pelo app;
- a imagem sai **no mesmo visual da folha de papel** que os clientes já conhecem, pronta para colar no WhatsApp Web com `Ctrl+V`;
- tudo fica guardado no próprio computador, com histórico, filtros e backup automático;
- o app se atualiza sozinho pelo botão **Verificar atualizações**, sem ninguém precisar ir até a loja.

## Funcionalidades

| | |
|---|---|
| ![Tela de orçamento](docs/imagens/orcamento.png) | **Orçamento** para cliente cadastrado ou avulso, entrega ou retirada, itens com sugestão do catálogo (por código ou nome), desconto opcional em R$ ou %, frete, validade opcional e observações. |
| ![Orçamento pronto](docs/imagens/orcamento-final.png) | **Saída** em A4 retrato, igual à folha em papel: **Copiar imagem**, PNG, PDF, impressão com pré-visualização e botão **WhatsApp**, que abre a conversa com o cliente. |
| ![Histórico](docs/imagens/historico.png) | **Histórico** com filtros combináveis (período, cadastrado/avulso, cliente, frete, desconto, texto), soma dos valores e ações para reabrir, duplicar e exportar de novo. |
| ![Produtos](docs/imagens/produtos.png) | **Produtos** com preço editável direto na lista e reajuste em lote; **clientes** com busca sem acento e aviso de duplicado. |

E também: backup automático diário (com pasta extra para pendrive) e restauração, configurações dos dados da loja no cabeçalho, atalhos de teclado, manual (`F1`), **modo de demonstração** com dados fictícios e tela de diagnóstico para suporte.

Manual completo: [docs/MANUAL.md](docs/MANUAL.md).

## Stack

| Parte | Escolha |
|---|---|
| Linguagem e interface | C# + **WinForms** sobre **.NET Framework 4.8**, x86 |
| Dados | **SQLite** (`System.Data.SQLite`) + **Dapper**, migrações embutidas no exe |
| PDF, PNG e impressão | **PDFsharp** 6.2.4 (variante GDI, última versão MIT) e `System.Drawing` |
| Testes | **xUnit** (cálculos, banco, exportação e telas WinForms) |
| CI/CD | **GitHub Actions**: compila e testa a cada push; a tag `vX.Y.Z` gera o zip, o `.sha256` e a Release |

Sem instalador, sem serviço em segundo plano e sem telemetria: a única chamada de rede do app é a verificação de versão nova no GitHub. Detalhes em [docs/ARQUITETURA.md](docs/ARQUITETURA.md).

## Como instalar

1. **Windows 7:** instalar uma vez o .NET Framework 4.8 ([passo a passo](docs/INSTALACAO-WINDOWS7.md)). No Windows 10 e 11 ele já vem instalado.
2. Baixar o [`PontoDoGesso.zip`](https://github.com/LuccaBueno/orcamentos-ponto-do-gesso/releases/latest/download/PontoDoGesso.zip), extrair em `C:\` e abrir `C:\PontoDoGesso\PontoDoGesso.exe`.
3. Os dados ficam em `%APPDATA%\PontoDoGesso\`, fora da pasta do app: atualizar ou trocar a pasta não apaga nada.

| Item | Requisito |
|---|---|
| Sistema | Windows 7 SP1, 8.1, 10 e 11 (32 ou 64 bits) |
| Runtime | .NET Framework 4.8 |
| Instalação | Portátil: descompactar a pasta e abrir `PontoDoGesso.exe` |

## Como rodar o código

Pré-requisitos: Windows 10/11, [.NET SDK](https://dotnet.microsoft.com/download) 8 ou mais novo e Git. O pacote de referência do .NET Framework 4.8 vem pelo NuGet, então não precisa de Visual Studio.

```powershell
git clone https://github.com/LuccaBueno/orcamentos-ponto-do-gesso.git
cd orcamentos-ponto-do-gesso
dotnet build PontoDoGesso.sln -c Debug
dotnet test  PontoDoGesso.sln
.\src\PontoDoGesso\bin\x86\Debug\net48\PontoDoGesso.exe
```

- Para não mexer nos seus dados, defina `PONTODOGESSO_DADOS` com outra pasta antes de abrir o app (os testes gravam tudo em `.tmp\`).
- `scripts\empacotar.ps1` gera o zip da release; `scripts\gerar-prints.ps1` refaz os prints e o GIF deste README com o modo de demonstração.

## Desafios técnicos

### Rodar no Windows 7
O PC da loja tem Windows 7. Flutter, Electron, Tauri (WebView2) e .NET 5+ não rodam nele, então a escolha foi **WinForms sobre .NET Framework 4.8**, o último .NET que suporta o Windows 7 SP1. O exe é **x86** e roda nos Windows de 32 e de 64 bits. Cada dependência foi escolhida e travada pensando nisso: o `SQLite.Interop.dll` foi conferido por inspeção (só importa funções que existem no Windows 7 e não precisa do VC++ Redistributable) e o **PDFsharp ficou na 6.2.4**, porque a 6.5.0 mudou para AGPL-3.0 e passou a exigir licença em tempo de execução.

### TLS 1.2 para falar com o GitHub
O GitHub só aceita TLS 1.2 ou mais novo, mas o .NET Framework no Windows 7 não liga o TLS 1.2 por padrão: a verificação de versão nova falharia na loja. Antes de qualquer chamada de rede o app define `ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12`, e a tela **Ajuda → Diagnóstico** tem um botão para testar a conexão no próprio PC.

### Dinheiro em centavos
Valores são `long` em **centavos** no banco e nas contas (nunca `double`, que erra em somas como 0,1 + 0,2), e quantidades são `decimal` (`2,5 m²`). O total de cada item é `quantidade × unitário` arredondado para o centavo com **arredondamento comercial** (meio para cima, `MidpointRounding.AwayFromZero`), e o desconto percentual segue a mesma regra. Toda regra de cálculo tem teste, e a formatação é sempre pt-BR (`R$ 1.234,56`), qualquer que seja a configuração regional do Windows.

### Snapshot do cliente e do produto
Um orçamento é um documento: se o preço do produto ou o endereço do cliente mudar amanhã, o orçamento de ontem não pode mudar. Por isso cada orçamento guarda uma **cópia** do nome, telefone e endereço de entrega, e cada item guarda descrição, unidade e preço. O id do cliente e do produto fica só como referência, para filtros e para o histórico do cliente.

### Atualização automática com SHA-256 e rollback
O botão **Verificar atualizações** consulta a última Release, baixa o zip e confere o **SHA-256** com o `.sha256` publicado pelo CI (se não bater, nada muda). Antes de instalar, faz backup do banco. Um segundo exe, o `PontoDoGesso.Atualizador`, é copiado para a pasta temporária (assim ele pode trocar a pasta do app), espera o app fechar, guarda a versão atual em `<pasta>.anterior` e põe a nova no lugar, tentando de novo se o antivírus prender algum arquivo. Se a troca falhar **ou a versão nova não abrir**, ele volta a anterior sozinho. Os dados ficam em `%APPDATA%`, longe da pasta trocada.

### Um único código de desenho para PDF, PNG e impressão
O layout do orçamento (cópia fiel da folha em papel, em milímetros) está numa única classe, `DesenhoOrcamento`, que desenha num `XGraphics` do PDFsharp. O PDF usa `XGraphics.FromPdfPage`; o PNG e a impressão usam `XGraphics.FromGraphics` sobre um `Bitmap` e sobre o `Graphics` da impressora. Mudar o layout muda as três saídas de uma vez, e a pré-visualização da impressão é exatamente o que sai no PNG.

## Documentação

| Documento | Conteúdo |
|---|---|
| [docs/MANUAL.md](docs/MANUAL.md) | Manual do usuário |
| [docs/ARQUITETURA.md](docs/ARQUITETURA.md) | Stack, estrutura de pastas, banco de dados, regras de cálculo |
| [docs/LAYOUT-ORCAMENTO.md](docs/LAYOUT-ORCAMENTO.md) | Especificação visual do PDF/PNG (baseada na folha em papel) |
| [docs/ATUALIZACAO.md](docs/ATUALIZACAO.md) | Como funciona o botão de atualização e como publicar versões |
| [docs/INSTALACAO-WINDOWS7.md](docs/INSTALACAO-WINDOWS7.md) | Instalar e testar no PC da loja com Windows 7 |
| [docs/COMO-COMECAR.md](docs/COMO-COMECAR.md) | Preparar o PC de desenvolvimento |
| [docs/fases/](docs/fases/) | Plano de desenvolvimento em fases |
| [CHANGELOG.md](CHANGELOG.md) | Histórico de versões |

## Status e licença

Fase 10 (polimento) em andamento: falta o teste no Windows 7 da loja para publicar a v1.0.0 — ver [docs/fases/README.md](docs/fases/README.md).

Licença [MIT](LICENSE).
