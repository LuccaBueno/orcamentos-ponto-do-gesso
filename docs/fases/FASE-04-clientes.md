# Fase 4 — Cadastro de clientes

**Objetivo:** cadastrar e encontrar clientes rapidamente.

## Tarefas

- [ ] `Forms\FormClientes.cs`: lista (**código**, nome, telefone, bairro, cidade), campo de busca no topo filtrando enquanto digita (código, nome ou telefone, sem acento), botões **Novo**, **Editar**, **Inativar/Reativar**, filtro "Mostrar inativos"
- [ ] `Forms\FormClienteEditar.cs`: nome (obrigatório), telefone, telefone 2, endereço, número, bairro, cidade (padrão Pindamonhangaba), ponto de referência, observações internas (com aviso "não sai no orçamento")
- [ ] Telefone com **DDD sempre visível e editável**: campo separado `(DD)` já preenchido com o DDD padrão (12, configurável), mais o número; para cliente de outro estado é só trocar o DDD
- [ ] Validação: DDD precisa ser um DDD brasileiro válido (lista fixa da Anatel) e o número precisa ter 8 dígitos (fixo) ou 9 começando com 9 (celular); se não for válido, aviso ao salvar
- [ ] Telefone gravado sempre completo (DDD + número), nunca sem DDD — o mesmo valor gera o link do WhatsApp na Fase 7
- [ ] Aviso de possível duplicado ao salvar (mesmo telefone ou mesmo nome)
- [ ] Duplo clique abre edição; Enter na busca abre o primeiro resultado; Esc fecha
- [ ] Botão **Novo orçamento para este cliente** (desabilitado até a Fase 6)
- [ ] Habilitar o botão **Clientes** na tela principal
- [ ] Não excluir clientes, só inativar (orçamentos antigos podem apontar para eles)

## Testes

- [ ] Busca por "joao" encontra "João"
- [ ] Busca por "99244" encontra pelo telefone
- [ ] Cliente com DDD 11 salva e exibe `(11) 98765-4321`; DDD `00` é recusado
- [ ] Busca por "45" encontra o cliente Cód. 0045
- [ ] Código aparece no título da tela de edição ("Cliente Cód. 0045")
- [ ] Cliente inativo não aparece na busca padrão

## Pronto quando

Publicar v0.4.0, atualizar a loja e cadastrar 3 clientes reais de teste.

## Observações

