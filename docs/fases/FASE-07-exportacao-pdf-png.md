# Fase 7 — Exportação (PNG, copiar imagem, PDF) e impressão

**Objetivo:** gerar o orçamento no visual da folha em papel. Especificação completa em `C:\dev\orcamentos-ponto-do-gesso\docs\LAYOUT-ORCAMENTO.md`.

## Tarefas

- [x] `Exportacao\DesenhoOrcamento.cs`: desenha o orçamento num `XGraphics` em milímetros (**A4 retrato**), com paginação, lendo dados da loja das configurações
- [x] Cabeçalho com moldura arredondada, título, subtítulo, telefone com ícone (`assets\icone-whatsapp.png`, opcional), endereço em 2 linhas
- [x] Número do orçamento **discreto** (7 pt, cinza claro, canto inferior direito); códigos de cliente e produto **não** aparecem, conforme `C:\dev\orcamentos-ponto-do-gesso\docs\LAYOUT-ORCAMENTO.md`
- [x] Campos Data, Nome, Tel, End, Nº, Bairro, Cidade, P.Ref, Entrega/Retirada
- [x] Tabela só com os itens preenchidos, cabeçalho com fundo `#FBE4D5`, quebra de descrição em 2 linhas
- [x] SUBTOTAL e DESCONTO **só quando houver desconto**; FRETE (`—` se não houver), TOTAL (fundo `#FBE4D5`), OBS, rodapé; validade **só quando ligada no orçamento**
- [x] Muitos itens: continuação em nova página com "Página X de Y" (PDF/impressão) / imagem única mais alta (PNG)
- [x] `Exportacao\ExportadorPng.cs`: A4 a 150 dpi, fundo branco
- [x] `Exportacao\ExportadorPdf.cs`: A4 retrato
- [ ] `Exportacao\Impressao.cs`: `PrintDocument` + `PrintPreviewDialog` (pré-visualização) + `PrintDialog` (impressora e cópias), papel A4 retrato
- [ ] **Copiar imagem**: PNG para a área de transferência (`Clipboard.SetImage`, em thread STA)
- [x] Nome dos arquivos e pasta conforme a ARQUITETURA; criar pastas `AAAA\MM`
- [ ] Após salvar: mensagem com **Abrir arquivo** e **Abrir pasta** — a pasta abre no Explorer **com o arquivo já selecionado** (`explorer.exe /select,"<arquivo>"`), pronto para arrastar para o WhatsApp Web
- [ ] Botão **Abrir pasta de orçamentos** também na tela principal
- [ ] Botão **Abrir conversa no WhatsApp** (habilitado quando o orçamento tem telefone): salva o PNG se ainda não salvou, abre a pasta com o arquivo selecionado e abre `https://wa.me/55DDDNUMERO` no navegador padrão — basta arrastar o arquivo (ou `Ctrl+V` se usou Copiar imagem)
- [ ] Link montado com o telefone completo gravado (55 + DDD + número); o botão mostra o número na dica ("Abrir conversa com (11) 98765-4321") para conferir antes; telefone inválido ou sem DDD mostra aviso em vez de abrir
- [ ] Exportar exige o orçamento salvo (salva automaticamente se houver alterações, avisando)
- [x] Teste visual: gerar orçamentos com 3, 15 e 20 itens, com e sem desconto, com e sem validade e salvar as imagens em `C:\dev\orcamentos-ponto-do-gesso\docs\referencia\comparacoes\`

## Testes no Windows 7

- [ ] Fontes Times New Roman, Arial e Calibri disponíveis (fallback para Arial se faltar Calibri)
- [ ] PDF abre no leitor de PDF da loja
- [ ] Imprimir pelo botão **Imprimir** na impressora da loja (margens corretas, nada cortado)
- [ ] Arrastar o PNG salvo da pasta para o WhatsApp Web
- [ ] **Abrir conversa no WhatsApp** abre a conversa certa no navegador do PC da loja
- [ ] Colar a imagem copiada no WhatsApp Web
- [ ] Conferir que o número continua legível ao dar zoom na imagem recebida no celular, mas sem chamar atenção

## Pronto quando

Publicar v0.7.0; o orçamento impresso e o PNG enviado ficam claramente reconhecíveis como da loja.

## Observações

