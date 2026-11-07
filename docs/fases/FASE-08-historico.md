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
- [x] Na tela do cliente: aba/lista "Orçamentos deste cliente"
- [x] Paginação ou carregamento sob demanda (rápido com milhares de orçamentos)
- [x] Rodapé da lista: quantidade e soma dos totais do filtro
- [x] Habilitar **Histórico** na tela principal

## Testes

- [x] Cada filtro isolado e combinado (ex.: avulsos + com frete + mês passado)
- [x] Cliente específico traz só os orçamentos daquele cadastro
- [ ] Gerar 5.000 orçamentos falsos num banco de teste e medir a busca (< 1 s no Win7) — medido no PC de desenvolvimento: ~65 ms para as 4 consultas (teste `BuscaRapidaComCincoMilOrcamentos`); **aguardando teste do Lucca** no Windows 7

## Pronto quando

Publicar v0.8.0. — **aguardando teste do Lucca** (sem tag nem release nesta sessão).

## Observações


- Fase concluída no código em 07/11/2026, sem tag nem release. Falta medir a busca no Windows 7 da loja: aguardando teste do Lucca.
- Tela em `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\FormHistorico.cs`; filtros em `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Dados\FiltroHistorico.cs`; consulta em `OrcamentoRepositorio.Buscar` e `OrcamentoRepositorio.Somar` (`C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Dados\OrcamentoRepositorio.cs`). Testes em `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso.Testes\HistoricoTestes.cs` e `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso.Testes\TelaHistoricoTestes.cs`.
- **Período padrão: Todos** (para achar qualquer orçamento antigo); a lista já vem dos mais recentes para os mais antigos.
- **Carregamento aos poucos:** 200 por vez; os próximos 200 vêm sozinhos ao rolar até perto do fim, ou pelo botão **Carregar mais**. O rodapé mostra a quantidade e a soma de **tudo** que o filtro encontrou (não só do que foi carregado), e "mostrando 200" enquanto faltar carregar.
- **Ordenação** feita no banco (para valer com a paginação), só por uma lista fixa de colunas. Primeiro clique: texto de A a Z; datas, números e valores dos maiores para os menores. Segundo clique inverte. Empate: número mais novo primeiro.
- **Texto da busca:** acha pelo nº do orçamento, pelo código do cliente cadastrado, pelo nome gravado no orçamento (sem acento, também avulsos) e pelo telefone (só os dígitos). Digitar "12" acha também telefones com 12, como na busca de clientes.
- **Com/sem desconto** olha o valor final do desconto (desconto 0 conta como sem desconto, como no cálculo).
- **Cliente específico:** autocomplete dos cadastrados (inclusive inativos, para achar orçamentos antigos); apagar o texto tira o filtro. Com "Só avulsos" o campo fica desabilitado e o filtro de cliente é ignorado; escolher um cliente com "Só avulsos" marcado volta para "Todos".
- **Filtros lembrados** enquanto o app estiver aberto (inclusive a ordenação). Quando o histórico é aberto pelo botão do cliente, usa o filtro daquele cliente e não troca os lembrados.
- **Orçamentos deste cliente:** em vez de uma aba dentro da edição do cliente, é um botão na lista de clientes (`C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\FormClientes.cs`) que abre o histórico já filtrado por aquele cadastro, com todas as datas. Assim a mesma tela (com exportar, abrir, duplicar) serve para os dois casos.
- **Validade vencida:** data final em vermelho com "(vencida)".
- **Duplicar** abre a tela de orçamento já como orçamento novo (sem número, data de hoje), igual ao Duplicar da própria tela.
- **Excluir** pergunta com "Não" como padrão; o número excluído não volta a ser usado (já garantido pelo `sqlite_sequence` desde a Fase 6).
- Enter na lista e duplo clique abrem o orçamento; Esc fecha.
