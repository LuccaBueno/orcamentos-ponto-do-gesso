# Arquitetura

## Por que C# WinForms + .NET Framework 4.8

| Requisito | Como é atendido |
|---|---|
| Rodar no Windows 7 SP1 (32 e 64 bits) | .NET Framework 4.8 é suportado no Win7 SP1; build **x86** roda nos dois |
| Executável simples | `PontoDoGesso.exe` + DLLs numa pasta, sem instalador |
| PDF e PNG | PDFsharp (variante GDI): o **mesmo código de desenho** gera a página PDF e a imagem PNG |
| Leve no PC antigo | WinForms abre rápido e usa pouca memória |

Flutter, Electron, Tauri (WebView2) e .NET 6+ foram descartados porque não suportam Windows 7.

## Estrutura do repositório

```
C:\dev\orcamentos-ponto-do-gesso\
├─ PontoDoGesso.sln
├─ README.md
├─ CLAUDE.md
├─ CHANGELOG.md
├─ .gitignore
├─ .github\workflows\
│  ├─ build.yml              # compila e testa a cada push
│  └─ release.yml            # ao criar tag vX.Y.Z, gera o zip e publica a Release
├─ scripts\
│  ├─ empacotar.ps1          # monta PontoDoGesso-vX.Y.Z.zip + .sha256 + PontoDoGesso.zip
│  └─ gerar-icone.ps1        # desenha o assets\icone.ico
├─ assets\                   # icone.ico (ícone do app), ícone do WhatsApp para o cabeçalho
├─ docs\
│  ├─ ARQUITETURA.md
│  ├─ LAYOUT-ORCAMENTO.md
│  ├─ ATUALIZACAO.md
│  ├─ COMO-COMECAR.md
│  ├─ INSTALACAO-WINDOWS7.md
│  ├─ MANUAL.md              # manual do usuário (F1 no app)
│  ├─ imagens\               # prints e GIF do README e do manual (scripts\gerar-prints.ps1)
│  └─ fases\
└─ src\
   ├─ Directory.Build.props         # versão do app (<Version>) e configurações comuns
   ├─ PontoDoGesso\              # app WinForms (net48, x86)
   │  ├─ Program.cs
   │  ├─ Forms\                     # FormPrincipal (a única janela) e os diálogos (editar cliente/produto, reajuste, impressão...)
   │  │  └─ Telas\                  # telas da área principal (UserControl): Início, Orçamento, Histórico, Clientes, Produtos, Empresa, Configurações
   │  ├─ Controles\                 # menu lateral, ícones, barra "Adicionar item", quantidade com −/+, grade de itens, moeda, telefone, autocomplete
   │  ├─ Dominio\                   # Orcamento, ItemOrcamento, Cliente, Produto
   │  ├─ Dados\                     # Banco (conexão + migrações), repositórios
   │  │  └─ Migracoes\              # 001_inicial.sql, 002_...
   │  ├─ Servicos\                  # CalculoOrcamento, Formatacao, Backup, Atualizacao, Log
   │  └─ Exportacao\                # DesenhoOrcamento (layout), ExportadorPdf, ExportadorPng
   ├─ PontoDoGesso.Atualizador\  # exe pequeno que troca os arquivos na atualização (net48, x86)
   └─ PontoDoGesso.Testes\       # xUnit (net48)
```

## Projetos e dependências

| Projeto | Alvo | Pacotes |
|---|---|---|
| `PontoDoGesso` | `net48`, `WinExe`, `UseWindowsForms`, `PlatformTarget x86` | `System.Data.SQLite.Core`, `Dapper`, `PDFsharp` (variante GDI com alvo .NET Framework) |
| `PontoDoGesso.Atualizador` | `net48`, `WinExe`, x86 | nenhum |
| `PontoDoGesso.Testes` | `net48`, x86 | `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk` |

- Todos os `.csproj` no formato SDK, com `Microsoft.NETFramework.ReferenceAssemblies`, para compilar com `dotnet build` sem precisar do Visual Studio.
- Versão do app definida uma única vez em `src\Directory.Build.props` (`<Version>1.2.3</Version>`) e lida em tempo de execução pelo botão de atualização.
- As versões exatas estão travadas na tabela abaixo (Fase 1).

### Versões travadas

