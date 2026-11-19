# Layout do orçamento (PDF e PNG)

Referência: folha em papel atual, em `docs/referencia/folha-orcamento-atual.pdf` (duas vias numa folha A4 paisagem só para economizar papel; no app o orçamento é **uma via em A4 retrato**).

O orçamento gerado deve **parecer a folha em papel**, para o cliente reconhecer, com alguns campos novos.

## Página do orçamento (base para PDF, PNG e impressão)

Tamanho: **A4 retrato (210 × 297 mm)**, margens de 12 mm. Foco é o uso **digital** (PNG/WhatsApp), com impressão como opção. Desenhada uma única vez em `Exportacao\DesenhoOrcamento.cs` usando `XGraphics` (PDFsharp):
- PDF: desenha numa página PDF.
- PNG: desenha num `Bitmap` via `XGraphics.FromGraphics(...)` a **150 dpi** (≈ 1240 × 1754 px, legível no celular e leve para o WhatsApp), fundo branco.
- Impressão: desenha no `Graphics` do `PrintDocument` via `XGraphics.FromGraphics(...)`.

```
╭──────────────────────────────────────────────╮
│              PONTO DO GESSO                  │  ← serifada (Times New Roman), grande
│            Gesso e Ferramentas               │  ← sem serifa (Arial)
│ (12) 99244-5402 [ícone]   Rua do Cardoso,    │
│                           104 – Alto do      │
│                           Cardoso - Pinda... │
╰──────────────────────────────────────────────╯
   Nome:_______________________ Tel: (12) _______     ← mesma ordem da folha em papel
   End:_________________________________ Nº ______
   Bairro:_______________ Cidade:_________________    ← campos novos (não existem na folha)
   P.Ref:__________________________ Data: 06/11/2026
   Entrega ☐   Retirada na loja ☐                      ← campo novo
┌───────┬─────────────────────┬─────────┬───────┐
│ QUANT │    DISCRIMINAÇÃO    │ UNITÁRIO│TOTAL R$│  ← fundo claro e faixa clara grossa embaixo
├───────┼─────────────────────┼─────────┼───────┤
│ 10 un │ Placa de gesso 60x60│   12,50 │ 125,00│
│ ...   │ ...                 │         │       │  ← só as linhas com itens (sem linhas vazias)
├───────┼─────────────────────┼─────────┼───────┤
│       │                     │SUBTOTAL:│ 125,00│  ← SUBTOTAL e DESCONTO só aparecem
│       │                     │DESC. 5%:│  -6,25│    se houver desconto aplicado
│       │                     │ FRETE:  │  20,00│
├───────┴─────────────────────┼─────────┴───────┤
│ OBS: ... (só se houver)     │ TOTAL: R$ 138,75│  ← célula TOTAL com fundo claro
│ As entregas são realizadas  │                 │
│ apenas até o térreo.        │                 │
│ Orçamento válido por 7 dias │                 │  ← só se a validade foi ligada
└─────────────────────────────┴─────────────────┘
```

### Cores e fontes

| Elemento | Valor |
|---|---|
| Laranja principal (texto, linhas da tabela e dos campos) | `#C55A11`, linhas de 0,5 pt |
| Moldura do cabeçalho | `#ED7D31` (laranja mais vivo, como na folha), 1 pt, cantos de 7 mm, 60 mm de altura |
| Faixa sob o cabeçalho da tabela | `#F4B183`, 1,5 pt |
| Fundo do cabeçalho da tabela e da célula TOTAL | `#FBE4D5` |
| Texto dos dados preenchidos | preto `#000000` |
| Título "PONTO DO GESSO" | Times New Roman 43 pt |
| Subtítulo "Gesso e Ferramentas" | Arial 23 pt |
| Telefone e endereço da loja | Calibri: DDD 15,5 pt, número 25 pt, endereço 17 pt (diminui sozinho se não couber) |
| Rótulos e valores dos campos | Calibri 12 pt |
| Cabeçalho da tabela | Calibri 13 pt negrito |
| Itens | Calibri 11 pt |
| FRETE:/TOTAL: e valor do total | Calibri 13 pt negrito / 14 pt negrito |
| "As entregas…" / OBS e validade | Calibri 12,5 pt negrito laranja / 11 pt preto |

Sem a Calibri (Windows 7 sem Office) tudo que é Calibri sai em Arial.

### Regras

