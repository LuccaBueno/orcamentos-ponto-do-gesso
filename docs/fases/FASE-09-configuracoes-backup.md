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

Publicar v0.9.0 e configurar a pasta extra de backup na loja.

## Observações

