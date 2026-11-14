# Fase 10 — Polimento e versão 1.0.0

**Objetivo:** deixar o app confortável para uso diário e fechar a primeira versão estável.

## Tarefas

- [x] Atalhos: `Ctrl+N` novo orçamento, `Ctrl+S` salvar, `Ctrl+Shift+C` copiar imagem, `Ctrl+P` imprimir, `Ctrl+E` PDF, `Ctrl+F` buscar, `F1` ajuda
- [x] Ícone definitivo do app (`assets\icone.ico`) e título das janelas
- [x] Revisar tamanhos de fonte e telas em 1366×768 e 1024×768 (monitores comuns em PCs antigos)
- [x] Revisar ordem do Tab em todas as telas
- [x] Mensagens de erro em português claro, sem termos técnicos (detalhe técnico só no log)
- [x] Tela **Sobre**: versão, link do repositório, pasta de dados, botão abrir pasta de dados
- [ ] Manual curto do usuário em `C:\dev\orcamentos-ponto-do-gesso\docs\MANUAL.md` (com prints) e link em **Ajuda**
- [ ] Revisar `CHANGELOG.md`

### Portfólio (README do GitHub)
- [x] **Modo de demonstração**: menu **Ajuda → Dados de demonstração** cria um banco separado (`%APPDATA%\PontoDoGesso\demo.db`) com clientes, produtos e orçamentos **fictícios**, e um aviso "MODO DEMONSTRAÇÃO" no topo; **Sair da demonstração** volta ao banco real sem tocar nele. Usado para todos os prints e o GIF
- [x] Prints em `C:\dev\orcamentos-ponto-do-gesso\docs\imagens\`: tela principal, orçamento preenchido, histórico com filtros, produtos, PNG final
- [x] GIF curto (≤ 30 s) fazendo um orçamento do zero até **Copiar imagem**, em `C:\dev\orcamentos-ponto-do-gesso\docs\imagens\demo.gif`
- [ ] README reorganizado para portfólio: o problema (talão de papel), a solução, GIF no topo, funcionalidades, stack, como instalar, como rodar o código
- [ ] Seção **Desafios técnicos** no README:
  - compatibilidade com Windows 7 (escolha de .NET Framework 4.8 em vez de Flutter/Electron/.NET 8)
  - TLS 1.2 no Windows 7 para falar com a API do GitHub
  - dinheiro em centavos (`long`) e arredondamento comercial
  - snapshot dos dados do cliente/produto no orçamento
  - atualização automática com verificação SHA-256 e rollback
  - um único código de desenho gerando PDF, PNG e impressão
- [ ] Badges: CI, última versão, licença
- [ ] Conferir que nenhum print, GIF ou arquivo do repositório tem dado real de cliente
- [ ] Rodada de testes completa no Windows 7 da loja e no Windows 10/11
- [ ] Publicar **v1.0.0**

## Ideias para depois da v1.0 (não fazer agora)

- Converter orçamento em pedido/venda
- Controle de estoque
- Enviar direto pelo WhatsApp
- Desconto por item
- Relatórios (orçamentos por período, produtos mais orçados)

## Pronto quando

v1.0.0 publicada e a loja usando o app no lugar do talão de papel por uma semana sem problemas.

## Observações


- **Atalhos (14/11/2026):** na tela de orçamento, `Ctrl+N` transforma a mesma janela num orçamento em branco (pergunta antes se há alterações), `Ctrl+F` vai para a busca de cliente (num avulso, volta para Cadastrado sem apagar nome e telefone) e `Ctrl+Shift+C`/`Ctrl+P`/`Ctrl+E` salvam antes, se preciso, como os botões (que mostram o atalho na dica). Na tela principal, `Ctrl+N` abre um orçamento novo e `Ctrl+F` abre o Histórico (os dois botões mostram o atalho). Em Clientes, Produtos e Histórico, `Ctrl+F` vai para o campo de busca. `F1` em qualquer janela (e **Ajuda → Manual do usuário**) abre o manual no GitHub pelo navegador, como o botão WhatsApp já faz: o app em si continua sem chamar a internet, quem abre é o navegador.
- **Ícone (14/11/2026):** não havia arte pronta, então o ícone definitivo é desenhado em código por `C:\dev\orcamentos-ponto-do-gesso\scripts\gerar-icone.ps1`: quadrado arredondado no laranja da loja, moldura branca fina (como a do orçamento) e "PG" em Times New Roman, a fonte do título da loja. Tamanhos 16, 24, 32 e 48 em bitmap e 256 em PNG. Saiu o provisório `assets\icone-app.ico`; os dois exes usam `C:\dev\orcamentos-ponto-do-gesso\assets\icone.ico`. Se o Lucca tiver uma arte, basta trocar o arquivo (mesmo nome). Títulos: a janela principal passou a "Ponto do Gesso – Orçamentos"; as outras já tinham títulos claros ("Clientes", "Histórico de orçamentos", "Orçamento Nº 0001"…) e ficaram como estavam.
- **Telas em 1366×768 e 1024×768 (14/11/2026):** com a barra de tarefas do Windows 7 sobram cerca de 728 px de altura. Dois problemas encontrados: a tela **Imprimir** (760×860) passava da altura da tela e o **Histórico** (1040 de largura) passava da largura de um monitor 1024. Em vez de mudar tamanho por tela, toda janela agora é reduzida ao tamanho da área de trabalho ao abrir e empurrada para dentro dela (`C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\AjusteTela.cs`); num monitor maior nada muda. As outras telas cabem (a de orçamento fica com 936×719). Fontes: Segoe UI 9,5 nas telas e 10–13 na principal, legíveis nos dois tamanhos; ficaram como estavam. Prints conferidos com `PONTODOGESSO_CAPTURA=1` em `C:\dev\orcamentos-ponto-do-gesso\.tmp\telas\tela-1024-*.png`.
- **Ordem do Tab (14/11/2026):** em Clientes, Produtos, Histórico e Reajuste a lista vinha antes da busca/filtros (o WinForms segue a ordem de inclusão, que depende do Dock); e nas janelas com botões alinhados à direita (editar cliente, editar produto, Configurações, Reajuste) o Tab passava pelos botões da direita para a esquerda. Agora tudo segue a tela, de cima para baixo e da esquerda para a direita. Cursor ao abrir: Clientes e Produtos na busca (é só digitar), Histórico na lista (setas e Enter), orçamento na busca de cliente (a data continua primeira no Tab, mas quase nunca muda). Testes em `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso.Testes\OrdemTabTestes.cs`.
- **Mensagens de erro (14/11/2026):** em 9 lugares a tela mostrava o texto técnico do .NET (em inglês, ex.: "The process cannot access the file…"). Agora passam por `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Servicos\MensagemErro.cs`, que troca pelo motivo em português ("O arquivo está aberto em outro programa…", "Sem conexão com a internet.", "O disco está cheio."…); o detalhe técnico continua indo para o log. Erros previstos pelo próprio app (backup inválido, pacote que não confere, resposta do GitHub sem o .zip, Atualizador bloqueado) são `ErroConhecidoException` e mostram a mensagem que já foi escrita para o usuário. A tela **Diagnóstico** continua técnica de propósito: é para investigar problemas.
- **Tela Sobre (14/11/2026):** em **Ajuda → Sobre**: versão, link do repositório (abre no navegador), pasta de dados num campo só de leitura (dá para copiar) e o botão **Abrir pasta de dados**, além de licença e copyright (lido do próprio exe).
- **Ordem dos itens (14/11/2026):** o modo de demonstração foi feito antes do manual, porque os prints do manual saem dele.
- **Modo de demonstração (14/11/2026):** **Ajuda → Dados de demonstração** recria do zero `%APPDATA%\PontoDoGesso\demo.db` (8 clientes, 16 produtos e 12 orçamentos inventados, com datas contadas a partir do dia, para os filtros do histórico funcionarem) e as telas passam a usar esse banco. A tela principal mostra a faixa amarela **MODO DEMONSTRAÇÃO** e o título ganha "– DEMONSTRAÇÃO". **Sair da demonstração** volta ao banco real, que nunca é aberto pela demonstração (há uma trava contra usar o `dados.db`). Decisões: o app sempre abre no banco real (a demonstração não fica ligada depois de fechar); backups da demonstração vão para `demo-backups\` e as exportações para `demo-exportados\`, para não se misturarem com os orçamentos de verdade; a verificação e a instalação de atualizações sempre usam o banco real (o backup antes de atualizar é dos dados reais). Telefones fictícios na faixa (12) 90000-00xx. Código em `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Servicos\Demonstracao.cs`.
- **Prints e GIF (14/11/2026):** gerados por `powershell -ExecutionPolicy Bypass -File C:\dev\orcamentos-ponto-do-gesso\scripts\gerar-prints.ps1`, que roda `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso.Testes\PrintsReadme.cs` (só funciona com a variável `PONTODOGESSO_PRINTS`; no CI não faz nada). Ele cria um banco de demonstração dentro de `.tmp\`, abre as telas fora da área visível e fotografa cada uma (`DrawToBitmap`): `principal.png` (com a faixa MODO DEMONSTRAÇÃO), `orcamento.png`, `historico.png` (últimos 30 dias, com frete), `produtos.png`, `clientes.png`, `configuracoes.png` e `orcamento-final.png` (a imagem que o cliente recebe, reduzida para 800 px). O GIF (`demo.gif`, 28 s, 673×557, ~240 KB) faz um orçamento do zero: busca do cliente, três itens pelo nome, frete, desconto, Salvar e a imagem final com a legenda "Copiar imagem… colar no WhatsApp". Como o .NET não grava GIF animado, o próprio projeto de testes tem um gravador simples (`GifAnimado.cs`: paleta de 256 cores e só a parte que mudou em cada quadro) — nada foi instalado. Limitações: a barra de título sai com a aparência de janela inativa e o cursor piscando não aparece; o clique em **Copiar imagem** é mostrado pelo quadro final (a área de transferência não aparece em print).
