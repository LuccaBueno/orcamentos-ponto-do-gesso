# Instalação no PC da loja (Windows 7)

## 1. Conferir a versão do Windows (32 ou 64 bits e Service Pack)

1. Menu Iniciar → clicar com o botão direito em **Computador** → **Propriedades**.
2. Anotar:
   - **Edição do Windows**: deve aparecer **Service Pack 1**. Se não aparecer, instalar o SP1 pelo Windows Update antes de continuar.
   - **Tipo de sistema**: 32 bits ou 64 bits. (O app funciona nos dois; anote só para referência.)
3. Registrar o resultado em `C:\dev\orcamentos-ponto-do-gesso\docs\fases\README.md`, na seção "PC da loja".

## 2. Instalar o .NET Framework 4.8 (uma vez só)

1. Num PC com internet, baixar o **instalador offline do .NET Framework 4.8** no site da Microsoft (procurar por "Download .NET Framework 4.8 offline installer"; arquivo `ndp48-x86-x64-allos-enu.exe`, serve para 32 e 64 bits).
2. Levar num pendrive para o PC da loja e executar como administrador.
3. Reiniciar o PC.
4. Se o instalador reclamar de atualização faltando, rodar o Windows Update (ou instalar a atualização indicada na mensagem) e tentar de novo.

Conferir se instalou: Painel de Controle → Programas e Recursos → deve aparecer "Microsoft .NET Framework 4.8".

## 3. Instalar o app

1. Baixar `PontoDoGesso-vX.Y.Z.zip` em https://github.com/LuccaBueno/orcamentos-ponto-do-gesso/releases/latest
2. Extrair para `C:\PontoDoGesso\` (o resultado deve ser `C:\PontoDoGesso\PontoDoGesso.exe`).
3. Criar atalho na Área de Trabalho: botão direito em `C:\PontoDoGesso\PontoDoGesso.exe` → Enviar para → Área de trabalho (criar atalho).
4. Abrir. Na primeira vez o app cria `%APPDATA%\PontoDoGesso\dados.db`.

Não instalar em `C:\Arquivos de Programas\`: lá o app não consegue se atualizar sozinho sem permissão de administrador.

## 4. Teste de compatibilidade (Fase 1)

Rodar no PC da loja e anotar o resultado em `C:\dev\orcamentos-ponto-do-gesso\docs\fases\FASE-01-esqueleto-win7.md`:

- [ ] O app abre sem erro
- [ ] Menu **Ajuda → Diagnóstico** mostra: versão do Windows, 32/64 bits, versão do .NET, caminho do banco
- [ ] Botão **Testar banco** cria, grava e lê um registro
- [ ] Botão **Testar PDF/PNG** gera os dois arquivos de teste e eles abrem
- [ ] Botão **Testar conexão com GitHub** retorna OK

### Se o teste de conexão com o GitHub falhar

1. Rodar o Windows Update até não haver mais atualizações importantes.
2. Testar de novo.
3. Se ainda falhar, o app continua funcionando normalmente; só a atualização passa a ser manual (baixar o zip novo e extrair por cima de `C:\PontoDoGesso\`, os dados não são afetados).

## 5. Antivírus

O `PontoDoGesso.exe` não tem assinatura digital paga e se atualiza sozinho baixando arquivos da internet. Alguns antivírus desconfiam disso e podem bloquear o app, apagar o `PontoDoGesso.Atualizador.exe` ou impedir a atualização.

Se acontecer:
1. Abrir o antivírus instalado no PC da loja → Exceções / Exclusões / Itens permitidos.
2. Adicionar a pasta `C:\PontoDoGesso\` inteira (não só o exe).
3. Adicionar também `%APPDATA%\PontoDoGesso\atualizacao\` (onde a atualização é baixada) e `%TEMP%\PontoDoGesso-atualizador\` (de onde o Atualizador roda durante a atualização). Se o antivírus pedir arquivos, adicionar `C:\PontoDoGesso\PontoDoGesso.exe` e `C:\PontoDoGesso\PontoDoGesso.Atualizador.exe`.
4. Se o antivírus colocou algum arquivo em quarentena, restaurar e marcar como confiável.
5. Anotar o nome do antivírus em `C:\dev\orcamentos-ponto-do-gesso\docs\fases\README.md`, seção "PC da loja".

Para conferir se um arquivo baixado é o original: o hash SHA-256 do zip deve ser igual ao do arquivo `.sha256` publicado na Release (no PowerShell: `Get-FileHash C:\caminho\PontoDoGesso-vX.Y.Z.zip`). No Windows 7 com PowerShell antigo, usar `certutil -hashfile C:\caminho\PontoDoGesso-vX.Y.Z.zip SHA256`.

## 6. Backup

O app faz backup automático diário em `%APPDATA%\PontoDoGesso\backups\`. Recomendado: em **Configurações → Backup**, escolher também uma pasta em pendrive ou HD externo e copiar de vez em quando.
