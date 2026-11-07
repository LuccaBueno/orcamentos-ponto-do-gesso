# Fase 8 — Histórico de orçamentos

**Objetivo:** achar qualquer orçamento antigo em segundos.

## Tarefas

- [x] `Forms\FormHistorico.cs`: lista com Nº, data, cliente, tipo (Cadastrado/Avulso), telefone, frete, desconto, total, entrega/retirada, validade (com destaque se vencida); mais recentes primeiro; clique no cabeçalho ordena
- [x] Painel de filtros (combináveis entre si):

| Filtro | Opções |
|---|---|
| **Data** | Hoje · Últimos 7 dias · Últimos 30 dias · Este mês · Mês passado · Personalizado (de/até) · Todos |
| **Cliente** | Todos · Só cadastrados · Só avulsos |
| **Cliente específico** | campo com autocomplete dos cadastrados (habilitado quando "Todos" ou "Só cadastrados") |
| **Frete** | Todos · Com frete · Sem frete |
| **Desconto** | Todos · Com desconto · Sem desconto |
| **Texto** | nº do orçamento, código do cliente, nome ou telefone (sem acento; também acha avulsos pelo nome digitado) |

- [x] Botão **Limpar filtros**; filtros lembrados enquanto o app estiver aberto
- [x] Consulta montada em `Dados\OrcamentoRepositorio.Buscar(FiltroHistorico)` com parâmetros (sem concatenar texto no SQL)
- [x] Ações: **Abrir**, **Duplicar**, **Copiar imagem**, **PNG**, **PDF**, **Imprimir**, **Excluir** (com confirmação; o número não é reaproveitado)
- [ ] Na tela do cliente: aba/lista "Orçamentos deste cliente"
- [x] Paginação ou carregamento sob demanda (rápido com milhares de orçamentos)
- [x] Rodapé da lista: quantidade e soma dos totais do filtro
- [x] Habilitar **Histórico** na tela principal

## Testes

- [x] Cada filtro isolado e combinado (ex.: avulsos + com frete + mês passado)
- [x] Cliente específico traz só os orçamentos daquele cadastro
- [ ] Gerar 5.000 orçamentos falsos num banco de teste e medir a busca (< 1 s no Win7)

## Pronto quando

Publicar v0.8.0.

## Observações

