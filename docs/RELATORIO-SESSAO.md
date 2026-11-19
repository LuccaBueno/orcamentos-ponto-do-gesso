# Relatório da sessão — 19/11/2026 (melhorias depois do uso real na loja)

Sessão sem acompanhamento, a partir do pedido feito depois do uso real na loja (Windows 7): janela única, tela Empresa com CNPJ, data do orçamento e itens mais rápidos de lançar. **Nenhuma versão, tag ou release foi criada**: tudo está no `main` (commits com `build`, `test` e `push` a cada etapa) e entra em [Não lançado] no `C:\dev\orcamentos-ponto-do-gesso\CHANGELOG.md`. A versão em `C:\dev\orcamentos-ponto-do-gesso\src\Directory.Build.props` continua 0.9.3.

Ao final: **506 testes passando** (eram 442 no início da sessão).

## O que mudou

| Item | O que foi feito | Onde |
|---|---|---|
| Data do orçamento | **Causa do orçamento com a data de ontem:** a tela de orçamento gravava a data uma vez só, ao abrir, e nunca mais conferia; com a tela aberta de um dia para o outro (ou aberta antes da meia-noite e exportada depois), saía a data de ontem. Não havia data guardada em campo estático nem cache em outro lugar (conferido em todo o código: o histórico, a demonstração e os backups leem o relógio a cada vez). Agora o orçamento **novo** (sem número) usa a data de hoje do Windows ao criar, ao duplicar e, se o dia virou, ao salvar, exportar e voltar para a tela, sem perguntar. Mudando a data à mão no calendário aparece **"data alterada manualmente"** e o app não mexe mais. Orçamento **já salvo** mantém a data dele ao reabrir e reexportar. | `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Servicos\DataOrcamento.cs`, `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\Telas\TelaOrcamento.cs` |
| CNPJ | Validação dos dígitos verificadores e máscara `00.000.000/0000-00`. Caixa **"Mostrar CNPJ da empresa neste orçamento"** na tela de orçamento: desmarcada em todo orçamento novo, no Ctrl+N e ao duplicar; sem CNPJ cadastrado fica desligada com "Cadastre o CNPJ na tela Empresa" ao lado. Marcada, `CNPJ: 11.222.333/0001-81` sai no cabeçalho, abaixo do endereço, na mesma fonte e cor (as três linhas dividem o espaço do endereço). Desmarcada, o cabeçalho é **pixel a pixel igual** ao de antes (há teste comparando). O orçamento guarda se mostrou e o CNPJ usado: reabrir ou reexportar um antigo sai igual mesmo se o CNPJ da empresa mudar. | `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Servicos\Cnpj.cs`, `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Exportacao\DesenhoOrcamento.cs` |
| Migração 003 | `orcamentos.mostrar_cnpj` (0 nos orçamentos antigos) e `orcamentos.empresa_cnpj`; chave `loja.cnpj` vazia. Backup automático antes (`%APPDATA%\PontoDoGesso\backups\dados_AAAA-MM-DD_HHmm.db`), como em toda migração. Os dados da loja **não foram movidos**: continuam nas mesmas chaves `loja.*` da tabela `configuracoes`, só passaram a ser editados na tela Empresa. Teste com um banco na versão 2 com dados da loja alterados e um orçamento: tudo continua igual depois da migração e o backup é criado. | `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Dados\Migracoes\003_cnpj_empresa.sql` |
| Janela única | Menu lateral fixo à esquerda com ícones (desenhados em código; não dependem de fonte de símbolos, que o Windows 7 não tem completa): Início, Novo orçamento, Histórico, Clientes, Produtos, Empresa, Configurações e, embaixo, Abrir pasta de orçamentos, Verificar atualizações e a versão. O item da tela atual fica em laranja. Cada tela virou um `UserControl` na área da direita. Trocar de tela ou fechar o app com alterações não salvas pergunta **Salvar / Não salvar / Cancelar** (orçamento, Empresa, Configurações). "Abrir" e "Duplicar" do histórico, "Novo orçamento para este cliente" e "Orçamentos deste cliente" trocam a tela. Continuam como janelas por cima só os diálogos pequenos: confirmar, editar cliente/produto, reajuste, pré-visualização de impressão, arquivo salvo, Verificar atualizações, Diagnóstico e Sobre. O botão **Voltar ao menu** e o `Esc` que fechava as janelas saíram. A janela abre **maximizada**. | `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\FormPrincipal.cs`, `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\Telas\`, `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Controles\MenuLateral.cs`, `Icones.cs` |
| Tela inicial | Atalhos grandes (Novo orçamento, Histórico, Clientes, Produtos) e os **últimos 5 orçamentos** mexidos (Enter ou clique duplo abre); sem orçamentos, mostra uma dica. | `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\Telas\TelaInicio.cs` |
| Tela Empresa | Nome, subtítulo, telefone com DDD (mesmo campo e validação dos clientes; grava `(12) 99244-5402`), endereço em 2 linhas, CNPJ (opcional, com "CNPJ válido" / "CNPJ não existe" ao lado enquanto digita) e rodapé; pré-visualização ao vivo do cabeçalho (com o CNPJ, se válido); Salvar e `Ctrl+S`. A aba Loja saiu de Configurações (que ficou com Orçamento, Exportação e Backup). | `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\Telas\TelaEmpresa.cs` |
| Barra "Adicionar item" | Acima da lista: **Itens · Produto · Qtd [− 1 +] · Un · Unitário · Adicionar**. No Produto a lista do catálogo aparece enquanto se digita (nome ou código); ↑/↓ + Enter ou o clique escolhem, unidade e preço são preenchidos e o cursor vai para a quantidade. Quantidade com − e + (passo 1, mínimo 1; teclas + e − ou ↑/↓; aceita 2,5). Enter vai para o Unitário e, de novo, adiciona; a barra limpa e o cursor volta ao Produto. Descrição livre continua valendo. Esc fecha a lista; sem lista, limpa a barra. Salvar com algo na barra adiciona antes (ou avisa o que falta). Escolher o cliente pelo teclado já leva o cursor para o Produto. | `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Controles\BarraItem.cs`, `CampoQuantidade.cs`, `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Servicos\PassoQuantidade.cs` |
| Lista de itens | Cada linha com **−** e **+** na quantidade e a **lixeira**; preço, quantidade, unidade e descrição editáveis na lista (F2 ou digitando). Teclas + e − na linha selecionada (valem no teclado numérico e no principal, em qualquer layout), Delete remove, Ctrl+↑/↓ ou **arrastar** com o mouse mudam a ordem. | `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Controles\GradeItens.cs` |
| Tela de orçamento mais compacta | Títulos Cliente/Entrega/Itens na mesma linha dos primeiros campos e a dica da lista numa linha só: em 1024×768 a lista de itens mostra ~6 linhas (no primeiro desenho com a barra mostrava só 2). | `TelaOrcamento.cs` |
| Documentação | `ARQUITETURA.md` (telas, navegação, entrada de itens, migração 003, chave `loja.cnpj`, regras de data/quantidade/CNPJ), `LAYOUT-ORCAMENTO.md` (CNPJ no cabeçalho), `MANUAL.md` (janela única, barra de itens, data, CNPJ, Empresa, atalhos), `docs\fases\README.md` e CHANGELOG. Imagens de comparação com e sem CNPJ (CNPJ fictício 11.222.333/0001-81) e os prints do README/manual regenerados só com o modo de demonstração. | `C:\dev\orcamentos-ponto-do-gesso\docs\`, `C:\dev\orcamentos-ponto-do-gesso\docs\referencia\comparacoes\cnpj\`, `C:\dev\orcamentos-ponto-do-gesso\docs\imagens\` |

## Cronômetro: 5 itens do catálogo

Teste `EntradaItensTestes.CincoItensDoCatalogoEmMenosDeUmMinuto`: lança 5 produtos do catálogo **só pelo teclado** (as teclas são enviadas como mensagens do Windows ao campo com o cursor, o mesmo caminho do teclado de verdade), um por vez: parte do nome, `Enter`, a quantidade, `Enter`, `Enter`.

- **57 teclas** (11,4 por item), nenhum clique.
- Tempo do teste automático: **0,37 s** (o computador não "pensa"; serve para provar que o caminho funciona sem passos escondidos).
- Estimativa para uma pessoa: **37 s** = 57 teclas × 0,3 s + 5 itens × 4 s para achar o produto na lista e conferir. Meta: menos de 60 s.
- Registro em `C:\dev\orcamentos-ponto-do-gesso\.tmp\cronometro-itens.txt` (regravado a cada execução).
- O mesmo orçamento **só com o mouse** (fora digitar o nome do produto) também tem teste: clique na sugestão, cliques no + e em Adicionar.

**O tempo de verdade só você pode medir** — ver "O que testar".

## Testes novos

- Data: virada de dia ao salvar e ao exportar, data manual, orçamento salvo reaberto e reexportado, duplicar (e as regras em `DataOrcamentoTestes`).
- CNPJ: validação e máscara, cabeçalho igual sem a caixa, cabeçalho com o CNPJ, cópia guardada no orçamento, migração a partir de um banco na versão 2 com backup.
- Navegação: cada botão do menu troca a tela na mesma janela e fica destacado; Salvar/Não salvar/Cancelar ao trocar de tela e ao fechar o app; Abrir/Duplicar do histórico; ações do cliente; tela inicial com os últimos 5; nenhuma tela com o Voltar ao menu.
- Tamanho: com a janela do tamanho de um monitor **1024×768 e 1366×768** (menos a barra de tarefas), cada tela cabe sem barra de rolagem, nenhum controle visível passa da borda e nenhum campo fica com o texto rolado para fora da vista (esse último pegou um problema real: a busca do cliente e o nome da empresa pareciam vazios).
- Ordem do Tab de todas as telas, incluindo a barra de itens e a tela Empresa.
- Entrada de itens pelo teclado e pelo mouse (15 testes, com mensagens do Windows de verdade). Esses rodam numa coleção sem paralelismo, porque dependem do foco do teclado e outra janela de teste abrindo ao mesmo tempo roubava o foco.
- Os testes de tela continuam falhando com a mensagem em vez de abrir janela de erro do .NET; as perguntas das telas passam pela janela principal, que nos testes responde sozinha.

## Problemas no caminho

- A busca do cliente e o nome na tela Empresa apareciam vazios: o campo era criado estreito e alargado depois, e o Windows deixava o texto rolado para a esquerda. Corrigido para todas as telas e coberto por teste.
- As teclas + e − na lista começavam a editar a descrição (a lista abre a edição já no aperto da tecla) e o "+" ia parar no texto. Corrigido olhando o caractere que o Windows traduziu para a tecla.
- Com a barra nova, a lista de itens ficou com 2 linhas em 1024×768; a tela foi compactada (ver acima).
- O teste de tamanho em 1024 falhou 1 vez em ~10 execuções da suíte: media o rótulo do subtotal antes de o layout terminar. O teste passou a esperar o layout assentar; depois disso, 4 execuções seguidas sem falha.
- O GIF do README passou 0,24 s do limite de 30 s com a barra nova; um quadro foi encurtado.

## Pendências e limites

- Nada disso foi testado num **Windows 7 de verdade**, nem em monitor físico 1024×768: o tamanho foi conferido no Windows 11 com a janela do tamanho dessas telas.
- A pergunta Salvar/Não salvar/Cancelar ao **fechar o app** tem teste automático, mas convém ver na tela.
- Arrastar com o mouse para mudar a ordem foi testado chamando o "soltar" direto (o arrastar do Windows não dá para simular em teste automático).
- Os ícones do menu são desenhos simples feitos em código; se você tiver ícones melhores, dá para trocar em `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Controles\Icones.cs`.
- "Verificar atualizações", "Diagnóstico" e "Sobre" continuam abrindo uma janela por cima (diálogos), como antes.

## O que o Lucca precisa testar

Antes de qualquer teste com os dados reais, o app vai aplicar a migração 003 e guardar um backup em `%APPDATA%\PontoDoGesso\backups\`. Para testar sem os dados reais, use **Ajuda → Dados de demonstração**.

### No seu PC

1. Compilar e abrir: `cd C:\dev\orcamentos-ponto-do-gesso` e `dotnet build PontoDoGesso.sln -c Debug`, depois abrir `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\bin\x86\Debug\net48\PontoDoGesso.exe`.
2. **Janela única:** clicar em cada item do menu lateral e conferir que a tela troca sem abrir janela nova e o item fica laranja. Num orçamento com algo digitado, clicar em outro item e conferir **Salvar / Não salvar / Cancelar**; fazer o mesmo fechando o app pelo X.
3. **Histórico:** Abrir e Duplicar um orçamento (devem aparecer na área principal). **Clientes:** "Novo orçamento para este cliente" e "Orçamentos deste cliente".
4. **Tela inicial:** os últimos 5 orçamentos aparecem e abrem com Enter ou clique duplo.
5. **Empresa:** conferir que nome, telefone, endereço e rodapé vieram iguais aos de antes. Digitar um CNPJ errado (ex.: `11.222.333/0001-82`) e ver o aviso; digitar o CNPJ certo da loja, ver a pré-visualização e Salvar.
6. **CNPJ no orçamento:** num orçamento novo a caixa vem desmarcada; marcar, exportar PNG e PDF e conferir o CNPJ abaixo do endereço; Duplicar e ver a caixa desmarcada; desmarcar num orçamento e conferir que o PNG sai igual a antes.
7. **Data:** escolher outra data no calendário e ver "data alterada manualmente". Para a virada do dia: abrir um orçamento novo à noite, deixar a tela aberta e exportar no dia seguinte — deve sair a data do dia seguinte (ou mudar a data do Windows para amanhã com a tela aberta, exportar e voltar o relógio).
8. **Itens:** **cronometrar** um orçamento de 5 itens do catálogo só pelo teclado (parte do nome, Enter, quantidade, Enter, Enter) e depois só com o mouse. Testar + e − (botões e teclas, inclusive o teclado numérico), a lixeira, Delete, arrastar uma linha e Ctrl+↑/↓.
9. Diminuir a janela (tirar do maximizado) e conferir que aparecem barras de rolagem em vez de cortar.

### Na loja (Windows 7)

1. Depois que você publicar uma versão com estas mudanças: atualizar pelo botão e conferir que os dados (inclusive os da loja, agora na tela Empresa) continuam iguais.
2. Conferir no monitor da loja que nada fica cortado com a janela maximizada (a resolução da loja ainda não foi anotada na tabela "PC da loja" do `C:\dev\orcamentos-ponto-do-gesso\docs\fases\README.md`).
3. Conferir os ícones do menu lateral e os botões − e + no Windows 7 (fontes e desenho).
4. Cronometrar um orçamento de 5 itens com quem usa o app no balcão.

# Relatório da sessão — 18/11/2026 (depois do primeiro teste na loja: instalador, pasta lib\ e Voltar ao menu)

Sessão sem acompanhamento, a partir do primeiro teste real no PC da loja (Windows 7 SP1, 64 bits): o app só abriu depois do .NET Framework 4.8, cuja instalação foi difícil; a pasta do app tinha dezenas de DLLs; e não ficou óbvio como sair da tela de orçamento. Isso ficou registrado na tabela "PC da loja" de `C:\dev\orcamentos-ponto-do-gesso\docs\fases\README.md`.

Durante a sessão nenhuma tag nem release foi criada.

**Depois da sessão (18/11/2026, a pedido do Lucca):** publicada a **v0.9.3** (https://github.com/LuccaBueno/orcamentos-ponto-do-gesso/releases/tag/v0.9.3), com tudo desta sessão e a revisão visual do orçamento (seção [0.9.3] do `C:\dev\orcamentos-ponto-do-gesso\CHANGELOG.md`). A release tem `PontoDoGesso-v0.9.3.zip`, o `.sha256`, `PontoDoGesso.zip`, `PontoDoGesso-Instalador-v0.9.3.exe` e `PontoDoGesso-Instalador.exe`, e o texto começa com "Instalação nova: baixe PontoDoGesso-Instalador.exe. Quem já tem o app: use o botão Verificar atualizações." Conferido baixando pelos links do README: `releases/latest/download/PontoDoGesso-Instalador.exe` e `releases/latest/download/PontoDoGesso.zip` entregam exatamente os arquivos da v0.9.3 (instalador versão 0.9.3, SHA-256 `cdd38695dbb92bd90662cce4acc3dc167228ca5675ad75b332d3f1396e01cc1f`; zip com o exe 0.9.3.0, a raiz organizada com `lib\` e SHA-256 `eb1063ac65712e2e7d6688957f72efb32404c82d3dea17842e964c989220bf65`, igual ao `.sha256`). O código da v0.9.2, consultando o GitHub de verdade, enxerga a v0.9.3 como versão nova. A v1.0.0 **não** foi publicada. O roteiro da loja (parte C, mais abaixo) passou a ser o teste v0.9.2 → v0.9.3. Ao final: **442 testes passando** (eram 417), CI verde gerando o pacote e o instalador a cada push.

## O que mudou

| Item | O que foi feito | Onde |
|---|---|---|
| Voltar ao menu | Botão laranja **← Voltar ao menu (Esc)** no alto do orçamento, histórico, clientes, produtos e configurações (no lugar dos botões Fechar/Cancelar). `Esc` faz o mesmo, menos quando a lista de sugestões está aberta (aí só fecha a lista). Com alterações não salvas, o orçamento e as configurações perguntam **Salvar / Não salvar / Cancelar** (janela própria, porque o MessageBox só tem Sim/Não). Ao fechar qualquer janela, inclusive pelo X, o menu volta para a frente (e sai do minimizado). | `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\Estilo.cs`, `PerguntaSalvar.cs`, `FormPrincipal.cs`, `FormConfiguracoes.cs` e as outras quatro janelas |
| Pasta `lib\` | A raiz do app fica só com `PontoDoGesso.exe`, `PontoDoGesso.exe.config`, `PontoDoGesso.Atualizador.exe` (+ `.config`) e `assets\`; as 21 DLLs vão para `lib\` e o `SQLite.Interop.dll` para `lib\x86\` (`<probing privatePath="lib"/>` no App.config). | `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\App.config`, `C:\dev\orcamentos-ponto-do-gesso\scripts\empacotar.ps1` |
| Autoteste do pacote | `PontoDoGesso.exe --autoteste [arquivo]` confere, sem janelas, SQLite, criação do banco com as migrações e PDF/PNG, e diz de onde as DLLs foram carregadas. O `empacotar.ps1` roda isso na pasta montada e falha se algo não vier de `lib\`. Contraprova: sem o `lib\x86\SQLite.Interop.dll` o autoteste falha com `DllNotFoundException`. | `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Servicos\Autoteste.cs` |
| Instalador | Inno Setup 6.7.4 (o 7.x pode não servir para o Windows 7). Instala em `C:\PontoDoGesso\` sem perguntar pasta e **sem pedir administrador** (a atualização pelo botão precisa trocar essa pasta com o usuário comum); atalhos na Área de Trabalho e no Menu Iniciar com o ícone do app; "Abrir o Ponto do Gesso" no fim; por cima de instalação antiga apaga as DLLs da raiz e a pasta `x86\`; os dados em `%APPDATA%` não são tocados. Sem .NET 4.8 mostra "Instale primeiro o .NET Framework 4.8 (versão Runtime, NÃO a 4.8.1 nem o Developer Pack)" com o link direto, oferece abrir o link e **não instala**. | `C:\dev\orcamentos-ponto-do-gesso\instalador\PontoDoGesso.iss`, `C:\dev\orcamentos-ponto-do-gesso\scripts\gerar-instalador.ps1` |
| Workflows | `release.yml` instala o Inno 6.7.4 (baixado do GitHub oficial, SHA-256 fixo e assinatura conferida) e publica também `PontoDoGesso-Instalador-vX.Y.Z.exe` e `PontoDoGesso-Instalador.exe`. `build.yml` agora empacota (com autoteste) e gera o instalador a cada push, sem publicar nada. As notas da release passam a mandar baixar o instalador. | `C:\dev\orcamentos-ponto-do-gesso\.github\workflows\`, `C:\dev\orcamentos-ponto-do-gesso\scripts\instalar-inno-ci.ps1`, `notas-versao.ps1` |
| Desinstalador e atualização | A atualização pelo botão troca a pasta inteira, o que apagaria o `unins000.exe` do instalador. O Atualizador novo leva os `unins000.*` para a pasta nova. Desinstalar remove `C:\PontoDoGesso\` e `C:\PontoDoGesso.anterior\`, nunca os dados. | `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso.Atualizador\Instalacao.cs` |
| Documentação | README com o botão para `releases/latest/download/PontoDoGesso-Instalador.exe` e os passos (.NET 4.8 só no Windows 7, baixar, abrir, Instalar, "O Windows protegeu o computador"); zip como opção avançada. `INSTALACAO-WINDOWS7.md` com o link direto (confirmado: leva ao `NDP48-x86-x64-AllOS-ENU.exe`, instalador offline de ~116 MB) e o aviso sobre 4.8.1/Developer Pack. Manual com o Voltar ao menu e o `Esc`. `ATUALIZACAO.md` com a pasta `lib\` e o instalador. | `C:\dev\orcamentos-ponto-do-gesso\README.md`, `C:\dev\orcamentos-ponto-do-gesso\docs\` |

## Testes de verdade (no seu PC, tudo dentro de `C:\dev\orcamentos-ponto-do-gesso\.tmp\`)

- **Instalador** — `C:\dev\orcamentos-ponto-do-gesso\scripts\teste-atualizacao\teste-instalador.ps1` (modo de teste: sem atalhos e sem registro em Programas e Recursos):
  1. sem .NET 4.8 (simulado): termina com código 1, não copia nada e registra o motivo no log;
  2. instalação limpa: raiz só com exe, `.config` e Atualizador, desinstalador presente, autoteste do app instalado OK;
  3. por cima da **v0.9.2 publicada** (zip baixado do GitHub, SHA-256 conferido), com dados criados por ela: nenhuma DLL na raiz, `x86\` removida, clientes e orçamentos iguais, autoteste OK.
- **Atualização pelo botão v0.9.2 → versão com `lib\`** — `C:\dev\orcamentos-ponto-do-gesso\scripts\teste-atualizacao\teste-v092-para-lib.ps1`: como a v0.9.2 só consulta o GitHub, o teste carrega **o código da própria v0.9.2** do exe publicado e faz o caminho do botão (consulta uma release num JSON local, baixa, confere o SHA-256 e chama o Atualizador da v0.9.2 com o app aberto). Resultado: o app v0.9.2 fecha, o Atualizador troca a pasta e reabre o app novo; DLLs da raiz e `x86\` somem, a v0.9.2 fica em `PontoDoGesso.anterior`, dados iguais, autoteste OK.
- **CI:** o `build.yml` instalou o Inno 6.7.4 no runner, rodou o autoteste (DLLs carregadas de `lib\`) e gerou o instalador.

## Problemas no caminho

- ⚠️ **Seu atalho "Ponto do Gesso" foi sobrescrito e depois removido.** Na primeira rodada do teste do instalador, o parâmetro de teste foi passado como `/TESTE`, mas o Inno só entende `/TESTE=1`; o instalador rodou em modo normal e **sobrescreveu os atalhos `Ponto do Gesso` que já existiam** na sua Área de Trabalho e no Menu Iniciar (`C:\Users\Lucca-PC\Desktop\Ponto do Gesso.lnk` e `...\Start Menu\Programs\Ponto do Gesso.lnk`, criados em 05/10/2026), apontando para a pasta de teste em `.tmp`, e criou um registro em Programas e Recursos. Não dá para saber para onde os atalhos originais apontavam. Para ninguém abrir uma versão de teste com os dados reais, **removi os dois atalhos** e o registro de teste. **Recrie o atalho** se ele ainda for útil. Correção: o instalador passa a aceitar `/TESTE` e `/TESTE=1`, e o roteiro de teste para tudo se o primeiro cenário (que não deveria instalar nada) instalar, ou se o log mostrar atalho criado.
- Na primeira rodada da atualização v0.9.2 → nova, o meu roteiro esperou o Atualizador (que herdou a saída do PowerShell) enquanto o Atualizador esperava o app fechar. Depois de 30 s o Atualizador da v0.9.2 desistiu sem mexer em nada, como deveria, e mostrou o aviso "não fechou a tempo". Encerrei os processos de teste e corrigi o roteiro.
- O primeiro desenho do botão usava "◄", que a Segoe UI desenha errado; trocado por "←".

## Pendências e limites

- As telas do instalador (textos em português, botão Instalar, "Abrir o Ponto do Gesso", mensagem do .NET) só foram testadas em modo silencioso, não vistas na tela.
- Atualizar pelo botão uma instalação feita pelo instalador e conferir que o desinstalador continua funcionando: coberto por teste automático do Atualizador, não por teste real.
- Depois de atualizar pelo botão, Programas e Recursos continua mostrando a versão que o instalador instalou (só o instalador atualiza esse número).
- Nada disso foi testado num Windows 7 de verdade; o Inno 6.7.4 declara Windows 7 SP1 como mínimo.

## O que o Lucca precisa testar

### No seu PC

1. Abrir o app e conferir o **← Voltar ao menu** no orçamento, histórico, clientes, produtos e configurações: clicar, apertar `Esc` e fechar pelo X; o menu deve voltar para a frente. Num orçamento com alterações, conferir a pergunta **Salvar / Não salvar / Cancelar**. Na descrição de um item, com a lista de sugestões aberta, o `Esc` deve só fechar a lista.
2. Ver as telas do instalador: `C:\dev\orcamentos-ponto-do-gesso\publicacao\PontoDoGesso-Instalador.exe` (gere antes com `powershell -ExecutionPolicy Bypass -File C:\dev\orcamentos-ponto-do-gesso\scripts\empacotar.ps1` e `powershell -ExecutionPolicy Bypass -File C:\dev\orcamentos-ponto-do-gesso\scripts\gerar-instalador.ps1`). **Atenção:** ele instala de verdade em `C:\PontoDoGesso\` e cria os atalhos; os seus dados em `%APPDATA%\PontoDoGesso\` não são tocados.
3. Recriar o atalho "Ponto do Gesso" que o teste apagou (ver "Problemas no caminho"), se ainda precisar dele.

### Na loja (Windows 7), com a v0.9.3 publicada

1. **Atualização pelo botão** a partir da v0.9.2 já instalada: Verificar atualizações → v0.9.3 → Atualizar agora. Passo a passo completo na **Parte C — teste da atualização pelo botão: v0.9.2 → v0.9.3**, mais abaixo neste arquivo. Conferir que `C:\PontoDoGesso\` ficou organizada (só o exe, os `.config`, o Atualizador, `lib\` e `assets\`) e que os dados continuam.
2. **Instalador por cima:** baixar `PontoDoGesso-Instalador.exe` pelo botão do README e instalar. Conferir: não pediu senha de administrador; aviso do SmartScreen ("Mais informações › Executar assim mesmo"); atalhos na Área de Trabalho e no Menu Iniciar com o ícone laranja; "Abrir o Ponto do Gesso" no fim; dados mantidos.
3. Ajuda → Diagnóstico → Testar banco e Testar PDF/PNG (confirmam o `lib\` no Windows 7).
4. Se possível, num outro PC com Windows 7 sem o .NET 4.8: abrir o instalador e conferir a mensagem em português com o link.
5. Anotar o antivírus e se ele reclamou do instalador na tabela "PC da loja".

# Relatório da sessão — 14/11/2026 (Fase 10: polimento para a v1.0.0)

Sessão sem acompanhamento. A Fase 10 ficou **pronta no código**, menos os dois itens que dependem da loja: a rodada de testes no Windows 7 e a publicação da **v1.0.0** (os dois ficaram desmarcados com "aguardando teste do Lucca na loja"). Nenhuma tag nem release foi criada, nada foi instalado e nenhum navegador foi aberto.

**Depois da sessão (14/11/2026, a pedido do Lucca):** publicada a **v0.9.2** (https://github.com/LuccaBueno/orcamentos-ponto-do-gesso/releases/tag/v0.9.2), com tudo desde a v0.9.1, inclusive a Fase 10. A release tem `PontoDoGesso-v0.9.2.zip`, o `.sha256` e `PontoDoGesso.zip`. O link do README (`releases/latest/download/PontoDoGesso.zip`) entrega exatamente esse zip: o arquivo foi baixado e o SHA-256 bate com o publicado, `19326367263bd28e90722f657f28b08ea6c2f3aa68b4787b64f21cb8a9d089cb`. O roteiro da visita foi refeito para instalar a v0.9.2; o teste da atualização pelo botão passa a ser v0.9.2 → v1.0.0. A v1.0.0 **não** foi publicada.

Ao final: **415 testes passando** (eram 371). Build Debug, Release e `scripts\empacotar.ps1` funcionando. O CI ficou vermelho em 6 commits seguidos por causa de um teste novo e voltou a ficar verde (ver "Correções feitas no caminho").

Antes da Fase 10: na tabela "PC da loja" de `C:\dev\orcamentos-ponto-do-gesso\docs\fases\README.md` ficou registrado **32/64 bits: 64 bits (conferido pelo Lucca em 14/11/2026)**.

## O que foi feito

| Item | Resultado | Onde |
|---|---|---|
| Atalhos | `Ctrl+N`, `Ctrl+S`, `Ctrl+Shift+C`, `Ctrl+P`, `Ctrl+E`, `Ctrl+F` e `F1`. Os botões mostram o atalho (no texto ou na dica). | `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\FormOrcamento.cs`, `FormPrincipal.cs`, `Estilo.cs` |
| Ícone e títulos | Ícone novo (laranja da loja, moldura branca, "PG" em Times New Roman), desenhado por script. Janela principal: "Ponto do Gesso – Orçamentos". | `C:\dev\orcamentos-ponto-do-gesso\assets\icone.ico`, `C:\dev\orcamentos-ponto-do-gesso\scripts\gerar-icone.ps1` |
| Telas em 1024×768 e 1366×768 | Imprimir (860 px de altura) e Histórico (1040 px de largura) não cabiam. Agora toda janela diminui para caber na área de trabalho ao abrir. | `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\AjusteTela.cs` |
| Ordem do Tab | Corrigida em Clientes, Produtos, Histórico, Reajuste e nos botões alinhados à direita. Cada janela abre com o cursor no lugar certo. | `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso.Testes\OrdemTabTestes.cs` |
| Mensagens de erro | 9 lugares mostravam o texto técnico do .NET (em inglês). Agora mostram o motivo em português; o detalhe vai para o log. | `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Servicos\MensagemErro.cs` |
| Tela Sobre | **Ajuda → Sobre**: versão, link do repositório, pasta de dados e **Abrir pasta de dados**. | `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\FormSobre.cs` |
| Modo de demonstração | **Ajuda → Dados de demonstração** recria `%APPDATA%\PontoDoGesso\demo.db` com 8 clientes, 16 produtos e 12 orçamentos inventados e mostra a faixa **MODO DEMONSTRAÇÃO**. **Sair da demonstração** volta ao banco real. | `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Servicos\Demonstracao.cs` |
| Prints e GIF | 7 prints e o `demo.gif` (28 s), todos do modo de demonstração. | `C:\dev\orcamentos-ponto-do-gesso\docs\imagens\`, gerados por `C:\dev\orcamentos-ponto-do-gesso\scripts\gerar-prints.ps1` |
| Manual | Manual curto com prints. Abre por **Ajuda → Manual do usuário** ou `F1`. | `C:\dev\orcamentos-ponto-do-gesso\docs\MANUAL.md` |
| CHANGELOG | Seções publicadas conferidas; a Fase 10 entrou em **[Não lançado]**. | `C:\dev\orcamentos-ponto-do-gesso\CHANGELOG.md` |
| README | Reorganizado para portfólio, com **Desafios técnicos** e badges. O bloco **⬇️ Baixar** ficou igual, no topo. | `C:\dev\orcamentos-ponto-do-gesso\README.md` |
| Dados reais | Conferidos todos os PNG, PDF e GIF e uma busca por telefones, CPFs e e-mails: só há dados fictícios e os dados públicos da loja. | — |

Detalhes de cada item nas Observações de `C:\dev\orcamentos-ponto-do-gesso\docs\fases\FASE-10-polimento-v1.md`.

## Decisões tomadas sozinho

- **Ordem dos itens:** o modo de demonstração foi feito antes do manual, porque os prints do manual saem dele.
- **`Ctrl+N` dentro do orçamento** transforma a mesma janela num orçamento em branco (pergunta antes se há alterações), em vez de abrir outra janela por cima. **`Ctrl+F` na tela principal** abre o Histórico (ali não há campo de busca).
- **`Ctrl+F` num orçamento avulso** volta para Cadastrado, sem apagar nome e telefone, porque a busca é de cliente cadastrado.
- **Manual no GitHub:** `F1` abre `https://github.com/LuccaBueno/orcamentos-ponto-do-gesso/blob/main/docs/MANUAL.md` no navegador, como o botão WhatsApp já faz. Não vai cópia do manual no zip. Quem acessa a internet é o navegador, não o app. Sem internet, o app mostra o endereço.
- **Ícone desenhado em código**, porque não havia arte. Se houver uma, basta trocar `C:\dev\orcamentos-ponto-do-gesso\assets\icone.ico` (mesmo nome).
- **Tamanho das janelas:** em vez de mexer em cada tela, toda janela é reduzida à área de trabalho ao abrir. Num monitor grande nada muda.
- **Demonstração:**
  - o app sempre abre no banco real;
  - a demonstração é recriada do zero a cada entrada, com datas a partir do dia;
  - backups e exportações dela vão para `demo-backups\` e `demo-exportados\`;
  - a verificação e a instalação de atualizações sempre usam o banco real, então o backup antes de atualizar é dos dados de verdade;
  - há uma trava que impede a demonstração de usar o `dados.db`;
  - os telefones fictícios ficam na faixa (12) 90000-00xx.
- **GIF sem instalar nada:** o .NET não grava GIF animado, então o projeto de testes ganhou um gravador simples (`C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso.Testes\GifAnimado.cs`). As telas são fotografadas fora da área visível, sem abrir nada na frente de ninguém.
- **Tela Diagnóstico continua técnica** de propósito: ela serve para investigar problemas.
- **Erros previstos pelo app** (backup inválido, pacote que não confere, release sem o zip, Atualizador bloqueado) passaram a ser `ErroConhecidoException` e mostram a mensagem escrita para o usuário.

## Correções feitas no caminho

- **CI vermelho em 6 commits** (de "janelas cabem em monitores 1024×768" até "garante threads no pool"). O teste `AjusteTelaTestes.TamanhoMinimoMaiorQueATelaTambemDiminui` pedia uma janela de 1024×728, mas a tela do servidor do GitHub é menor e o Windows não deixa a janela passar dela. O teste passou a usar uma área de 500×400. Só demorei a ver porque conferi o CI no meio da sessão, não a cada commit. O app não tinha problema.
- **Teste instável:** `TelaAtualizacaoTestes.HashErradoNaoFazBackupNemChamaOAtualizador` às vezes ficava parado em "Baixando…". Com mais classes de teste rodando em paralelo, o pool de threads demorava a atender o download. Agora a classe `Tela` dos testes garante no mínimo 64 threads. Depois disso: 8 execuções seguidas sem falha. **Atenção:** o commit "docs: prints e GIF do README…" foi enviado com esse teste falhando (falha intermitente). A correção veio no commit seguinte.

## Pendências

- **Rodada de testes no Windows 7 da loja e publicação da v1.0.0**: aguardando o teste do Lucca (roteiro abaixo).
- **Badges e GIF não foram vistos no navegador** (regra da sessão). Conferir na página do GitHub.
- **Prints:** a barra de título aparece com o visual de janela inativa (limitação de fotografar a janela fora da tela). O GIF não mostra o cursor piscando nem a caixa "Imagem copiada"; o passo **Copiar imagem** aparece como um quadro final com a imagem do orçamento e uma legenda.
- **Teste de 5.000 orçamentos no Windows 7** (pendência da Fase 8): resolvido na continuação da sessão. O **Diagnóstico** ganhou o botão **Testar velocidade do histórico**, que cria 5.000 orçamentos falsos num banco descartável dentro da pasta de dados, mede as mesmas 4 consultas da tela, mostra o tempo (meta: menos de 1.000 ms) e apaga tudo. O banco real não é aberto. No PC de desenvolvimento, o teste inteiro leva cerca de 1 s (quase tudo é a criação dos orçamentos); as consultas, ~65 ms. Fica faltando rodar no Windows 7 e anotar o tempo em `C:\dev\orcamentos-ponto-do-gesso\docs\fases\FASE-08-historico.md`.

## O que o Lucca precisa conferir

### No seu PC

1. Página do repositório no GitHub (`C:\dev\orcamentos-ponto-do-gesso\README.md`):
   - o GIF anda e os prints aparecem;
   - os 5 badges carregam;
   - o texto de **"O problema"** descreve bem a loja. Foi escrito a partir dos documentos (talão de papel, envio pelo WhatsApp); ajuste se precisar.
2. O ícone: `C:\dev\orcamentos-ponto-do-gesso\assets\icone.ico` (prévias em `C:\dev\orcamentos-ponto-do-gesso\.tmp\icone\icone-256.png` e `icone-16.png`). Se quiser outro, troque o arquivo ou mude as cores e o texto em `C:\dev\orcamentos-ponto-do-gesso\scripts\gerar-icone.ps1` e rode o script.
3. Abrir `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\bin\x86\Debug\net48\PontoDoGesso.exe` e testar:
   - **Ajuda → Dados de demonstração**: aparece a faixa amarela e os clientes e orçamentos inventados;
   - **Ajuda → Sair da demonstração**: os seus dados continuam iguais;
   - atalhos no orçamento: `Ctrl+N`, `Ctrl+F`, `Ctrl+E` (PDF), `Ctrl+P`, `Ctrl+Shift+C` e colar no WhatsApp Web;
   - `F1` em qualquer janela abre o manual no navegador;
   - **Ajuda → Sobre** e **Abrir pasta de dados**;
   - o Tab passa pelos campos na ordem da tela.
4. O manual `C:\dev\orcamentos-ponto-do-gesso\docs\MANUAL.md`: está claro para quem vai usar no balcão?

### Na loja (Windows 7)

**Atualizado em 14/11/2026:** a Fase 10 foi publicada na **v0.9.2** (https://github.com/LuccaBueno/orcamentos-ponto-do-gesso/releases/tag/v0.9.2), e o botão **⬇️ Baixar** do README já entrega essa versão. Não é mais preciso gerar o zip à mão.

1. Siga o "Roteiro da visita à loja", mais abaixo neste arquivo. Ele instala a v0.9.2 pelo link do README e já inclui os testes da Fase 10 (etapa 8) e a medição de velocidade (etapa 4.4).
2. Se tudo der certo, publique a v1.0.0:
   - em `C:\dev\orcamentos-ponto-do-gesso\CHANGELOG.md`, coloque as novidades em `## [1.0.0] - AAAA-MM-DD`. Se não houver mudança depois da v0.9.2, uma linha como "Primeira versão estável, igual à 0.9.2 testada na loja" basta, porque o workflow recusa seção vazia;
   - em `C:\dev\orcamentos-ponto-do-gesso\src\Directory.Build.props`, mude `<Version>` para `1.0.0`;
   - faça o commit e crie a tag:
     ```
     git tag v1.0.0
     git push origin v1.0.0
     ```
   - o workflow gera a release sozinho;
   - depois faça a parte C do roteiro (atualização pelo botão na loja, v0.9.2 → v1.0.0) e marque os itens em `C:\dev\orcamentos-ponto-do-gesso\docs\fases\FASE-10-polimento-v1.md` e `FASE-02-atualizacao.md`.

