# Fase 5 — Catálogo simples de produtos

**Objetivo:** lista de produtos com preço para preencher os itens do orçamento mais rápido. **Sem estoque.**

## Tarefas

- [x] `Forms\FormProdutos.cs`: lista (**código**, nome, unidade, preço), busca enquanto digita (código ou nome), **Novo**, **Editar**, **Inativar/Reativar**
- [x] `Forms\FormProdutoEditar.cs`: nome (obrigatório), unidade (lista editável: un, m², m, kg, saco, cx, pç, rolo), preço
- [x] `Controles\CampoMoeda.cs`: campo de valor que aceita `12,5`, `12,50`, `1.250,00` e formata ao sair
- [x] **Edição rápida de preços na própria lista**: coluna Preço editável direto na grade (Enter grava e desce para o próximo), sem abrir janela
- [x] **Cadastro em sequência**: na tela de novo produto, botão **Salvar e cadastrar outro** (mantém a unidade escolhida), para lançar a lista inicial da loja rápido
- [x] **Reajuste em lote**: selecionar vários produtos (ou todos do filtro) → aplicar +X% ou −X% (ou valor fixo), com prévia "antes → depois" e confirmação
- [x] Mostrar "Preço atualizado em dd/MM/aaaa" em cada produto
- Tudo dentro do app; sem planilhas ou arquivos externos.
- [x] Habilitar o botão **Produtos** na tela principal

## Testes

- [x] Reajuste de 5% em R$ 12,50 → R$ 13,13 (arredondamento meio para cima)
- [x] Edição na grade grava e não altera orçamentos já salvos
- [x] Busca por "12" encontra o produto Cód. 0012

## Pronto quando

Publicar v0.5.0 e cadastrar a lista real de produtos da loja pelo app. — **aguardando teste do Lucca** (sem tag nem release nesta sessão).

## Observações

- Data do preço: criada a migração `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Dados\Migracoes\002_preco_atualizado_em.sql` com a coluna `produtos.preco_atualizado_em`. Ela só muda quando o preço muda (cadastro, grade ou reajuste); corrigir só o nome não mexe na data. Produtos que já existiam recebem a data de `atualizado_em`.
- Edição na grade: o preço é gravado ao sair da célula (Enter grava e desce). Valor inválido mostra o erro na linha e segura o cursor; Esc desfaz.
- Reajuste em lote: com **mais de um** produto selecionado na lista (Ctrl/Shift + clique), reajusta só os selecionados; com um ou nenhum, reajusta **todos os que a busca está mostrando**. O título da janela diz quantos produtos serão alterados. Escolhe-se Aumentar/Diminuir e % ou R$; reajuste que deixaria preço negativo fica bloqueado. Arredondamento meio para cima.
- Preço vazio não é aceito no cadastro (aviso "Informe o preço (pode ser 0,00)"), para não gravar produto com preço zero por esquecimento.
- Os itens de tela foram feitos e testados juntos num commit só, porque a lista depende da edição e do reajuste para compilar.
- Testes em `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso.Testes\TelaProdutosTestes.cs` e `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso.Testes\CampoMoedaTestes.cs`.
- Fase concluída no código em 07/11/2026, sem tag nem release. Falta o Lucca cadastrar a lista real de produtos e conferir o reajuste: aguardando teste do Lucca.
