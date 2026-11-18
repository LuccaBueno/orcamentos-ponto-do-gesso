# Instalação no PC da loja (Windows 7)

## 1. Conferir a versão do Windows (32 ou 64 bits e Service Pack)

1. Menu Iniciar → clicar com o botão direito em **Computador** → **Propriedades**.
2. Anotar:
   - **Edição do Windows**: deve aparecer **Service Pack 1**. Se não aparecer, instalar o SP1 pelo Windows Update antes de continuar.
   - **Tipo de sistema**: 32 bits ou 64 bits. (O app funciona nos dois; anote só para referência.)
3. Registrar o resultado em `C:\dev\orcamentos-ponto-do-gesso\docs\fases\README.md`, na seção "PC da loja".

## 2. Instalar o .NET Framework 4.8 (uma vez só)

No primeiro teste na loja (18/11/2026) esta foi a parte difícil: a mensagem do Windows aparece em inglês e o site da Microsoft oferece várias versões parecidas.

> ⚠️ Tem que ser o **.NET Framework 4.8 Runtime**.
> **Não** baixe o **4.8.1** (não existe para o Windows 7) nem o **Developer Pack** (é para programadores e não faz o app abrir).

1. Baixar pelo **link direto** do instalador offline do 4.8 Runtime (arquivo `ndp48-x86-x64-allos-enu.exe`, serve para 32 e 64 bits):
   **https://go.microsoft.com/fwlink/?linkid=2088631**
   Se o PC da loja não tiver internet, baixar em outro PC e levar num pendrive.
2. Executar o arquivo (botão direito → **Executar como administrador**). As telas são em inglês: aceitar a licença (*I have read and accept the license terms*) → **Install**.
3. Reiniciar o PC quando ele pedir.
4. Se o instalador reclamar de atualização faltando, rodar o Windows Update (ou instalar a atualização indicada na mensagem) e tentar de novo.

Conferir se instalou: Painel de Controle → Programas e Recursos → deve aparecer "Microsoft .NET Framework 4.8".

O instalador do Ponto do Gesso confere isso sozinho: sem o .NET 4.8 ele mostra a mensagem "Instale primeiro o .NET Framework 4.8 (versão Runtime, NÃO a 4.8.1 nem o Developer Pack)", com o link acima, e não instala.

## 3. Instalar o app

1. Baixar o instalador: https://github.com/LuccaBueno/orcamentos-ponto-do-gesso/releases/latest/download/PontoDoGesso-Instalador.exe (sempre a versão mais nova).
2. Abrir o `PontoDoGesso-Instalador.exe`. Se aparecer **"O Windows protegeu o computador"**, clicar em **Mais informações › Executar assim mesmo** (o app ainda não tem assinatura digital).
3. Clicar em **Instalar**. Ele não pergunta pasta: instala sempre em `C:\PontoDoGesso\`, cria o atalho **Ponto do Gesso** na Área de Trabalho e no Menu Iniciar e no fim oferece **Abrir o Ponto do Gesso**.
4. Na primeira vez o app cria `%APPDATA%\PontoDoGesso\dados.db`.

Instalar por cima de uma versão antiga (inclusive a v0.9.2 que foi descompactada do zip) mantém os orçamentos, clientes e produtos: eles ficam em `%APPDATA%\PontoDoGesso\`, fora da pasta do app. O instalador não pede administrador, para o app conseguir se atualizar sozinho depois.

A pasta `C:\PontoDoGesso\` fica com poucos arquivos: `PontoDoGesso.exe`, os `.config`, o `PontoDoGesso.Atualizador.exe`, o desinstalador (`unins000.exe`) e as subpastas `lib\` (as DLLs) e `assets\`.

### Opção avançada: sem instalador (zip)

1. Baixar `PontoDoGesso.zip` em https://github.com/LuccaBueno/orcamentos-ponto-do-gesso/releases/latest/download/PontoDoGesso.zip (é o mesmo arquivo que o `PontoDoGesso-vX.Y.Z.zip` da release).
2. Extrair em `C:\` (botão direito → Extrair tudo... → `C:\`): o zip já traz a pasta `PontoDoGesso`, e o resultado deve ser `C:\PontoDoGesso\PontoDoGesso.exe`.
3. Criar atalho na Área de Trabalho: botão direito em `C:\PontoDoGesso\PontoDoGesso.exe` → Enviar para → Área de trabalho (criar atalho).

Não instalar em `C:\Arquivos de Programas\`: lá o app não consegue se atualizar sozinho sem permissão de administrador.

## 4. Teste de compatibilidade (Fase 1)

Rodar no PC da loja e anotar o resultado em `C:\dev\orcamentos-ponto-do-gesso\docs\fases\FASE-01-esqueleto-win7.md`:

- [ ] O app abre sem erro
- [ ] Menu **Ajuda → Diagnóstico** mostra: versão do Windows, 32/64 bits, versão do .NET, caminho do banco
- [ ] Botão **Testar banco** cria, grava e lê um registro
- [ ] Botão **Testar PDF/PNG** gera os dois arquivos de teste e eles abrem
- [ ] Botão **Testar conexão com GitHub** retorna OK
- [ ] Botão **Testar velocidade do histórico** (versão com a Fase 10) responde em menos de 1.000 ms: anotar o tempo em `C:\dev\orcamentos-ponto-do-gesso\docs\fases\FASE-08-historico.md`

### Se o teste de conexão com o GitHub falhar

1. Rodar o Windows Update até não haver mais atualizações importantes.
2. Testar de novo.
3. Se ainda falhar, o app continua funcionando normalmente; só a atualização passa a ser manual (baixar o instalador novo e instalar por cima; os dados não são afetados).

## 5. Antivírus

O `PontoDoGesso.exe` não tem assinatura digital paga e se atualiza sozinho baixando arquivos da internet. Alguns antivírus desconfiam disso e podem bloquear o app ou o instalador, apagar o `PontoDoGesso.Atualizador.exe` ou impedir a atualização.

Se acontecer:
1. Abrir o antivírus instalado no PC da loja → Exceções / Exclusões / Itens permitidos.
2. Adicionar a pasta `C:\PontoDoGesso\` inteira (não só o exe).
3. Adicionar também `%APPDATA%\PontoDoGesso\atualizacao\` (onde a atualização é baixada) e `%TEMP%\PontoDoGesso-atualizador\` (de onde o Atualizador roda durante a atualização). Se o antivírus pedir arquivos, adicionar `C:\PontoDoGesso\PontoDoGesso.exe` e `C:\PontoDoGesso\PontoDoGesso.Atualizador.exe`.
4. Se o antivírus colocou algum arquivo em quarentena, restaurar e marcar como confiável.
5. Anotar o nome do antivírus em `C:\dev\orcamentos-ponto-do-gesso\docs\fases\README.md`, seção "PC da loja".

Para conferir se um arquivo baixado é o original: o hash SHA-256 do zip deve ser igual ao do arquivo `.sha256` publicado na Release (no PowerShell: `Get-FileHash C:\caminho\PontoDoGesso-vX.Y.Z.zip`). No Windows 7 com PowerShell antigo, usar `certutil -hashfile C:\caminho\PontoDoGesso-vX.Y.Z.zip SHA256`.

## 6. Backup

O app faz backup automático diário em `%APPDATA%\PontoDoGesso\backups\`. Recomendado: em **Configurações → Backup**, escolher também uma pasta em pendrive ou HD externo e copiar de vez em quando.
