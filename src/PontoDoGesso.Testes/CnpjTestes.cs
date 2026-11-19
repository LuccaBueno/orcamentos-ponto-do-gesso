using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Dapper;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;
using PontoDoGesso.Exportacao;
using PontoDoGesso.Servicos;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class CnpjTestes : IDisposable
    {
        /// <summary>CNPJ fictício de exemplo, com os dígitos verificadores certos.</summary>
        public const string Ficticio = "11222333000181";

        private readonly BancoTemporario _temporario = new BancoTemporario();

        public void Dispose() => _temporario.Dispose();

        // ---------- validação e máscara ----------

        [Theory]
        [InlineData("11.222.333/0001-81")]
        [InlineData("11222333000181")]
        [InlineData("45.723.174/0001-10")]
        public void CnpjComDigitosCertosEhValido(string cnpj) => Assert.True(Cnpj.Valido(cnpj));

        [Theory]
        [InlineData("11.222.333/0001-82")]
        [InlineData("11.222.333/0001-91")]
        [InlineData("00.000.000/0000-00")]
        [InlineData("11.111.111/1111-11")]
        [InlineData("1122233300018")]
        [InlineData("")]
        [InlineData(null)]
        public void CnpjErradoNaoEhValido(string cnpj) => Assert.False(Cnpj.Valido(cnpj));

        [Fact]
        public void FormataComAMascara()
        {
            Assert.Equal("11.222.333/0001-81", Cnpj.Formatar(Ficticio));
            Assert.Equal("123", Cnpj.Formatar("123"));
        }

        [Fact]
        public void CampoMostraPontosMesmoComWindowsEmIngles()
        {
            var cultura = System.Threading.Thread.CurrentThread.CurrentCulture;
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                using (var campo = new System.Windows.Forms.MaskedTextBox { Mask = Cnpj.MascaraCampo, TextMaskFormat = System.Windows.Forms.MaskFormat.IncludeLiterals })
                {
                    campo.Text = Ficticio;
                    Assert.Equal("11.222.333/0001-81", campo.Text);
                }
            }
            finally
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = cultura;
            }
        }

        [Fact]
        public void VazioEhPermitidoEDigitadoComMascaraViraSoDigitos()
        {
            Assert.True(Cnpj.TentarLer("  ", out var vazio, out _));
            Assert.Null(vazio);

            Assert.True(Cnpj.TentarLer("11.222.333/0001-81", out var digitos, out _));
            Assert.Equal(Ficticio, digitos);
        }

        [Theory]
        [InlineData("11.222.333/0001", "14 dígitos")]
        [InlineData("11.222.333/0001-82", "não existe")]
        public void CnpjInvalidoExplicaOProblema(string cnpj, string trecho)
        {
            Assert.False(Cnpj.TentarLer(cnpj, out _, out var erro));
            Assert.Contains(trecho, erro);
        }

        [Fact]
        public void EmpresaComCnpjErradoNaoGrava()
        {
            var loja = DadosLoja.Padrao;
            loja.Cnpj = "11.222.333/0001-82";
            Assert.Contains("não existe", loja.Validar());

            loja.Cnpj = "";
            Assert.Null(loja.Validar());
        }

        // ---------- cabeçalho do orçamento ----------

        private static Bitmap Topo(Orcamento orcamento)
        {
            using (var pagina = ExportadorPng.CriarImagem(orcamento, DadosLoja.Padrao, 96))
                return pagina.Clone(new Rectangle(0, 0, pagina.Width, (int)(80 / 25.4 * 96)), pagina.PixelFormat);
        }

        private static bool Iguais(Bitmap a, Bitmap b)
        {
            for (var y = 0; y < a.Height; y++)
                for (var x = 0; x < a.Width; x++)
                    if (a.GetPixel(x, y) != b.GetPixel(x, y))
                        return false;
            return true;
        }

        [Fact]
        public void SemMarcarOCabecalhoFicaExatamenteComoAntes()
        {
            var comum = ExportacaoTestes.Exemplo(3);
            var comCnpjGuardadoMasDesmarcado = ExportacaoTestes.Exemplo(3);
            comCnpjGuardadoMasDesmarcado.EmpresaCnpj = Ficticio;

            Assert.Null(DesenhoOrcamento.TextoCnpj(comCnpjGuardadoMasDesmarcado));
            using (var antes = Topo(comum))
            using (var depois = Topo(comCnpjGuardadoMasDesmarcado))
                Assert.True(Iguais(antes, depois));
        }

        [Fact]
        public void MarcadoMostraOCnpjGuardadoNoOrcamento()
        {
            var orcamento = ExportacaoTestes.Exemplo(3);
            orcamento.MostrarCnpj = true;
            orcamento.EmpresaCnpj = Ficticio;

            Assert.Equal("CNPJ: 11.222.333/0001-81", DesenhoOrcamento.TextoCnpj(orcamento));
            using (var sem = Topo(ExportacaoTestes.Exemplo(3)))
            using (var com = Topo(orcamento))
                Assert.False(Iguais(sem, com));
        }

        [Fact]
        public void MarcadoSemCnpjValidoNaoMudaNada()
        {
            var orcamento = ExportacaoTestes.Exemplo(3);
            orcamento.MostrarCnpj = true;
            orcamento.EmpresaCnpj = "123";

            Assert.Null(DesenhoOrcamento.TextoCnpj(orcamento));
        }

        // ---------- banco ----------

        [Fact]
        public void OrcamentoGuardaSeMostrouEQualCnpjUsou()
        {
            _temporario.Banco.Preparar();
            var repositorio = new OrcamentoRepositorio(_temporario.Banco);
            var orcamento = ExportacaoTestes.Exemplo(2);
            orcamento.Numero = 0;
            orcamento.ClienteId = null;
            orcamento.MostrarCnpj = true;
            orcamento.EmpresaCnpj = Ficticio;
            repositorio.Salvar(orcamento);

            var semCnpj = ExportacaoTestes.Exemplo(2);
            semCnpj.Numero = 0;
            semCnpj.ClienteId = null;
            semCnpj.EmpresaCnpj = Ficticio;
            repositorio.Salvar(semCnpj);

            // Trocar o CNPJ da empresa depois não muda o orçamento antigo
            new ConfiguracaoRepositorio(_temporario.Banco).Definir("loja.cnpj", "45723174000110");

            var lido = repositorio.ObterPorId(orcamento.Id);
            Assert.True(lido.MostrarCnpj);
            Assert.Equal(Ficticio, lido.EmpresaCnpj);

            var lidoSem = repositorio.ObterPorId(semCnpj.Id);
            Assert.False(lidoSem.MostrarCnpj);
            Assert.Null(lidoSem.EmpresaCnpj);
        }

        [Fact]
        public void MigracaoDoBancoDaLojaMantemOsDadosDaEmpresaEFazBackup()
        {
            var banco = _temporario.Banco;
            var ate2 = Migracao.Embutidas().Where(m => m.Versao <= 2).ToList();
            ExecutorMigracoes.Aplicar(banco, ate2);
            ConfiguracoesPadrao.Inserir(banco);

            using (var conexao = banco.Abrir())
            {
                conexao.Execute("DELETE FROM configuracoes WHERE chave = 'loja.cnpj'");
                conexao.Execute("UPDATE configuracoes SET valor = 'LOJA FICTÍCIA' WHERE chave = 'loja.nome'");
                conexao.Execute("UPDATE configuracoes SET valor = 'Rua Inventada, 1' WHERE chave = 'loja.endereco_linha1'");
                conexao.Execute(@"INSERT INTO orcamentos (numero, data, cli_nome, subtotal_centavos, total_centavos, criado_em, atualizado_em)
                                  VALUES (1, '2026-11-01', 'Cliente antigo', 1000, 1000, '2026-11-01T10:00:00', '2026-11-01T10:00:00')");
            }

            Assert.Equal(Migracao.Embutidas().Max(m => m.Versao), banco.Preparar());

            var loja = DadosLoja.Ler(new ConfiguracaoRepositorio(banco));
            Assert.Equal("LOJA FICTÍCIA", loja.Nome);
            Assert.Equal("Rua Inventada, 1", loja.EnderecoLinha1);
            Assert.Equal("Gesso e Ferramentas", loja.Subtitulo);
            Assert.Equal("", loja.Cnpj);

            var antigo = new OrcamentoRepositorio(banco).ObterPorId(1);
            Assert.Equal("Cliente antigo", antigo.CliNome);
            Assert.False(antigo.MostrarCnpj);
            Assert.Null(antigo.EmpresaCnpj);

            Assert.Single(Directory.GetFiles(banco.PastaBackups, "dados_*.db"));
        }
    }
}
