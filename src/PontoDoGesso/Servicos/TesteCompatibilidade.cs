using System;
using System.Data.SQLite;
using System.IO;
using System.Net;
using Dapper;
using PontoDoGesso.Dados;
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

        public const int OrcamentosDoTesteDeVelocidade = 5000;
        public const long MetaVelocidadeMs = 1000;

        /// <summary>
        /// Mede o histórico com 5.000 orçamentos falsos num banco descartável (o banco real não é aberto):
        /// as mesmas 4 consultas da tela (lista, soma, busca por texto e filtros combinados). Meta no Windows 7: menos de 1 s.
        /// </summary>
        public static ResultadoTeste TestarVelocidadeHistorico(string pasta, DateTime hoje)
        {
            var nome = "teste_historico_" + Guid.NewGuid().ToString("N");
            var arquivo = Path.Combine(pasta, nome + ".db");
            var backups = Path.Combine(pasta, nome + "-backups");

            try
            {
                var banco = new Banco(arquivo, backups);
                banco.Preparar();
                GerarOrcamentosFalsos(banco, OrcamentosDoTesteDeVelocidade, hoje);

                var orcamentos = new OrcamentoRepositorio(banco);
                var relogio = System.Diagnostics.Stopwatch.StartNew();
                var lista = orcamentos.Buscar(new FiltroHistorico(), hoje);
                var soma = orcamentos.Somar(new FiltroHistorico(), hoje);
                var texto = orcamentos.Buscar(new FiltroHistorico { Texto = "ficticio " + (OrcamentosDoTesteDeVelocidade - 1) }, hoje);
                orcamentos.Somar(new FiltroHistorico { Periodo = PeriodoHistorico.MesPassado, Frete = PresencaHistorico.Com, TipoCliente = TipoClienteHistorico.Avulsos, Texto = "cliente" }, hoje);
                relogio.Stop();

                if (lista.Count == 0 || soma.Quantidade != OrcamentosDoTesteDeVelocidade || texto.Count != 1)
                    return ResultadoTeste.Falha("as consultas não encontraram os orçamentos de teste.");

                var tempo = "histórico com " + OrcamentosDoTesteDeVelocidade.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("pt-BR")) + " orçamentos respondeu em " + relogio.ElapsedMilliseconds + " ms (meta: menos de " + MetaVelocidadeMs + " ms).";
                return relogio.ElapsedMilliseconds < MetaVelocidadeMs ? ResultadoTeste.Sucesso(tempo) : ResultadoTeste.Falha("lento: " + tempo);
            }
            catch (Exception erro)
            {
                return ResultadoTeste.Falha(erro.ToString());
            }
            finally
            {
                SQLiteConnection.ClearAllPools();
                foreach (var sobra in new[] { arquivo, arquivo + "-wal", arquivo + "-shm" })
                    ApagarSemFalhar(sobra);
                try
                {
                    if (Directory.Exists(backups))
                        Directory.Delete(backups, true);
                }
                catch (Exception erro) when (erro is IOException || erro is UnauthorizedAccessException)
                {
                    // Pasta vazia ou quase: não atrapalha
                }
            }
        }

        /// <summary>Orçamentos avulsos falsos ("Cliente Fictício N"), espalhados por quase 2 anos, um terço com frete e um quarto com desconto.</summary>
        public static void GerarOrcamentosFalsos(Banco banco, int quantidade, DateTime hoje)
        {
            using (var conexao = banco.Abrir())
            using (var transacao = conexao.BeginTransaction())
            {
                for (var i = 1; i <= quantidade; i++)
                {
                    conexao.Execute(
                        @"INSERT INTO orcamentos (numero, data, cli_nome, cli_telefone, tipo_entrega, subtotal_centavos, desconto_centavos, frete_centavos, total_centavos, criado_em, atualizado_em)
                          VALUES (@i, @data, @nome, @telefone, 'ENTREGA', 10000, @desconto, @frete, 10000, '2026-01-01T00:00:00', '2026-01-01T00:00:00')",
                        new
                        {
                            i,
                            data = hoje.AddDays(-(i % 700)).ToString("yyyy-MM-dd"),
                            nome = "Cliente Fictício " + i,
                            telefone = "1299" + (1000000 + i),
                            desconto = i % 4 == 0 ? 500 : 0,
                            frete = i % 3 == 0 ? 2000 : 0
                        }, transacao);
                }

                transacao.Commit();
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
