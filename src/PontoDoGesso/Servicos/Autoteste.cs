using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using PontoDoGesso.Dados;

namespace PontoDoGesso.Servicos
{
    /// <summary>
    /// PontoDoGesso.exe --autoteste [arquivo]: confere, sem abrir janelas, que o app empacotado carrega o SQLite
    /// (inclusive o SQLite.Interop.dll em lib\x86), cria o banco com as migrações e gera PDF e PNG.
    /// Usado pelo scripts\empacotar.ps1 depois de montar a pasta do app. Devolve 0 se tudo funcionou.
    /// </summary>
    public static class Autoteste
    {
        public const string Argumento = "--autoteste";

        public static int Executar(string arquivoResultado) =>
            Executar(arquivoResultado, Path.Combine(Caminhos.PastaDados, "autoteste"));

        public static int Executar(string arquivoResultado, string pasta)
        {
            var linhas = new List<string> { "Ponto do Gesso " + Diagnostico.VersaoApp + " — autoteste " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") };
            var ok = true;

            void Anotar(string nome, ResultadoTeste resultado)
            {
                linhas.Add(nome + ": " + resultado);
                ok &= resultado.Ok;
            }

            try
            {
                Anotar("SQLite", TesteCompatibilidade.TestarBanco(pasta));
                Anotar("Banco do app", TestarMigracoes(pasta));
                Anotar("PDF e PNG", TesteCompatibilidade.TestarPdfPng(pasta));
                linhas.Add("System.Data.SQLite carregado de: " + typeof(SQLiteConnection).Assembly.Location);
                linhas.Add("PdfSharp carregado de: " + typeof(PdfSharp.Drawing.XGraphics).Assembly.Location);
            }
            catch (Exception erro)
            {
                // DLL faltando na pasta lib\ aparece aqui como FileNotFoundException
                linhas.Add("FALHOU: " + erro);
                ok = false;
            }

            linhas.Add(ok ? "RESULTADO: OK" : "RESULTADO: FALHOU");

            if (!string.IsNullOrEmpty(arquivoResultado))
            {
                var pastaResultado = Path.GetDirectoryName(Path.GetFullPath(arquivoResultado));
                Directory.CreateDirectory(pastaResultado);
                File.WriteAllLines(arquivoResultado, linhas, new UTF8Encoding(false));
            }

            return ok ? 0 : 1;
        }

        /// <summary>Banco novo num arquivo descartável, com todas as migrações (as mesmas do banco real).</summary>
        private static ResultadoTeste TestarMigracoes(string pasta)
        {
            var arquivo = Path.Combine(pasta, "autoteste_" + Guid.NewGuid().ToString("N") + ".db");
            try
            {
                Directory.CreateDirectory(pasta);
                var banco = new Banco(arquivo, Path.Combine(pasta, "backups"));
                banco.Preparar();
                return ResultadoTeste.Sucesso("banco criado com as migrações (" + Diagnostico.DescricaoBanco(banco) + ").");
            }
            catch (Exception erro)
            {
                return ResultadoTeste.Falha(erro.ToString());
            }
            finally
            {
                SQLiteConnection.ClearAllPools();
                try
                {
                    File.Delete(arquivo);
                }
                catch (Exception erro) when (erro is IOException || erro is UnauthorizedAccessException)
                {
                    // Fica para trás um arquivo de teste na pasta autoteste; não atrapalha
                    Log.RegistrarErro(erro);
                }
            }
        }
    }
}
