# Changelog

Formato baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/); versões seguem [SemVer](https://semver.org/lang/pt-BR/).

## [Não lançado]

## [0.9.8] - 2026-11-20

Correções e melhorias pedidas no uso real da v0.9.7. O banco recebe a migração 005 (com backup antes); os orçamentos existentes viram a versão 1, sem sufixo.

### Adicionado
- **Versões do orçamento:** o orçamento conta como "enviado" ao ser exportado em PNG/PDF, copiado, impresso ou mandado pelo WhatsApp. Alterar e salvar um orçamento já enviado cria a versão seguinte com o mesmo número ("0001" → "0001-2" → "0001-3"), com o aviso "Este orçamento já foi enviado; as alterações viram a versão 0001-2.". Antes de enviar, salvar só sobrescreve. As versões antigas ficam guardadas e abrem só para leitura (prévia) e para exportar de novo pelo botão **Versões** (tela de orçamento e histórico). O histórico mostra uma linha por orçamento, com o Nº da versão mais recente; o número no PNG/PDF e o nome dos arquivos usam a versão (Orc-0001-2_...). Duplicar cria um número novo.
- **Saída de material** (opcional, só no app): caixa **Controlar saídas** (desmarcada por padrão) mostra na lista as colunas **Já saiu** (com − e +, sem passar da quantidade) e **Falta**, um **Saiu tudo** por item e um geral, e o resumo "Saídas: 2 de 5 itens completos" / "Tudo saiu". Grava na hora, sem criar versão; não sai no PNG/PDF/impressão; Duplicar zera. Histórico com filtro **Saídas** (Todos / Com material faltando / Tudo saiu / Sem controle) e coluna **Saídas**.
- Observação do cadastro do cliente: botão **Adicionar ao orçamento** na faixa amarela, que acrescenta o texto às Observações (limite de 300). Sem o botão, ela continua nunca indo para o orçamento.
- Dicas (ao passar o mouse) em todos os botões de ação da tela de orçamento e do histórico.

### Alterado
- PNG e Copiar imagem terminam logo abaixo do conteúdo, com margem pequena (sem o branco até o fim do A4). PDF e impressão continuam em A4.
- Cabeçalho: o telefone fica centrado na altura da 1ª linha do endereço; o CNPJ continua alinhado com a 2ª.
- OBS e linha da validade em cinza (#595959) no PNG/PDF/impressão; o rodapé "As entregas…" não muda.

### Corrigido
- **Subtotal sem uma linha:** mudar a ordem de um item (arrastar ou Ctrl+↑/↓) tirava a linha e a punha de volta, e o recálculo feito na remoção ficava sem ela (ex.: 5 itens de R$ 810,00 mostrando subtotal R$ 750,00). Agora recalcula depois de mudar a ordem. Salvar, PNG, PDF, Imprimir, Copiar imagem e WhatsApp confirmam a célula em edição antes de calcular e gravar (o valor gravado já era recalculado ao salvar).

## [0.9.7] - 2026-11-19

### Alterado
- Cabeçalho do orçamento com CNPJ: o CNPJ sai embaixo do telefone, à esquerda, em fonte um pouco menor (Calibri 18 pt), alinhado com a 2ª linha do endereço; o endereço volta às posições de sempre. Sem CNPJ marcado, nada muda. Vale para PNG, PDF, impressão e pré-visualização.
- Menu lateral: "Empresa" virou a primeira aba de **Configurações** (Empresa, Orçamento, Exportação, Backup), com um Salvar (Ctrl+S) só para a tela toda e a pergunta Salvar/Não salvar/Cancelar ao sair com alterações. "Configurações" foi para a parte de baixo do menu, junto de "Abrir pasta de orçamentos" e "Verificar atualizações". A dica do orçamento passa a dizer "Cadastre o CNPJ em Configurações → Empresa".

### Corrigido
- Campo CNPJ mostrava vírgulas ("12,337,597/0001-43") em Windows com cultura em inglês; agora sempre mostra e grava com pontos.

## [0.9.6] - 2026-11-19

Melhorias pedidas no uso real da v0.9.5. O banco recebe a migração 004 (com backup antes).

### Adicionado
- Enter vai para o próximo campo, como o Tab, nos formulários (cliente, produto, Empresa, Configurações, cliente/entrega do orçamento). Nos diálogos, Enter no último campo salva. Observações continuam quebrando linha; a barra "Adicionar item" e as listas de sugestões não mudam.
- Orçamento: botão **Usar endereço do cadastro** e lista **Endereços usados antes** (até 10 endereços de entrega distintos dos orçamentos anteriores do cliente, mais recentes primeiro, com a data do último uso).
- Campos opcionais **Receber com** e **Telefone no local** na entrega; saem no PNG/PDF/impressão só quando preenchidos.
- Busca do histórico também encontra pelo endereço ou bairro de entrega.

### Alterado
- Ao escolher um cliente cadastrado, o orçamento preenche só nome e telefone; a entrega começa vazia (cidade padrão), porque quase toda entrega vai para uma obra. O que foi digitado à mão na entrega não é apagado ao trocar de cliente nem ao passar por Retirada.

### Corrigido
- Menu lateral: "Abrir pasta de orçamentos" saía cortado; agora quebra em duas linhas.

## [0.9.5] - 2026-11-19

Só a correção dos totais da tela de orçamento. Inclui tudo da v0.9.4: quem está na v0.9.2, v0.9.3 ou v0.9.4 atualiza pelo botão **Verificar atualizações** e continua com os mesmos clientes, produtos e orçamentos (vindo da v0.9.2 ou v0.9.3, o banco recebe a migração 003, com backup antes).

### Corrigido
- Tela de orçamento: os valores de Subtotal, Desconto, Frete e TOTAL às vezes saíam da borda direita da tela (o painel dos totais ficava com o layout velho). Os totais têm agora colunas de largura fixa.

## [0.9.4] - 2026-11-19

Melhorias pedidas depois do uso real na loja (Windows 7): uma janela só, tela Empresa com CNPJ opcional no orçamento, data do orçamento sempre certa e itens mais rápidos de lançar. O banco ganha a migração 003 (com backup antes); os dados da loja continuam onde estavam. Quem está na v0.9.2 ou v0.9.3 atualiza pelo botão **Verificar atualizações** e continua com os mesmos clientes, produtos e orçamentos.

### Adicionado
- **Janela única com menu lateral:** Início, Novo orçamento, Histórico, Clientes, Produtos, Empresa e Configurações, com ícones; embaixo, Abrir pasta de orçamentos e Verificar atualizações. O item da tela atual fica em laranja e a tela troca na área da direita, sem abrir janelas novas (só os diálogos pequenos continuam como janelas). A janela abre maximizada e cabe sem cortar nada em 1024×768 e 1366×768.
- **Tela inicial:** atalhos grandes e os últimos 5 orçamentos (Enter ou clique duplo abre).
- **Tela Empresa:** nome, subtítulo, telefone (com DDD, mesma validação dos clientes), endereço em 2 linhas, CNPJ (máscara `00.000.000/0000-00`, dígitos verificadores conferidos, opcional) e rodapé, com a pré-visualização do cabeçalho ao vivo e `Ctrl+S`.
- **"Mostrar CNPJ da empresa neste orçamento"** na tela de orçamento: desmarcada em todo orçamento novo e ao duplicar; desligada com a dica "Cadastre o CNPJ na tela Empresa" quando não há CNPJ. Marcada, o CNPJ sai no cabeçalho do PNG/PDF/impressão, abaixo do endereço, no mesmo estilo. O orçamento guarda se mostrou e qual CNPJ usou, então reabrir ou reexportar um orçamento antigo sai igual. Desmarcada, o orçamento sai exatamente como antes. Comparação em `docs\referencia\comparacoes\cnpj\`.
- **Barra "Adicionar item"** acima da lista: Produto (busca por nome ou código com sugestões; setas, Enter ou clique escolhem e preenchem unidade e preço) · Qtd com − e + (teclas + e − ou ↑/↓; aceita 2,5) · Un · Unitário · Adicionar. Enter no último campo adiciona, limpa a barra e volta para o Produto. Descrição livre continua valendo.
- **Lista de itens:** − e + na quantidade e lixeira em cada linha; teclas + e − na linha selecionada, Delete remove, Ctrl+↑/↓ ou arrastar muda a ordem; quantidade, descrição e preço editáveis na lista.
- Aviso **"data alterada manualmente"** ao lado da data quando ela é escolhida à mão.
- Migração `003_cnpj_empresa.sql`: colunas `mostrar_cnpj` e `empresa_cnpj` nos orçamentos e a chave `loja.cnpj`.
- Testes: navegação, tela Empresa, CNPJ, data do orçamento, entrada de itens pelo teclado e pelo mouse (mensagens do Windows de verdade) e o cronômetro de um orçamento de 5 itens do catálogo (57 teclas; estimativa de 37 s para uma pessoa; meta de 1 minuto).

### Alterado
- Os dados da loja saem de **Configurações** (que fica com Orçamento, Exportação e Backup) e vão para a tela **Empresa**. Ficam nas mesmas chaves do banco: nada é perdido.
- "Abrir" e "Duplicar" no histórico, "Novo orçamento para este cliente" e "Orçamentos deste cliente" trocam a tela em vez de abrir outra janela.
- Trocar de tela ou fechar o app com alterações não salvas pergunta **Salvar / Não salvar / Cancelar**.
- Escolher o cliente pelo teclado leva o cursor direto para o Produto.
- Salvar com um item digitado na barra (sem clicar em Adicionar) adiciona o item antes ou avisa o que falta.
- Tela de orçamento mais compacta (títulos na mesma linha dos campos) para sobrar altura para a lista de itens.
- Manual do usuário (`docs\MANUAL.md`), `docs\ARQUITETURA.md` e `docs\LAYOUT-ORCAMENTO.md` atualizados.

### Removido
- Botão **← Voltar ao menu** e o `Esc` que fechava as janelas (o menu lateral substitui).
- Busca do catálogo dentro da coluna Descrição da lista (passou para a barra "Adicionar item").

### Corrigido
- **Orçamento com a data de ontem:** a tela gravava a data uma vez só, ao abrir. Agora o orçamento novo (não salvo) usa a data de hoje do Windows ao criar, ao duplicar e, se o dia virou com a tela aberta, ao salvar, exportar ou voltar para a tela — a não ser que a data tenha sido mudada à mão. Orçamento já salvo mantém a data dele.
- Campos de texto que pareciam vazios ao abrir uma tela (o texto ficava rolado para fora da vista quando o campo era alargado depois de criado).

## [0.9.3] - 2026-11-18

Depois do primeiro teste no PC da loja: instalador, pasta do app organizada (DLLs em `lib\`), botão **Voltar ao menu** e o orçamento mais parecido com a folha em papel. Os dados não mudam de formato: quem está na v0.9.2 atualiza pelo botão **Verificar atualizações** e continua com os mesmos clientes, produtos e orçamentos.

### Adicionado
- **Instalador** `PontoDoGesso-Instalador.exe` (Inno Setup): instala em `C:\PontoDoGesso\` sem perguntar pasta nem pedir administrador, cria atalhos na Área de Trabalho e no Menu Iniciar com o ícone do app e oferece **Abrir o Ponto do Gesso** no fim. Confere o .NET Framework 4.8: se faltar, avisa em português ("Instale primeiro o .NET Framework 4.8 (versão Runtime, NÃO a 4.8.1 nem o Developer Pack)") com o link direto https://go.microsoft.com/fwlink/?linkid=2088631 e não instala. Instalar por cima mantém os dados e apaga as DLLs antigas da raiz. A release passa a ter `PontoDoGesso-Instalador-vX.Y.Z.exe` e a cópia com nome fixo `PontoDoGesso-Instalador.exe`.
- **← Voltar ao menu** (botão laranja no alto, ou `Esc`) no orçamento, histórico, clientes, produtos e configurações. Com alterações não salvas pergunta **Salvar / Não salvar / Cancelar**. Ao fechar qualquer janela, inclusive pelo X, o menu volta para a frente.
- `PontoDoGesso.exe --autoteste [arquivo]`: confere sem abrir janelas que o app carrega o SQLite, cria o banco e gera PDF e PNG; o empacotamento roda isso na pasta montada.

### Alterado
- **Pasta do app organizada:** a raiz de `C:\PontoDoGesso\` fica só com `PontoDoGesso.exe`, os `.config` e o Atualizador; as DLLs vão para `lib\` (e o `SQLite.Interop.dll` para `lib\x86\`). A atualização pelo botão a partir da v0.9.2 (DLLs na raiz) troca a pasta inteira e remove as DLLs antigas, sem tocar nos dados (testado de verdade com o código da v0.9.2 publicada).
- O Atualizador leva o desinstalador do instalador (`unins000.*`) para a pasta nova a cada atualização.
- README e `docs\INSTALACAO-WINDOWS7.md`: download pelo instalador, com o link direto do .NET 4.8 Runtime e o aviso sobre o 4.8.1 e o Developer Pack; o zip vira opção avançada.
- Os botões **Fechar**/**Cancelar** dessas janelas foram substituídos pelo **Voltar ao menu**.
- **Orçamento (PNG, PDF e impressão) mais parecido com a folha em papel:** cabeçalho maior com a moldura no laranja da folha e o ícone do WhatsApp, telefone e endereço em Calibri, campos na ordem da folha (Nome/Tel, End/Nº, P.Ref/Data), colunas e cabeçalho da tabela com as medidas da folha e o "As entregas…" na última linha, ao lado do TOTAL. Detalhes e o que ficou diferente de propósito em `docs\LAYOUT-ORCAMENTO.md` → "Revisão visual (18/11/2026)"; comparações lado a lado em `docs\referencia\comparacoes\lado-a-lado\`.
- Com o cabeçalho maior cabem ~21 itens na primeira página do PDF (antes ~28); o PNG continua sendo uma imagem só.

### Corrigido
- PDF e impressão perdiam o último item quando os totais não cabiam na mesma página (ex.: 25 itens com desconto e validade saíam com 24).
- Rótulos de SUBTOTAL/DESC./FRETE/TOTAL diminuem se não couberem na coluna (Arial no lugar da Calibri no Windows 7 sem Office).

## [0.9.2] - 2026-11-14

Versão para instalar na loja: tudo da v0.9.1 mais a Fase 10 (polimento). A v1.0.0 sai depois do teste no Windows 7 da loja. Os dados não mudam de formato: quem está na v0.9.0 ou v0.9.1 continua com os mesmos clientes, produtos e orçamentos.

### Adicionado
- **Atalhos:** `Ctrl+N` novo orçamento, `Ctrl+S` salvar, `Ctrl+Shift+C` copiar imagem, `Ctrl+P` imprimir, `Ctrl+E` PDF, `Ctrl+F` buscar e `F1` ajuda. Os botões mostram o atalho.
- **Ajuda → Manual do usuário** (`F1` em qualquer janela) abre o manual (`docs\MANUAL.md`) no navegador.
- **Ajuda → Diagnóstico → Testar velocidade do histórico:** mede o histórico com 5.000 orçamentos falsos num banco descartável (meta: menos de 1 s no Windows 7), sem tocar no banco real.
- **Ajuda → Sobre:** versão, link do repositório, pasta de dados e botão **Abrir pasta de dados**.
- **Ajuda → Dados de demonstração:** banco separado (`demo.db`) com clientes, produtos e orçamentos fictícios e a faixa **MODO DEMONSTRAÇÃO**; **Sair da demonstração** volta aos dados reais sem tocar neles.
- Ícone definitivo do app (`assets\icone.ico`, gerado por `scripts\gerar-icone.ps1`).
- Prints e GIF de demonstração em `docs\imagens\`, gerados com dados fictícios por `scripts\gerar-prints.ps1`.
- Manual do usuário (`docs\MANUAL.md`) com prints.
- README de portfólio: o problema, a solução, GIF, funcionalidades, stack, como instalar e rodar o código, **Desafios técnicos** e badges de CI, última versão e licença.
- Scripts dos testes reais da atualização (instalação com dados de teste, hash errado, rollback e atualização pela interface) em `scripts\teste-atualizacao\`.
- Seção **⬇️ Baixar o app** no topo do README, com link fixo para `releases/latest/download/PontoDoGesso.zip`.
- Cada release publica também `PontoDoGesso.zip`, cópia idêntica do `PontoDoGesso-vX.Y.Z.zip` (o atualizador continua usando só o zip com a versão e o `.sha256`), e o texto da release começa com "Para instalar, baixe PontoDoGesso.zip abaixo em Assets.". Adicionado também às releases v0.9.0 e v0.9.1 já publicadas.

### Alterado
- Mensagens de erro em português claro, sem o texto técnico do Windows/.NET (que continua indo para o log).
- Toda janela cabe em monitores 1024×768 e 1366×768: as maiores (Imprimir e Histórico) diminuem para caber na tela.
- Ordem do Tab segue a tela em todas as janelas; Clientes e Produtos abrem com o cursor na busca e o orçamento na busca de cliente.
- Título da janela principal: "Ponto do Gesso – Orçamentos".

## [0.9.1] - 2026-11-07

Versão de teste da atualização automática: serve para conferir que o botão **Verificar atualizações** leva a v0.9.0 para a v0.9.1 sem perder nada. Nenhuma mudança no uso do app; o rodapé passa a mostrar **Versão 0.9.1**.

### Alterado
- Só o número da versão (0.9.0 → 0.9.1).

## [0.9.0] - 2026-11-07

Primeira versão completa do **Ponto do Gesso**, para instalar no PC da loja. Roda no Windows 7 SP1 (32 e 64 bits) com o .NET Framework 4.8. Os dados ficam em `%APPDATA%\PontoDoGesso\` e nunca saem do computador.

### O que o app faz
- **Orçamentos:** cliente cadastrado (busca por código, nome ou telefone) ou avulso, entrega ou retirada, itens digitados pelo teclado com sugestões do catálogo, desconto opcional (R$ ou %, só nos produtos), frete, validade opcional e observações. Ctrl+S salva; dá para duplicar um orçamento.
- **Folha do orçamento** em A4 retrato, igual ao modelo em papel: PNG, PDF, impressão com pré-visualização, **Copiar imagem** para colar no WhatsApp Web e botão **WhatsApp**, que abre a conversa com o cliente.
- **Histórico** com filtros (período, cadastrado ou avulso, cliente, com ou sem frete, com ou sem desconto, texto), ordenação, soma dos valores e ações: abrir, duplicar, exportar, imprimir e excluir.
- **Clientes** e **produtos** com códigos próprios, busca sem acento, inativação, reajuste de preços em lote e aviso de cliente duplicado.
- **Configurações:** dados da loja no cabeçalho, validade sugerida, cidade e DDD padrão, pasta dos orçamentos e impressora.
- **Backup:** automático todo dia (guarda os últimos 30), backup na hora, cópia para pendrive, pasta extra e restauração conferida.
- **Atualização pelo próprio app:** **Verificar atualizações** no rodapé, aviso de versão nova ao abrir (no máximo 1 vez por dia), download conferido por SHA-256, backup antes de atualizar e volta automática para a versão anterior se algo der errado.
- **Ajuda → Diagnóstico** para conferir o Windows, o .NET, o banco, o PDF/PNG e a conexão com o GitHub.

### Adicionado
- Fase 2: botão **Verificar atualizações** e aviso **Nova versão X!** no rodapé da tela principal; tela com a versão instalada, a nova e o que mudou, com **Atualizar agora**, **Depois** e **Abrir página de downloads**.
- Fase 2: download do `.zip` com barra de progresso, conferência do `.zip.sha256` (se não bater, nada é alterado), backup `dados_..._antes-de-atualizar.db` e `PontoDoGesso.Atualizador.exe`, que troca a pasta do app guardando a versão anterior em `<pasta>.anterior` e volta para ela se a nova não instalar ou não abrir. Log em `%APPDATA%\PontoDoGesso\logs\atualizacao_AAAA-MM.log`.
- Fase 2: se o Atualizador for apagado ou bloqueado pelo antivírus, o app avisa e abre a página de downloads (atualização manual).
- Fase 2: releases automáticas no GitHub ao criar a tag `vX.Y.Z`: o workflow confere a tag com a versão, compila, testa, gera o zip e o `.sha256` (`scripts\empacotar.ps1`) e usa a seção da versão no CHANGELOG como texto da release.
- Fase 2: metadados completos nos exes (título, empresa, autor, copyright e versão) para reduzir alarmes falsos de antivírus.
- Fase 3: banco SQLite em `%APPDATA%\PontoDoGesso\dados.db` com migrações embutidas, backup antes de migrar e configurações padrão.
- Fase 3: domínio, repositórios, regras de cálculo, formatação pt-BR e busca sem acento por nome, telefone e código.
- Fase 3: Diagnóstico mostra a versão do banco e o modo do diário; o teste de banco passa pelo Dapper (travado na 2.1.35).
- Fase 4: cadastro de clientes com busca enquanto digita (código, nome ou telefone, sem acento), inativar/reativar, telefone com DDD separado e validado (lista da Anatel), aviso de possível duplicado e botão **Clientes** habilitado na tela principal.
- Fase 5: catálogo de produtos com busca, preço editável direto na lista, "Salvar e cadastrar outro", reajuste em lote (% ou R$, com prévia e confirmação), data da última mudança de preço (migração 002) e botão **Produtos** habilitado.
- Fase 6: tela de orçamento com cliente cadastrado (autocomplete por código, nome ou telefone, "+ Novo", aviso das observações internas) ou avulso (pergunta se quer cadastrar), entrega ou retirada, grade de itens pelo teclado com autocomplete do catálogo, desconto opcional (R$ ou %), frete, validade opcional, observações, Ctrl+S, aviso de alterações não salvas e Duplicar. Botões **Novo orçamento** (tela principal) e **Novo orçamento para este cliente** (clientes) habilitados; exportação continua desabilitada até a Fase 7.
- Fase 7: orçamento no visual da folha em papel (A4 retrato) em PNG (150 dpi), PDF e impressão com pré-visualização; Copiar imagem para colar no WhatsApp Web; WhatsApp abre a conversa e mostra o PNG na pasta; arquivos em `<pasta>\AAAA\MM\Orc-0123_Nome_dd-MM-aaaa.png`; botão **Abrir pasta de orçamentos** na tela principal.
- Fase 8: histórico de orçamentos com filtros combináveis (período, cadastrado/avulso, cliente específico, com/sem frete, com/sem desconto, texto por nº, código, nome ou telefone), ordenação pelo cabeçalho, carregamento aos poucos, quantidade e soma no rodapé, validade vencida em vermelho e ações Abrir, Duplicar, Copiar imagem, PNG, PDF, Imprimir e Excluir. Botões **Histórico** (tela principal) e **Orçamentos deste cliente** (clientes) habilitados.
- Fase 9: configurações em abas — dados da loja com pré-visualização do cabeçalho, validade sugerida, cidade e DDD padrão, pergunta do cliente avulso, pasta de exportação e impressora padrão. Backup automático diário ao abrir (API de backup do SQLite, guarda os últimos 30), **Fazer backup agora**, **Copiar backup para…**, pasta extra opcional e **Restaurar backup** validado (guarda os dados atuais antes e reinicia o app). Botão **Configurações** habilitado.

## [0.1.0] - 2026-11-06

Versão de teste de compatibilidade: ainda não faz orçamentos.

### Adicionado
- Fase 1: janela principal com os botões ainda desabilitados, cultura pt-BR fixa, manifesto com suporte ao Windows 7 e tratamento global de erros com log em `%APPDATA%\PontoDoGesso\logs\`.
- Fase 1: menu Ajuda → Diagnóstico com dados do sistema e os testes de banco, PDF/PNG e conexão com o GitHub.
- Fase 1: versões travadas de `System.Data.SQLite.Core` 1.0.119 e `PDFsharp-GDI` 6.2.4 (última versão MIT do PDFsharp).
- Fase 0: solução `PontoDoGesso.sln`, projetos net48 x86 (app, atualizador e testes) e CI no GitHub Actions.
- Kit inicial: README, CLAUDE.md, arquitetura, layout do orçamento, atualização, instalação no Windows 7 e plano de 11 fases.
- Planejamento: desconto opcional (R$ ou %), validade opcional por orçamento e filtros do histórico (data, cadastrado/avulso, frete, desconto).
- Planejamento: orçamento em A4 retrato (sem 2 vias), foco digital e botão Imprimir com pré-visualização.
- Planejamento: número do orçamento discreto no canto inferior da imagem/PDF.
- Planejamento: códigos sequenciais para clientes e produtos cadastrados, usados só dentro do app (não saem no orçamento).
- Planejamento: produtos sem planilha — edição de preço na grade, cadastro em sequência e reajuste em lote dentro do app.
- Nome do app definido: **Ponto do Gesso** (repositório `LuccaBueno/orcamentos-ponto-do-gesso`, nome pensado para o portfólio; executável `PontoDoGesso.exe`).
- Planejamento: botão Abrir conversa no WhatsApp, orientações e testes de antivírus, modo de demonstração e README de portfólio; licença MIT.
