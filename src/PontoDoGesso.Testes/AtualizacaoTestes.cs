using System;
using System.IO;
using System.Threading;
using PontoDoGesso.Servicos;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class AtualizacaoTestes : IDisposable
    {
        private readonly string _pasta = PastaTestes.Nova("PontoDoGesso-atualização-");

        public AtualizacaoTestes()
        {
            Directory.CreateDirectory(_pasta);
        }

        public void Dispose()
        {
            if (Directory.Exists(_pasta))
                Directory.Delete(_pasta, true);
        }

        [Theory]
        [InlineData("v0.10.0", "0.9.3", true)]
        [InlineData("v0.9.1", "0.9.0", true)]
        [InlineData("v1.0.0", "0.9.9", true)]
        [InlineData("v0.9.0", "0.9.0", false)]
        [InlineData("v0.9", "0.9.0", false)]
        [InlineData("0.9.0", "0.9.0.0", false)]
        [InlineData("v0.8.5", "0.9.0", false)]
        [InlineData("v0.9.3", "0.10.0", false)]
        public void ComparaVersoesPeloNumeroENaoPeloTexto(string tag, string atual, bool maisNova)
        {
            Assert.Equal(maisNova, Atualizacao.EhMaisNova(Atualizacao.LerVersao(tag), Version.Parse(atual)));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("versao-nova")]
        [InlineData("v1.x")]
        public void TagInvalidaNaoEhVersao(string tag)
        {
            Assert.Null(Atualizacao.LerVersao(tag));
            Assert.False(Atualizacao.EhMaisNova(Atualizacao.LerVersao(tag), new Version(0, 1, 0)));
        }

        [Fact]
        public void VersaoAtualTemTresPartes()
        {
            Assert.Equal(-1, Atualizacao.VersaoAtual.Revision);
            Assert.Equal(Diagnostico.VersaoApp, Atualizacao.VersaoAtual.ToString(3));
        }

        private const string RespostaGitHub = @"{
  ""tag_name"": ""v0.9.1"",
  ""name"": ""Ponto do Gesso v0.9.1"",
  ""html_url"": ""https://github.com/LuccaBueno/orcamentos-ponto-do-gesso/releases/tag/v0.9.1"",
  ""body"": ""### Corrigido\r\n- Texto do rodapé com acentuação"",
  ""assets"": [
    { ""name"": ""PontoDoGesso-v0.9.1.zip.sha256"", ""browser_download_url"": ""https://exemplo/PontoDoGesso-v0.9.1.zip.sha256"" },
    { ""name"": ""PontoDoGesso-v0.9.1.zip"", ""browser_download_url"": ""https://exemplo/PontoDoGesso-v0.9.1.zip"" }
  ]
}";

        [Fact]
        public void LeAReleaseDoGitHub()
        {
            var versao = Atualizacao.LerRelease(RespostaGitHub);

            Assert.Equal("v0.9.1", versao.Tag);
            Assert.Equal(new Version(0, 9, 1), versao.Versao);
            Assert.Equal("### Corrigido" + Environment.NewLine + "- Texto do rodapé com acentuação", versao.Notas);
            Assert.Equal("https://github.com/LuccaBueno/orcamentos-ponto-do-gesso/releases/tag/v0.9.1", versao.Pagina);
            Assert.Equal("PontoDoGesso-v0.9.1.zip", versao.NomeZip);
            Assert.Equal("https://exemplo/PontoDoGesso-v0.9.1.zip", versao.UrlZip);
            Assert.Equal("https://exemplo/PontoDoGesso-v0.9.1.zip.sha256", versao.UrlSha256);
        }

        [Fact]
        public void NotasViramTextoSimples()
        {
            var notas = "### Adicionado\n- Botão **Verificar atualizações** no rodapé\n- Log em `%APPDATA%`\n\nTexto comum";

            Assert.Equal(
                "ADICIONADO" + Environment.NewLine + "• Botão Verificar atualizações no rodapé" + Environment.NewLine
                + "• Log em %APPDATA%" + Environment.NewLine + Environment.NewLine + "Texto comum",
                Atualizacao.TextoSimples(notas));
        }

        [Fact]
        public void IgnoraOZipComNomeFixoDoReadme()
        {
            var comZipFixo = RespostaGitHub.Replace(@"""assets"": [", @"""assets"": [
    { ""name"": ""PontoDoGesso.zip"", ""browser_download_url"": ""https://exemplo/PontoDoGesso.zip"" },");

            var versao = Atualizacao.LerRelease(comZipFixo);

            Assert.Equal("PontoDoGesso-v0.9.1.zip", versao.NomeZip);
            Assert.Equal("https://exemplo/PontoDoGesso-v0.9.1.zip", versao.UrlZip);
            Assert.Equal("https://exemplo/PontoDoGesso-v0.9.1.zip.sha256", versao.UrlSha256);
        }

        [Fact]
        public void ReleaseSemOShaNaoServe()
        {
            var semSha = RespostaGitHub.Replace("PontoDoGesso-v0.9.1.zip.sha256", "outro.txt");
            Assert.Throws<InvalidDataException>(() => Atualizacao.LerRelease(semSha));
        }

        [Fact]
        public void ReleaseComTagInvalidaNaoServe()
        {
            Assert.Throws<InvalidDataException>(() => Atualizacao.LerRelease(RespostaGitHub.Replace("\"v0.9.1\"", "\"ultima\"")));
            Assert.Throws<InvalidDataException>(() => Atualizacao.LerRelease("[]"));
        }

        // ---------- SHA-256 ----------

        private string CriarArquivo(string nome, string conteudo)
        {
            var arquivo = Path.Combine(_pasta, nome);
            File.WriteAllText(arquivo, conteudo);
            return arquivo;
        }

        // SHA-256 de "abc" (vetor de teste do FIPS 180-2)
        private const string HashAbc = "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad";

        [Fact]
        public void CalculaOSha256()
        {
            Assert.Equal(HashAbc, Atualizacao.CalcularSha256(CriarArquivo("abc.txt", "abc")));
        }

        [Theory]
        [InlineData(HashAbc + "  PontoDoGesso-v0.9.1.zip\n")]
        [InlineData(HashAbc)]
        [InlineData("  BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD *PontoDoGesso-v0.9.1.zip\r\n")]
        public void HashConfereNosFormatosAceitos(string conteudo)
        {
            Assert.True(Atualizacao.HashConfere(CriarArquivo("abc.zip", "abc"), conteudo));
        }

        [Theory]
        [InlineData("")]
        [InlineData("não é um hash")]
        [InlineData("ba7816bf")]
        [InlineData("ca7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad  PontoDoGesso-v0.9.1.zip")]
        public void HashErradoOuInvalidoNaoConfere(string conteudo)
        {
            Assert.False(Atualizacao.HashConfere(CriarArquivo("abc.zip", "abc"), conteudo));
        }

        [Fact]
        public void ArquivoAlteradoNaoConfere()
        {
            Assert.False(Atualizacao.HashConfere(CriarArquivo("abc.zip", "abd"), HashAbc));
            Assert.False(Atualizacao.HashConfere(Path.Combine(_pasta, "nao-existe.zip"), HashAbc));
        }

        // ---------- download (file://, sem rede) ----------

        private VersaoPublicada Publicar(string conteudoZip, string conteudoSha)
        {
            var origem = Path.Combine(_pasta, "publicado");
            Directory.CreateDirectory(origem);
            var zip = Path.Combine(origem, "PontoDoGesso-v0.9.1.zip");
            File.WriteAllText(zip, conteudoZip);
            File.WriteAllText(zip + ".sha256", conteudoSha);

            return new VersaoPublicada
            {
                Tag = "v0.9.1",
                Versao = new Version(0, 9, 1),
                NomeZip = "PontoDoGesso-v0.9.1.zip",
                UrlZip = new Uri(zip).AbsoluteUri,
                UrlSha256 = new Uri(zip + ".sha256").AbsoluteUri
            };
        }

        [Fact]
        public void BaixaEConfereOPacote()
        {
            var destino = Path.Combine(_pasta, "atualizacao");
            var progresso = 0;

            var zip = new Atualizacao("file:///nao-usado", destino).Baixar(Publicar("abc", HashAbc + "  PontoDoGesso-v0.9.1.zip"), p => progresso = p, CancellationToken.None);

            Assert.Equal(Path.Combine(destino, "PontoDoGesso-v0.9.1.zip"), zip);
            Assert.Equal("abc", File.ReadAllText(zip));
            Assert.Equal(100, progresso);
            Assert.Empty(Directory.GetFiles(destino, "*.parcial"));
        }

        [Fact]
        public void PacoteCorrompidoEhApagadoENaoServe()
        {
            var destino = Path.Combine(_pasta, "atualizacao");

            Assert.Throws<PacoteInvalidoException>(() =>
                new Atualizacao("file:///nao-usado", destino).Baixar(Publicar("abc corrompido", HashAbc), null, CancellationToken.None));

            Assert.False(File.Exists(Path.Combine(destino, "PontoDoGesso-v0.9.1.zip")));
        }

        [Fact]
        public void ConsultaPelaUrlInformada()
        {
            var resposta = CriarArquivo("latest.json", RespostaGitHub);
            var versao = new Atualizacao(new Uri(resposta).AbsoluteUri, _pasta).ConsultarUltima();
            Assert.Equal("v0.9.1", versao.Tag);
        }

        // ---------- Atualizador ----------

        [Fact]
        public void SemOAtualizadorNaPastaDoAppAvisa()
        {
            var erro = Assert.Throws<AtualizadorAusenteException>(() =>
                Atualizacao.IniciarAtualizador(_pasta, Path.Combine(_pasta, "x.zip"), 1, _pasta));
            Assert.Contains(Atualizacao.NomeAtualizador, erro.Message);
        }
    }
}
