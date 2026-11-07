using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;
using PontoDoGesso.Forms;
using PontoDoGesso.Servicos;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class TelaConfiguracoesTestes : IDisposable
    {
        private readonly BancoTemporario _temporario = new BancoTemporario();
        private readonly ConfiguracaoRepositorio _configuracoes;

        public TelaConfiguracoesTestes()
        {
            _temporario.Banco.Preparar();
            _configuracoes = new ConfiguracaoRepositorio(_temporario.Banco);
            _configuracoes.Definir("exportacao.pasta", Path.Combine(_temporario.Pasta, "exportados"));
        }

        public void Dispose() => _temporario.Dispose();

        private void Rodar(Action<ConfiguracoesSemDialogos> teste)
        {
            Tela.EmSta(() =>
            {
                using (var form = new ConfiguracoesSemDialogos(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    // Botão de aba escondida não responde ao PerformClick: a aba Backup fica à vista
                    Tela.Achar<TabControl>(form, "abas").SelectedIndex = 3;
                    teste(form);
                }
            });
        }

        private static T Achar<T>(Form form, string nome) where T : Control => Tela.Achar<T>(form, nome);

        [Fact]
        public void MostraOsValoresAtuaisEAPrevia()
        {
            Rodar(form =>
            {
                Tela.Achar<TabControl>(form, "abas").SelectedIndex = 0;
                Tela.Capturar(form, "configuracoes-loja");
                Assert.Equal("PONTO DO GESSO", Achar<TextBox>(form, "lojaNome").Text);
                Assert.Equal("Cardoso - Pindamonhangaba - SP", Achar<TextBox>(form, "lojaEndereco2").Text);
                Assert.Equal(7, Achar<NumericUpDown>(form, "validade").Value);
                Assert.Equal("Pindamonhangaba", Achar<TextBox>(form, "cidade").Text);
                Assert.Equal("12", Achar<TextBox>(form, "ddd").Text);
                Assert.True(Achar<CheckBox>(form, "perguntarAvulso").Checked);
                Assert.Equal(FormConfiguracoes.ImpressoraDoWindows, Achar<ComboBox>(form, "impressora").SelectedItem);
                Assert.Equal("Nenhum backup feito ainda.", Achar<Label>(form, "ultimoBackup").Text);

                var previa = Achar<PictureBox>(form, "previa").Image;
                Assert.Equal(794, previa.Width);
                Assert.InRange(previa.Height, 200, 220);
            });
        }

        [Fact]
        public void SalvarGravaTudoEOAppUsa()
        {
            Rodar(form =>
            {
                Achar<TextBox>(form, "lojaNome").Text = "LOJA FICTÍCIA";
                Achar<TextBox>(form, "lojaRodape").Text = "Entregamos só no térreo.";
                Achar<NumericUpDown>(form, "validade").Value = 15;
                Achar<TextBox>(form, "cidade").Text = "Taubaté";
                Achar<TextBox>(form, "ddd").Text = "11";
                Achar<CheckBox>(form, "perguntarAvulso").Checked = false;
                Achar<TextBox>(form, "pasta").Text = Path.Combine(_temporario.Pasta, "outra");

                Assert.True(form.Salvar(), string.Join(" | ", form.Avisos));
            });

            Assert.Equal("LOJA FICTÍCIA", _configuracoes.Obter("loja.nome"));
            Assert.Equal("Entregamos só no térreo.", _configuracoes.Obter("loja.rodape"));
            Assert.Equal("15", _configuracoes.Obter("orcamento.validade_sugerida_dias"));
            Assert.Equal("Taubaté", _configuracoes.Obter("cliente.cidade_padrao"));
            Assert.Equal("11", _configuracoes.Obter("cliente.ddd_padrao"));
            Assert.Equal("0", _configuracoes.Obter("orcamento.perguntar_cadastro_avulso"));
            Assert.Equal(Path.Combine(_temporario.Pasta, "outra"), _configuracoes.Obter("exportacao.pasta"));
            Assert.Equal("", _configuracoes.Obter("impressao.impressora"));

            // Orçamento novo e cliente novo usam a cidade e o DDD padrão
            Tela.EmSta(() =>
            {
                using (var orcamento = new FormOrcamento(_temporario.Banco))
                {
                    Assert.Equal("Taubaté", Tela.Achar<TextBox>(orcamento, "cidade").Text);
                    Assert.Equal(15, Tela.Achar<NumericUpDown>(orcamento, "validadeDias").Value);
                }

                using (var cliente = new FormClienteEditar(_temporario.Banco, new Cliente()))
                    Assert.Equal("Taubaté", cliente.Controls.Find("cidade", true).OfType<TextBox>().Single().Text);
            });
        }

        [Theory]
        [InlineData("lojaNome", " ", "Informe o nome da loja.")]
        [InlineData("ddd", "00", "O DDD padrão \"00\" não existe.")]
        [InlineData("pasta", "pasta-relativa", "Escolha a pasta")]
        public void ValoresInvalidosNaoSaoGravados(string campo, string valor, string aviso)
        {
            Rodar(form =>
            {
                Achar<TextBox>(form, campo).Text = valor;
                Assert.False(form.Salvar());
                Assert.StartsWith(aviso, form.Avisos.Single());
            });

            Assert.Equal("PONTO DO GESSO", _configuracoes.Obter("loja.nome"));
            Assert.Equal("12", _configuracoes.Obter("cliente.ddd_padrao"));
        }

        [Fact]
        public void ImpressoraEscolhidaEGravada()
        {
            Rodar(form =>
            {
                var combo = Achar<ComboBox>(form, "impressora");
                Assert.Contains("Impressora Fictícia", combo.Items.Cast<object>());
                combo.SelectedItem = "Impressora Fictícia";
                Assert.True(form.Salvar());
            });

            Assert.Equal("Impressora Fictícia", _configuracoes.Obter("impressao.impressora"));
        }

        [Fact]
        public void BackupAgoraMostraADataEPastaExtraValeNaHora()
        {
            var extra = Path.Combine(_temporario.Pasta, "pendrive");
            Directory.CreateDirectory(extra);

            Rodar(form =>
            {
                form.PastaEscolhida = extra;
                Achar<Button>(form, "escolherPastaExtra").PerformClick();
                Assert.Equal(extra, Achar<TextBox>(form, "pastaExtra").Text);

                Achar<Button>(form, "fazerBackup").PerformClick();
                Assert.StartsWith("Backup feito:", form.Informacoes.Single());
                Assert.StartsWith("Último backup: " + DateTime.Now.ToString("dd/MM/yyyy"), Achar<Label>(form, "ultimoBackup").Text);
                Assert.Single(Directory.GetFiles(extra, "*_manual.db"));

                Tela.Capturar(form, "configuracoes");
            });

            Assert.Equal(extra, new Backup(_temporario.Banco).PastaExtra);
        }

        [Fact]
        public void CopiarBackupParaOutraPasta()
        {
            var destino = Path.Combine(_temporario.Pasta, "hd");

            Rodar(form =>
            {
                form.PastaEscolhida = destino;
                Achar<Button>(form, "copiarBackup").PerformClick();
                Assert.Single(Directory.GetFiles(destino, "*_copia.db"));
                Assert.Contains(destino, form.Informacoes.Single());
            });
        }

        [Fact]
        public void RestaurarValidaConfirmaEReinicia()
        {
            var orcamentos = new OrcamentoRepositorio(_temporario.Banco);
            orcamentos.Salvar(new Orcamento { Data = DateTime.Today, CliNome = "Antigo", Itens = { new ItemOrcamento { Quantidade = 1, Descricao = "Placa", UnitarioCentavos = 100 } } });
            var backup = new Backup(_temporario.Banco).FazerAgora(DateTime.Now);
            orcamentos.Salvar(new Orcamento { Data = DateTime.Today, CliNome = "Novo", Itens = { new ItemOrcamento { Quantidade = 1, Descricao = "Placa", UnitarioCentavos = 100 } } });

            var invalido = Path.Combine(_temporario.Pasta, "invalido.db");
            File.WriteAllText(invalido, "não é banco");

            Rodar(form =>
            {
                form.ArquivoEscolhido = invalido;
                Achar<Button>(form, "restaurar").PerformClick();
                Assert.Contains("não pode ser restaurado", form.Avisos.Single());
                Assert.Equal(0, form.Reinicios);

                form.ArquivoEscolhido = backup;
                form.Resposta = false;
                Achar<Button>(form, "restaurar").PerformClick();
                Assert.Equal(0, form.Reinicios);
                Assert.NotNull(orcamentos.ObterPorId(2));

                form.Resposta = true;
                Achar<Button>(form, "restaurar").PerformClick();
                Assert.Equal(1, form.Reinicios);
                Assert.Contains("antes-de-restaurar", form.Informacoes.Last());
            });

            Assert.Equal("Antigo", orcamentos.ObterPorId(1).CliNome);
            Assert.Null(orcamentos.ObterPorId(2));
        }

        private class ConfiguracoesSemDialogos : FormConfiguracoes
        {
            public List<string> Avisos { get; } = new List<string>();
            public List<string> Informacoes { get; } = new List<string>();
            public string PastaEscolhida { get; set; }
            public string ArquivoEscolhido { get; set; }
            public bool Resposta { get; set; }
            public int Reinicios { get; private set; }

            public ConfiguracoesSemDialogos(Banco banco) : base(banco)
            {
            }

            protected override string[] Impressoras() => new[] { "Impressora Fictícia" };

            protected override string EscolherPasta(string descricao, string inicial) => PastaEscolhida;

            protected override string EscolherArquivoBackup(string pasta) => ArquivoEscolhido;

            protected override void AbrirPasta(string pasta) => Informacoes.Add("abrir " + pasta);

            protected override void Reiniciar() => Reinicios++;

            protected override void Avisar(string mensagem) => Avisos.Add(mensagem);

            protected override void Informar(string mensagem) => Informacoes.Add(mensagem);

            protected override bool Confirmar(string pergunta) => Resposta;
        }
    }
}
