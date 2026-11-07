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

- **Uma caixa de erro do .NET pode ter ficado aberta no PC** ("Microsoft .NET Framework – Unhandled exception"), do processo de teste `testhost.net48.x86` (PID 7888) que travou antes da correção. A sessão não teve permissão para encerrá-lo. Enquanto ele estiver aberto, o build **Debug** falha (arquivo em uso); por isso os últimos commits foram compilados e testados em **Release**. Basta clicar em **Fechar** na caixa (ou encerrar o `testhost.net48.x86` no Gerenciador de Tarefas).
- Teste no PC da loja e com o antivírus: ver o roteiro abaixo.
- O teste de 5.000 orçamentos (Fase 8) não dá para rodar na loja sem ferramentas de desenvolvimento. Na visita, só observar se o histórico responde rápido. A medição formal no Windows 7 fica para quando houver o modo de demonstração (Fase 10).

## Roteiro da visita à loja

Roteiro único, na ordem de fazer, com todos os itens "aguardando teste do Lucca" das Fases 1 a 9. Primeiro tudo com **dados de teste** (parte A); depois os dados de teste são guardados num canto e começa o uso de verdade (parte B).

### Antes de sair de casa (num PC com internet)

1. Fechar a caixa de erro do .NET que ficou aberta neste PC (ver Pendências).
2. Num pendrive, salvar:
   - o **instalador offline do .NET Framework 4.8**, `ndp48-x86-x64-allos-enu.exe` (site da Microsoft, "Download .NET Framework 4.8 offline installer");
   - `PontoDoGesso-v0.9.0.zip` e `PontoDoGesso-v0.9.0.zip.sha256`, de https://github.com/LuccaBueno/orcamentos-ponto-do-gesso/releases/tag/v0.9.0 ;
   - `PontoDoGesso-v0.9.1.zip`, de https://github.com/LuccaBueno/orcamentos-ponto-do-gesso/releases/tag/v0.9.1 (só para o caso de a atualização pelo botão não funcionar).
3. Levar: um 2º pendrive ou HD externo para a pasta extra de backup; algumas folhas do talão de papel para comparar; a lista de produtos e preços; o celular com WhatsApp; um cronômetro (o do celular serve).

### Parte A — testes com dados de teste

**1. Windows** (`C:\dev\orcamentos-ponto-do-gesso\docs\INSTALACAO-WINDOWS7.md`, seção 1)
1. Iniciar → botão direito em **Computador** → **Propriedades**. Anotar a edição (precisa ter **Service Pack 1**) e se é 32 ou 64 bits.
2. Anotar o nome do **antivírus** instalado (ícone perto do relógio). **Não** criar exclusões ainda: o teste da etapa 5 é com o antivírus ligado e sem exceções.
   → Resultado vai para a tabela "PC da loja" em `C:\dev\orcamentos-ponto-do-gesso\docs\fases\README.md`.

**2. .NET Framework 4.8** (seção 2 do mesmo arquivo)
1. Painel de Controle → Programas e Recursos: se já aparece "Microsoft .NET Framework 4.8", pular.
2. Senão, executar `ndp48-x86-x64-allos-enu.exe` do pendrive como administrador e reiniciar o PC.