- Rótulos (Nome:, Tel:, End:, Nº, P.Ref, Data:) em laranja; valores preenchidos em preto sobre a linha.
- **Número do orçamento — discreto:** `Nº 0123` em Arial 7 pt, cinza claro `#A6A6A6`, no **canto inferior direito da página**, abaixo do rodapé (na continuação, junto de "Página X de Y"). Não aparece no cabeçalho nem em destaque; serve para a loja localizar o orçamento quando a imagem voltar pelo WhatsApp.
- **Códigos de cliente e de produto não aparecem** no PDF, PNG ou impressão — só dentro do app.
- Colunas da tabela (largura relativa, medidas na folha): QUANT 16% · DISCRIMINAÇÃO 52% · UNITÁRIO 18% · TOTAL R$ 14%.
- Quantidade exibida com a unidade quando houver (`10 un`, `2,5 m²`). Valores no formato `1.234,56`; o TOTAL com `R$`.
- Descrição longa quebra em até 2 linhas na mesma célula; se passar disso, é cortada com `…`.
- **Sem linhas vazias:** como o foco é digital, a tabela mostra só os itens preenchidos e o rodapé (frete/total) vem logo abaixo. A página fica com espaço em branco no fim quando há poucos itens.
- **Muitos itens (cabem ~21 na primeira página e ~38 nas seguintes):** o PDF e a impressão continuam em nova página com cabeçalho resumido ("Orçamento Nº 0123 — continuação") e numeração "Página 1 de 2"; o PNG vira uma imagem só, mais alta.
- Retirada na loja: imprime "Retirada na loja" no lugar dos dados de entrega vazios.
- Observações: na última linha da tabela, à esquerda do TOTAL, antes do "As entregas…"; se vazia, não imprime o rótulo OBS.
- **Desconto:** as linhas `SUBTOTAL` e `DESCONTO` só são desenhadas quando o orçamento tem desconto. Sem desconto, o rodapé da tabela fica igual à folha em papel (só FRETE e TOTAL). Rótulo `DESC.:` para valor em R$ e `DESC. 5%:` para percentual; valor com sinal de menos.
- **Frete:** sem frete, imprime `—` na célula (mantém a linha, como na folha).
- **Validade:** só imprime se foi ligada naquele orçamento, com a data final calculada.
- Ícone ao lado do telefone: arquivo `assets\icone-whatsapp.png`, recortado da própria folha atual (`folha-orcamento-atual.pdf`). Se o arquivo não existir, desenha só o telefone.
- Dados da empresa (nome, subtítulo, telefone, endereço, rodapé e CNPJ) vêm da tela **Empresa** do app.

### CNPJ da empresa (opcional, por orçamento)

Na tela de orçamento há a caixa **"Mostrar CNPJ da empresa neste orçamento"**, **desmarcada** em todo orçamento novo e também ao duplicar. Sem CNPJ cadastrado na tela Empresa, a caixa fica desligada com a dica "Cadastre o CNPJ na tela Empresa".

- **Desmarcada:** o cabeçalho sai exatamente como antes (o teste `CnpjTestes.SemMarcarOCabecalhoFicaExatamenteComoAntes` compara os pixels).
- **Marcada:** `CNPJ: 00.000.000/0000-00` sai no cabeçalho, abaixo do endereço, na mesma fonte e cor do endereço (Calibri 17 pt laranja, diminui junto com o endereço se não couber). As três linhas dividem o espaço que o endereço ocupava: linhas de base a 39,0 / 46,7 / 54,4 mm do topo da moldura (sem CNPJ: endereço a 43,7 e 54,4 mm, na altura do telefone, como na folha).

```
│ (12) 99244-5402 [ícone]   Rua do Cardoso, 104 – Alto do  │
│                           Cardoso - Pindamonhangaba - SP │
│                           CNPJ: 11.222.333/0001-81       │  ← só com a caixa marcada
```

