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
- [x] `Exportacao\Impressao.cs`: `PrintDocument` + `PrintPreviewDialog` (pré-visualização) + `PrintDialog` (impressora e cópias), papel A4 retrato
- [x] **Copiar imagem**: PNG para a área de transferência (`Clipboard.SetImage`, em thread STA)
- [x] Nome dos arquivos e pasta conforme a ARQUITETURA; criar pastas `AAAA\MM`
- [x] Após salvar: mensagem com **Abrir arquivo** e **Abrir pasta** — a pasta abre no Explorer **com o arquivo já selecionado** (`explorer.exe /select,"<arquivo>"`), pronto para arrastar para o WhatsApp Web
- [x] Botão **Abrir pasta de orçamentos** também na tela principal
- [x] Botão **Abrir conversa no WhatsApp** (habilitado quando o orçamento tem telefone): salva o PNG se ainda não salvou, abre a pasta com o arquivo selecionado e abre `https://wa.me/55DDDNUMERO` no navegador padrão — basta arrastar o arquivo (ou `Ctrl+V` se usou Copiar imagem)
- [x] Link montado com o telefone completo gravado (55 + DDD + número); o botão mostra o número na dica ("Abrir conversa com (11) 98765-4321") para conferir antes; telefone inválido ou sem DDD mostra aviso em vez de abrir
- [x] Exportar exige o orçamento salvo (salva automaticamente se houver alterações, avisando)
- [x] Teste visual: gerar orçamentos com 3, 15 e 20 itens, com e sem desconto, com e sem validade e salvar as imagens em `C:\dev\orcamentos-ponto-do-gesso\docs\referencia\comparacoes\`

## Testes no Windows 7

- [ ] Fontes Times New Roman, Arial e Calibri disponíveis (fallback para Arial se faltar Calibri) — **aguardando teste do Lucca**
- [ ] PDF abre no leitor de PDF da loja — **aguardando teste do Lucca**
- [ ] Imprimir pelo botão **Imprimir** na impressora da loja (margens corretas, nada cortado) — **aguardando teste do Lucca**
- [ ] Arrastar o PNG salvo da pasta para o WhatsApp Web — **aguardando teste do Lucca**
- [ ] **Abrir conversa no WhatsApp** abre a conversa certa no navegador do PC da loja — **aguardando teste do Lucca**
- [ ] Colar a imagem copiada no WhatsApp Web — **aguardando teste do Lucca**
- [ ] Conferir que o número continua legível ao dar zoom na imagem recebida no celular, mas sem chamar atenção — **aguardando teste do Lucca**

## Pronto quando

Publicar v0.7.0; o orçamento impresso e o PNG enviado ficam claramente reconhecíveis como da loja. — **aguardando teste do Lucca** (sem tag nem release nesta sessão).

## Observações


- Fase concluída no código em 07/11/2026, sem tag nem release. Faltam os testes no PC da loja (fontes, leitor de PDF, impressora, WhatsApp Web, zoom no celular): aguardando teste do Lucca.
- Código em `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Exportacao\` (`DesenhoOrcamento.cs`, `ExportadorPng.cs`, `ExportadorPdf.cs`, `Impressao.cs`, `AreaTransferencia.cs`, `ArquivosOrcamento.cs`, `ExportacaoOrcamento.cs`, `DadosLoja.cs`). As ações dos botões ficam em `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\AcoesExportacao.cs`, que o histórico (Fase 8) também usa.
- **Comparação com a folha em papel:** imagens e PDFs em `C:\dev\orcamentos-ponto-do-gesso\docs\referencia\comparacoes\` (3 itens sem desconto; 3 itens retirada com validade; 15 itens com desconto, validade e observação; 20 itens com desconto; 35 itens em 2 páginas), todos com cliente e produtos fictícios. Para gerar de novo: `$env:PONTODOGESSO_COMPARACOES = "1"; dotnet test C:\dev\orcamentos-ponto-do-gesso\PontoDoGesso.sln --filter GerarComparacoes`.
- **Telefone e Entrega/Retirada na mesma linha** (o esboço do layout tinha uma linha só para Entrega/Retirada): economiza espaço e deixa mais lugar para itens. As caixinhas são marcadas com X.
- **Retirada na loja:** as linhas de endereço continuam desenhadas (para parecer a folha), com "Retirada na loja" no End: e o resto em branco.
- **Telefone da loja como na folha:** o DDD sai menor que o número. Sem `C:\dev\orcamentos-ponto-do-gesso\assets\icone-whatsapp.png` no repositório, o orçamento sai sem o ícone; quando o arquivo for colocado lá, o build copia para `assets\` ao lado do exe e o ícone passa a sair sozinho.
- **Rodapé da loja** em negrito laranja (como na folha); a linha da validade sai em preto, embaixo dele, na mesma faixa.
- **Observações** à esquerda dos totais, começando por "OBS:"; se forem longas, a célula do TOTAL estica para acompanhar (até 12 linhas).
- **Paginação:** cabem ~29 itens de uma linha na 1ª página e ~41 nas de continuação (com os totais e o rodapé, ~24 numa página só); o fecho (totais e rodapé) nunca fica sozinho numa página: leva junto ao menos o último item. "Página X de Y" sai junto do número, no canto inferior direito, em cinza.
- **Impressão:** pré-visualização própria (`C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Forms\FormImpressao.cs`) com **Imprimir…** (abre a janela do Windows para escolher impressora e cópias; Ctrl+P também) e navegação entre páginas. O deslocamento da margem física da impressora é compensado, para as margens de 12 mm valerem no papel. Testado só com pré-visualização, sem imprimir.
- A impressora padrão do app (chave `impressao.impressora`) é usada se ainda existir; senão, a padrão do Windows.
- **Copiar imagem** põe na área de transferência o bitmap e também o PNG (alguns navegadores só aceitam um dos dois).
- **Exportar com alterações** salva antes; o aviso "O orçamento foi salvo (Nº 0001) antes de exportar." vai na mesma mensagem da exportação, para não abrir duas janelas.
- **WhatsApp:** o botão fica habilitado quando há número digitado; a dica mostra "Abrir conversa com (11) 98765-4321" ou o erro do telefone. Telefone antigo sem DDD (8 ou 9 dígitos) mostra aviso em vez de abrir. Nos testes, o navegador e o Explorer não são abertos.
- Erro ao gravar (pasta sem permissão, arquivo aberto em outro programa) mostra aviso com a pasta e não fecha a tela.
