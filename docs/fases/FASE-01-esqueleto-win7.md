# Fase 1 — Esqueleto do app e teste no Windows 7

**Objetivo:** eliminar cedo o maior risco do projeto — garantir que o exe, o SQLite, o PDFsharp e a conexão HTTPS funcionam no Windows 7 da loja — antes de escrever as funções.

## Tarefas

- [ ] `Program.cs`: cultura `pt-BR` fixa (`CultureInfo.DefaultThreadCurrentCulture/UICulture`), `Application.EnableVisualStyles`, `SetHighDpiMode` **não** (não existe no net48) — usar `app.manifest` com `dpiAware` true/PM e testar
- [ ] Tratamento global de erros (`Application.ThreadException`, `AppDomain.UnhandledException`): grava em `%APPDATA%\PontoDoGesso\logs\erros_AAAA-MM.log` e mostra mensagem amigável
- [ ] `Servicos\Caminhos.cs`: pastas de dados, backups, logs, atualizacao (cria se não existirem)
- [ ] `Forms\FormPrincipal.cs`: janela com o nome da loja, botões grandes (Novo orçamento, Histórico, Clientes, Produtos, Configurações) **desabilitados** por enquanto, rodapé com versão
- [ ] Menu **Ajuda → Diagnóstico** (`Forms\FormDiagnostico.cs`) mostrando: versão do app, versão do Windows e Service Pack, 32/64 bits do SO, processo 32 bits, versão do .NET Framework (registro `Release`), caminho do banco; botão **Copiar informações**
- [ ] Botão **Testar banco**: adicionar `System.Data.SQLite.Core`, criar banco temporário, gravar e ler
- [ ] Botão **Testar PDF/PNG**: adicionar PDFsharp (variante GDI compatível com net48), gerar `teste.pdf` e `teste.png` com um texto e um retângulo laranja arredondado, em `Documentos\Orçamentos Ponto do Gesso\teste\`
- [ ] Botão **Testar conexão com GitHub**: TLS 1.2 + GET em `https://api.github.com/repos/LuccaBueno/orcamentos-ponto-do-gesso` com timeout de 15 s; mostrar OK ou o erro completo
- [ ] Travar as versões dos pacotes NuGet que passaram no teste e anotar em `C:\dev\orcamentos-ponto-do-gesso\docs\ARQUITETURA.md`
- [ ] Gerar zip manualmente (pasta `bin\x86\Release\net48`) e publicar a **v0.1.0** como Release manual no GitHub

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

