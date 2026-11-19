# Relatório da sessão — melhorias pós-v0.9.5 (não publicado)

O que testar na loja (build de `C:\dev\orcamentos-ponto-do-gesso`, sem release):

1. **Enter avança:** no cadastro de cliente e de produto, Empresa, Configurações e nos campos de cliente/entrega do orçamento, Enter vai para o próximo campo. No produto, Enter no último campo salva. Observações (várias linhas) quebra linha; barra "Adicionar item" e listas de sugestões funcionam como antes.
2. **Menu lateral:** "Abrir pasta de orçamentos" aparece inteiro, em duas linhas.
3. **Entrega:** escolher cliente cadastrado → só nome e telefone; entrega vazia com a cidade padrão.
4. **Usar endereço do cadastro:** preenche a entrega; desabilitado no avulso e em cliente sem endereço.
5. **Endereços usados antes:** abre a lista (mouse ou Enter/Espaço no botão, ↑/↓ e Enter); escolher preenche a entrega, inclusive Receber com/Telefone no local.
6. **Receber com / Telefone no local:** telefone inválido é recusado ao salvar; preenchidos, saem no PNG, PDF e impressão na última linha da área de entrega; vazios, não aparecem.
7. **Duplicar** mantém entrega, Receber com e Telefone no local; **Retirada** esconde tudo isso.
8. **Histórico:** buscar por rua ou bairro de entrega encontra o orçamento.
9. **Migração 004:** ao abrir com o banco da v0.9.5, conferir backup novo em `%APPDATA%\PontoDoGesso\` (pasta de backups) e os orçamentos antigos intactos.
