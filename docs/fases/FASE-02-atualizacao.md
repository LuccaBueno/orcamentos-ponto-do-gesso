# Fase 2 — Botão de atualização e releases automáticas

**Objetivo:** a partir daqui, toda versão nova chega ao PC da loja pelo botão **Verificar atualizações**. Especificação completa em `C:\dev\orcamentos-ponto-do-gesso\docs\ATUALIZACAO.md`.

## Tarefas

### Publicação
- [x] `C:\dev\orcamentos-ponto-do-gesso\scripts\empacotar.ps1`: compila Release, copia para `publicacao\PontoDoGesso\` (exe, config, dlls, `x86\SQLite.Interop.dll`, Atualizador, assets), gera `PontoDoGesso-vX.Y.Z.zip` e `.zip.sha256` (lendo a versão do `Directory.Build.props`)
- [ ] `C:\dev\orcamentos-ponto-do-gesso\.github\workflows\release.yml`: em push de tag `v*`, roda build + testes + `empacotar.ps1` e cria a Release com os dois arquivos e o trecho do `CHANGELOG.md` daquela versão
- [ ] Validar que a tag bate com `<Version>` (falhar o workflow se não bater)

### No app
- [ ] `Servicos\Atualizacao.cs`: consultar `releases/latest`, comparar versões (`System.Version`), baixar zip e sha256 com barra de progresso, validar hash
- [ ] `Forms\FormAtualizacao.cs`: mostra versão atual, versão nova, notas da versão, botões **Atualizar agora** / **Depois** / **Abrir página de downloads**
- [ ] Verificação automática ao abrir, no máximo 1 vez por dia, em segundo plano (aviso no rodapé se houver versão nova)
- [ ] Antes de atualizar: backup do banco (pode ser cópia simples do arquivo nesta fase)

### Atualizador
- [ ] `PontoDoGesso.Atualizador\Program.cs`: argumentos `--pid --zip --destino`; espera o processo, extrai (`System.IO.Compression.ZipFile`), faz rollback via `<destino>.anterior` se falhar, reabre o app, grava log em `%APPDATA%\PontoDoGesso\logs\`
- [ ] O app copia o Atualizador para a pasta temporária antes de executá-lo (para poder substituir o próprio Atualizador)

### Testes
- [ ] Teste unitário de comparação de versões (`v0.10.0` > `v0.9.3`)
- [ ] Teste unitário da validação SHA-256
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

