using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using PontoDoGesso.Controles;
using PontoDoGesso.Dados;
using PontoDoGesso.Exportacao;
using PontoDoGesso.Forms;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class TelaEmpresaTestes : IDisposable
    {
        private readonly BancoTemporario _temporario = new BancoTemporario();
        private readonly ConfiguracaoRepositorio _configuracoes;

        public TelaEmpresaTestes()
        {
            _temporario.Banco.Preparar();
            _configuracoes = new ConfiguracaoRepositorio(_temporario.Banco);
        }

        public void Dispose() => _temporario.Dispose();

        private void Rodar(Action<EmpresaSemDialogos> teste)
        {
            Tela.EmSta(() =>
            {
                using (var tela = new EmpresaSemDialogos(_temporario.Banco))
                {
                    Tela.Exibir(tela);
                    teste(tela);
                }
            });
        }

        private static T Achar<T>(Control tela, string nome) where T : Control => Tela.Achar<T>(tela, nome);

        [Fact]
        public void MostraOsDadosDaLojaQueEstavamNasConfiguracoes()
        {
            Rodar(tela =>
            {
                Tela.Capturar(tela, "empresa");
                Assert.Equal("Empresa", tela.Text);
                Assert.Equal("PONTO DO GESSO", Achar<TextBox>(tela, "lojaNome").Text);
                Assert.Equal("Gesso e Ferramentas", Achar<TextBox>(tela, "lojaSubtitulo").Text);
                Assert.Equal("12", Achar<CampoTelefone>(tela, "lojaTelefone").Ddd);
                Assert.Equal("99244-5402", Achar<CampoTelefone>(tela, "lojaTelefone").Numero);
                Assert.Equal("Cardoso - Pindamonhangaba - SP", Achar<TextBox>(tela, "lojaEndereco2").Text);
                Assert.Equal("", Achar<MaskedTextBox>(tela, "lojaCnpj").Text);
                Assert.Equal("não sai no orçamento", Achar<Label>(tela, "situacaoCnpj").Text);
                Assert.False(tela.Alterado);

                var previa = Achar<PictureBox>(tela, "previa").Image;
                Assert.Equal(794, previa.Width);
                Assert.InRange(previa.Height, 280, 290);
            });
        }

        [Fact]
        public void SalvarGravaTudoComOCnpjEOTelefoneFormatados()
        {
            Rodar(tela =>
            {
                Achar<TextBox>(tela, "lojaNome").Text = "LOJA FICTÍCIA";
                Achar<TextBox>(tela, "lojaRodape").Text = "Entregamos só no térreo.";
                Achar<CampoTelefone>(tela, "lojaTelefone").Ddd = "11";
                Achar<CampoTelefone>(tela, "lojaTelefone").Numero = "33334444";
                Achar<MaskedTextBox>(tela, "lojaCnpj").Text = CnpjTestes.Ficticio;
                Assert.Equal("CNPJ válido", Achar<Label>(tela, "situacaoCnpj").Text);
                Assert.True(tela.Alterado);

                Assert.True(tela.Salvar(), string.Join(" | ", tela.Avisos));
                Assert.False(tela.Alterado);
            });

            Assert.Equal("LOJA FICTÍCIA", _configuracoes.Obter("loja.nome"));
            Assert.Equal("Entregamos só no térreo.", _configuracoes.Obter("loja.rodape"));
            Assert.Equal("(11) 3333-4444", _configuracoes.Obter("loja.telefone"));
            Assert.Equal(CnpjTestes.Ficticio, _configuracoes.Obter("loja.cnpj"));
            Assert.Equal(CnpjTestes.Ficticio, DadosLoja.Ler(_configuracoes).Cnpj);
        }

        [Theory]
        [InlineData("cnpj", "11222333000182", "não existe")]
        [InlineData("cnpj", "112223330001", "14 dígitos")]
        [InlineData("nome", " ", "Informe o nome da empresa.")]
        [InlineData("ddd", "00", "Telefone: O DDD \"00\" não existe.")]
        [InlineData("numero", "1234", "Telefone: O telefone precisa ter 8 dígitos")]
        public void ValoresInvalidosNaoSaoGravados(string campo, string valor, string aviso)
        {
            Rodar(tela =>
            {
                switch (campo)
                {
                    case "cnpj":
                        Achar<MaskedTextBox>(tela, "lojaCnpj").Text = valor;
                        break;
                    case "nome":
                        Achar<TextBox>(tela, "lojaNome").Text = valor;
                        break;
                    case "ddd":
                        Achar<CampoTelefone>(tela, "lojaTelefone").Ddd = valor;
                        break;
                    default:
                        Achar<CampoTelefone>(tela, "lojaTelefone").Numero = valor;
                        break;
                }

                Assert.False(tela.Salvar());
                Assert.Contains(aviso, tela.Avisos.Single());
            });

            Assert.Equal("PONTO DO GESSO", _configuracoes.Obter("loja.nome"));
            Assert.Equal("", _configuracoes.Obter("loja.cnpj"));
            Assert.Equal("(12) 99244-5402", _configuracoes.Obter("loja.telefone"));
        }

        [Fact]
        public void PreviaMostraOCabecalhoComOCnpjDigitado()
        {
            Rodar(tela =>
            {
                var antes = (System.Drawing.Bitmap)Achar<PictureBox>(tela, "previa").Image.Clone();
                Achar<MaskedTextBox>(tela, "lojaCnpj").Text = CnpjTestes.Ficticio;
                tela.AtualizarPrevia();
                var depois = (System.Drawing.Bitmap)Achar<PictureBox>(tela, "previa").Image;

                var diferentes = false;
                for (var y = 0; y < antes.Height && !diferentes; y += 2)
                    for (var x = 0; x < antes.Width && !diferentes; x += 2)
                        diferentes = antes.GetPixel(x, y) != depois.GetPixel(x, y);
                Assert.True(diferentes);
                antes.Dispose();
            });
        }

        [Fact]
        public void CtrlSSalvaEAvisa()
        {
            Rodar(tela =>
            {
                Achar<TextBox>(tela, "lojaSubtitulo").Text = "Outro";
                Assert.True(Tela.Teclar(Achar<TextBox>(tela, "lojaSubtitulo"), Keys.Control | Keys.S));
                Assert.Equal("Dados da empresa salvos.", tela.Informacoes.Single());
            });

            Assert.Equal("Outro", _configuracoes.Obter("loja.subtitulo"));
        }

        private class EmpresaSemDialogos : TelaEmpresa
        {
            public List<string> Avisos { get; } = new List<string>();
            public List<string> Informacoes { get; } = new List<string>();

            public EmpresaSemDialogos(Banco banco) : base(banco)
            {
            }

            protected override void Avisar(string mensagem) => Avisos.Add(mensagem);

            protected override void Informar(string mensagem) => Informacoes.Add(mensagem);
        }
    }
}
