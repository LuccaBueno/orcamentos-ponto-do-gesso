# Relatório da sessão — 07/11/2026 (Fases 7, 8 e 9)

Sessão sem acompanhamento: Fases 7, 8 e 9. **Nenhuma tag nem release foi criada.** Tudo foi enviado para `main`, com o CI verde. Ao final: **317 testes passando** (eram 244), e os builds Debug e Release compilam sem avisos. O app compilado em Release abre normalmente com os dados numa pasta de teste e faz o backup automático na abertura.

## O que foi feito

### Fase 7 — Exportação (concluída no código)

- O orçamento sai no visual da folha em papel, em **A4 retrato**. O mesmo desenho gera o PNG, o PDF e a impressão.
  - Desenho: `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Exportacao\DesenhoOrcamento.cs`
  - PNG (150 dpi, 1240 px de largura): `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Exportacao\ExportadorPng.cs`
  - PDF: `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Exportacao\ExportadorPdf.cs`
  - Impressão: `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Exportacao\Impressao.cs` e a pré-visualização `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\FormImpressao.cs`
- Com muitos itens, o PDF e a impressão continuam em outra página ("continuação", "Página 1 de 2"). O PNG vira uma imagem só, mais alta.
- Na tela de orçamento, os botões **Copiar imagem**, **PNG**, **PDF**, **Imprimir** e **WhatsApp** estão funcionando. Se houver alterações, o orçamento é salvo antes, com aviso.
- Depois de salvar o arquivo, aparece uma janela com **Abrir arquivo** e **Abrir pasta**. A pasta abre com o arquivo já selecionado.
- **WhatsApp:** salva o PNG, mostra o arquivo na pasta e abre `https://wa.me/55DDDNUMERO`. A dica do botão mostra o número para conferir.
- Os arquivos ficam em `<pasta de exportação>\AAAA\MM\Orc-0123_Nome-do-Cliente_dd-MM-aaaa.png` (ou `.pdf`).
- Botão **Abrir pasta de orçamentos** na tela principal.
- **Imagens de comparação** com a folha em papel, só com dados fictícios: `C:\dev\orcamentos-ponto-do-gesso\docs\referencia\comparacoes\`
  - 3 itens;
  - 3 itens com retirada e validade;
  - 15 itens com desconto, validade e observação;
  - 20 itens com desconto;
  - 35 itens (2 páginas).

### Fase 8 — Histórico (concluída no código)

- Tela: `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\FormHistorico.cs`
- Filtros combináveis:
  - período (Hoje, 7 dias, 30 dias, este mês, mês passado, personalizado, todos);
  - cadastrado/avulso e cliente específico (autocomplete);
  - com/sem frete e com/sem desconto;
  - texto: nº do orçamento, código do cliente, nome sem acento ou telefone.
- Clique no cabeçalho ordena. Carrega 200 por vez, e o próximo lote vem sozinho ao rolar a lista.
- O rodapé mostra a quantidade e a soma de tudo o que o filtro encontrou. A validade vencida aparece em vermelho.
- Ações: Abrir, Duplicar, Copiar imagem, PNG, PDF, Imprimir e Excluir (com confirmação; o número não volta a ser usado).
- Botões **Histórico** (tela principal) e **Orçamentos deste cliente** (lista de clientes) habilitados.
- Busca com 5.000 orçamentos: ~65 ms no PC de desenvolvimento.

### Fase 9 — Configurações e backup (concluída no código)

- Tela: `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\FormConfiguracoes.cs`. Tem 4 abas:
  - **Loja**, com pré-visualização do cabeçalho;
  - **Orçamento**: validade sugerida, cidade padrão, DDD padrão e pergunta do cliente avulso;
  - **Exportação**: pasta e impressora padrão;
  - **Backup**.
- Backup: `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Servicos\Backup.cs`
  - backup automático ao abrir o app, se o último tiver mais de 24 h, guardando os últimos 30;
  - **Fazer backup agora** e **Copiar backup para…**;
  - pasta extra opcional (pendrive ou HD);
  - **Restaurar backup**: valida o arquivo, guarda os dados atuais antes e reinicia o app.
- Botão **Configurações** habilitado.

## Decisões tomadas sozinho

Todas estão registradas em "Observações" de cada fase:

- `C:\dev\orcamentos-ponto-do-gesso\docs\fases\FASE-07-exportacao-pdf-png.md`
- `C:\dev\orcamentos-ponto-do-gesso\docs\fases\FASE-08-historico.md`
- `C:\dev\orcamentos-ponto-do-gesso\docs\fases\FASE-09-configuracoes-backup.md`

As mais importantes:

1. **Telefone e Entrega/Retirada na mesma linha** do orçamento, em vez de uma linha só para Entrega/Retirada. Sobra mais espaço para itens.
2. **Retirada na loja:** as linhas de endereço continuam desenhadas, como na folha, com "Retirada na loja" no campo End:.
3. **Sem ícone do WhatsApp:** o arquivo `C:\dev\orcamentos-ponto-do-gesso\assets\icone-whatsapp.png` não existe, então o orçamento sai sem o ícone. Quando o arquivo for colocado lá, o ícone passa a sair sozinho, sem mexer no código.
4. **Exportar com alterações salva antes.** O aviso vai na mesma mensagem da exportação, para não abrir duas janelas.
5. **Copiar imagem** põe na área de transferência o bitmap e também o PNG (alguns navegadores só aceitam um dos dois).
6. **Pré-visualização de impressão própria**, em português, com **Imprimir…** (abre a janela do Windows para escolher impressora e cópias).
7. **Histórico:**
   - o período padrão é **Todos**;
   - **Orçamentos deste cliente** é um botão na lista de clientes que abre o histórico filtrado, em vez de uma aba dentro da edição do cliente;
   - os filtros ficam lembrados enquanto o app estiver aberto.
8. **Nomes dos backups:** `_auto`, `_manual`, `_copia` e `_antes-de-restaurar`. Só os `_auto` contam para o limite de 30; os outros nunca são apagados pelo app.
9. **Restaurar** copia o backup para dentro do banco atual pela API do SQLite (sem apagar arquivo com o app aberto) e reinicia o app.
10. **Novas chaves de configuração:**
    - `cliente.cidade_padrao`: a cidade, que antes era fixa no código, agora é configurável;
    - `backup.pasta_extra`.

    As duas estão documentadas em `C:\dev\orcamentos-ponto-do-gesso\docs\ARQUITETURA.md`.
11. A **pasta extra de backup** é gravada assim que é escolhida, sem precisar do botão Salvar.

## Pendências

- **Nenhuma tag/release** (v0.7.0, v0.8.0 e v0.9.0 não foram publicadas), como pedido.
- A impressão foi testada só com a pré-visualização. Nada foi impresso de verdade.
- O navegador, o Explorer e a área de transferência não foram usados nos testes: essas ações são substituídas por versões falsas.
- As Fases 1 e 2 continuam esperando o teste no PC da loja com Windows 7.

## O que o Lucca precisa testar

Abrir o app com uma pasta de dados de teste, no PowerShell:

```powershell
$env:PONTODOGESSO_DADOS = "C:\dev\orcamentos-ponto-do-gesso\.tmp\dados-teste"
& "C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\bin\x86\Debug\net48\PontoDoGesso.exe"
```

Antes, em **Configurações → Exportação**, escolher uma pasta de teste para os orçamentos (ex.: `C:\dev\orcamentos-ponto-do-gesso\.tmp\exportados`). Assim, os arquivos não vão para Documentos.

### Exportação (Fase 7)

1. Comparar as imagens de `C:\dev\orcamentos-ponto-do-gesso\docs\referencia\comparacoes\` com a folha em papel e dizer o que ajustar (tamanhos, posições, cores).
2. Se quiser o ícone do WhatsApp no cabeçalho, salvar o ícone como `C:\dev\orcamentos-ponto-do-gesso\assets\icone-whatsapp.png` e compilar de novo.
3. Fazer um orçamento e testar **PNG**, **PDF**, **Abrir arquivo** e **Abrir pasta** (o arquivo tem que vir selecionado).
4. **Copiar imagem** e colar com Ctrl+V numa conversa do WhatsApp Web.
5. **WhatsApp:** conferir a dica com o número e se a conversa certa abre no navegador.
6. **Imprimir** na impressora da loja: conferir margens e se nada fica cortado.
7. Mandar o PNG para o próprio celular e dar zoom: o "Nº 0001" tem que ficar legível, mas discreto.
8. No Windows 7:
   - conferir as fontes (sem Calibri, o app usa Arial);
   - abrir o PDF no leitor de PDF da loja.

### Histórico (Fase 8)

1. Abrir o **Histórico** e testar cada filtro e algumas combinações (ex.: avulsos + com frete + mês passado).
2. Clicar nos cabeçalhos para ordenar.
3. Na lista de clientes, **Orçamentos deste cliente**.
4. Excluir um orçamento e conferir que o próximo salvo não reaproveita o número.
5. No PC da loja, ver se a busca é rápida.

### Configurações e backup (Fase 9)

1. Mudar o rodapé da loja, ver a pré-visualização e conferir num PDF novo.
2. Mudar a cidade padrão e o DDD padrão e conferir num cliente novo.
3. **Fazer backup agora**.
4. **Copiar backup para…** um pendrive.
5. Escolher o pendrive como **pasta extra**.
6. Restaurar um backup antigo:
   - o app reinicia;
   - os orçamentos voltam como estavam;
   - existe um arquivo `_antes-de-restaurar.db` em `C:\dev\orcamentos-ponto-do-gesso\.tmp\dados-teste\backups\`.
7. Tentar restaurar um arquivo qualquer (ex.: um .txt renomeado para .db): tem que recusar.

Todos os itens que dependem disso estão desmarcados nos arquivos das fases com "aguardando teste do Lucca".

---

# Relatório da sessão — 07/11/2026 (Fases 4, 5 e 6)

Sessão sem acompanhamento: continuação das Fases 4, 5 e 6. **Nenhuma tag nem release foi criada.** Tudo foi enviado para `main`, com o CI verde em todos os commits. Ao final: 244 testes passando (eram 137), e os builds Debug e Release compilam sem avisos.

## O que foi feito

### Fase 4 — Clientes (concluída no código)

- Telefone com **DDD em campo separado**, já preenchido com o DDD padrão (`cliente.ddd_padrao`, 12) e editável.
  - Campo: `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Controles\CampoTelefone.cs`
  - Regras: `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Servicos\Telefone.cs`
- Validação: DDD da lista da Anatel (67 DDDs) e número com 8 dígitos (fixo) ou 9 começando com 9 (celular).
- Telefone gravado sempre completo, só com dígitos (ex.: `12992445402`).
- Aviso de possível duplicado ao salvar: mesmo nome, sem acento, ou mesmo telefone.
- Duplo clique abre a edição, Enter na busca abre o primeiro resultado e Esc fecha.
- Botões **Clientes** (tela principal) e **Novo orçamento para este cliente** habilitados.

### Fase 5 — Produtos (concluída no código)

- `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\FormProdutos.cs`: lista com busca. O **preço é editável direto na grade**: Enter grava e desce.
- `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\FormProdutoEditar.cs`: tem o botão **Salvar e cadastrar outro**, que mantém a unidade.
- `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\FormReajuste.cs`: reajuste em lote, em % ou R$, para cima ou para baixo, com prévia "antes → depois" e confirmação.
- `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Controles\CampoMoeda.cs`: aceita `12,5`, `12,50` e `1.250,00`.
- "Preço atualizado em": nova migração `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Dados\Migracoes\002_preco_atualizado_em.sql`.
- Botão **Produtos** habilitado.

### Fase 6 — Tela de orçamento (concluída no código)

- `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\FormOrcamento.cs`. Tem:
  - cliente cadastrado com autocomplete e **+ Novo**;
  - cliente avulso, com a pergunta "Deseja cadastrar este cliente?";
  - faixa amarela com as observações internas do cliente;
  - entrega ou retirada;
  - grade de itens pelo teclado, com autocomplete do catálogo;
  - desconto, frete e validade;
  - observações com contador;
  - Ctrl+S, aviso ao fechar com alterações não salvas e **Duplicar**.
- Peças reaproveitáveis:
  - `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Controles\ListaSugestoes.cs`: autocomplete que acha em qualquer parte do texto;
  - `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Controles\GradeItens.cs`: Enter avança, Delete remove a linha e Ctrl+↑/↓ reordena.
- Botões de exportação visíveis, mas desabilitados até a Fase 7.
- **Novo orçamento** habilitado na tela principal.

## Decisões tomadas sozinho

Todas estão registradas em "Observações" de cada fase:

- `C:\dev\orcamentos-ponto-do-gesso\docs\fases\FASE-04-clientes.md`
- `C:\dev\orcamentos-ponto-do-gesso\docs\fases\FASE-05-produtos.md`
- `C:\dev\orcamentos-ponto-do-gesso\docs\fases\FASE-06-tela-orcamento.md`

As mais importantes:

1. **Telefone inválido impede o salvamento** (em vez de só avisar). Número vazio = sem telefone.
2. **Data do preço em coluna própria** (`preco_atualizado_em`, migração 002), porque `atualizado_em` também muda quando só o nome é corrigido. Coluna documentada em `C:\dev\orcamentos-ponto-do-gesso\docs\ARQUITETURA.md`.
3. **Reajuste em lote:** com mais de um produto selecionado, reajusta só os selecionados. Com um ou nenhum, reajusta todos os que a busca está mostrando. Reajuste que deixaria preço negativo fica bloqueado.
4. **Autocomplete:** a lista abre com o primeiro resultado marcado. **Enter escolhe**; **Esc fecha mantendo o texto** (para descrição livre).
5. **Depois de escolher um produto**, a quantidade vazia vira 1 e o cursor vai para a Qtd da próxima linha.
6. **Duplicar** transforma a própria janela no orçamento novo (sem número, data de hoje), em vez de abrir outra janela.
7. **Cliente avulso:** a pergunta de cadastro aparece só no primeiro salvamento. Para desligá-la, a nova chave `orcamento.perguntar_cadastro_avulso` vale `0`; a tela para mudar a chave fica para a Fase 9.
8. **Preço vazio não é aceito** no cadastro de produto ("Informe o preço (pode ser 0,00)").

Os itens de tela da Fase 5 foram num **commit só**, porque a lista depende da edição e do reajuste para compilar.

## Correções feitas no caminho

- Nas listas de clientes e produtos, os botões (Editar, Inativar, Novo orçamento) às vezes olhavam a linha anterior. Corrigido.
- Na tela de orçamento, o GroupBox com AutoSize cortava o nome do cliente ao esconder o aviso. Trocado por blocos com título.
- Os totais também quebravam linha e ficavam fora da tela. Corrigido.
- A montagem da tela de orçamento levava ~350 ms. Com o layout suspenso durante a montagem, caiu para ~65 ms.

## Pendências

- **Nenhuma tag/release** (v0.4.0, v0.5.0 e v0.6.0 não foram publicadas), como pedido.
- Abrir a janela de orçamento ainda leva ~300 ms no PC de desenvolvimento: é o custo do WinForms para ~90 controles. No PC da loja pode ser mais lento.
- As Fases 1 e 2 continuam esperando o teste no PC da loja com Windows 7.

## O que o Lucca precisa testar

Abrir o app compilado em:

`C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\bin\x86\Debug\net48\PontoDoGesso.exe`

Para não misturar com os dados reais, dá para apontar os dados para uma pasta de teste antes de abrir, no PowerShell:

```powershell
$env:PONTODOGESSO_DADOS = "C:\dev\orcamentos-ponto-do-gesso\.tmp\dados-teste"
& "C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\bin\x86\Debug\net48\PontoDoGesso.exe"
```

### Clientes (Fase 4)

1. Cadastrar 3 clientes reais de teste, um deles com DDD 11.
2. Tentar DDD `00`: tem que recusar.
3. Cadastrar outro com o mesmo telefone: tem que avisar que pode ser duplicado.
4. Na lista: duplo clique abre a edição, Enter na busca abre o primeiro resultado e Esc fecha.

### Produtos (Fase 5)

1. Cadastrar a lista real da loja com **Salvar e cadastrar outro**.
2. Mudar um preço direto na lista (digitar e Enter).
3. Selecionar 2 produtos com Ctrl+clique e reajustar 5%. Conferir a prévia: R$ 12,50 vira R$ 13,13.

### Orçamento (Fase 6)

1. **Cronometrar** um orçamento de 5 itens do catálogo. A meta é menos de 1 minuto.
2. Fazer 3 orçamentos reais em paralelo com a folha de papel: cadastrado, avulso e retirada; com desconto em R$ e em %; com validade.
3. Conferir que a faixa amarela aparece para cliente com observações.
4. Fechar com alterações não salvas: tem que perguntar.
5. Duplicar um orçamento salvo: tem que ganhar o próximo número ao salvar.
6. Ver se a tela de orçamento abre rápido **no PC da loja**.

Todos os itens que dependem disso estão desmarcados nos arquivos das fases com "aguardando teste do Lucca".
