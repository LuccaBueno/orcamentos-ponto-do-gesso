# Ponto do Gesso — app de orçamentos

[![build](https://github.com/LuccaBueno/orcamentos-ponto-do-gesso/actions/workflows/build.yml/badge.svg)](https://github.com/LuccaBueno/orcamentos-ponto-do-gesso/actions/workflows/build.yml)

App desktop para fazer orçamentos da loja **Ponto do Gesso** (Gesso e Ferramentas — Pindamonhangaba/SP) de forma rápida, com cadastro de clientes, catálogo simples de produtos e exportação em **PDF** e **PNG** no mesmo visual da folha de orçamento em papel.

> Escopo atual: **somente orçamentos**. Sem controle de estoque, vendas ou financeiro (por enquanto).

## Principais funções

- Orçamento para **cliente cadastrado** ou **cliente avulso** (só nome e telefone, sem salvar cadastro)
- Dados de entrega: endereço, número, bairro, cidade, ponto de referência
- Itens com quantidade, descrição, valor unitário e total (com sugestão automática a partir do catálogo de produtos)
- Frete, desconto opcional (em R$ ou %; só aparece no orçamento se for aplicado), validade opcional, campo de observações e total geral
- Foco digital: **copiar a imagem** do orçamento para colar direto no WhatsApp Web, salvar em **PNG** ou **PDF** (A4 retrato)
- Botão **Abrir conversa no WhatsApp** com o número do cliente, com o orçamento já salvo e a pasta aberta para arrastar
- Opção de **imprimir** com pré-visualização
- Histórico de orçamentos com filtros por data, cliente (cadastrado ou avulso) e com/sem frete; reabrir, duplicar e exportar de novo
- Botão **Verificar atualizações**, que baixa a versão nova direto das Releases deste repositório
- Backup e restauração do banco de dados

## Compatibilidade

| Item | Requisito |
|---|---|
| Sistema | Windows 7 SP1, 8.1, 10 e 11 (32 ou 64 bits) |
| Runtime | .NET Framework 4.8 (já vem no Windows 10/11; no Windows 7 precisa instalar uma vez — ver [docs/INSTALACAO-WINDOWS7.md](docs/INSTALACAO-WINDOWS7.md)) |
| Instalação | Portátil: descompactar a pasta e abrir `PontoDoGesso.exe` |

## Tecnologia

C# + WinForms sobre .NET Framework 4.8, SQLite para os dados, PDFsharp para PDF/PNG. Detalhes em [docs/ARQUITETURA.md](docs/ARQUITETURA.md).

## Documentação

| Documento | Conteúdo |
|---|---|
| [docs/COMO-COMECAR.md](docs/COMO-COMECAR.md) | Preparar o PC de desenvolvimento e criar o repositório |
| [docs/ARQUITETURA.md](docs/ARQUITETURA.md) | Stack, estrutura de pastas, banco de dados, regras de cálculo |
| [docs/LAYOUT-ORCAMENTO.md](docs/LAYOUT-ORCAMENTO.md) | Especificação visual do PDF/PNG (baseada na folha em papel) |
| [docs/ATUALIZACAO.md](docs/ATUALIZACAO.md) | Como funciona o botão de atualização e como publicar versões |
| [docs/INSTALACAO-WINDOWS7.md](docs/INSTALACAO-WINDOWS7.md) | Instalar e testar no PC da loja com Windows 7 |
| [docs/fases/](docs/fases/) | Plano de desenvolvimento em fases |
| [CHANGELOG.md](CHANGELOG.md) | Histórico de versões |

## Status

Em desenvolvimento — ver a fase atual em [docs/fases/README.md](docs/fases/README.md).
