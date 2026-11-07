# Fase 9 — Configurações e backup

**Objetivo:** ajustar os dados da loja sem mexer no código e não perder dados nunca.

## Tarefas

### Configurações (`Forms\FormConfiguracoes.cs`)
- [x] Aba **Loja**: nome, subtítulo, telefone, endereço (2 linhas), rodapé — com **pré-visualização** do cabeçalho do orçamento
- [x] Aba **Orçamento**: validade sugerida (dias que aparecem ao ligar a validade num orçamento), cidade padrão, DDD padrão, perguntar se cadastra cliente avulso
- [x] Aba **Exportação**: pasta padrão, impressora padrão (opcional)
- [x] Aba **Backup** (abaixo)
- [x] Habilitar **Configurações** na tela principal

### Backup (`Servicos\Backup.cs`)
- [x] Backup automático ao abrir, se o último tiver mais de 24 h, usando a API de backup do SQLite (seguro com o banco aberto)
- [x] Manter os últimos 30 backups automáticos
- [x] **Fazer backup agora** e **Copiar backup para…** (pendrive/HD)
- [x] Pasta extra de backup opcional (copia também para lá se estiver acessível)
- [x] **Restaurar backup**: escolhe arquivo, valida que é um banco do app, faz backup do atual antes, reinicia o app
- [x] Mostrar data do último backup na aba

## Testes

- [x] Restaurar um backup antigo e conferir orçamentos
- [x] Restaurar arquivo inválido é recusado

## Pronto quando

Publicar v0.9.0 e configurar a pasta extra de backup na loja. — **aguardando teste do Lucca** (sem tag nem release nesta sessão; a pasta extra é configurada no PC da loja).

## Observações


- Fase concluída no código em 07/11/2026, sem tag nem release. Falta no PC da loja: escolher a pasta extra de backup (pendrive ou HD) e testar uma restauração de verdade — aguardando teste do Lucca.
- Tela em `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\FormConfiguracoes.cs`; leitura, validação e gravação em `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Servicos\ConfiguracoesApp.cs`; backup em `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Servicos\Backup.cs`. Testes em `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso.Testes\BackupTestes.cs` e `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso.Testes\TelaConfiguracoesTestes.cs`.
- **Novas chaves:** `cliente.cidade_padrao` (Pindamonhangaba) e `backup.pasta_extra` (vazia). A cidade padrão agora vale para cliente novo e para a entrega de orçamento avulso (antes era fixa no código).
- **Nomes dos backups** (na pasta `backups` dos dados): `dados_AAAA-MM-DD_HHmm_auto.db` (automático diário), `_manual.db` (Fazer backup agora), `_copia.db` (Copiar backup para…, gravado direto na pasta escolhida) e `_antes-de-restaurar.db`. Os feitos antes de uma migração continuam sem sufixo. **Só os automáticos entram na conta dos 30**; os outros nunca são apagados pelo app. Dois no mesmo minuto ganham `-2`, `-3`…
- **Backup automático:** roda ao abrir o app (depois das migrações), se o último backup (de qualquer tipo, chave `backup.ultimo`) tiver mais de 24 h — ou se a data do último estiver no futuro (relógio do PC errado). Falha no backup automático é registrada no log e não impede o app de abrir.
- **Pasta extra:** cada backup automático ou manual é copiado também para lá, se a pasta existir na hora; pendrive fora ou rede caída não impede o backup principal (o erro vai para o log). A pasta extra é gravada assim que é escolhida (não depende do botão Salvar), como as outras ações da aba Backup.
- **Restaurar:** confere o cabeçalho do SQLite, as 6 tabelas do app, `PRAGMA quick_check` e se a versão do banco não é mais nova que a do app; depois guarda o banco atual (`_antes-de-restaurar`), copia o backup para dentro do banco atual pela própria API de backup (sem apagar arquivo com o app aberto) e reinicia o app (`Application.Restart`). Backup de versão antiga é atualizado pelas migrações ao reabrir.
- **Pré-visualização do cabeçalho:** é o próprio desenho do orçamento (o mesmo do PNG/PDF), com os dados digitados e ainda não salvos, atualizada 0,3 s depois de parar de digitar.
- **Impressora padrão:** lista das impressoras instaladas + "(impressora padrão do Windows)". Se a impressora gravada não estiver mais instalada, ela continua aparecendo na lista para não se perder, e a impressão usa a padrão do Windows.
- Validações ao salvar: nome da loja obrigatório, DDD padrão da lista da Anatel, validade de 1 a 365 dias e pasta de exportação com caminho completo.
