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

- [x] Cliente **cadastrado**: busca com autocomplete (código, nome ou telefone; a lista mostra `0045 – João Silva – (12) 9...`); ao escolher, preenche nome, telefone e endereço de entrega (editáveis só neste orçamento)
- [x] Botão **+ Novo** cadastra o cliente sem sair da tela
- [x] Cliente **avulso**: nome e telefone livres (telefone com o mesmo campo de DDD editável e validação da Fase 4), não cria cadastro; ao salvar, perguntar uma vez "Deseja cadastrar este cliente?" (opção "não perguntar de novo" nas configurações)
- [x] Mostrar as observações internas do cliente cadastrado num aviso discreto (ex.: faixa amarela "Cliente pede tudo discriminado")
- [x] **Retirada na loja** esconde/limpa os campos de entrega
- [x] Grade de itens: Qtd → Un → Descrição → Unitário; Enter avança de célula; na descrição, digitar o **código** ou parte do nome do produto mostra o autocomplete do catálogo (`0012 – Placa de gesso 60x60 – R$ 12,50`) e preenche unidade e preço; coluna Cód. visível na grade (preço pode ser alterado só neste orçamento); descrição livre permitida
- [x] Totais recalculados a cada alteração; linhas novas automáticas; Delete remove linha; Ctrl+↑/↓ reordena
- [x] Frete com `CampoMoeda` (vazio ou 0 = sem frete)
- [x] **Desconto opcional**: caixa "Desconto" desmarcada por padrão; ao marcar, escolhe R$ ou % e informa o valor; mostra o valor calculado em R$; bloqueia desconto maior que o subtotal
- [x] **Validade opcional**: caixa "Validade" desmarcada por padrão; ao marcar, sugere os dias das configurações (editável) e mostra a data final
- [x] Observações (multilinha, até ~300 caracteres, contador)
- [x] **Salvar** (Ctrl+S): valida (nome obrigatório, ao menos 1 item, quantidades > 0), atribui número no primeiro salvamento
- [x] Aviso ao fechar com alterações não salvas
- [x] **Duplicar**: cria um novo orçamento (sem número) com os mesmos dados e data de hoje
- [x] Botões Copiar imagem/PNG/PDF/Imprimir/WhatsApp visíveis mas desabilitados até a Fase 7
- [x] Habilitar **Novo orçamento** na tela principal e **Novo orçamento para este cliente** na tela de clientes

## Testes

- [x] Orçamento com cliente cadastrado, com avulso, com retirada
- [x] Quantidade fracionada e frete
- [x] Com e sem desconto (R$ e %), com e sem validade
- [x] Alterar preço do item não muda o catálogo
- [ ] Cronometrar: orçamento de 5 itens do catálogo em menos de 1 minuto — **aguardando teste do Lucca** (precisa de uma pessoa com o cronômetro na mão)

## Pronto quando

Publicar v0.6.0 e fazer 3 orçamentos reais em paralelo com a folha de papel. — **aguardando teste do Lucca** (sem tag nem release nesta sessão).

## Observações

- Tela em `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\FormOrcamento.cs`; autocomplete em `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Controles\ListaSugestoes.cs` e grade em `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Controles\GradeItens.cs`. Testes em `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso.Testes\TelaOrcamentoTestes.cs`.
- Autocomplete próprio (e não o `AutoCompleteMode` do WinForms), porque o do WinForms só acha pelo começo do texto e aqui é preciso achar no meio do nome, pelo código e pelo telefone. A lista abre com o primeiro resultado marcado: **Enter escolhe**; **Esc fecha mantendo o texto digitado** (descrição livre); Tab também sai mantendo o texto.
- Ao escolher um produto, a quantidade vazia vira 1 e o cursor vai para a **Qtd da próxima linha** (o mais rápido para lançar vários itens). Para mudar o preço só neste orçamento, é clicar no Unitário.
- Se a descrição de um item do catálogo for trocada à mão, o item deixa de apontar para o produto (o Cód. some).
- Cliente cadastrado: o rádio "Cadastrado" vem marcado; salvar com ele marcado sem ter escolhido ninguém mostra "Busque e escolha o cliente cadastrado, ou marque Avulso.". O **+ Novo** usa o texto da busca como nome inicial.
- Cliente avulso: a pergunta "Deseja cadastrar este cliente?" aparece só no **primeiro** salvamento daquele orçamento. A chave `orcamento.perguntar_cadastro_avulso` (`1` = pergunta, `0` = não pergunta) já existe; a tela para mudar fica para a Fase 9 (Configurações).
- Retirada na loja limpa o endereço; voltar para Entrega preenche de novo com o endereço do cliente cadastrado (ou só a cidade padrão, se for avulso).
- Desconto ou frete inválido, ou desconto maior que o subtotal, aparecem em vermelho embaixo do total enquanto se digita, e bloqueiam o salvamento.
- **Duplicar** transforma a própria janela no orçamento novo (sem número, data de hoje), em vez de abrir outra janela. Se houver alterações não salvas num orçamento já numerado, pergunta antes se quer salvar.
- Desempenho: a montagem da tela levava ~350 ms; com o layout suspenso durante a montagem caiu para ~65 ms no PC de desenvolvimento. Exibir a janela ainda leva ~300 ms (custo do WinForms para ~90 controles). **Aguardando teste do Lucca**: ver se a tela abre rápido no PC da loja.
- O GroupBox com AutoSize cortava linhas (o nome do cliente sumia ao esconder o aviso); trocado por blocos com título.
- O botão **Novo orçamento para este cliente** fica desabilitado para cliente inativo.
- Corrigido junto: os botões das listas de clientes e produtos às vezes olhavam a linha anterior (o `SelectionChanged` dispara antes de a linha atual mudar); agora também atualizam no `CurrentCellChanged`.
- Fase concluída no código em 07/11/2026, sem tag nem release. Falta o Lucca: cronometrar 5 itens do catálogo em menos de 1 minuto, fazer 3 orçamentos reais em paralelo com o papel e ver se a tela abre rápido no PC da loja: aguardando teste do Lucca.
