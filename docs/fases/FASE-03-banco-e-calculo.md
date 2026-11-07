# Fase 3 — Banco de dados, domínio e cálculo

**Objetivo:** camada de dados completa e regras de cálculo testadas, sem telas novas.

## Tarefas

- [x] `Dados\Banco.cs`: abre/cria `%APPDATA%\PontoDoGesso\dados.db`, `PRAGMA foreign_keys=ON`, `journal_mode=WAL` (testar no Win7; se der problema, `DELETE`)
- [x] `Dados\Migracoes\001_inicial.sql` com o esquema de `C:\dev\orcamentos-ponto-do-gesso\docs\ARQUITETURA.md` (recurso embutido)
- [x] Executor de migrações: lê `versao_banco`, faz backup em `backups\` antes de migrar, aplica em transação
- [x] Inserir configurações padrão (tabela da ARQUITETURA)
- [x] `Dominio\`: `Cliente`, `Produto`, `Orcamento`, `ItemOrcamento`
- [x] `Servicos\Formatacao.cs`: centavos ↔ texto (`"1.234,56"`, `"R$ 1.234,56"`), parse de quantidade com vírgula, telefone `(12) 99244-5402`
- [x] `Servicos\CalculoOrcamento.cs`: total do item (arredondamento meio para cima), subtotal, desconto (VALOR ou PERCENTUAL, só sobre o subtotal), total = subtotal − desconto + frete
- [x] Repositórios (`Dados\`): `ClienteRepositorio`, `ProdutoRepositorio`, `OrcamentoRepositorio` (salvar orçamento + itens numa transação; número atribuído no primeiro salvamento), `ConfiguracaoRepositorio`
- [x] Busca sem acento e sem diferenciar maiúsculas (coluna normalizada ou função `remover_acentos` registrada no SQLite)

## Testes obrigatórios

- [x] `2,5 × R$ 12,33 = R$ 30,83` (30,825 → 30,83)
- [x] `3 × R$ 0,10 = R$ 0,30` (sem erro de ponto flutuante)
- [x] Subtotal + frete
- [x] Desconto em valor: subtotal R$ 125,00 − R$ 10,00 + frete R$ 20,00 = R$ 135,00
- [x] Desconto percentual: 5% de R$ 125,00 = R$ 6,25; 3% de R$ 33,33 = R$ 1,00 (0,9999 → 1,00)
- [x] Desconto não incide sobre o frete
- [x] Desconto maior que o subtotal é recusado
- [x] Desconto 0 vira "sem desconto"
- [x] Validade nula é aceita; validade de 7 dias a partir de 06/11/2026 = 13/11/2026
- [x] Parse: `"10"`, `"2,5"`, `"1.000,5"`, `"abc"` (erro)
- [x] Formatação de `123456` → `"1.234,56"`
- [x] Numeração: dois orçamentos seguidos recebem números consecutivos; excluir não libera o número
- [x] Códigos de cliente e produto sequenciais e não reaproveitados
- [x] `Formatacao.Codigo(45)` → `"0045"`; busca por `"45"` e `"0045"` acha o mesmo registro
- [x] Snapshot: alterar o cliente depois de salvar não altera o orçamento
- [x] Migração em banco novo e em banco já migrado (idempotente)

## Pronto quando

`dotnet test` verde com todos os testes acima; o app abre, cria o banco e mostra no Diagnóstico a versão do banco.

## Observações

- `journal_mode=WAL` passou no Windows 11. **Falta testar no Windows 7**: o modo em uso aparece no Diagnóstico; se der problema, trocar para `DELETE` em `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Dados\Banco.cs`.
- Dapper travado na 2.1.35 e funções `remover_acentos` e `somente_digitos` registradas no SQLite: passaram no Windows 11, **falta testar no Windows 7**.
- A abertura real do app (janela e Diagnóstico) não foi conferida à mão nesta fase; a criação do banco e a linha do Diagnóstico estão cobertas por testes automáticos.
- v0.3.0 não publicada: sem tag nem release nesta fase.
