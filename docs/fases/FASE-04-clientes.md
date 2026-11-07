# Fase 4 — Cadastro de clientes

**Objetivo:** cadastrar e encontrar clientes rapidamente.

## Tarefas

- [x] `Forms\FormClientes.cs`: lista (**código**, nome, telefone, bairro, cidade), campo de busca no topo filtrando enquanto digita (código, nome ou telefone, sem acento), botões **Novo**, **Editar**, **Inativar/Reativar**, filtro "Mostrar inativos"
- [x] `Forms\FormClienteEditar.cs`: nome (obrigatório), telefone, telefone 2, endereço, número, bairro, cidade (padrão Pindamonhangaba), ponto de referência, observações internas (com aviso "não sai no orçamento")
- [x] Telefone com **DDD sempre visível e editável**: campo separado `(DD)` já preenchido com o DDD padrão (12, configurável), mais o número; para cliente de outro estado é só trocar o DDD
- [x] Validação: DDD precisa ser um DDD brasileiro válido (lista fixa da Anatel) e o número precisa ter 8 dígitos (fixo) ou 9 começando com 9 (celular); se não for válido, aviso ao salvar
- [x] Telefone gravado sempre completo (DDD + número), nunca sem DDD — o mesmo valor gera o link do WhatsApp na Fase 7
- [x] Aviso de possível duplicado ao salvar (mesmo telefone ou mesmo nome)
- [ ] Duplo clique abre edição; Enter na busca abre o primeiro resultado; Esc fecha
- [ ] Botão **Novo orçamento para este cliente** (desabilitado até a Fase 6)
- [ ] Habilitar o botão **Clientes** na tela principal
- [ ] Não excluir clientes, só inativar (orçamentos antigos podem apontar para eles)

## Testes

- [ ] Busca por "joao" encontra "João"
- [ ] Busca por "99244" encontra pelo telefone
- [x] Cliente com DDD 11 salva e exibe `(11) 98765-4321`; DDD `00` é recusado
- [ ] Busca por "45" encontra o cliente Cód. 0045
- [ ] Código aparece no título da tela de edição ("Cliente Cód. 0045")
- [ ] Cliente inativo não aparece na busca padrão

## Pronto quando

Publicar v0.4.0, atualizar a loja e cadastrar 3 clientes reais de teste.

## Observações

- Sessão interrompida a pedido do Lucca depois dos dois primeiros itens (`Forms\FormClientes.cs` e `Forms\FormClienteEditar.cs`). O próximo item é o do telefone com DDD visível e editável.
- Até lá, os campos Telefone e Telefone 2 de `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\FormClienteEditar.cs` são texto livre, sem DDD separado e sem validação. O botão **Clientes** da tela principal ainda não foi habilitado, então as telas só são exercitadas pelos testes automáticos: aguardando teste do Lucca.
- Os testes agora gravam em `C:\dev\orcamentos-ponto-do-gesso\.tmp\`, e `PONTODOGESSO_DADOS` troca a pasta de dados (`C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Servicos\Caminhos.cs`).
- Telefone: decidido gravar **só os dígitos** (DDD + número, ex.: `12992445402`); a formatação `(12) 99244-5402` é feita na exibição. Regras em `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Servicos\Telefone.cs` e o campo em `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Controles\CampoTelefone.cs`. Número vazio = sem telefone (o DDD é ignorado). DDD ou número inválido **impede o salvamento** com o aviso, em vez de só avisar e gravar. Telefone antigo, gravado sem DDD, aparece com o DDD padrão ao abrir a edição.
- Duplicado: compara o nome sem acento e sem maiúsculas (igual, não parecido) e qualquer um dos dois telefones, incluindo clientes inativos. Mostra até 5 clientes parecidos e pergunta "Salvar mesmo assim?" (o padrão é Não).