| Pacote | Versão | Licença | Situação |
|---|---|---|---|
| `System.Data.SQLite.Core` | `[1.0.119]` (SQLite 3.46.1) | domínio público | Passou no Windows 11; **falta o teste no Windows 7** |
| `PDFsharp-GDI` | `[6.2.4]` | MIT | Passou no Windows 11; **falta o teste no Windows 7** |
| `Dapper` | `[2.1.35]` | Apache-2.0 | Passou no Windows 11; **falta o teste no Windows 7** (o botão Testar banco do Diagnóstico lê pelo Dapper). Sem dependências no alvo net461; a 2.1.66 traria `Microsoft.Bcl.AsyncInterfaces` |
| `xunit` / `xunit.runner.visualstudio` / `Microsoft.NET.Test.Sdk` | `2.9.3` / `3.1.5` / `17.14.1` | — | Só nos testes, não vão para a loja |

- **Não atualizar o PDFsharp para 6.5.0 ou mais novo.** A partir da 6.5.0 a licença mudou de MIT para **AGPL-3.0** e a biblioteca passou a exigir uma licença válida em tempo de execução (`PdfSharpLicenseException: The loaded license is not valid`). A 6.2.4 é a última versão MIT. Por isso a versão está travada com colchetes no `.csproj`.
- `SQLite.Interop.dll` (x86) conferido por inspeção do binário: exige Windows 6.0 ou mais novo, não depende do VC++ Redistributable e só importa funções que existem no Windows 7.

## Onde ficam os dados

```
%APPDATA%\PontoDoGesso\          (ex.: C:\Users\Loja\AppData\Roaming\PontoDoGesso\)
├─ dados.db                 # banco SQLite
├─ backups\                 # dados_AAAA-MM-DD_HHmm_auto.db (diário, guarda 30), _manual.db, _antes-de-restaurar.db; sem sufixo = antes de migração
├─ logs\                    # erros_AAAA-MM.log
└─ atualizacao\             # downloads temporários da atualização
```

- Pasta padrão de exportação: `Documentos\Orçamentos Ponto do Gesso\AAAA\MM\` (configurável).
- Nome dos arquivos: `Orc-0123_Joao-Silva_06-11-2026.pdf` / `.png` (sem acentos nem espaços).

## Banco de dados (SQLite)

Valores em **centavos** (`INTEGER`). Quantidades em `TEXT` no formato invariante (ex.: `"2.5"`), convertidas para `decimal`. Datas em `TEXT` ISO (`AAAA-MM-DD` ou `AAAA-MM-DDTHH:mm:ss`).

```sql
CREATE TABLE clientes (
  id               INTEGER PRIMARY KEY AUTOINCREMENT,
  nome             TEXT NOT NULL,
  telefone         TEXT,
  telefone2        TEXT,
  endereco         TEXT,           -- rua
  numero           TEXT,
  bairro           TEXT,
  cidade           TEXT DEFAULT 'Pindamonhangaba',
  ponto_referencia TEXT,
  observacoes      TEXT,           -- nota interna (ex.: "pede tudo discriminado"); NÃO sai no orçamento
  ativo            INTEGER NOT NULL DEFAULT 1,
  criado_em        TEXT NOT NULL,
  atualizado_em    TEXT NOT NULL
);

CREATE TABLE produtos (
  id               INTEGER PRIMARY KEY AUTOINCREMENT,
  nome             TEXT NOT NULL,
  unidade          TEXT,           -- un, m², m, kg, saco, cx...
  preco_centavos   INTEGER NOT NULL DEFAULT 0,
  preco_atualizado_em TEXT,        -- última mudança de preço (migração 002)
  ativo            INTEGER NOT NULL DEFAULT 1,
  criado_em        TEXT NOT NULL,
  atualizado_em    TEXT NOT NULL
);

