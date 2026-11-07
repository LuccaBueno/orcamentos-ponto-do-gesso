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
 Nome: ______________________     Data: 06/11/2026
 Tel: (12) _______
 End: _______________________________  Nº ______
 Bairro: ______________  Cidade: _______________
 P.Ref: _________________________________________
 Entrega  ☐ / Retirada na loja ☐
┌──────┬──────────────────────┬─────────┬────────┐
│QUANT │    DISCRIMINAÇÃO     │ UNITÁRIO│TOTAL R$│  ← cabeçalho com fundo claro
├──────┼──────────────────────┼─────────┼────────┤
│ 10 un│ Placa de gesso 60x60 │   12,50 │ 125,00 │
│ ...  │ ...                  │         │        │  ← só as linhas com itens (sem linhas vazias)
├──────┴──────────────────────┼─────────┼────────┤
│                             │SUBTOTAL:│ 125,00 │  ← SUBTOTAL e DESCONTO só aparecem
│ OBS: ...                    │DESC. 5%:│  -6,25 │    se houver desconto aplicado
│                             │ FRETE:  │  20,00 │
│                             ├─────────┴────────┤
│                             │ TOTAL: R$ 138,75 │
├─────────────────────────────┴──────────────────┤
│ As entregas são realizadas apenas até o térreo.│
│ Orçamento válido por 7 dias (até 13/11/2026).  │  ← só se a validade foi ligada
└────────────────────────────────────────────────┘
```

### Cores e fontes

| Elemento | Valor |
|---|---|
| Laranja principal (texto, linhas, bordas) | `#C55A11` |
| Fundo do cabeçalho da tabela e da célula TOTAL | `#FBE4D5` |
| Texto dos dados preenchidos | preto `#000000` |
| Título "PONTO DO GESSO" | Times New Roman, ~26 pt |
| Subtítulo, telefone, endereço | Arial, 11–13 pt |
| Rótulos e tabela | Calibri 9–10 pt (fallback Arial se não existir) |
| Moldura do cabeçalho | retângulo com cantos arredondados, linha laranja 1 pt |

### Regras

- Rótulos (Nome:, Tel:, End:, Nº, P.Ref, Data:) em laranja; valores preenchidos em preto sobre a linha.
- **Número do orçamento — discreto:** `Nº 0123` em Arial 7 pt, cinza claro `#A6A6A6`, no **canto inferior direito da página**, abaixo do rodapé (na continuação, junto de "Página X de Y"). Não aparece no cabeçalho nem em destaque; serve para a loja localizar o orçamento quando a imagem voltar pelo WhatsApp.
- **Códigos de cliente e de produto não aparecem** no PDF, PNG ou impressão — só dentro do app.
- Colunas da tabela (largura relativa): QUANT 14% · DISCRIMINAÇÃO 52% · UNITÁRIO 17% · TOTAL R$ 17%.
- Quantidade exibida com a unidade quando houver (`10 un`, `2,5 m²`). Valores no formato `1.234,56`; o TOTAL com `R$`.
- Descrição longa quebra em até 2 linhas na mesma célula; se passar disso, é cortada com `…`.
- **Sem linhas vazias:** como o foco é digital, a tabela mostra só os itens preenchidos e o rodapé (frete/total) vem logo abaixo. A página fica com espaço em branco no fim quando há poucos itens.
- **Muitos itens (cabem ~28 por página):** o PDF e a impressão continuam em nova página com cabeçalho resumido ("Orçamento Nº 0123 — continuação") e numeração "Página 1 de 2"; o PNG vira uma imagem só, mais alta.
- Retirada na loja: imprime "Retirada na loja" no lugar dos dados de entrega vazios.
- Observações: área à esquerda do FRETE/TOTAL; se vazia, não imprime o rótulo OBS.
- **Desconto:** as linhas `SUBTOTAL` e `DESCONTO` só são desenhadas quando o orçamento tem desconto. Sem desconto, o rodapé da tabela fica igual à folha em papel (só FRETE e TOTAL). Rótulo `DESC.:` para valor em R$ e `DESC. 5%:` para percentual; valor com sinal de menos.
- **Frete:** sem frete, imprime `—` na célula (mantém a linha, como na folha).
- **Validade:** só imprime se foi ligada naquele orçamento, com a data final calculada.
- Ícone ao lado do telefone: arquivo `assets\icone-whatsapp.png` fornecido pelo Lucca (o mesmo usado na folha atual). Se o arquivo não existir, desenha só o telefone.

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