- O orçamento guarda se o CNPJ foi mostrado e **uma cópia do CNPJ usado** (`orcamentos.mostrar_cnpj` e `orcamentos.empresa_cnpj`): reabrir ou exportar de novo um orçamento antigo sai igual, mesmo que o CNPJ da empresa mude ou seja apagado depois.
- Comparação com e sem CNPJ (CNPJ fictício 11.222.333/0001-81): `docs\referencia\comparacoes\cnpj\` — `sem-cnpj.png`, `com-cnpj.png`, `com-cnpj.pdf` e `sem-e-com-cnpj.png` (lado a lado).

## Opções de exportação

| Opção | Saída |
|---|---|
| **PNG** | A4 retrato, 150 dpi, fundo branco — salvo na pasta de exportação |
| **Copiar imagem** | Mesmo PNG na área de transferência, para colar no WhatsApp Web com `Ctrl+V` |
| **PDF** | A4 retrato — para e-mail ou guardar |
| **Imprimir** | Abre a janela de impressão do Windows (escolher impressora e cópias), com **pré-visualização** antes |

Após salvar: botão para **abrir o arquivo** e outro para **abrir a pasta com o arquivo selecionado**, para arrastar direto para o WhatsApp Web.

## Validação visual

Na Fase 7, gerar orçamentos de teste com 3, 15 e 35 itens (este último força 2 páginas) e comparar com a folha em papel. As imagens de comparação ficam em `docs\referencia\comparacoes\`.

Para gerar de novo (só dados fictícios): `$env:PONTODOGESSO_COMPARACOES = "1"; dotnet test C:\dev\orcamentos-ponto-do-gesso\PontoDoGesso.sln --filter GerarComparacoes`. Além das imagens soltas, o teste monta `docs\referencia\comparacoes\lado-a-lado\` com a primeira via da folha em papel (`docs\referencia\folha-orcamento-atual-via1.png`, ampliada de A5 para A4) à esquerda e o PNG novo à direita, para 3, 15 e 35 itens e um caso com desconto, validade e observação. O mesmo teste gera `docs\referencia\comparacoes\cnpj\` (orçamento sem e com o CNPJ).

## Revisão visual (18/11/2026)

A primeira via da folha em papel foi convertida em imagem e medida (posições, alturas e cores dos pixels) e o orçamento gerado foi ajustado para ficar nas mesmas proporções. A via de papel tem 141 mm de largura útil e o A4 tem 186 mm, então as **fontes ficaram ~1,2× maiores que na folha** e as **distâncias verticais do cabeçalho ~1,12×** (um pouco menos, para sobrar espaço para os itens).

### O que mudou

| Item | Antes | Agora (como na folha) |
|---|---|---|
| Moldura do cabeçalho | 40 mm de altura, laranja escuro `#C55A11`, cantos de 4,5 mm | 60 mm, laranja vivo `#ED7D31`, cantos de 7 mm |
| Título / subtítulo | Times 30 pt / Arial 14 pt | Times 43 pt (68% da largura, igual à folha) / Arial 23 pt |
| Telefone e endereço | Arial 11–15 pt | Calibri, como na folha: DDD 15,5 pt, número 25 pt, endereço 17 pt; telefone e 1ª linha do endereço na mesma linha de base |
| Ícone do WhatsApp | não saía (arquivo não existia) | recortado da própria folha em papel (`assets\icone-whatsapp.png`), 6,8 mm |
| Campos do cliente | Nome/Data, Tel/Entrega, End/Nº, Bairro/Cidade, P.Ref | ordem da folha: Nome/Tel, End/Nº, (Bairro/Cidade), P.Ref/Data, (Entrega/Retirada); Calibri 12 pt; a linha de cada campo vai até o rótulo seguinte |
| Colunas da tabela | 14/52/17/17 % | 16/52/18/14 % (medidas na folha) |
| Cabeçalho da tabela | Calibri 9,5 pt, linha fina embaixo | Calibri 13 pt negrito, faixa clara grossa (`#F4B183`, 1,5 pt) embaixo |
| Linhas da tabela | 0,6 pt, itens em 10 pt, linhas de 6 mm | 0,5 pt, itens em 11 pt, linhas de 6,5 mm |
| FRETE/TOTAL | rótulos em 10 pt; área vazia à esquerda | rótulos em 13 pt negrito; nas linhas de totais continua só a divisória QUANT/DISCRIMINAÇÃO, como na linha do FRETE da folha |
| "As entregas…" | faixa separada abaixo da tabela, 9,5 pt | dentro da última linha da tabela, à esquerda da célula TOTAL, em 12,5 pt negrito — igual à folha |

Junto com a revisão foi corrigido um defeito da paginação do PDF/impressão: quando os itens cabiam na página mas os totais não, o último item era descartado (ex.: 25 itens com desconto e validade saíam com 24). Agora o último item vai para a página seguinte junto com os totais.

### O que ficou diferente de propósito

- **A4 retrato com uma via**, sem as linhas vazias da folha: a tabela termina no último item e a página fica com espaço em branco embaixo quando há poucos itens.
- **Campos novos** que a folha não tem: Bairro/Cidade e Entrega/Retirada na loja (com X na opção escolhida).
- **Número do orçamento** discreto no canto inferior direito (cinza, 7 pt), fora da folha.
- **SUBTOTAL e DESC.** só com desconto; **validade** e **OBS** só quando existem, na mesma célula do "As entregas…".
- **TOTAL com o valor impresso** ao lado do rótulo (na folha o espaço é para escrever à mão).
- **Linhas da tabela mais baixas que na folha ampliada** (6,5 mm em vez de ~8,5 mm) e cabeçalho um pouco menos alto (60 mm em vez de ~71 mm), para caber ~21 itens na primeira página do PDF. Com isso, 20 itens com desconto e validade passaram a ocupar 2 páginas no PDF/impressão (o PNG continua sendo uma imagem só).
- **"Cardoso - Pindamonhangaba - SP"** com espaço depois do hífen (a folha tem "Cardoso -Pindamonhangaba"); o texto vem das configurações da loja.
- **"Tel:"** sem os parênteses de preencher à mão, porque o telefone já sai formatado; **"P.Ref:"** com dois-pontos, como na segunda via da folha.
- O título "PONTO DO GESSO" na folha parece levemente condensado; aqui sai em Times New Roman normal com a mesma largura relativa, então as letras ficam um pouco mais baixas.