CREATE TABLE orcamentos (
  id                INTEGER PRIMARY KEY AUTOINCREMENT,
  numero            INTEGER NOT NULL UNIQUE,   -- sequencial 1, 2, 3... exibido como 0001
  data              TEXT NOT NULL,
  validade_dias     INTEGER,                   -- NULL = sem validade (não imprime); opcional por orçamento
  cliente_id        INTEGER REFERENCES clientes(id),   -- NULL = cliente avulso
  -- cópia (snapshot) do cliente/entrega no momento do orçamento
  cli_nome          TEXT NOT NULL,
  cli_telefone      TEXT,
  ent_endereco      TEXT,
  ent_numero        TEXT,
  ent_bairro        TEXT,
  ent_cidade        TEXT,
  ent_referencia    TEXT,
  tipo_entrega      TEXT NOT NULL DEFAULT 'ENTREGA',   -- ENTREGA | RETIRADA
  mostrar_cnpj      INTEGER NOT NULL DEFAULT 0,  -- 1 = o CNPJ da empresa sai no cabeçalho deste orçamento (migração 003)
  empresa_cnpj      TEXT,                      -- cópia (snapshot) do CNPJ usado, só dígitos, quando mostrar_cnpj = 1
  observacoes       TEXT,                      -- sai impresso no orçamento
  subtotal_centavos INTEGER NOT NULL,
  desconto_tipo     TEXT,                      -- NULL (sem desconto) | VALOR | PERCENTUAL
  desconto_valor    TEXT,                      -- centavos se VALOR; percentual invariante (ex.: "5.5") se PERCENTUAL
  desconto_centavos INTEGER NOT NULL DEFAULT 0,  -- valor final do desconto em centavos
  frete_centavos    INTEGER NOT NULL DEFAULT 0,
  total_centavos    INTEGER NOT NULL,
  criado_em         TEXT NOT NULL,
  atualizado_em     TEXT NOT NULL
);

CREATE TABLE itens_orcamento (
  id                INTEGER PRIMARY KEY AUTOINCREMENT,
  orcamento_id      INTEGER NOT NULL REFERENCES orcamentos(id) ON DELETE CASCADE,
  ordem             INTEGER NOT NULL,
  quantidade        TEXT NOT NULL,
  unidade           TEXT,
  descricao         TEXT NOT NULL,
  unitario_centavos INTEGER NOT NULL,
  total_centavos    INTEGER NOT NULL,
  produto_id        INTEGER REFERENCES produtos(id)   -- só referência; descrição e preço ficam copiados
);

CREATE TABLE configuracoes ( chave TEXT PRIMARY KEY, valor TEXT );
CREATE TABLE versao_banco  ( versao INTEGER NOT NULL );

