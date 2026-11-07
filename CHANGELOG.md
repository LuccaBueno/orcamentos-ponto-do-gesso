# Changelog

Formato baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/); versões seguem [SemVer](https://semver.org/lang/pt-BR/).

## [Não lançado]

### Adicionado
- Fase 3: banco SQLite em `%APPDATA%\PontoDoGesso\dados.db` com migrações embutidas, backup antes de migrar e configurações padrão.
- Fase 3: domínio, repositórios, regras de cálculo, formatação pt-BR e busca sem acento por nome, telefone e código.
- Fase 3: Diagnóstico mostra a versão do banco e o modo do diário; o teste de banco passa pelo Dapper (travado na 2.1.35).
- Fase 4: cadastro de clientes com busca enquanto digita (código, nome ou telefone, sem acento), inativar/reativar, telefone com DDD separado e validado (lista da Anatel), aviso de possível duplicado e botão **Clientes** habilitado na tela principal.
- Fase 5: catálogo de produtos com busca, preço editável direto na lista, "Salvar e cadastrar outro", reajuste em lote (% ou R$, com prévia e confirmação), data da última mudança de preço (migração 002) e botão **Produtos** habilitado.
- Fase 6: tela de orçamento com cliente cadastrado (autocomplete por código, nome ou telefone, "+ Novo", aviso das observações internas) ou avulso (pergunta se quer cadastrar), entrega ou retirada, grade de itens pelo teclado com autocomplete do catálogo, desconto opcional (R$ ou %), frete, validade opcional, observações, Ctrl+S, aviso de alterações não salvas e Duplicar. Botões **Novo orçamento** (tela principal) e **Novo orçamento para este cliente** (clientes) habilitados; exportação continua desabilitada até a Fase 7.

## [0.1.0] - 2026-11-06

Versão de teste de compatibilidade: ainda não faz orçamentos.

### Adicionado
- Fase 1: janela principal com os botões ainda desabilitados, cultura pt-BR fixa, manifesto com suporte ao Windows 7 e tratamento global de erros com log em `%APPDATA%\PontoDoGesso\logs\`.
- Fase 1: menu Ajuda → Diagnóstico com dados do sistema e os testes de banco, PDF/PNG e conexão com o GitHub.
- Fase 1: versões travadas de `System.Data.SQLite.Core` 1.0.119 e `PDFsharp-GDI` 6.2.4 (última versão MIT do PDFsharp).
- Fase 0: solução `PontoDoGesso.sln`, projetos net48 x86 (app, atualizador e testes) e CI no GitHub Actions.
- Kit inicial: README, CLAUDE.md, arquitetura, layout do orçamento, atualização, instalação no Windows 7 e plano de 11 fases.
- Planejamento: desconto opcional (R$ ou %), validade opcional por orçamento e filtros do histórico (data, cadastrado/avulso, frete, desconto).
- Planejamento: orçamento em A4 retrato (sem 2 vias), foco digital e botão Imprimir com pré-visualização.
- Planejamento: número do orçamento discreto no canto inferior da imagem/PDF.
- Planejamento: códigos sequenciais para clientes e produtos cadastrados, usados só dentro do app (não saem no orçamento).
- Planejamento: produtos sem planilha — edição de preço na grade, cadastro em sequência e reajuste em lote dentro do app.
- Nome do app definido: **Ponto do Gesso** (repositório `LuccaBueno/orcamentos-ponto-do-gesso`, nome pensado para o portfólio; executável `PontoDoGesso.exe`).
- Planejamento: botão Abrir conversa no WhatsApp, orientações e testes de antivírus, modo de demonstração e README de portfólio; licença MIT.
