# Fase 1 — Esqueleto do app e teste no Windows 7

**Objetivo:** eliminar cedo o maior risco do projeto — garantir que o exe, o SQLite, o PDFsharp e a conexão HTTPS funcionam no Windows 7 da loja — antes de escrever as funções.

## Tarefas

- [x] `Program.cs`: cultura `pt-BR` fixa (`CultureInfo.DefaultThreadCurrentCulture/UICulture`), `Application.EnableVisualStyles`, `SetHighDpiMode` **não** (não existe no net48) — usar `app.manifest` com `dpiAware` true/PM e testar
- [x] Tratamento global de erros (`Application.ThreadException`, `AppDomain.UnhandledException`): grava em `%APPDATA%\PontoDoGesso\logs\erros_AAAA-MM.log` e mostra mensagem amigável
- [x] `Servicos\Caminhos.cs`: pastas de dados, backups, logs, atualizacao (cria se não existirem)
- [x] `Forms\FormPrincipal.cs`: janela com o nome da loja, botões grandes (Novo orçamento, Histórico, Clientes, Produtos, Configurações) **desabilitados** por enquanto, rodapé com versão
- [x] Menu **Ajuda → Diagnóstico** (`Forms\FormDiagnostico.cs`) mostrando: versão do app, versão do Windows e Service Pack, 32/64 bits do SO, processo 32 bits, versão do .NET Framework (registro `Release`), caminho do banco; botão **Copiar informações**
- [x] Botão **Testar banco**: adicionar `System.Data.SQLite.Core`, criar banco temporário, gravar e ler
- [x] Botão **Testar PDF/PNG**: adicionar PDFsharp (variante GDI compatível com net48), gerar `teste.pdf` e `teste.png` com um texto e um retângulo laranja arredondado, em `Documentos\Orçamentos Ponto do Gesso\teste\`
- [x] Botão **Testar conexão com GitHub**: TLS 1.2 + GET em `https://api.github.com/repos/LuccaBueno/orcamentos-ponto-do-gesso` com timeout de 15 s; mostrar OK ou o erro completo
- [x] Travar as versões dos pacotes NuGet que passaram no teste e anotar em `C:\dev\orcamentos-ponto-do-gesso\docs\ARQUITETURA.md`
- [x] Gerar zip manualmente (pasta `bin\x86\Release\net48`) e publicar a **v0.1.0** como Release manual no GitHub

## Teste no PC da loja

Seguir `C:\dev\orcamentos-ponto-do-gesso\docs\INSTALACAO-WINDOWS7.md`, seções 1 a 4, e preencher:

| Teste | Resultado |
|---|---|
| Abre sem erro | |
| Diagnóstico (copiar e colar aqui) | |
| Testar banco | |
| Testar PDF/PNG | |
| Testar conexão com GitHub | |
| WhatsApp Web abre no navegador do PC da loja e aceita arrastar uma imagem (`teste.png`) para uma conversa | |

Também testar no seu PC (Windows 10/11) para garantir que nada depende só do Win7.

## Pronto quando

Os quatro testes do app passam no Windows 7 da loja e o teste do WhatsApp Web foi registrado (se falhar, anotar o navegador e a versão e decidir o plano B: Firefox mais recente que ainda rode no Win7, ou enviar pelo celular). Se o teste do GitHub falhar mesmo após o Windows Update, registrar e seguir — a Fase 2 terá o modo de atualização manual.

## Observações

**Situação em 06/11/2026:** tudo que dá para fazer no PC de desenvolvimento está pronto e a v0.1.0 está publicada. A fase continua em andamento até o teste no PC da loja.

Teste no PC de desenvolvimento (Windows 11 Pro 10.0.26200, 64 bits, .NET 4.8.1), com o conteúdo do zip extraído numa pasta limpa:

| Teste | Resultado |
|---|---|
| Abre sem erro | OK (`PontoDoGesso.exe` aberto de verdade; cria as pastas em `%APPDATA%\PontoDoGesso\`) |
| Diagnóstico | OK; processo 32 bits, sistema 64 bits |
| Testar banco | OK (SQLite 3.46.1) |
| Testar PDF/PNG | OK; PDF e PNG conferidos visualmente, com o mesmo desenho |
| Testar conexão com GitHub | OK (200) |
| Copiar informações | OK |

- O clique no menu e nos botões foi feito por um programa auxiliar que abriu os formulários do exe publicado, porque a sessão automatizada não recebia foco de teclado. Falta alguém usar com mouse e teclado.
- **PDFsharp:** a 6.5.0 mudou para AGPL-3.0 e exige licença em tempo de execução; ficou travado na 6.2.4 (MIT). Detalhes em `C:\dev\orcamentos-ponto-do-gesso\docs\ARQUITETURA.md`. As versões só podem ser consideradas aprovadas depois do teste no Windows 7.
- **Lição para a Fase 7:** `XGraphics.FromGraphics` ignora a resolução do bitmap e desenha 1 px por ponto. Para gerar PNG em alta resolução é preciso chamar `ScaleTransform(dpi / 72.0)`, como em `C:\dev\orcamentos-ponto-do-gesso\src\PontoDoGesso\Exportacao\DesenhoTeste.cs`.
- `dpiAware` `true/PM` no `app.manifest`: testado só a 100% de escala. No PC da loja, conferir se a janela fica legível.
- O zip da v0.1.0 ainda não tem o `PontoDoGesso.Atualizador.exe` (é vazio até a Fase 2) nem a pasta `assets\`. O `dados.db` também só passa a existir na Fase 3: o caminho aparece no Diagnóstico, mas o arquivo não é criado ainda.
- `C:\dev\orcamentos-ponto-do-gesso\assets\icone-whatsapp.png` ainda não foi colocado; será necessário na Fase 7.

