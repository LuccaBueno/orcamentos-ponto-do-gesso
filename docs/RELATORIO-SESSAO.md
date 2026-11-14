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

O **teste da atualização pelo botão** fica para quando a v1.0.0 for publicada (parte C). A v0.9.2 já é a última versão, então hoje o botão não teria para onde atualizar.

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

### Parte C — teste da atualização pelo botão (quando a v1.0.0 for publicada)

Fica para depois da visita: com a v0.9.2 instalada na loja e a **v1.0.0** publicada, o botão testa a atualização de verdade (v0.9.2 → v1.0.0), com o antivírus ligado e os dados reais.

1. Antes: **Configurações → Backup → Fazer backup agora** e **Copiar backup para…** um pendrive.
2. Abrir o app. Em até um dia aparece **Nova versão 1.0.0!** no rodapé (ou clicar em **Verificar atualizações**). Conferir a tela com "Versão nova: 1.0.0" → **Atualizar agora**.
3. O app tem de fechar e abrir sozinho em poucos segundos com **Versão 1.0.0** no rodapé. Clientes, produtos e orçamentos continuam lá.
4. Conferir que existem:
   - `C:\PontoDoGesso.anterior\`;
   - em `%APPDATA%\PontoDoGesso\backups\`, um `dados_..._antes-de-atualizar.db`.
5. **Se o antivírus reclamou ou bloqueou:** anote a mensagem exata (foto), aplique as exclusões da seção 5 de `C:\dev\orcamentos-ponto-do-gesso\docs\INSTALACAO-WINDOWS7.md` e repita.
6. **Atualização manual**, se nada funcionar: fechar o app, baixar o `PontoDoGesso.zip` pelo link do README (que então será a v1.0.0), extrair por cima de `C:\` e abrir de novo. Os dados não são afetados.
   - **Plano B** se a v1.0.0 não abrir: extrair a v0.9.2 (ou a v0.9.1, do pendrive) do mesmo jeito.
7. Copiar `%APPDATA%\PontoDoGesso\logs\` (inclui `atualizacao_AAAA-MM.log`) para o pendrive.
   → Resultados em `C:\dev\orcamentos-ponto-do-gesso\docs\fases\FASE-02-atualizacao.md`, nos itens "Teste real no PC da loja" e "Teste com o antivírus".

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