---

# Relatório da sessão — 07/11/2026 (Fase 2 e releases v0.9.0 / v0.9.1)

Sessão sem acompanhamento: Fase 2 (atualização pelo app e releases automáticas). **Releases publicadas:** v0.9.0 (versão completa, para instalar na loja) e v0.9.1 (só para testar a atualização).

- v0.9.0: https://github.com/LuccaBueno/orcamentos-ponto-do-gesso/releases/tag/v0.9.0
- **v0.9.1: https://github.com/LuccaBueno/orcamentos-ponto-do-gesso/releases/tag/v0.9.1**

Ao final: **370 testes passando** (eram 317), CI verde em todos os commits e as duas releases geradas pelo workflow, sem nenhuma correção nem tag apagada.

## O que foi feito

### Publicação
- `C:\dev\orcamentos-ponto-do-gesso\scripts\empacotar.ps1`: compila em Release e monta `publicacao\PontoDoGesso\` com exe, config, dlls, `x86\SQLite.Interop.dll` e Atualizador (sem `.pdb` e sem `x64\`). Gera `PontoDoGesso-vX.Y.Z.zip` (com a pasta `PontoDoGesso/` dentro) e o `.zip.sha256`.
- `C:\dev\orcamentos-ponto-do-gesso\.github\workflows\release.yml`: roda ao enviar uma tag `v*`. Confere a tag com a `<Version>` (`scripts\conferir-versao.ps1`), compila, testa, empacota e cria a release com o zip, o `.sha256` e a seção da versão no CHANGELOG (`scripts\notas-versao.ps1`).

### No app
- Rodapé da tela principal: **Verificar atualizações** e o aviso laranja **Nova versão X!**, que aparece sozinho quando há versão nova. A verificação automática acontece ao abrir, no máximo 1 vez a cada 24 h, em segundo plano.
- Tela **Verificar atualizações**: versão instalada, versão nova e o que mudou, com **Atualizar agora**, **Depois** e **Abrir página de downloads**. Barra de progresso e Cancelar durante o download.
- Antes de instalar, o app confere o SHA-256 (se não bater, nada muda) e faz o backup `dados_..._antes-de-atualizar.db`. Depois copia o Atualizador para a pasta temporária e fecha. Se o Atualizador tiver sido apagado ou bloqueado, avisa e abre a página de downloads.
- `PontoDoGesso.Atualizador.exe`: espera o app fechar, extrai em `<pasta>.nova`, guarda a versão atual em `<pasta>.anterior` e coloca a nova no lugar. Tenta 10 vezes se um arquivo estiver preso. Volta para a anterior se algo falhar ou se a versão nova não abrir, reabre o app e grava o log em `%APPDATA%\PontoDoGesso\logs\atualizacao_AAAA-MM.log`.
- Metadados completos nos dois exes (título, empresa, autor, copyright, versão), para reduzir alarmes falsos de antivírus.

### Testes de verdade (no PC do Lucca, tudo dentro de `C:\dev\orcamentos-ponto-do-gesso\.tmp\`)
- **Atualização v0.9.0 → v0.9.1 pela interface:** a v0.9.0 foi instalada a partir da release e um cliente e um orçamento foram gravados. Ao abrir, o rodapé mostrou **Nova versão 0.9.1!**. **Verificar atualizações** → **Atualizar agora**: em cerca de 3 s o app fechou, foi atualizado e reabriu com **Versão 0.9.1**. Cliente, orçamento (R$ 37,50) e backups continuam lá, e a v0.9.0 ficou em `PontoDoGesso.anterior`.
- **Hash errado:** zip corrompido → recusado, zip apagado, pasta do app idêntica.
- **Rollback:** pacote com um exe que não abre → o Atualizador instalou, viu que não abria, voltou a v0.9.0, reabriu e avisou.
- **Arquivo preso** (achado sem querer: o próprio script de teste travava um arquivo da pasta): o Atualizador tentou 10 vezes, desistiu sem mudar nada, reabriu a versão antiga e avisou.
- Scripts em `C:\dev\orcamentos-ponto-do-gesso\scripts\teste-atualizacao\`.

## Decisões tomadas sozinho

- **Versões v0.9.0 e v0.9.1** no lugar de v0.2.0/v0.2.1, como pedido (registrado na Fase 2 e no `docs\fases\README.md`). A v0.9.1 só muda o número da versão: o texto do rodapé já é "Versão 0.9.1".
- **JSON do GitHub** lido com o `JavaScriptSerializer`, que já vem no .NET Framework 4.8: nenhum pacote NuGet novo.
- **Backup antes de atualizar** pela API de backup do SQLite (a mesma da Fase 9), em vez da cópia simples do arquivo permitida pela fase. Se o backup falhar, a atualização é cancelada.
- **Sem internet, a verificação automática não mostra nada** e só tenta de novo no outro dia (o erro vai para o log). O botão **Verificar atualizações** mostra o motivo.
- **Rollback também quando a versão nova não abre**, além de quando a troca de pastas falha.
- **`<pasta>.anterior` fica guardada** até a próxima atualização, para voltar à mão se preciso (explicado em `docs\ATUALIZACAO.md`).
- **Notas da versão** na tela em texto simples (sem `###`, `**` e crases).
- **Scripts `.ps1` em UTF-8 com BOM:** sem o BOM, o PowerShell 5.1 mostra os acentos quebrados.
- Exclusões do antivírus em `docs\INSTALACAO-WINDOWS7.md` agora incluem `%TEMP%\PontoDoGesso-atualizador\`, de onde o Atualizador roda.

## Correções feitas no caminho

- **Testes de tela travando com a caixa "Unhandled exception" do .NET:** o erro (`Value Dispose() cannot be called while doing CreateHandle()`) vinha da continuação do download, que rodava numa thread do thread pool porque a thread do teste herdava o contexto do xunit. Os testes de tela agora falham em vez de abrir a caixa, e a thread do teste usa o contexto do WinForms. No app, as continuações voltam para a tela por `BeginInvoke` e ignoram janela já fechada. Depois disso: 25 execuções seguidas sem falha. Detalhes nas Observações da Fase 2.
- Zip com `\` nos nomes (padrão do PowerShell 5.1) → as entradas agora são montadas com `/`.

## Pendências

- Resolvido: a caixa de erro do .NET do processo de teste que travou antes da correção foi fechada. Enquanto ela esteve aberta, o build Debug ficou bloqueado e alguns commits foram compilados e testados só em Release. Depois que ela fechou, o Debug compilou e passou nos 370 testes.
- Teste no PC da loja e com o antivírus: ver o roteiro abaixo.
- O teste de 5.000 orçamentos (Fase 8) não dá para rodar na loja sem ferramentas de desenvolvimento. Na visita, só observar se o histórico responde rápido. A medição formal no Windows 7 fica para quando houver o modo de demonstração (Fase 10).

## Roteiro da visita à loja

*Atualizado em 14/11/2026 para a **v0.9.2**.*

Roteiro único, na ordem de fazer, com todos os itens "aguardando teste do Lucca" das Fases 1 a 10. Na loja se instala a **v0.9.2** pelo link do README; ela tem tudo, inclusive a Fase 10.

Primeiro vem a parte A, com **dados de teste**. Depois os dados de teste são guardados num canto e começa o uso de verdade (parte B).

O **teste da atualização pelo botão** é a parte C: a loja já está com a v0.9.2 e a **v0.9.3** foi publicada em 18/11/2026, então o botão tem para onde atualizar (v0.9.2 → v0.9.3).

### Antes de sair de casa (num PC com internet)

1. Num pendrive, salvar:
   - o **instalador offline do .NET Framework 4.8**, `ndp48-x86-x64-allos-enu.exe` (site da Microsoft, "Download .NET Framework 4.8 offline installer");
   - o `PontoDoGesso.zip` pelo botão **⬇️ Baixar** do README (https://github.com/LuccaBueno/orcamentos-ponto-do-gesso). Ele é a v0.9.2: é a cópia, caso o navegador da loja não consiga baixar;
   - o `PontoDoGesso-v0.9.2.zip.sha256`, de https://github.com/LuccaBueno/orcamentos-ponto-do-gesso/releases/tag/v0.9.2 , para conferir o arquivo. O SHA-256 da v0.9.2 é `19326367263bd28e90722f657f28b08ea6c2f3aa68b4787b64f21cb8a9d089cb`;
   - **plano B:** o `PontoDoGesso-v0.9.1.zip`, de https://github.com/LuccaBueno/orcamentos-ponto-do-gesso/releases/tag/v0.9.1 . Só serve para instalar à mão se a v0.9.2 não funcionar no Windows 7 (etapa 3.5).
2. Levar:
   - um 2º pendrive ou HD externo para a pasta extra de backup;
   - algumas folhas do talão de papel para comparar;
   - a lista de produtos e preços;
   - o celular com WhatsApp;
   - um cronômetro (o do celular serve).

### Parte A — testes com dados de teste

**1. Windows** (`C:\dev\orcamentos-ponto-do-gesso\docs\INSTALACAO-WINDOWS7.md`, seção 1)
1. Iniciar → botão direito em **Computador** → **Propriedades**. Anotar a edição (precisa ter **Service Pack 1**). Os 64 bits já foram conferidos.
2. Anotar o nome do **antivírus** instalado (ícone perto do relógio). **Não** criar exclusões ainda: os testes são com o antivírus ligado e sem exceções.
   → Resultado vai para a tabela "PC da loja" em `C:\dev\orcamentos-ponto-do-gesso\docs\fases\README.md`.

**2. .NET Framework 4.8** (seção 2 do mesmo arquivo)
1. Painel de Controle → Programas e Recursos: se já aparece "Microsoft .NET Framework 4.8", pular.
2. Senão, executar `ndp48-x86-x64-allos-enu.exe` do pendrive como administrador e reiniciar o PC.

**3. Instalar a v0.9.2 pelo link do README** (seção 3)
1. No navegador da loja, abrir https://github.com/LuccaBueno/orcamentos-ponto-do-gesso e clicar em **⬇️ Baixar o Ponto do Gesso**. Baixa o `PontoDoGesso.zip`.
   - Se o navegador não abrir o GitHub ou não baixar, use o `PontoDoGesso.zip` do pendrive. Anote o que aconteceu: é um teste da Fase 1.
2. Conferir o arquivo: copiar o zip para `C:\` e, no Prompt de Comando, rodar:
   ```
   certutil -hashfile C:\PontoDoGesso.zip SHA256
   ```
   O resultado tem de ser `19326367263bd28e90722f657f28b08ea6c2f3aa68b4787b64f21cb8a9d089cb`, o mesmo do `.sha256`.
3. Botão direito no zip → **Extrair tudo…** → `C:\`. O resultado tem de ser `C:\PontoDoGesso\PontoDoGesso.exe`, e **não** `C:\Arquivos de Programas\`.
   - Se aparecer "O Windows protegeu o computador": **Mais informações → Executar assim mesmo**.
4. Botão direito em `C:\PontoDoGesso\PontoDoGesso.exe` → **Enviar para** → **Área de trabalho (criar atalho)**. Abrir pelo atalho:
   - o rodapé tem de mostrar **Versão 0.9.2**;
   - o atalho e a barra de tarefas mostram o ícone laranja "PG".
5. **Plano B:** se a v0.9.2 não abrir ou der erro logo no começo:
   - fotografe a mensagem;
   - apague `C:\PontoDoGesso\`, extraia o `PontoDoGesso-v0.9.1.zip` do pendrive em `C:\` e siga o roteiro com ela (os passos da Fase 10, etapa 8, ficam de fora);
   - traga a pasta `%APPDATA%\PontoDoGesso\logs\`.

**4. Diagnóstico** (Fases 1, 3 e 8)
1. **Ajuda → Diagnóstico** → **Copiar informações** e colar num Bloco de Notas salvo no pendrive (`diagnostico-loja.txt`). Vai para a tabela de `C:\dev\orcamentos-ponto-do-gesso\docs\fases\FASE-01-esqueleto-win7.md`.
2. Conferir na tela: .NET **4.8**, processo **32 bits**, "Versão do banco" com **diário: wal** (Fase 3).
3. Testes do Diagnóstico:
   - **Testar banco**;
   - **Testar PDF/PNG**: os dois arquivos abrem? O PDF abre no leitor de PDF da loja?
   - **Testar conexão com GitHub**: responde OK 200?
4. **Testar velocidade do histórico** (Fase 8): anotar o tempo. A meta é menos de 1.000 ms.
5. Conferir se as janelas ficam legíveis, sem texto cortado (escala do Windows 7).

**5. Dados de teste e antivírus** (Fases 2 e 3)
1. Criar dados de teste:
   - **Clientes** → cadastrar "João Teste" (com acento), telefone `12 99999-0000`;
   - **Novo orçamento** para ele com 2 itens e **salvar**;
   - em **Clientes**, buscar `joao` (sem acento): tem de achar (funções do SQLite no Windows 7, Fase 3).
2. **Verificar atualizações** no rodapé: tem de dizer **"Você já está com a versão mais nova."** Isso confirma internet, TLS 1.2 e GitHub pelo próprio app.
   - Se der "Não foi possível falar com o GitHub", anote a mensagem. Rode o Windows Update até não haver atualizações importantes e teste de novo.
3. **Se o antivírus reclamou ou bloqueou** algo até aqui (ao extrair, abrir ou verificar):
   - anote a mensagem exata (foto com o celular);
   - aplique a seção 5 de `C:\dev\orcamentos-ponto-do-gesso\docs\INSTALACAO-WINDOWS7.md`: exclusões de `C:\PontoDoGesso\`, `%APPDATA%\PontoDoGesso\atualizacao\` e `%TEMP%\PontoDoGesso-atualizador\`.
   → Resultado no item "Teste com o antivírus" de `C:\dev\orcamentos-ponto-do-gesso\docs\fases\FASE-02-atualizacao.md`. A atualização com o antivírus ligado é testada na parte C.

**6. Exportação, impressão e WhatsApp** (Fases 1 e 7)
1. Abrir o orçamento do João Teste → **PNG** e **PDF**:
   - o PDF abre no leitor da loja?
   - as letras estão certas? Sem Calibri, o app usa Arial;
   - comparar com a folha de papel.
2. **Imprimir** (`Ctrl+P`) na impressora da loja: margens certas, nada cortado. A janela de impressão cabe na tela. Levar a folha impressa para casa.
3. Abrir o WhatsApp Web no navegador da loja (Fase 1): funciona? Anotar o navegador e a versão.
4. **WhatsApp** no orçamento: abre a conversa certa no navegador? Use o próprio celular como cliente, trocando o telefone do João Teste.
5. **Copiar imagem** (`Ctrl+Shift+C`) → `Ctrl+V` na conversa do WhatsApp Web: a imagem vai?
6. **Abrir pasta de orçamentos** → arrastar o PNG para a conversa: vai?
7. No celular, abrir a imagem recebida e dar zoom no canto de baixo: o "Nº 0001" fica legível, mas discreto?
   → Resultados em `C:\dev\orcamentos-ponto-do-gesso\docs\fases\FASE-07-exportacao-pdf-png.md` e na última linha da tabela de `FASE-01-esqueleto-win7.md`.

**7. Histórico e backup** (Fases 8 e 9)
1. Fazer mais 2 orçamentos de teste (1 avulso com frete, 1 com desconto). No **Histórico** (`Ctrl+F` na tela principal):
   - testar os filtros e clicar nos cabeçalhos;
   - a busca é rápida?
   - a janela cabe na tela?
2. **Configurações → Backup** → **Fazer backup agora** → **Copiar backup para…** o pendrive.
3. Excluir 1 orçamento de teste no Histórico e depois **Restaurar backup** com o backup recém-feito: o app reinicia e o orçamento excluído volta.
4. Tentar restaurar um arquivo qualquer (ex.: o `diagnostico-loja.txt` renomeado para `.db`): tem de recusar, com a mensagem em português.

**8. Fase 10 — acabamento**
1. Atalhos na tela de orçamento: `Ctrl+N` (orçamento em branco; pergunta se há alterações), `Ctrl+S`, `Ctrl+E` (PDF), `Ctrl+F` (vai para a busca de cliente).
2. Atalhos na tela principal: `Ctrl+N` abre um orçamento e `Ctrl+F` abre o histórico.
3. `F1` em qualquer janela abre o manual no navegador da loja.
4. Tab: na tela de orçamento e em Clientes, o cursor passa pelos campos na ordem da tela.
5. **Ajuda → Sobre** mostra **Versão 0.9.2**. **Abrir pasta de dados** abre `%APPDATA%\PontoDoGesso`.
6. **Ajuda → Dados de demonstração**:
   - aparece a faixa amarela **MODO DEMONSTRAÇÃO** e os clientes e orçamentos inventados;
   - **Ajuda → Sair da demonstração** volta, e o João Teste continua lá.
7. Mensagem em português: deixe um PDF de orçamento aberto no leitor e salve o PDF de novo. Tem de aparecer "O arquivo está aberto em outro programa…".
   → Resultados no item "Rodada de testes completa no Windows 7" de `C:\dev\orcamentos-ponto-do-gesso\docs\fases\FASE-10-polimento-v1.md`.

**9. Guardar os dados de teste**
1. Fechar o app.
2. No Explorer, colar `%APPDATA%` na barra de endereço e renomear a pasta `PontoDoGesso` para `PontoDoGesso-teste-visita`.
3. Abrir o app de novo: ele começa com banco limpo (orçamento nº 0001 de verdade).

### Parte B — começar o uso de verdade

**10. Configurações** (Fase 9)
1. **Configurações → Loja:** conferir nome, telefone, endereço e rodapé na pré-visualização.
2. **Orçamento:** validade sugerida, cidade e DDD padrão.
3. **Exportação:** pasta dos orçamentos (padrão `Documentos\Orçamentos Ponto do Gesso`) e impressora.
4. **Backup:** escolher o pendrive ou HD que vai ficar na loja como **pasta extra**.

**11. Produtos e clientes** (Fases 4 e 5)
1. **Produtos:**
   - cadastrar a lista real com **Salvar e cadastrar outro**;
   - mudar um preço direto na lista (digitar e Enter);
   - selecionar 2 produtos com Ctrl+clique e reajustar 5% (R$ 12,50 → R$ 13,13 na prévia).
2. **Clientes:**
   - cadastrar 3 clientes reais, um deles com DDD 11;
   - tentar DDD `00` (tem de recusar);
   - repetir um telefone (tem de avisar que pode ser duplicado);
   - a janela abre com o cursor na busca; Enter na busca abre o 1º, duplo clique edita e Esc fecha.

**12. Orçamentos reais** (Fase 6)
1. **Cronometrar** um orçamento de 5 itens do catálogo: meta **menos de 1 minuto**.
2. Fazer 3 orçamentos reais em paralelo com o talão:
   - um de cliente cadastrado, um avulso e um com retirada;
   - com desconto em R$ e em %;
   - com validade.
3. Cliente com observações: aparece a faixa amarela?
4. Fechar com alterações não salvas: tem de perguntar. **Duplicar** um orçamento salvo: ganha o próximo número ao salvar.
5. A tela de orçamento abre rápido no PC da loja?
6. **Clientes → Orçamentos deste cliente** mostra os dele.

**13. Antes de ir embora**
1. **Configurações → Backup → Fazer backup agora** (vai também para a pasta extra).
2. Trazer no pendrive:
   - o `diagnostico-loja.txt`;
   - a pasta `logs`;
   - fotos das mensagens de erro (se houver);
   - a folha impressa;
   - o tempo do teste de velocidade.
3. Passar os resultados para a próxima sessão, que:
   - marca os itens nos arquivos das fases (`C:\dev\orcamentos-ponto-do-gesso\docs\fases\FASE-01-esqueleto-win7.md` a `FASE-10-polimento-v1.md`);
   - preenche a tabela "PC da loja" de `C:\dev\orcamentos-ponto-do-gesso\docs\fases\README.md`.

### Parte C — teste da atualização pelo botão: v0.9.2 → v0.9.3

*Atualizado em 18/11/2026: a **v0.9.3** foi publicada (https://github.com/LuccaBueno/orcamentos-ponto-do-gesso/releases/tag/v0.9.3).* A loja está com a **v0.9.2** (descompactada do zip, com as DLLs soltas na raiz de `C:\PontoDoGesso\`). O botão testa de verdade a atualização para a v0.9.3, que muda a pasta do app para a organização nova com `lib\`, com o antivírus ligado e os dados reais. O código da v0.9.2 já foi conferido contra o GitHub: ele enxerga a v0.9.3 como versão nova.

1. **Antes:** **Configurações → Backup → Fazer backup agora** e **Copiar backup para…** um pendrive. Tirar uma foto da pasta `C:\PontoDoGesso\` (dezenas de DLLs na raiz e a pasta `x86\`) e anotar quantos orçamentos aparecem no **Histórico**.
2. Abrir o app. Em até um dia aparece **Nova versão 0.9.3!** no rodapé (ou clicar em **Verificar atualizações**). Conferir a tela com "Versão nova: 0.9.3" → **Atualizar agora**.
3. O app tem de fechar e abrir sozinho em poucos segundos com **Versão 0.9.3** no rodapé.
4. **Os dados continuam:** o Histórico mostra os mesmos orçamentos de antes; abrir um orçamento antigo, um cliente e um produto.
5. **A pasta `C:\PontoDoGesso\` ficou organizada:** na raiz só `PontoDoGesso.exe`, `PontoDoGesso.exe.config`, `PontoDoGesso.Atualizador.exe`, `PontoDoGesso.Atualizador.exe.config` e as pastas `lib\` e `assets\`. **Nenhuma DLL solta na raiz e nenhuma pasta `x86\`**; as DLLs estão em `C:\PontoDoGesso\lib\` e o `SQLite.Interop.dll` em `C:\PontoDoGesso\lib\x86\`. Tirar foto.
6. Conferir que existem:
   - `C:\PontoDoGesso.anterior\` (a v0.9.2 guardada, com as DLLs na raiz);
   - em `%APPDATA%\PontoDoGesso\backups\`, um `dados_..._antes-de-atualizar.db`.
7. Na versão nova: **Ajuda → Diagnóstico → Testar banco** e **Testar PDF/PNG** (confirmam que o app acha as DLLs em `lib\` no Windows 7); fazer um orçamento de teste e **Copiar imagem**; abrir o orçamento e sair pelo **← Voltar ao menu** e pelo `Esc`.
8. **Se o antivírus reclamou ou bloqueou:** anote a mensagem exata (foto), aplique as exclusões da seção 5 de `C:\dev\orcamentos-ponto-do-gesso\docs\INSTALACAO-WINDOWS7.md` e repita.
9. **Se o botão não funcionar:** fechar o app, baixar o `PontoDoGesso-Instalador.exe` pelo botão do README (é a v0.9.3) e instalar por cima. Os dados não são afetados. O instalador também cria os atalhos na Área de Trabalho e no Menu Iniciar e apaga as DLLs antigas da raiz.
   - **Plano B** se a v0.9.3 não abrir: `C:\PontoDoGesso.anterior\` tem a v0.9.2; ou extrair o zip da v0.9.2 (pendrive) em `C:\`.
10. Opcional, depois que o botão funcionou: rodar também o `PontoDoGesso-Instalador.exe` por cima, para ganhar os atalhos e a entrada em Programas e Recursos. Conferir que não pediu senha de administrador e que os dados continuam.
11. Copiar `%APPDATA%\PontoDoGesso\logs\` (inclui `atualizacao_AAAA-MM.log`) para o pendrive.
   → Resultados em `C:\dev\orcamentos-ponto-do-gesso\docs\fases\FASE-02-atualizacao.md`, nos itens "Teste real no PC da loja" e "Teste com o antivírus", e na tabela "PC da loja" de `C:\dev\orcamentos-ponto-do-gesso\docs\fases\README.md`.

A **v1.0.0** só sai depois desse teste.

---

# Relatório da sessão — 07/11/2026 (Fases 7, 8 e 9)

Sessão sem acompanhamento: Fases 7, 8 e 9. **Nenhuma tag nem release foi criada.** Tudo foi enviado para `main`, com o CI verde. Ao final: **317 testes passando** (eram 244), e os builds Debug e Release compilam sem avisos. O app compilado em Release abre normalmente com os dados numa pasta de teste e faz o backup automático na abertura.

## O que foi feito

### Fase 7 — Exportação (concluída no código)

- O orçamento sai no visual da folha em papel, em **A4 retrato**. O mesmo desenho gera o PNG, o PDF e a impressão.
  - Desenho: `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Exportacao\DesenhoOrcamento.cs`
  - PNG (150 dpi, 1240 px de largura): `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Exportacao\ExportadorPng.cs`
  - PDF: `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Exportacao\ExportadorPdf.cs`
  - Impressão: `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Exportacao\Impressao.cs` e a pré-visualização `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\FormImpressao.cs`
- Com muitos itens, o PDF e a impressão continuam em outra página ("continuação", "Página 1 de 2"). O PNG vira uma imagem só, mais alta.
- Na tela de orçamento, os botões **Copiar imagem**, **PNG**, **PDF**, **Imprimir** e **WhatsApp** estão funcionando. Se houver alterações, o orçamento é salvo antes, com aviso.
- Depois de salvar o arquivo, aparece uma janela com **Abrir arquivo** e **Abrir pasta**. A pasta abre com o arquivo já selecionado.
- **WhatsApp:** salva o PNG, mostra o arquivo na pasta e abre `https://wa.me/55DDDNUMERO`. A dica do botão mostra o número para conferir.
- Os arquivos ficam em `<pasta de exportação>\AAAA\MM\Orc-0123_Nome-do-Cliente_dd-MM-aaaa.png` (ou `.pdf`).
- Botão **Abrir pasta de orçamentos** na tela principal.
- **Imagens de comparação** com a folha em papel, só com dados fictícios: `C:\dev\orcamentos-ponto-do-gesso\docs\referencia\comparacoes\`
  - 3 itens;
  - 3 itens com retirada e validade;
  - 15 itens com desconto, validade e observação;
  - 20 itens com desconto;
  - 35 itens (2 páginas).

### Fase 8 — Histórico (concluída no código)

- Tela: `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\FormHistorico.cs`
- Filtros combináveis:
  - período (Hoje, 7 dias, 30 dias, este mês, mês passado, personalizado, todos);
  - cadastrado/avulso e cliente específico (autocomplete);
  - com/sem frete e com/sem desconto;
  - texto: nº do orçamento, código do cliente, nome sem acento ou telefone.
- Clique no cabeçalho ordena. Carrega 200 por vez, e o próximo lote vem sozinho ao rolar a lista.
- O rodapé mostra a quantidade e a soma de tudo o que o filtro encontrou. A validade vencida aparece em vermelho.
- Ações: Abrir, Duplicar, Copiar imagem, PNG, PDF, Imprimir e Excluir (com confirmação; o número não volta a ser usado).
- Botões **Histórico** (tela principal) e **Orçamentos deste cliente** (lista de clientes) habilitados.
- Busca com 5.000 orçamentos: ~65 ms no PC de desenvolvimento.

### Fase 9 — Configurações e backup (concluída no código)

- Tela: `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\FormConfiguracoes.cs`. Tem 4 abas:
  - **Loja**, com pré-visualização do cabeçalho;
  - **Orçamento**: validade sugerida, cidade padrão, DDD padrão e pergunta do cliente avulso;
  - **Exportação**: pasta e impressora padrão;
  - **Backup**.
- Backup: `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Servicos\Backup.cs`
  - backup automático ao abrir o app, se o último tiver mais de 24 h, guardando os últimos 30;
  - **Fazer backup agora** e **Copiar backup para…**;
  - pasta extra opcional (pendrive ou HD);
  - **Restaurar backup**: valida o arquivo, guarda os dados atuais antes e reinicia o app.
- Botão **Configurações** habilitado.

## Decisões tomadas sozinho

Todas estão registradas em "Observações" de cada fase:

- `C:\dev\orcamentos-ponto-do-gesso\docs\fases\FASE-07-exportacao-pdf-png.md`
- `C:\dev\orcamentos-ponto-do-gesso\docs\fases\FASE-08-historico.md`
- `C:\dev\orcamentos-ponto-do-gesso\docs\fases\FASE-09-configuracoes-backup.md`

As mais importantes:

1. **Telefone e Entrega/Retirada na mesma linha** do orçamento, em vez de uma linha só para Entrega/Retirada. Sobra mais espaço para itens.
2. **Retirada na loja:** as linhas de endereço continuam desenhadas, como na folha, com "Retirada na loja" no campo End:.
3. **Sem ícone do WhatsApp:** o arquivo `C:\dev\orcamentos-ponto-do-gesso\assets\icone-whatsapp.png` não existe, então o orçamento sai sem o ícone. Quando o arquivo for colocado lá, o ícone passa a sair sozinho, sem mexer no código.
4. **Exportar com alterações salva antes.** O aviso vai na mesma mensagem da exportação, para não abrir duas janelas.
5. **Copiar imagem** põe na área de transferência o bitmap e também o PNG (alguns navegadores só aceitam um dos dois).
6. **Pré-visualização de impressão própria**, em português, com **Imprimir…** (abre a janela do Windows para escolher impressora e cópias).
7. **Histórico:**
   - o período padrão é **Todos**;
   - **Orçamentos deste cliente** é um botão na lista de clientes que abre o histórico filtrado, em vez de uma aba dentro da edição do cliente;
   - os filtros ficam lembrados enquanto o app estiver aberto.
8. **Nomes dos backups:** `_auto`, `_manual`, `_copia` e `_antes-de-restaurar`. Só os `_auto` contam para o limite de 30; os outros nunca são apagados pelo app.
9. **Restaurar** copia o backup para dentro do banco atual pela API do SQLite (sem apagar arquivo com o app aberto) e reinicia o app.
10. **Novas chaves de configuração:**
    - `cliente.cidade_padrao`: a cidade, que antes era fixa no código, agora é configurável;
    - `backup.pasta_extra`.

    As duas estão documentadas em `C:\dev\orcamentos-ponto-do-gesso\docs\ARQUITETURA.md`.
11. A **pasta extra de backup** é gravada assim que é escolhida, sem precisar do botão Salvar.

## Pendências

- **Nenhuma tag/release** (v0.7.0, v0.8.0 e v0.9.0 não foram publicadas), como pedido.
- A impressão foi testada só com a pré-visualização. Nada foi impresso de verdade.
- O navegador, o Explorer e a área de transferência não foram usados nos testes: essas ações são substituídas por versões falsas.
- As Fases 1 e 2 continuam esperando o teste no PC da loja com Windows 7.

## O que o Lucca precisa testar

Abrir o app com uma pasta de dados de teste, no PowerShell:

```powershell
$env:PONTODOGESSO_DADOS = "C:\dev\orcamentos-ponto-do-gesso\.tmp\dados-teste"
& "C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\bin\x86\Debug\net48\PontoDoGesso.exe"
```

Antes, em **Configurações → Exportação**, escolher uma pasta de teste para os orçamentos (ex.: `C:\dev\orcamentos-ponto-do-gesso\.tmp\exportados`). Assim, os arquivos não vão para Documentos.

### Exportação (Fase 7)

1. Comparar as imagens de `C:\dev\orcamentos-ponto-do-gesso\docs\referencia\comparacoes\` com a folha em papel e dizer o que ajustar (tamanhos, posições, cores).
2. Se quiser o ícone do WhatsApp no cabeçalho, salvar o ícone como `C:\dev\orcamentos-ponto-do-gesso\assets\icone-whatsapp.png` e compilar de novo.
3. Fazer um orçamento e testar **PNG**, **PDF**, **Abrir arquivo** e **Abrir pasta** (o arquivo tem que vir selecionado).
4. **Copiar imagem** e colar com Ctrl+V numa conversa do WhatsApp Web.
5. **WhatsApp:** conferir a dica com o número e se a conversa certa abre no navegador.
6. **Imprimir** na impressora da loja: conferir margens e se nada fica cortado.
7. Mandar o PNG para o próprio celular e dar zoom: o "Nº 0001" tem que ficar legível, mas discreto.
8. No Windows 7:
   - conferir as fontes (sem Calibri, o app usa Arial);
   - abrir o PDF no leitor de PDF da loja.

### Histórico (Fase 8)

1. Abrir o **Histórico** e testar cada filtro e algumas combinações (ex.: avulsos + com frete + mês passado).
2. Clicar nos cabeçalhos para ordenar.
3. Na lista de clientes, **Orçamentos deste cliente**.
4. Excluir um orçamento e conferir que o próximo salvo não reaproveita o número.
5. No PC da loja, ver se a busca é rápida.

### Configurações e backup (Fase 9)

1. Mudar o rodapé da loja, ver a pré-visualização e conferir num PDF novo.
2. Mudar a cidade padrão e o DDD padrão e conferir num cliente novo.
3. **Fazer backup agora**.
4. **Copiar backup para…** um pendrive.
5. Escolher o pendrive como **pasta extra**.
6. Restaurar um backup antigo:
   - o app reinicia;
   - os orçamentos voltam como estavam;
   - existe um arquivo `_antes-de-restaurar.db` em `C:\dev\orcamentos-ponto-do-gesso\.tmp\dados-teste\backups\`.
7. Tentar restaurar um arquivo qualquer (ex.: um .txt renomeado para .db): tem que recusar.

Todos os itens que dependem disso estão desmarcados nos arquivos das fases com "aguardando teste do Lucca".

---

# Relatório da sessão — 07/11/2026 (Fases 4, 5 e 6)

Sessão sem acompanhamento: continuação das Fases 4, 5 e 6. **Nenhuma tag nem release foi criada.** Tudo foi enviado para `main`, com o CI verde em todos os commits. Ao final: 244 testes passando (eram 137), e os builds Debug e Release compilam sem avisos.

## O que foi feito

### Fase 4 — Clientes (concluída no código)

- Telefone com **DDD em campo separado**, já preenchido com o DDD padrão (`cliente.ddd_padrao`, 12) e editável.
  - Campo: `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Controles\CampoTelefone.cs`
  - Regras: `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Servicos\Telefone.cs`
- Validação: DDD da lista da Anatel (67 DDDs) e número com 8 dígitos (fixo) ou 9 começando com 9 (celular).
- Telefone gravado sempre completo, só com dígitos (ex.: `12992445402`).
- Aviso de possível duplicado ao salvar: mesmo nome, sem acento, ou mesmo telefone.
- Duplo clique abre a edição, Enter na busca abre o primeiro resultado e Esc fecha.
- Botões **Clientes** (tela principal) e **Novo orçamento para este cliente** habilitados.

### Fase 5 — Produtos (concluída no código)

- `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\FormProdutos.cs`: lista com busca. O **preço é editável direto na grade**: Enter grava e desce.
- `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\FormProdutoEditar.cs`: tem o botão **Salvar e cadastrar outro**, que mantém a unidade.
- `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\FormReajuste.cs`: reajuste em lote, em % ou R$, para cima ou para baixo, com prévia "antes → depois" e confirmação.
- `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Controles\CampoMoeda.cs`: aceita `12,5`, `12,50` e `1.250,00`.
- "Preço atualizado em": nova migração `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Dados\Migracoes\002_preco_atualizado_em.sql`.
- Botão **Produtos** habilitado.

### Fase 6 — Tela de orçamento (concluída no código)

- `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\FormOrcamento.cs`. Tem:
  - cliente cadastrado com autocomplete e **+ Novo**;
  - cliente avulso, com a pergunta "Deseja cadastrar este cliente?";
  - faixa amarela com as observações internas do cliente;
  - entrega ou retirada;
  - grade de itens pelo teclado, com autocomplete do catálogo;
  - desconto, frete e validade;
  - observações com contador;
  - Ctrl+S, aviso ao fechar com alterações não salvas e **Duplicar**.
- Peças reaproveitáveis:
  - `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Controles\ListaSugestoes.cs`: autocomplete que acha em qualquer parte do texto;
  - `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Controles\GradeItens.cs`: Enter avança, Delete remove a linha e Ctrl+↑/↓ reordena.
- Botões de exportação visíveis, mas desabilitados até a Fase 7.
- **Novo orçamento** habilitado na tela principal.

## Decisões tomadas sozinho

Todas estão registradas em "Observações" de cada fase:

- `C:\dev\orcamentos-ponto-do-gesso\docs\fases\FASE-04-clientes.md`
- `C:\dev\orcamentos-ponto-do-gesso\docs\fases\FASE-05-produtos.md`
- `C:\dev\orcamentos-ponto-do-gesso\docs\fases\FASE-06-tela-orcamento.md`

As mais importantes:

1. **Telefone inválido impede o salvamento** (em vez de só avisar). Número vazio = sem telefone.
2. **Data do preço em coluna própria** (`preco_atualizado_em`, migração 002), porque `atualizado_em` também muda quando só o nome é corrigido. Coluna documentada em `C:\dev\orcamentos-ponto-do-gesso\docs\ARQUITETURA.md`.
3. **Reajuste em lote:** com mais de um produto selecionado, reajusta só os selecionados. Com um ou nenhum, reajusta todos os que a busca está mostrando. Reajuste que deixaria preço negativo fica bloqueado.
4. **Autocomplete:** a lista abre com o primeiro resultado marcado. **Enter escolhe**; **Esc fecha mantendo o texto** (para descrição livre).
5. **Depois de escolher um produto**, a quantidade vazia vira 1 e o cursor vai para a Qtd da próxima linha.
6. **Duplicar** transforma a própria janela no orçamento novo (sem número, data de hoje), em vez de abrir outra janela.
7. **Cliente avulso:** a pergunta de cadastro aparece só no primeiro salvamento. Para desligá-la, a nova chave `orcamento.perguntar_cadastro_avulso` vale `0`; a tela para mudar a chave fica para a Fase 9.
8. **Preço vazio não é aceito** no cadastro de produto ("Informe o preço (pode ser 0,00)").

Os itens de tela da Fase 5 foram num **commit só**, porque a lista depende da edição e do reajuste para compilar.

## Correções feitas no caminho

- Nas listas de clientes e produtos, os botões (Editar, Inativar, Novo orçamento) às vezes olhavam a linha anterior. Corrigido.
- Na tela de orçamento, o GroupBox com AutoSize cortava o nome do cliente ao esconder o aviso. Trocado por blocos com título.
- Os totais também quebravam linha e ficavam fora da tela. Corrigido.
- A montagem da tela de orçamento levava ~350 ms. Com o layout suspenso durante a montagem, caiu para ~65 ms.

## Pendências

- **Nenhuma tag/release** (v0.4.0, v0.5.0 e v0.6.0 não foram publicadas), como pedido.
- Abrir a janela de orçamento ainda leva ~300 ms no PC de desenvolvimento: é o custo do WinForms para ~90 controles. No PC da loja pode ser mais lento.
- As Fases 1 e 2 continuam esperando o teste no PC da loja com Windows 7.

## O que o Lucca precisa testar

Abrir o app compilado em:

`C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\bin\x86\Debug\net48\PontoDoGesso.exe`

Para não misturar com os dados reais, dá para apontar os dados para uma pasta de teste antes de abrir, no PowerShell:

```powershell
$env:PONTODOGESSO_DADOS = "C:\dev\orcamentos-ponto-do-gesso\.tmp\dados-teste"
& "C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\bin\x86\Debug\net48\PontoDoGesso.exe"
```

### Clientes (Fase 4)

1. Cadastrar 3 clientes reais de teste, um deles com DDD 11.
2. Tentar DDD `00`: tem que recusar.
3. Cadastrar outro com o mesmo telefone: tem que avisar que pode ser duplicado.
4. Na lista: duplo clique abre a edição, Enter na busca abre o primeiro resultado e Esc fecha.

### Produtos (Fase 5)

1. Cadastrar a lista real da loja com **Salvar e cadastrar outro**.
2. Mudar um preço direto na lista (digitar e Enter).
3. Selecionar 2 produtos com Ctrl+clique e reajustar 5%. Conferir a prévia: R$ 12,50 vira R$ 13,13.

### Orçamento (Fase 6)

1. **Cronometrar** um orçamento de 5 itens do catálogo. A meta é menos de 1 minuto.
2. Fazer 3 orçamentos reais em paralelo com a folha de papel: cadastrado, avulso e retirada; com desconto em R$ e em %; com validade.
3. Conferir que a faixa amarela aparece para cliente com observações.
4. Fechar com alterações não salvas: tem que perguntar.
5. Duplicar um orçamento salvo: tem que ganhar o próximo número ao salvar.
6. Ver se a tela de orçamento abre rápido **no PC da loja**.

Todos os itens que dependem disso estão desmarcados nos arquivos das fases com "aguardando teste do Lucca".
