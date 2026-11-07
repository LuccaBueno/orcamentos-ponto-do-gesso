# Changelog

Formato baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/); versões seguem [SemVer](https://semver.org/lang/pt-BR/).

## [Não lançado]

## [0.9.0] - 2026-11-07

Primeira versão completa do **Ponto do Gesso**, para instalar no PC da loja. Roda no Windows 7 SP1 (32 e 64 bits) com o .NET Framework 4.8. Os dados ficam em `%APPDATA%\PontoDoGesso\` e nunca saem do computador.

### O que o app faz
- **Orçamentos:** cliente cadastrado (busca por código, nome ou telefone) ou avulso, entrega ou retirada, itens digitados pelo teclado com sugestões do catálogo, desconto opcional (R$ ou %, só nos produtos), frete, validade opcional e observações. Ctrl+S salva; dá para duplicar um orçamento.
- **Folha do orçamento** em A4 retrato, igual ao modelo em papel: PNG, PDF, impressão com pré-visualização, **Copiar imagem** para colar no WhatsApp Web e botão **WhatsApp**, que abre a conversa com o cliente.
- **Histórico** com filtros (período, cadastrado ou avulso, cliente, com ou sem frete, com ou sem desconto, texto), ordenação, soma dos valores e ações: abrir, duplicar, exportar, imprimir e excluir.
- **Clientes** e **produtos** com códigos próprios, busca sem acento, inativação, reajuste de preços em lote e aviso de cliente duplicado.
- **Configurações:** dados da loja no cabeçalho, validade sugerida, cidade e DDD padrão, pasta dos orçamentos e impressora.
- **Backup:** automático todo dia (guarda os últimos 30), backup na hora, cópia para pendrive, pasta extra e restauração conferida.
- **Atualização pelo próprio app:** **Verificar atualizações** no rodapé, aviso de versão nova ao abrir (no máximo 1 vez por dia), download conferido por SHA-256, backup antes de atualizar e volta automática para a versão anterior se algo der errado.
- **Ajuda → Diagnóstico** para conferir o Windows, o .NET, o banco, o PDF/PNG e a conexão com o GitHub.

### Adicionado
- Fase 2: botão **Verificar atualizações** e aviso **Nova versão X!** no rodapé da tela principal; tela com a versão instalada, a nova e o que mudou, com **Atualizar agora**, **Depois** e **Abrir página de downloads**.
- Fase 2: download do `.zip` com barra de progresso, conferência do `.zip.sha256` (se não bater, nada é alterado), backup `dados_..._antes-de-atualizar.db` e `PontoDoGesso.Atualizador.exe`, que troca a pasta do app guardando a versão anterior em `<pasta>.anterior` e volta para ela se a nova não instalar ou não abrir. Log em `%APPDATA%\PontoDoGesso\logs\atualizacao_AAAA-MM.log`.
- Fase 2: se o Atualizador for apagado ou bloqueado pelo antivírus, o app avisa e abre a página de downloads (atualização manual).
- Fase 2: releases automáticas no GitHub ao criar a tag `vX.Y.Z`: o workflow confere a tag com a versão, compila, testa, gera o zip e o `.sha256` (`scripts\empacotar.ps1`) e usa a seção da versão no CHANGELOG como texto da release.
- Fase 2: metadados completos nos exes (título, empresa, autor, copyright e versão) para reduzir alarmes falsos de antivírus.
- Fase 3: banco SQLite em `%APPDATA%\PontoDoGesso\dados.db` com migrações embutidas, backup antes de migrar e configurações padrão.
- Fase 3: domínio, repositórios, regras de cálculo, formatação pt-BR e busca sem acento por nome, telefone e código.
- Fase 3: Diagnóstico mostra a versão do banco e o modo do diário; o teste de banco passa pelo Dapper (travado na 2.1.35).
- Fase 4: cadastro de clientes com busca enquanto digita (código, nome ou telefone, sem acento), inativar/reativar, telefone com DDD separado e validado (lista da Anatel), aviso de possível duplicado e botão **Clientes** habilitado na tela principal.
- Fase 5: catálogo de produtos com busca, preço editável direto na lista, "Salvar e cadastrar outro", reajuste em lote (% ou R$, com prévia e confirmação), data da última mudança de preço (migração 002) e botão **Produtos** habilitado.
- Fase 6: tela de orçamento com cliente cadastrado (autocomplete por código, nome ou telefone, "+ Novo", aviso das observações internas) ou avulso (pergunta se quer cadastrar), entrega ou retirada, grade de itens pelo teclado com autocomplete do catálogo, desconto opcional (R$ ou %), frete, validade opcional, observações, Ctrl+S, aviso de alterações não salvas e Duplicar. Botões **Novo orçamento** (tela principal) e **Novo orçamento para este cliente** (clientes) habilitados; exportação continua desabilitada até a Fase 7.
- Fase 7: orçamento no visual da folha em papel (A4 retrato) em PNG (150 dpi), PDF e impressão com pré-visualização; Copiar imagem para colar no WhatsApp Web; WhatsApp abre a conversa e mostra o PNG na pasta; arquivos em `<pasta>\AAAA\MM\Orc-0123_Nome_dd-MM-aaaa.png`; botão **Abrir pasta de orçamentos** na tela principal.
- Fase 8: histórico de orçamentos com filtros combináveis (período, cadastrado/avulso, cliente específico, com/sem frete, com/sem desconto, texto por nº, código, nome ou telefone), ordenação pelo cabeçalho, carregamento aos poucos, quantidade e soma no rodapé, validade vencida em vermelho e ações Abrir, Duplicar, Copiar imagem, PNG, PDF, Imprimir e Excluir. Botões **Histórico** (tela principal) e **Orçamentos deste cliente** (clientes) habilitados.
- Fase 9: configurações em abas — dados da loja com pré-visualização do cabeçalho, validade sugerida, cidade e DDD padrão, pergunta do cliente avulso, pasta de exportação e impressora padrão. Backup automático diário ao abrir (API de backup do SQLite, guarda os últimos 30), **Fazer backup agora**, **Copiar backup para…**, pasta extra opcional e **Restaurar backup** validado (guarda os dados atuais antes e reinicia o app). Botão **Configurações** habilitado.

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
