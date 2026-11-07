# Fase 6 — Tela de orçamento

**Objetivo:** montar e salvar um orçamento completo em poucos segundos, quase só com teclado.

## Layout da tela (`Forms\FormOrcamento.cs`)

```
┌ Orçamento Nº 0123 (novo: "Nº —" até salvar)                       Data [06/11/2026] ┐
│ Cliente: (•) Cadastrado [buscar nome/telefone......] [+ Novo]     │
│          ( ) Avulso     Nome [..........] Tel [...........]       │
│ ( ) Retirada na loja  (•) Entrega                                │
│ End [........................] Nº [....] Bairro [..........]      │
│ Cidade [Pindamonhangaba]  P.Ref [...............................] │
├──────────────────────────────────────────────────────────────────┤
│Cód.│Qtd │Un │ Descrição (código ou nome)       │ Unitário │ Total│
│ ... grade editável ...                                            │
├──────────────────────────────────────────────────────────────────┤
│ Observações [......................]   Subtotal      R$ 1.250,00 │
│             [......................]   [☐] Desconto [   ] (•)R$ ( )% │
│ [☐] Validade [ 7] dias (até 13/11)     Frete    [   ] R$   50,00 │
│                                        TOTAL         R$ 1.300,00 │
├──────────────────────────────────────────────────────────────────┤
│ [Salvar] [Copiar imagem] [PNG] [PDF] [Imprimir] [WhatsApp] [Duplicar]│
└──────────────────────────────────────────────────────────────────┘
```

## Tarefas

- [ ] Cliente **cadastrado**: busca com autocomplete (código, nome ou telefone; a lista mostra `0045 – João Silva – (12) 9...`); ao escolher, preenche nome, telefone e endereço de entrega (editáveis só neste orçamento)
- [ ] Botão **+ Novo** cadastra o cliente sem sair da tela
- [ ] Cliente **avulso**: nome e telefone livres (telefone com o mesmo campo de DDD editável e validação da Fase 4), não cria cadastro; ao salvar, perguntar uma vez "Deseja cadastrar este cliente?" (opção "não perguntar de novo" nas configurações)
- [ ] Mostrar as observações internas do cliente cadastrado num aviso discreto (ex.: faixa amarela "Cliente pede tudo discriminado")
- [ ] **Retirada na loja** esconde/limpa os campos de entrega
- [ ] Grade de itens: Qtd → Un → Descrição → Unitário; Enter avança de célula; na descrição, digitar o **código** ou parte do nome do produto mostra o autocomplete do catálogo (`0012 – Placa de gesso 60x60 – R$ 12,50`) e preenche unidade e preço; coluna Cód. visível na grade (preço pode ser alterado só neste orçamento); descrição livre permitida
- [ ] Totais recalculados a cada alteração; linhas novas automáticas; Delete remove linha; Ctrl+↑/↓ reordena
- [ ] Frete com `CampoMoeda` (vazio ou 0 = sem frete)
- [ ] **Desconto opcional**: caixa "Desconto" desmarcada por padrão; ao marcar, escolhe R$ ou % e informa o valor; mostra o valor calculado em R$; bloqueia desconto maior que o subtotal
- [ ] **Validade opcional**: caixa "Validade" desmarcada por padrão; ao marcar, sugere os dias das configurações (editável) e mostra a data final
- [ ] Observações (multilinha, até ~300 caracteres, contador)
- [ ] **Salvar** (Ctrl+S): valida (nome obrigatório, ao menos 1 item, quantidades > 0), atribui número no primeiro salvamento
- [ ] Aviso ao fechar com alterações não salvas
- [ ] **Duplicar**: cria um novo orçamento (sem número) com os mesmos dados e data de hoje
- [ ] Botões Copiar imagem/PNG/PDF/Imprimir/WhatsApp visíveis mas desabilitados até a Fase 7
- [ ] Habilitar **Novo orçamento** na tela principal e **Novo orçamento para este cliente** na tela de clientes

## Testes

- [ ] Orçamento com cliente cadastrado, com avulso, com retirada
- [ ] Quantidade fracionada e frete
- [ ] Com e sem desconto (R$ e %), com e sem validade
- [ ] Alterar preço do item não muda o catálogo
- [ ] Cronometrar: orçamento de 5 itens do catálogo em menos de 1 minuto

## Pronto quando

Publicar v0.6.0 e fazer 3 orçamentos reais em paralelo com a folha de papel.

## Observações

