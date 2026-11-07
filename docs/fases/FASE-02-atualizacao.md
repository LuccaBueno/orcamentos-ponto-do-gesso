# Fase 2 — Botão de atualização e releases automáticas

**Objetivo:** a partir daqui, toda versão nova chega ao PC da loja pelo botão **Verificar atualizações**. Especificação completa em `C:\dev\orcamentos-ponto-do-gesso\docs\ATUALIZACAO.md`.

## Tarefas

### Publicação
- [x] `C:\dev\orcamentos-ponto-do-gesso\scripts\empacotar.ps1`: compila Release, copia para `publicacao\PontoDoGesso\` (exe, config, dlls, `x86\SQLite.Interop.dll`, Atualizador, assets), gera `PontoDoGesso-vX.Y.Z.zip` e `.zip.sha256` (lendo a versão do `Directory.Build.props`)
- [x] `C:\dev\orcamentos-ponto-do-gesso\.github\workflows\release.yml`: em push de tag `v*`, roda build + testes + `empacotar.ps1` e cria a Release com os dois arquivos e o trecho do `CHANGELOG.md` daquela versão
- [x] Validar que a tag bate com `<Version>` (falhar o workflow se não bater)

### No app
- [x] `Servicos\Atualizacao.cs`: consultar `releases/latest`, comparar versões (`System.Version`), baixar zip e sha256 com barra de progresso, validar hash
- [x] `Forms\FormAtualizacao.cs`: mostra versão atual, versão nova, notas da versão, botões **Atualizar agora** / **Depois** / **Abrir página de downloads**
- [x] Verificação automática ao abrir, no máximo 1 vez por dia, em segundo plano (aviso no rodapé se houver versão nova)
- [x] Antes de atualizar: backup do banco (pode ser cópia simples do arquivo nesta fase)

### Atualizador
- [x] `PontoDoGesso.Atualizador\Program.cs`: argumentos `--pid --zip --destino`; espera o processo, extrai (`System.IO.Compression.ZipFile`), faz rollback via `<destino>.anterior` se falhar, reabre o app, grava log em `%APPDATA%\PontoDoGesso\logs\`
- [x] O app copia o Atualizador para a pasta temporária antes de executá-lo (para poder substituir o próprio Atualizador)

### Testes
- [x] Teste unitário de comparação de versões (`v0.10.0` > `v0.9.3`)
- [x] Teste unitário da validação SHA-256
- [x] Teste real no PC do Lucca: publicar **v0.9.0** (era v0.2.0), instalar, publicar **v0.9.1** (era v0.2.1; só muda a versão no rodapé) e atualizar pelo botão. Feito em 07/11/2026 dentro de `C:\dev\orcamentos-ponto-do-gesso\.tmp\` (ver Observações)
- [ ] Teste real no PC da loja: instalar a v0.9.0 e ir para a v0.9.1 pelo botão — **aguardando teste do Lucca**
- [x] Teste de falha: zip corrompido → hash não bate → nada é alterado
- [x] Teste de rollback: versão nova que não abre → o Atualizador volta a anterior sozinho (teste extra, ver Observações)
- [ ] **Teste com o antivírus do PC da loja**: baixar a release, abrir o app e atualizar com o antivírus ligado; se houver bloqueio, aplicar `C:\dev\orcamentos-ponto-do-gesso\docs\INSTALACAO-WINDOWS7.md` seção 5 e registrar aqui — **aguardando teste do Lucca**
- [x] Reduzir alarmes falsos: preencher metadados do exe (`Product`, `Company`, `Description`, `Copyright`, versão) no `Directory.Build.props`; não usar compactadores de exe (UPX) nem ofuscadores
- [x] Se o Atualizador for apagado/bloqueado: o app detecta, avisa e cai no modo manual (abrir página de downloads)

## Pronto quando

O PC da loja vai da v0.9.0 para a v0.9.1 pelo botão, sem perder nada, ou (se o HTTPS do Win7 falhar) o modo manual abre a página certa.

## Observações

- **Mudança de plano das versões (07/11/2026):** como as Fases 3 a 9 já estavam prontas quando a Fase 2 começou, as versões desta fase passaram a ser **v0.9.0** (versão completa, a que será instalada na loja) e **v0.9.1** (só para testar a atualização), no lugar de v0.2.0/v0.2.1.
- `empacotar.ps1`: as entradas do zip são gravadas uma a uma com `/` (o `ZipFile.CreateFromDirectory` do PowerShell 5.1 grava `\`, fora do padrão do formato zip). A pasta `x64\` e os `.pdb` ficam de fora (o app é só x86). O `.zip.sha256` segue o formato do `sha256sum` (`<hash>  <arquivo>`). Parâmetro `-SemCompilar` para o workflow, que já compilou e testou.
- Workflow de release: `C:\dev\orcamentos-ponto-do-gesso\scripts\conferir-versao.ps1` (falha se a tag não for `v` + `<Version>`) e `C:\dev\orcamentos-ponto-do-gesso\scripts\notas-versao.ps1` (copia a seção `## [X.Y.Z]` do `CHANGELOG.md`; falha se não existir ou estiver vazia). A release é criada pelo `gh release create` com o token do próprio workflow (`permissions: contents: write`).
- `Atualizacao.cs`: a resposta do GitHub é lida com o `JavaScriptSerializer` (`System.Web.Extensions`, que já vem no .NET Framework 4.8 — nenhum pacote NuGet novo). A comparação usa só MAJOR.MINOR.PATCH (`0.9` = `0.9.0` = `0.9.0.0`). O download vai para `<arquivo>.parcial` e só vira o nome final quando termina; se o SHA-256 não bater, o zip é apagado e nada é alterado. As URLs podem ser `file://`, o que permite testar o download e o hash errado sem rede.
- Atualizador: a troca de pastas fica em `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso.Atualizador\Instalacao.cs` (testada em `InstalacaoTestes`). O zip é extraído em `<destino>.nova` (ao lado da pasta do app, no mesmo disco, para mover ser instantâneo), a pasta atual vira `<destino>.anterior` e a nova toma o lugar. Se mover falhar (antivírus segurando arquivo), tenta 10 vezes, 1 s entre elas; se ainda falhar, volta a anterior. Se a versão nova não abrir, também volta a anterior e reabre o app antigo. A `.anterior` fica guardada até a próxima atualização (rollback manual). Log em `%APPDATA%\PontoDoGesso\logs\atualizacao_AAAA-MM.log` (o app passa `--logs`; sem ele, usa a mesma regra do `PONTODOGESSO_DADOS`).
- Tela: rodapé da tela principal com o link **Verificar atualizações** (abre `FormAtualizacao`, que consulta o GitHub em segundo plano) e o aviso laranja **Versão X disponível — clique para atualizar**, que só aparece quando a verificação automática acha versão nova. A verificação automática (`Servicos\VerificacaoAutomatica.cs`) grava a tentativa em `atualizacao.ultima_verificacao` antes de consultar: sem internet, só tenta de novo 24 h depois e não mostra nada (o erro vai para o log). Só o `Program.cs` liga a verificação (`VerificarAtualizacaoAoAbrir`), então os testes nunca acessam a internet.
- Backup antes de atualizar: em vez da cópia simples do arquivo, usa a API de backup do SQLite (a mesma da Fase 9), em `backups\dados_AAAA-MM-DD_HHmm_antes-de-atualizar.db`. Não conta como backup diário nem é apagado pela limpeza dos 30 automáticos. Se o backup falhar, a atualização é cancelada.
- Atualizador ausente: se `PontoDoGesso.Atualizador.exe` não estiver na pasta do app ou o Windows não deixar executá-lo (antivírus), o app avisa e abre a página de downloads da versão. O Atualizador é copiado para `%TEMP%\PontoDoGesso-atualizador\<guid>\` e executado com a pasta de trabalho lá (com a pasta de trabalho dentro da pasta do app, o Windows não deixaria renomeá-la).
- **Bug dos testes de tela (`Value Dispose() cannot be called while doing CreateHandle()`), corrigido:** o teste do hash errado falhava de forma intermitente (até 13 em 25 rodando sozinho) e, numa repetição, abriu a janela "Microsoft .NET Framework – Unhandled exception", que travou o processo de teste até alguém clicar. Causa encontrada registrando a pilha de criação e destruição do handle do painel de botões: a continuação do download rodava numa **thread do thread pool**, não na thread da tela. A thread STA criada pelo teste herdava o `SynchronizationContext` do xunit, cujo `Post` vai para o thread pool, e o `TaskScheduler.FromCurrentSynchronizationContext()` mandava a continuação para lá. Ela mexia nos controles (criando o handle do painel) enquanto a thread da tela fechava e descartava a janela. Correções:
  1. `Tela.EmSta` (testes): `Application.SetUnhandledExceptionMode(ThrowException, true)` e um `Application.ThreadException` que guarda a exceção e faz o teste **falhar**, sem abrir a caixa de erro. A thread do teste também instala um `WindowsFormsSynchronizationContext`.
  2. `Tela.Exibir` só volta quando a janela e todos os controles visíveis já têm handle. O novo `Tela.Fechar` fecha como o usuário faria (`Close`) e processa as mensagens antes do descarte, sempre na mesma thread STA.
  3. App (`FormAtualizacao` e a verificação automática do `FormPrincipal`): as continuações e o progresso não dependem mais do contexto da thread. Voltam para a tela por `Control.BeginInvoke` e só mexem nos controles se a janela ainda estiver aberta (`!_fechada && !IsDisposed && !Disposing && IsHandleCreated`). No app real o contexto já era o do WinForms, mas agora fechar a janela durante o download não tem como dar esse erro. Teste novo: `FecharDuranteODownloadIgnoraOResultadoQueChegaDepois`.
  - Resultado: 25 execuções seguidas sem falha das classes `TelaAtualizacaoTestes` + `TelaPrincipalTestes`, e mais 25 seguidas dos dois testes de download sozinhos.
- Metadados dos exes: `Product`, `Company`, `Authors`, `Copyright` e `Description` no `Directory.Build.props`. A descrição que o Windows mostra (FileDescription) vem do `AssemblyTitle` de cada projeto: "Ponto do Gesso - orçamentos" e "Atualizador do Ponto do Gesso". Os dois exes têm o ícone do app. Nada de UPX nem ofuscador.
- **Testes reais na v0.9.0 instalada (07/11/2026, no PC do Lucca, tudo dentro de `C:\dev\orcamentos-ponto-do-gesso\.tmp\`):** release v0.9.0 baixada com `gh release download` e hash conferido; instalada em `.tmp\instalacao\PontoDoGesso\` com `PONTODOGESSO_DADOS=.tmp\dados-atualizacao`; o app abriu, criou o banco e o backup automático; um cliente e um orçamento (3 × R$ 12,50) foram gravados pelos repositórios do próprio app (`scripts\teste-atualizacao\dados-teste.ps1`, no PowerShell 32 bits).
  - **Hash errado** (`scripts\teste-atualizacao\teste-hash-errado.ps1`): zip corrompido em 100 bytes, publicado em `file://` com o `.sha256` do original e baixado pelo `Atualizacao` do app instalado → `PacoteInvalidoException`, o zip foi apagado da pasta `atualizacao` e a pasta do app ficou idêntica (hash de todos os arquivos).
  - **Rollback** (`scripts\teste-atualizacao\teste-rollback.ps1`): zip válido, mas com um `PontoDoGesso.exe` que não é executável, entregue ao Atualizador real pelo `IniciarAtualizador` (com `TEMP`/`TMP` em `.tmp\temp`) com a v0.9.0 aberta → o app fechou, o Atualizador instalou, a versão nova não abriu ("not a valid application"), ele voltou a `.anterior`, reabriu a v0.9.0 e avisou. Pasta do app idêntica à de antes, sem `.anterior` nem `.nova` sobrando.
  - Achado da primeira tentativa: o script de teste tinha carregado o `PontoDoGesso.exe` da pasta instalada (`LoadFrom` trava o arquivo), e o Windows negou renomear a pasta. O Atualizador tentou 10 vezes, desistiu sem mudar nada, reabriu a v0.9.0 e avisou: é exatamente o comportamento esperado quando um antivírus ou outro programa segura um arquivo. O script passou a carregar o app de uma cópia.