CREATE INDEX ix_clientes_nome     ON clientes(nome);
CREATE INDEX ix_clientes_telefone ON clientes(telefone);
CREATE INDEX ix_orcamentos_data   ON orcamentos(data);
CREATE INDEX ix_orcamentos_cli    ON orcamentos(cli_nome);
CREATE INDEX ix_orcamentos_clid   ON orcamentos(cliente_id);
```

Migrações numeradas em `src\PontoDoGesso\Dados\Migracoes\` (embutidas no exe), aplicadas automaticamente na abertura, **sempre com backup do banco antes**.

| Migração | O que faz |
|---|---|
| `001_inicial.sql` | Esquema completo |
| `002_preco_atualizado_em.sql` | Data da última mudança de preço do produto |
| `003_cnpj_empresa.sql` | `orcamentos.mostrar_cnpj` e `orcamentos.empresa_cnpj`; chave `loja.cnpj`. Os dados da empresa continuam nas mesmas chaves `loja.*` (só passaram a ser editados na tela Empresa, em vez de Configurações): nada é apagado nem copiado |

### Configurações (chaves)

| Chave | Padrão |
|---|---|
| `loja.nome` | `PONTO DO GESSO` |
| `loja.subtitulo` | `Gesso e Ferramentas` |
| `loja.telefone` | `(12) 99244-5402` |
| `loja.endereco_linha1` | `Rua do Cardoso, 104 – Alto do` |
| `loja.endereco_linha2` | `Cardoso - Pindamonhangaba - SP` |
| `loja.rodape` | `As entregas são realizadas apenas até o térreo.` |
| `loja.cnpj` | vazio — só os 14 dígitos, editado na tela Empresa; sai no cabeçalho só dos orçamentos com "Mostrar CNPJ" marcado |
| `orcamento.validade_sugerida_dias` | `7` — só sugere o número quando a validade é ligada no orçamento |
| `orcamento.perguntar_cadastro_avulso` | `1` — ao salvar orçamento de cliente avulso pela primeira vez, pergunta se quer cadastrar (`0` = não perguntar; tela na Fase 9) |
| `cliente.ddd_padrao` | `12` (só pré-preenche o campo; sempre editável) |
| `cliente.cidade_padrao` | `Pindamonhangaba` (cliente novo e entrega de avulso) |
| `exportacao.pasta` | `Documentos\Orçamentos Ponto do Gesso` |
| `impressao.impressora` | vazio (= impressora padrão do Windows) |
| `backup.ultimo` | — (data e hora do último backup) |
| `backup.pasta_extra` | vazio (pasta extra opcional; cada backup é copiado para lá se ela existir) |
| `atualizacao.ultima_verificacao` | — (data e hora da última verificação automática de versão nova; no máximo 1 a cada 24 h) |

## Numeração (códigos)

Orçamento, cliente e produto têm **número próprio**, sequencial, começando em 1 e exibido com 4 dígitos:

| Registro | Exemplo na tela | Origem |
|---|---|---|
| Orçamento | `Nº 0123` | coluna `numero` (atribuída ao salvar pela primeira vez, a partir de `sqlite_sequence`: não volta atrás nem se o último for excluído) |
| Cliente cadastrado | `Cód. 0045` | `clientes.id` (`AUTOINCREMENT`: nunca é reaproveitado) |
| Produto cadastrado | `Cód. 0012` | `produtos.id` (`AUTOINCREMENT`: nunca é reaproveitado) |

- Cliente avulso e item digitado à mão (fora do catálogo) **não** têm código.
- Clientes e produtos não são excluídos, só inativados, então o código de um registro é permanente.
- Todas as buscas aceitam o código (digitar `45` ou `0045` acha o cliente 0045; no item do orçamento, digitar o código do produto preenche o item).
- O número do orçamento não é configurável: começa em 0001 e segue sozinho.

## Regras de cálculo

- `total_item = arredondar(quantidade × unitario_centavos)` — arredondamento comercial (meio para cima) para o centavo.
- `subtotal = Σ total_item`
- Desconto (opcional) incide **só sobre o subtotal dos produtos**, nunca sobre o frete:
  - `VALOR`: `desconto = valor informado`
  - `PERCENTUAL`: `desconto = arredondar(subtotal × % / 100)`
  - Desconto não pode passar do subtotal; desconto 0 é gravado como "sem desconto" (`desconto_tipo = NULL`).
- `total = subtotal − desconto + frete`
- Quantidade aceita vírgula (`2,5`). Linhas com descrição vazia são ignoradas ao salvar; quantidade 0 bloqueia o salvamento com aviso.
- Número do orçamento atribuído ao **salvar pela primeira vez**, numa transação; nunca é reaproveitado (nem se o orçamento for excluído).

- Data do orçamento (`Servicos\DataOrcamento.cs`): orçamento **novo** (ainda sem número) usa a data de hoje do Windows ao criar e ao duplicar, e passa sozinho para hoje ao salvar, exportar ou voltar para a tela se o dia virou — a não ser que a data tenha sido mudada à mão (aparece "data alterada manualmente"). Orçamento **já salvo** nunca muda de data sozinho. A data é lida do relógio a cada vez, nunca guardada em campo estático.
- Botões − e + da quantidade (`Servicos\PassoQuantidade.cs`): passo de 1, mínimo 1; fracionado só digitando (2,5 + 1 = 3,5).
- CNPJ (`Servicos\Cnpj.cs`): 14 dígitos com os dois dígitos verificadores conferidos; todos iguais (00.000.000/0000-00) é recusado.

## Telas e navegação

O app tem **uma janela só** (`Forms\FormPrincipal.cs`): menu lateral fixo à esquerda (`Controles\MenuLateral.cs`, ícones desenhados em código por `Controles\Icones.cs`) e, à direita, a tela escolhida. Cada tela é um `UserControl` que herda de `Forms\Telas\TelaBase.cs`:

- `INavegacao` (implementada pela janela principal) troca a tela: menu lateral, "Abrir"/"Duplicar" do histórico, "Novo orçamento para este cliente", "Orçamentos deste cliente", atalhos da tela inicial.
- Antes de trocar de tela ou fechar o app, `TelaBase.PodeSair()` pergunta Salvar / Não salvar / Cancelar se houver alterações (Orçamento, Empresa, Configurações). Cancelar fica na tela.
- `IDialogos` (também da janela principal) mostra as mensagens das telas; nos testes ela é trocada por uma que responde sozinha.
- Só diálogos pequenos abrem janela por cima: confirmar, editar cliente/produto, reajuste de preços, pré-visualização de impressão, arquivo salvo, Verificar atualizações, Diagnóstico e Sobre.
- A janela abre maximizada; tudo cabe sem barra de rolagem em 1024×768 e 1366×768 (teste `AjusteTelaTestes.TodasAsTelasCabemSemCortarNada`). Se a janela ficar menor que o mínimo da tela, a área da direita ganha barras de rolagem em vez de cortar.
- Atalhos: Ctrl+N (orçamento novo, em qualquer tela), Ctrl+F (busca da tela; onde não há busca, abre o Histórico), Ctrl+S, Ctrl+P, Ctrl+E e Ctrl+Shift+C na tela de orçamento, Ctrl+S na tela Empresa, F1 (manual).

| Tela (menu lateral) | Função |
|---|---|
| Início | Atalhos grandes (Novo orçamento, Histórico, Clientes, Produtos) e os últimos 5 orçamentos mexidos (Enter ou clique duplo abre) |
| Novo orçamento | Cliente (buscar cadastrado ou avulso), entrega/retirada, barra "Adicionar item" + lista de itens, desconto opcional, frete, validade opcional, observações, "Mostrar CNPJ da empresa neste orçamento", totais; Salvar, Copiar imagem, PNG, PDF, Imprimir, WhatsApp, Duplicar |
| Histórico | Filtros: período, tipo de cliente (todos/cadastrado/avulso), cliente específico, com/sem frete, com/sem desconto, texto (nº/nome/telefone); abrir e duplicar (na área principal), exportar, excluir |
| Clientes | Lista com busca por nome/telefone; novo, editar, inativar; "Novo orçamento para este cliente" e "Orçamentos deste cliente" trocam a tela |
| Produtos | Lista com busca; novo, editar, inativar; preço editável na lista; reajuste |
| Empresa | Nome, subtítulo, telefone (com DDD, mesma validação dos clientes), endereço (2 linhas), CNPJ (máscara e dígitos verificadores, opcional), rodapé; pré-visualização ao vivo do cabeçalho |
| Configurações | Validade sugerida, cidade e DDD padrão, pasta de exportação, impressora, backup/restauração |

### Entrada de itens

Barra `Controles\BarraItem.cs` acima da lista: **[Produto] [− Qtd +] [Un] [Unitário] [Adicionar]**.

- Produto: busca no catálogo por nome ou código enquanto se digita; ↑/↓ e Enter ou o clique escolhem; unidade e preço são preenchidos e o cursor vai para a quantidade. Texto fora do catálogo vale como descrição livre. Esc fecha a lista; sem lista aberta, limpa a barra.
- Quantidade (`Controles\CampoQuantidade.cs`): botões − e +, teclas + e − (ou ↑/↓); aceita digitar 2,5.
- Enter avança (Produto → Qtd → Un, ou direto para o Unitário se veio do catálogo → Unitário) e no Unitário adiciona; o botão Adicionar faz o mesmo. Depois de adicionar, a barra é limpa e o cursor volta para o Produto.
- Salvar com algo digitado na barra adiciona o item antes (ou avisa o que falta).
- Lista (`Controles\GradeItens.cs`): cada linha com − e + na quantidade e a lixeira; quantidade, unidade, descrição e preço editáveis na própria lista (F2 ou digitando). Teclas + e − mudam a quantidade da linha (tratadas no caractere traduzido pelo Windows, então valem em qualquer layout de teclado e no numérico), Delete remove, Ctrl+↑/↓ ou arrastar com o mouse mudam a ordem. Mudar a descrição tira o item do catálogo (o código some).
- Cronômetro: `EntradaItensTestes.CincoItensDoCatalogoEmMenosDeUmMinuto` lança 5 itens do catálogo só pelo teclado (teclas enviadas como mensagens do Windows) e grava o resultado em `C:\dev\orcamentos-ponto-do-gesso\.tmp\cronometro-itens.txt`.

## Atualização

Ver [ATUALIZACAO.md](ATUALIZACAO.md).
