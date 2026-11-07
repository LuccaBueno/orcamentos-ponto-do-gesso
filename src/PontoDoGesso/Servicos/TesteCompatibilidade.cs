using System;
using System.Data.SQLite;
using System.IO;
using System.Net;
using Dapper;
using PontoDoGesso.Exportacao;

namespace PontoDoGesso.Servicos
{
    /// <summary>Testes do menu Ajuda → Diagnóstico, usados para validar o PC da loja.</summary>
    public static class TesteCompatibilidade
    {
        private const string UrlRepositorio = "https://api.github.com/repos/LuccaBueno/orcamentos-ponto-do-gesso";
        private const int TempoLimiteMs = 15000;

        public static ResultadoTeste TestarBanco(string pasta)
        {
            var arquivo = Path.Combine(pasta, "teste_" + Guid.NewGuid().ToString("N") + ".db");
            const string textoGravado = "Orçamento de teste";
            const long valorGravado = 123456;

            try
            {
                Directory.CreateDirectory(pasta);
                var conexaoTexto = new SQLiteConnectionStringBuilder { DataSource = arquivo }.ToString();

                using (var conexao = new SQLiteConnection(conexaoTexto))
                {
                    conexao.Open();

                    using (var comando = conexao.CreateCommand())
                    {
                        comando.CommandText = "CREATE TABLE teste (texto TEXT NOT NULL, valor_centavos INTEGER NOT NULL)";
                        comando.ExecuteNonQuery();
                    }

                    using (var comando = conexao.CreateCommand())
                    {
                        comando.CommandText = "INSERT INTO teste (texto, valor_centavos) VALUES (@texto, @valor)";
                        comando.Parameters.AddWithValue("@texto", textoGravado);
                        comando.Parameters.AddWithValue("@valor", valorGravado);
                        comando.ExecuteNonQuery();
                    }

                    // Lido pelo Dapper de propósito: valida no PC da loja o mesmo caminho dos repositórios
                    var lido = conexao.QuerySingle<RegistroTeste>("SELECT texto AS Texto, valor_centavos AS ValorCentavos FROM teste");
                    if (lido.Texto != textoGravado || lido.ValorCentavos != valorGravado)
                        return ResultadoTeste.Falha("o registro lido é diferente do gravado.");
                }

                return ResultadoTeste.Sucesso("banco criado, registro gravado e lido (SQLite " + SQLiteConnection.SQLiteVersion + ").");
            }
            catch (Exception erro)
            {
                return ResultadoTeste.Falha(erro.ToString());
            }
            finally
            {
                ApagarSemFalhar(arquivo);
            }
        }

        public static ResultadoTeste TestarPdfPng(string pasta)
        {
            try
            {
                Directory.CreateDirectory(pasta);
                var pdf = Path.Combine(pasta, "teste.pdf");
                var png = Path.Combine(pasta, "teste.png");

                DesenhoTeste.GerarPdf(pdf);
                DesenhoTeste.GerarPng(png);

                return ResultadoTeste.Sucesso("arquivos gerados:" + Environment.NewLine + pdf + Environment.NewLine + png);
            }
            catch (Exception erro)
            {
                return ResultadoTeste.Falha(erro.ToString());
            }
        }

        public static ResultadoTeste TestarConexaoGitHub()
        {
            try
            {
                // O Windows 7 não ativa TLS 1.2 por padrão no .NET
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var requisicao = (HttpWebRequest)WebRequest.Create(UrlRepositorio);
                requisicao.Method = "GET";
                requisicao.Timeout = TempoLimiteMs;
                requisicao.ReadWriteTimeout = TempoLimiteMs;
                requisicao.UserAgent = "PontoDoGesso/" + Diagnostico.VersaoApp;
                requisicao.Accept = "application/vnd.github+json";

                using (var resposta = (HttpWebResponse)requisicao.GetResponse())
                    return ResultadoTeste.Sucesso("GitHub respondeu " + (int)resposta.StatusCode + " " + resposta.StatusDescription + ".");
            }
            catch (Exception erro)
            {
                return ResultadoTeste.Falha(erro.ToString());
            }
        }

        private class RegistroTeste
        {
            public string Texto { get; set; }
            public long ValorCentavos { get; set; }
        }

        private static void ApagarSemFalhar(string arquivo)
        {
            try
            {
                File.Delete(arquivo);
            }
            catch (Exception erro) when (erro is IOException || erro is UnauthorizedAccessException)
            {
                // Sobra só um arquivo de teste pequeno; não atrapalha o resultado
            }
        }
    }
}
