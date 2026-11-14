# Fase 10 — Polimento e versão 1.0.0

**Objetivo:** deixar o app confortável para uso diário e fechar a primeira versão estável.

## Tarefas

- [x] Atalhos: `Ctrl+N` novo orçamento, `Ctrl+S` salvar, `Ctrl+Shift+C` copiar imagem, `Ctrl+P` imprimir, `Ctrl+E` PDF, `Ctrl+F` buscar, `F1` ajuda
- [ ] Ícone definitivo do app (`assets\icone.ico`) e título das janelas
- [ ] Revisar tamanhos de fonte e telas em 1366×768 e 1024×768 (monitores comuns em PCs antigos)
- [ ] Revisar ordem do Tab em todas as telas
- [ ] Mensagens de erro em português claro, sem termos técnicos (detalhe técnico só no log)
- [ ] Tela **Sobre**: versão, link do repositório, pasta de dados, botão abrir pasta de dados
- [ ] Manual curto do usuário em `C:\dev\orcamentos-ponto-do-gesso\docs\MANUAL.md` (com prints) e link em **Ajuda**
- [ ] Revisar `CHANGELOG.md`

### Portfólio (README do GitHub)
- [ ] **Modo de demonstração**: menu **Ajuda → Dados de demonstração** cria um banco separado (`%APPDATA%\PontoDoGesso\demo.db`) com clientes, produtos e orçamentos **fictícios**, e um aviso "MODO DEMONSTRAÇÃO" no topo; **Sair da demonstração** volta ao banco real sem tocar nele. Usado para todos os prints e o GIF
- [ ] Prints em `C:\dev\orcamentos-ponto-do-gesso\docs\imagens\`: tela principal, orçamento preenchido, histórico com filtros, produtos, PNG final
- [ ] GIF curto (≤ 30 s) fazendo um orçamento do zero até **Copiar imagem**, em `C:\dev\orcamentos-ponto-do-gesso\docs\imagens\demo.gif`
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