**3. Instalar a v0.9.0** (seção 3)
1. Copiar `PontoDoGesso-v0.9.0.zip` e o `.sha256` para `C:\` e conferir o arquivo: no Prompt de Comando, `certutil -hashfile C:\PontoDoGesso-v0.9.0.zip SHA256`. O resultado tem de ser igual ao texto do `.sha256` (aberto no Bloco de Notas).
2. Botão direito no zip → **Extrair tudo…** → `C:\`. O resultado tem de ser `C:\PontoDoGesso\PontoDoGesso.exe`, e **não** `C:\Arquivos de Programas\`.
3. Botão direito em `C:\PontoDoGesso\PontoDoGesso.exe` → Enviar para → Área de trabalho (criar atalho). Abrir pelo atalho.
4. Se aparecer **Nova versão 0.9.1!** no rodapé, ótimo: a internet e o GitHub funcionam. **Ainda não clicar.**

**4. Diagnóstico** (Fases 1 e 3)
1. **Ajuda → Diagnóstico** → **Copiar informações** e colar num Bloco de Notas salvo no pendrive (`diagnostico-loja.txt`). Vai para a tabela de `C:\dev\orcamentos-ponto-do-gesso\docs\fases\FASE-01-esqueleto-win7.md`.
2. Conferir na tela: .NET **4.8**, processo **32 bits**, "Versão do banco" com **diário: wal** (Fase 3).
3. **Testar banco**, **Testar PDF/PNG** (os dois arquivos abrem? o PDF abre no leitor de PDF da loja?) e **Testar conexão com GitHub** (OK 200?).
4. Conferir se as janelas ficam legíveis, sem texto cortado (escala do Windows 7).

**5. Atualização pelo botão, com o antivírus ligado** (Fase 2)
1. Criar dados de teste: **Clientes** → cadastrar "João Teste" (com acento), telefone `12 99999-0000`. **Novo orçamento** para ele com 2 itens e **salvar**. Em **Clientes**, buscar `joao` (sem acento): tem de achar (funções do SQLite no Windows 7, Fase 3).
2. Clicar em **Nova versão 0.9.1!** (ou **Verificar atualizações**) → conferir a tela com "Versão nova: 0.9.1" → **Atualizar agora**.
3. O app tem de fechar e abrir sozinho em poucos segundos com **Versão 0.9.1** no rodapé. Conferir que o João Teste e o orçamento continuam lá.
4. Conferir que existem `C:\PontoDoGesso.anterior\` e, em `%APPDATA%\PontoDoGesso\backups\` (colar esse caminho na barra do Explorer), um `dados_..._antes-de-atualizar.db`.
5. **Se o antivírus reclamou ou bloqueou:** anotar a mensagem exata (foto com o celular), aplicar a seção 5 de `C:\dev\orcamentos-ponto-do-gesso\docs\INSTALACAO-WINDOWS7.md` (exclusões de `C:\PontoDoGesso\`, `%APPDATA%\PontoDoGesso\atualizacao\` e `%TEMP%\PontoDoGesso-atualizador\`) e repetir. Se nada funcionar: fechar o app, extrair `PontoDoGesso-v0.9.1.zip` do pendrive por cima de `C:\` e abrir de novo (atualização manual; os dados não são afetados).
6. **Se o GitHub não respondeu** (etapa 4.3 falhou): rodar o Windows Update até não haver atualizações importantes e testar de novo; se continuar, usar a atualização manual do item anterior.
7. Copiar `%APPDATA%\PontoDoGesso\logs\` inteiro para o pendrive.
   → Resultados em `C:\dev\orcamentos-ponto-do-gesso\docs\fases\FASE-02-atualizacao.md` (itens "Teste real no PC da loja" e "Teste com o antivírus").

**6. Exportação, impressão e WhatsApp** (Fases 1 e 7)
1. Abrir o orçamento do João Teste → **PNG** e **PDF**: o PDF abre no leitor da loja? As letras estão certas (sem Calibri o app usa Arial)? Comparar com a folha de papel.
2. **Imprimir** na impressora da loja: margens certas, nada cortado. Levar a folha impressa para casa.
3. Abrir o WhatsApp Web no navegador da loja (Fase 1): funciona? Anotar o navegador e a versão.
4. **WhatsApp** no orçamento: abre a conversa certa no navegador? (Usar o próprio celular como cliente, trocando o telefone do João Teste.)
5. **Copiar imagem** → Ctrl+V na conversa do WhatsApp Web: a imagem vai?
6. **Abrir pasta de orçamentos** → arrastar o PNG para a conversa: vai?
7. No celular, abrir a imagem recebida e dar zoom no canto de baixo: o "Nº 0001" fica legível, mas discreto?
   → Resultados em `C:\dev\orcamentos-ponto-do-gesso\docs\fases\FASE-07-exportacao-pdf-png.md` e na última linha da tabela de `FASE-01-esqueleto-win7.md`.

**7. Histórico e backup** (Fases 8 e 9)
1. Fazer mais 2 orçamentos de teste (1 avulso com frete, 1 com desconto). **Histórico**: testar os filtros e clicar nos cabeçalhos; a busca é rápida?
2. **Configurações → Backup** → **Fazer backup agora** → **Copiar backup para…** o pendrive.
3. Excluir 1 orçamento de teste no Histórico e depois **Restaurar backup** com o backup recém-feito: o app reinicia e o orçamento excluído volta.
4. Tentar restaurar um arquivo qualquer (ex.: o `diagnostico-loja.txt` renomeado para `.db`): tem de recusar.

**8. Guardar os dados de teste**
1. Fechar o app.
2. No Explorer, colar `%APPDATA%` na barra de endereço e renomear a pasta `PontoDoGesso` para `PontoDoGesso-teste-visita`.
3. Abrir o app de novo: ele começa com banco limpo (orçamento nº 0001 de verdade).

### Parte B — começar o uso de verdade

**9. Configurações** (Fase 9)
1. **Configurações → Loja:** conferir nome, telefone, endereço e rodapé na pré-visualização.
2. **Orçamento:** validade sugerida, cidade e DDD padrão.
3. **Exportação:** pasta dos orçamentos (padrão `Documentos\Orçamentos Ponto do Gesso`) e impressora.
4. **Backup:** escolher o pendrive ou HD que vai ficar na loja como **pasta extra**.

**10. Produtos e clientes** (Fases 4 e 5)
1. **Produtos:** cadastrar a lista real com **Salvar e cadastrar outro**; mudar um preço direto na lista (digitar e Enter); selecionar 2 produtos com Ctrl+clique e reajustar 5% (R$ 12,50 → R$ 13,13 na prévia).
2. **Clientes:** cadastrar 3 clientes reais, um deles com DDD 11; tentar DDD `00` (tem de recusar); repetir um telefone (tem de avisar que pode ser duplicado); na lista, duplo clique edita, Enter na busca abre o 1º e Esc fecha.

**11. Orçamentos reais** (Fase 6)
1. **Cronometrar** um orçamento de 5 itens do catálogo: meta **menos de 1 minuto**.
2. Fazer 3 orçamentos reais em paralelo com o talão: cadastrado, avulso e retirada; com desconto em R$ e em %; com validade.
3. Cliente com observações: aparece a faixa amarela?
4. Fechar com alterações não salvas: tem de perguntar. **Duplicar** um orçamento salvo: ganha o próximo número ao salvar.
5. A tela de orçamento abre rápido no PC da loja?
6. **Clientes → Orçamentos deste cliente** mostra os dele.

**12. Antes de ir embora**
1. **Configurações → Backup → Fazer backup agora** (vai também para a pasta extra).
2. Trazer no pendrive: `diagnostico-loja.txt`, a pasta `logs`, fotos das mensagens de erro (se houver) e a folha impressa.
3. Passar os resultados para a próxima sessão, que marca os itens nos arquivos das fases (`C:\dev\orcamentos-ponto-do-gesso\docs\fases\FASE-01-esqueleto-win7.md` a `FASE-09-configuracoes-backup.md`) e preenche a tabela "PC da loja" de `C:\dev\orcamentos-ponto-do-gesso\docs\fases\README.md`.

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