- **Atualização real v0.9.0 → v0.9.1 pela interface (07/11/2026, PC do Lucca)** com `C:\dev\orcamentos-ponto-do-gesso\scripts\teste-atualizacao\teste-atualizacao-real.ps1`, que usa UI Automation e, no LinkLabel, a ação padrão do MSAA (sem mexer no mouse). App instalado em `.tmp\instalacao\PontoDoGesso\`, dados em `.tmp\dados-atualizacao\`, `TEMP` em `.tmp\temp\`. Resultado:
  - Ao abrir a v0.9.0 (com a última verificação apagada), o rodapé mostrou sozinho **Nova versão 0.9.1!**.
  - **Verificar atualizações** → "Versão nova: 0.9.1" → **Atualizar agora**: baixou do GitHub, conferiu o SHA-256, fez `backups\dados_2026-11-07_1813_antes-de-atualizar.db`, fechou e o Atualizador instalou em cerca de 1 s.
  - O app reabriu sozinho com **Versão 0.9.1** no rodapé; o exe instalado ficou com a versão 0.9.1.0 e a v0.9.0 ficou guardada em `PontoDoGesso.anterior`. O zip baixado foi apagado.
  - **Dados intactos:** o cliente "Cliente Teste Atualização" e o orçamento de 3 × R$ 12,50 (3.750 centavos) continuam lá, e o backup automático do dia também.
  - Os scripts dos testes reais ficam em `C:\dev\orcamentos-ponto-do-gesso\scripts\teste-atualizacao\` (rodam no PowerShell de 32 bits, `C:\Windows\SysWOW64\WindowsPowerShell\v1.0\powershell.exe`, porque o app é x86).
- Os scripts `.ps1` ficam em UTF-8 **com BOM**: sem o BOM, o Windows PowerShell 5.1 lê o arquivo como ANSI e as mensagens com acento saem quebradas.

