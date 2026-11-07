# Plano de fases

Cada fase termina com o app compilando, testes passando e algo testável. A atualização automática vem cedo (Fase 2) de propósito: a partir dela, toda fase nova chega ao PC da loja pelo próprio botão do app.

| Fase | Arquivo | Entrega | Versão | Status |
|---|---|---|---|---|
| 0 | [FASE-00-repositorio-e-ci.md](FASE-00-repositorio-e-ci.md) | Solução, projetos, CI no GitHub | — | ✅ Concluída |
| 1 | [FASE-01-esqueleto-win7.md](FASE-01-esqueleto-win7.md) | Janela principal + diagnóstico; **teste no Windows 7** | v0.1.0 | 🟨 Em andamento |
| 2 | [FASE-02-atualizacao.md](FASE-02-atualizacao.md) | Botão de atualização + release automática | v0.9.0 / v0.9.1 | ✅ Concluída no código (v0.9.0 e v0.9.1 publicadas; atualização real testada no PC do Lucca; falta o teste no PC da loja e com o antivírus) |
| 3 | [FASE-03-banco-e-calculo.md](FASE-03-banco-e-calculo.md) | Banco, migrações, regras de cálculo com testes | v0.9.0 | ✅ Concluída (publicada na v0.9.0; falta testar no Windows 7) |
| 4 | [FASE-04-clientes.md](FASE-04-clientes.md) | Cadastro de clientes | v0.9.0 | ✅ Concluída (publicada na v0.9.0; falta o teste do Lucca) |
| 5 | [FASE-05-produtos.md](FASE-05-produtos.md) | Catálogo simples de produtos | v0.9.0 | ✅ Concluída (publicada na v0.9.0; falta o teste do Lucca) |
| 6 | [FASE-06-tela-orcamento.md](FASE-06-tela-orcamento.md) | Tela de orçamento completa (salvar) | v0.9.0 | ✅ Concluída (publicada na v0.9.0; falta o teste do Lucca) |
| 7 | [FASE-07-exportacao-pdf-png.md](FASE-07-exportacao-pdf-png.md) | PNG, copiar imagem, PDF e impressão (A4 retrato) | v0.9.0 | ✅ Concluída (publicada na v0.9.0; falta o teste do Lucca) |
| 8 | [FASE-08-historico.md](FASE-08-historico.md) | Histórico com filtros (data, cadastrado/avulso, frete, desconto) | v0.9.0 | ✅ Concluída (publicada na v0.9.0; falta o teste do Lucca) |
| 9 | [FASE-09-configuracoes-backup.md](FASE-09-configuracoes-backup.md) | Configurações e backup/restauração | v0.9.0 | ✅ Concluída (publicada na v0.9.0; falta o teste do Lucca) |
| 10 | [FASE-10-polimento-v1.md](FASE-10-polimento-v1.md) | Atalhos, acabamento, manual, **v1.0.0** | v1.0.0 | ⬜ Pendente |

Legenda: ⬜ Pendente · 🟨 Em andamento · ✅ Concluída

**Fase atual: nenhuma em andamento.** As Fases 2 a 9 estão concluídas no código e publicadas na **v0.9.0** (versão completa, para instalar na loja); a **v0.9.1** existe só para testar a atualização pelo botão. As Fases 1 e 2 esperam o teste no PC da loja com Windows 7: o roteiro único da visita está em `C:\dev\orcamentos-ponto-do-gesso\docs\RELATORIO-SESSAO.md` (seção "Roteiro da visita à loja"). Próxima fase: 10.

## PC da loja

| Item | Valor |
|---|---|
| Windows | 7 — *a conferir* |
| Service Pack 1 | *a conferir* |
| 32/64 bits | *a conferir* (o app é x86 e roda nos dois) |
| .NET Framework 4.8 instalado | *a conferir* |
| Conexão HTTPS com GitHub funciona | *a testar na Fase 1* |
| WhatsApp Web funciona no navegador | *a testar na Fase 1* |
| Antivírus instalado | *a conferir* |

## Fora do escopo (por enquanto)

Controle de estoque, vendas/pedidos, financeiro, nota fiscal, múltiplos PCs/sincronização, usuários e senhas, envio automático por WhatsApp.

## Decisões em aberto

Registrar aqui o que for decidido durante o desenvolvimento.

- Ordem das fases: as Fases 3 a 9 foram adiantadas; a Fase 2 veio depois delas, antes do teste no PC da loja.
- Versões da Fase 2 (07/11/2026): **v0.9.0** (completa, a que vai para a loja) e **v0.9.1** (só para testar a atualização), no lugar de v0.2.0/v0.2.1. As versões v0.2.0 a v0.8.0 não existem.
- Numeração: orçamento, cliente cadastrado e produto cadastrado têm número sequencial próprio (`0001`), começando do 1. No orçamento final só aparece o número do orçamento, discreto no rodapé; os códigos de cliente e produto ficam só no app.
- Validade: **opcional por orçamento** (desligada por padrão). Ao ligar, sugere 7 dias (configurável).
- Desconto: **opcional**, em R$ ou %, só sobre os produtos (não sobre o frete). Só aparece no PDF/PNG se aplicado.
- Status do orçamento (Pendente/Aprovado/Recusado): **a decidir** pelo Lucca.
- Licença: MIT (`C:\dev\orcamentos-ponto-do-gesso\LICENSE`).
- PDFsharp: travado na 6.2.4, a última versão MIT. A 6.5.0 passou para AGPL-3.0 (ver `C:\dev\orcamentos-ponto-do-gesso\docs\ARQUITETURA.md`).
