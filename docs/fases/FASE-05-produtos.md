# Fase 5 — Catálogo simples de produtos

**Objetivo:** lista de produtos com preço para preencher os itens do orçamento mais rápido. **Sem estoque.**

## Tarefas

- [ ] `Forms\FormProdutos.cs`: lista (**código**, nome, unidade, preço), busca enquanto digita (código ou nome), **Novo**, **Editar**, **Inativar/Reativar**
- [ ] `Forms\FormProdutoEditar.cs`: nome (obrigatório), unidade (lista editável: un, m², m, kg, saco, cx, pç, rolo), preço
- [ ] `Controles\CampoMoeda.cs`: campo de valor que aceita `12,5`, `12,50`, `1.250,00` e formata ao sair
- [ ] **Edição rápida de preços na própria lista**: coluna Preço editável direto na grade (Enter grava e desce para o próximo), sem abrir janela
- [ ] **Cadastro em sequência**: na tela de novo produto, botão **Salvar e cadastrar outro** (mantém a unidade escolhida), para lançar a lista inicial da loja rápido
- [ ] **Reajuste em lote**: selecionar vários produtos (ou todos do filtro) → aplicar +X% ou −X% (ou valor fixo), com prévia "antes → depois" e confirmação
- [ ] Mostrar "Preço atualizado em dd/MM/aaaa" em cada produto
- Tudo dentro do app; sem planilhas ou arquivos externos.
- [ ] Habilitar o botão **Produtos** na tela principal

## Testes

- [ ] Reajuste de 5% em R$ 12,50 → R$ 13,13 (arredondamento meio para cima)
- [ ] Edição na grade grava e não altera orçamentos já salvos
- [ ] Busca por "12" encontra o produto Cód. 0012

## Pronto quando

Publicar v0.5.0 e cadastrar a lista real de produtos da loja pelo app.

## Observações

