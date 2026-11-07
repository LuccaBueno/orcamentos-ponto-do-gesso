# Fase 2 — Botão de atualização e releases automáticas

**Objetivo:** a partir daqui, toda versão nova chega ao PC da loja pelo botão **Verificar atualizações**. Especificação completa em `C:\dev\orcamentos-ponto-do-gesso\docs\ATUALIZACAO.md`.

## Tarefas

### Publicação
- [x] `C:\dev\orcamentos-ponto-do-gesso\scripts\empacotar.ps1`: compila Release, copia para `publicacao\PontoDoGesso\` (exe, config, dlls, `x86\SQLite.Interop.dll`, Atualizador, assets), gera `PontoDoGesso-vX.Y.Z.zip` e `.zip.sha256` (lendo a versão do `Directory.Build.props`)
- [x] `C:\dev\orcamentos-ponto-do-gesso\.github\workflows\release.yml`: em push de tag `v*`, roda build + testes + `empacotar.ps1` e cria a Release com os dois arquivos e o trecho do `CHANGELOG.md` daquela versão
- [x] Validar que a tag bate com `<Version>` (falhar o workflow se não bater)

### No app
- [x] `Servicos\Atualizacao.cs`: consultar `releases/latest`, comparar versões (`System.Version`), baixar zip e sha256 com barra de progresso, validar hash
- [ ] `Forms\FormAtualizacao.cs`: mostra versão atual, versão nova, notas da versão, botões **Atualizar agora** / **Depois** / **Abrir página de downloads**
- [ ] Verificação automática ao abrir, no máximo 1 vez por dia, em segundo plano (aviso no rodapé se houver versão nova)
- [ ] Antes de atualizar: backup do banco (pode ser cópia simples do arquivo nesta fase)

### Atualizador
- [x] `PontoDoGesso.Atualizador\Program.cs`: argumentos `--pid --zip --destino`; espera o processo, extrai (`System.IO.Compression.ZipFile`), faz rollback via `<destino>.anterior` se falhar, reabre o app, grava log em `%APPDATA%\PontoDoGesso\logs\`
- [ ] O app copia o Atualizador para a pasta temporária antes de executá-lo (para poder substituir o próprio Atualizador)

### Testes
- [x] Teste unitário de comparação de versões (`v0.10.0` > `v0.9.3`)
- [x] Teste unitário da validação SHA-256
- [ ] Teste real: publicar **v0.2.0**, instalar no PC, publicar **v0.2.1** (só mudando o texto do rodapé) e atualizar pelo botão — no seu PC e no PC da loja
- [ ] Teste de falha: zip corrompido → hash não bate → nada é alterado
- [ ] **Teste com o antivírus do PC da loja**: baixar a release, abrir o app e atualizar com o antivírus ligado; se houver bloqueio, aplicar `C:\dev\orcamentos-ponto-do-gesso\docs\INSTALACAO-WINDOWS7.md` seção 5 e registrar aqui
- [ ] Reduzir alarmes falsos: preencher metadados do exe (`Product`, `Company`, `Description`, `Copyright`, versão) no `Directory.Build.props`; não usar compactadores de exe (UPX) nem ofuscadores
- [ ] Se o Atualizador for apagado/bloqueado: o app detecta, avisa e cai no modo manual (abrir página de downloads)

## Pronto quando

O PC da loja vai da v0.2.0 para a v0.2.1 pelo botão, sem perder nada, ou (se o HTTPS do Win7 falhar) o modo manual abre a página certa.

## Observações

- **Mudança de plano das versões (07/11/2026):** como as Fases 3 a 9 já estavam prontas quando a Fase 2 começou, as versões desta fase passaram a ser **v0.9.0** (versão completa, a que será instalada na loja) e **v0.9.1** (só para testar a atualização), no lugar de v0.2.0/v0.2.1.
- `empacotar.ps1`: as entradas do zip são gravadas uma a uma com `/` (o `ZipFile.CreateFromDirectory` do PowerShell 5.1 grava `\`, fora do padrão do formato zip). A pasta `x64\` e os `.pdb` ficam de fora (o app é só x86). O `.zip.sha256` segue o formato do `sha256sum` (`<hash>  <arquivo>`). Parâmetro `-SemCompilar` para o workflow, que já compilou e testou.
- Workflow de release: `C:\dev\orcamentos-ponto-do-gesso\scripts\conferir-versao.ps1` (falha se a tag não for `v` + `<Version>`) e `C:\dev\orcamentos-ponto-do-gesso\scripts\notas-versao.ps1` (copia a seção `## [X.Y.Z]` do `CHANGELOG.md`; falha se não existir ou estiver vazia). A release é criada pelo `gh release create` com o token do próprio workflow (`permissions: contents: write`).
- `Atualizacao.cs`: a resposta do GitHub é lida com o `JavaScriptSerializer` (`System.Web.Extensions`, que já vem no .NET Framework 4.8 — nenhum pacote NuGet novo). A comparação usa só MAJOR.MINOR.PATCH (`0.9` = `0.9.0` = `0.9.0.0`). O download vai para `<arquivo>.parcial` e só vira o nome final quando termina; se o SHA-256 não bater, o zip é apagado e nada é alterado. As URLs podem ser `file://`, o que permite testar o download e o hash errado sem rede.
- Atualizador: a troca de pastas fica em `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso.Atualizador\Instalacao.cs` (testada em `InstalacaoTestes`). O zip é extraído em `<destino>.nova` (ao lado da pasta do app, no mesmo disco, para mover ser instantâneo), a pasta atual vira `<destino>.anterior` e a nova toma o lugar. Se mover falhar (antivírus segurando arquivo), tenta 10 vezes, 1 s entre elas; se ainda falhar, volta a anterior. Se a versão nova não abrir, também volta a anterior e reabre o app antigo. A `.anterior` fica guardada até a próxima atualização (rollback manual). Log em `%APPDATA%\PontoDoGesso\logs\atualizacao_AAAA-MM.log` (o app passa `--logs`; sem ele, usa a mesma regra do `PONTODOGESSO_DADOS`).
- Os scripts `.ps1` ficam em UTF-8 **com BOM**: sem o BOM, o Windows PowerShell 5.1 lê o arquivo como ANSI e as mensagens com acento saem quebradas.

