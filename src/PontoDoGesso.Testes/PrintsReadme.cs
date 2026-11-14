using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PontoDoGesso.Controles;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;
using PontoDoGesso.Exportacao;
using PontoDoGesso.Forms;
using PontoDoGesso.Servicos;
using Xunit;

namespace PontoDoGesso.Testes
{
    /// <summary>
    /// Gera os prints e o GIF do README e do manual em docs\imagens, sempre com o modo de demonstração (dados fictícios).
    /// Só roda com PONTODOGESSO_PRINTS definida: powershell -File C:\dev\orcamentos-ponto-do-gesso\scripts\gerar-prints.ps1
    /// </summary>
    public class PrintsReadme : IDisposable
    {
        private static readonly bool Ligado = Environment.GetEnvironmentVariable("PONTODOGESSO_PRINTS") != null;

        private readonly string _pasta = PastaTestes.Nova("PontoDoGesso-prints-");
        private readonly string _destino = Path.Combine(PastaTestes.RaizRepositorio, "docs", "imagens");

        public void Dispose()
        {
            if (Directory.Exists(_pasta))
                Directory.Delete(_pasta, true);
        }

        private Banco CriarDemonstracao() =>
            Demonstracao.Criar(Path.Combine(_pasta, "demo.db"), Path.Combine(_pasta, "demo-backups"), Path.Combine(_pasta, "demo-exportados"), DateTime.Today);

        private static Bitmap Foto(Form form)
        {
            Application.DoEvents();
            var imagem = new Bitmap(form.Width, form.Height);
            form.DrawToBitmap(imagem, new Rectangle(Point.Empty, form.Size));

            // O DrawToBitmap ignora a ordem de sobreposição: a lista de sugestões ficaria escondida atrás dos campos
            foreach (var lista in form.Controls.OfType<ListaSugestoes>().Where(lista => lista.Visible))
            {
                var tela = form.PointToScreen(lista.Location);
                using (var foto = new Bitmap(lista.Width, lista.Height))
                using (var g = Graphics.FromImage(imagem))
                {
                    lista.DrawToBitmap(foto, new Rectangle(Point.Empty, lista.Size));
                    g.DrawImage(foto, tela.X - form.Left, tela.Y - form.Top);
                }
            }
            return imagem;
        }

        private void Salvar(Form form, string nome)
        {
            using (var imagem = Foto(form))
                imagem.Save(Path.Combine(_destino, nome + ".png"), ImageFormat.Png);
        }

        [Fact]
        public void Prints()
        {
            if (!Ligado)
                return;

            Directory.CreateDirectory(_destino);
            var banco = CriarDemonstracao();
            var orcamentos = new OrcamentoRepositorio(banco);
            var ultimo = orcamentos.Buscar(new FiltroHistorico(), DateTime.Today).OrderByDescending(o => o.Numero).First();

            Tela.EmSta(() =>
            {
                // O "banco real" da tela principal é só um arquivo vazio na pasta temporária: nunca o do %APPDATA%
                using (var form = new PrincipalDemo(new Banco(Path.Combine(_pasta, "real.db"), Path.Combine(_pasta, "backups")), () => banco))
                {
                    Tela.Exibir(form);
                    form.AlternarDemonstracao();
                    Salvar(form, "principal");
                }

                using (var form = new OrcamentoSemAvisos(banco, orcamentos.ObterPorId(ultimo.Id)))
                {
                    Tela.Exibir(form);
                    Salvar(form, "orcamento");
                    form.Close();
                }

                var filtro = new FiltroHistorico { Periodo = PeriodoHistorico.Ultimos30Dias, Frete = PresencaHistorico.Com };
                using (var form = new FormHistorico(banco, filtro))
                {
                    Tela.Exibir(form);
                    Salvar(form, "historico");
                    form.Close();
                }

                using (var form = new FormProdutos(banco))
                {
                    Tela.Exibir(form);
                    Salvar(form, "produtos");
                    form.Close();
                }

                using (var form = new FormClientes(banco))
                {
                    Tela.Exibir(form);
                    Salvar(form, "clientes");
                    form.Close();
                }

                using (var form = new FormConfiguracoes(banco))
                {
                    Tela.Exibir(form);
                    Salvar(form, "configuracoes");
                    form.Close();
                }
            });

            // O orçamento como o cliente recebe, reduzido para caber na página do GitHub
            using (var imagem = new ExportacaoOrcamento(banco).CriarImagem(orcamentos.ObterPorId(ultimo.Id)))
            using (var reduzida = Reduzir(imagem, 800))
                reduzida.Save(Path.Combine(_destino, "orcamento-final.png"), ImageFormat.Png);
        }

        [Fact]
        public void Gif()
        {
            if (!Ligado)
                return;

            Directory.CreateDirectory(_destino);
            var banco = CriarDemonstracao();
            var cliente = new ClienteRepositorio(banco).Buscar("Fernanda").Single();
            GifAnimado gif = null;

            Tela.EmSta(() =>
            {
                using (var form = new OrcamentoSemAvisos(banco))
                {
                    Tela.Exibir(form);
                    var escala = 0.72;
                    var largura = (int)(form.Width * escala);
                    var alturaTela = (int)(form.Height * escala);
                    gif = new GifAnimado(largura, alturaTela + 40);
                    var legenda = "";

                    void Quadro(int ms)
                    {
                        using (var foto = Foto(form))
                        using (var quadro = ComLegenda(foto, largura, alturaTela, legenda))
                            gif.Adicionar(quadro, ms);
                    }

                    void Digitar(TextBox campo, string texto)
                    {
                        for (var i = 1; i <= texto.Length; i++)
                        {
                            campo.Text = texto.Substring(0, i);
                            Quadro(220);
                        }
                    }

                    legenda = "1. Novo orçamento: digite parte do nome do cliente";
                    Quadro(1800);
                    var busca = Tela.Achar<TextBox>(form, "buscaCliente");
                    Digitar(busca, "fern");
                    Quadro(900);
                    var sugestoesCliente = form.Controls.OfType<ListaSugestoes>().First(lista => lista.Visible);
                    sugestoesCliente.Teclar(Keys.Enter);
                    Quadro(1600);

                    legenda = "2. Itens: digite a quantidade e o código ou o nome do produto";
                    var grade = Tela.Achar<GradeItens>(form, "itens");
                    foreach (var (quantidade, busque) in new[] { ("8", "drywall rU"), ("12", "montante"), ("1", "massa") })
                    {
                        var indice = grade.Rows.Add();
                        grade.Rows[indice].Cells["quantidade"].Value = quantidade;
                        Quadro(500);
                        grade.CurrentCell = grade.Rows[indice].Cells["descricao"];
                        grade.BeginEdit(false);
                        var campo = (TextBox)grade.EditingControl;
                        var sugestoes = form.Controls.OfType<ListaSugestoes>().Single(lista => lista.Campo == campo);
                        Digitar(campo, busque.ToLowerInvariant());
                        Quadro(700);
                        sugestoes.Teclar(Keys.Enter);
                        Quadro(900);
                    }

                    legenda = "3. Frete e desconto, se tiver";
                    Quadro(900);
                    Digitar(Tela.Achar<CampoMoeda>(form, "frete"), "30,00");
                    Tela.Achar<CheckBox>(form, "desconto").Checked = true;
                    Tela.Achar<RadioButton>(form, "descontoPercentual").Checked = true;
                    Digitar(Tela.Achar<TextBox>(form, "descontoValor"), "5");
                    Quadro(1400);

                    legenda = "4. Salvar (Ctrl+S): o número sai sozinho";
                    Quadro(700);
                    Assert.True(form.Salvar(), form.UltimoAviso);
                    Quadro(2000);

                    legenda = "5. Copiar imagem (Ctrl+Shift+C) e colar no WhatsApp (Ctrl+V)";
                    using (var imagem = new ExportacaoOrcamento(banco).CriarImagem(form.Orcamento))
                    using (var quadro = ImagemFinal(imagem, largura, alturaTela, legenda))
                        gif.Adicionar(quadro, 5000);

                    form.Close();
                }
            });

            Assert.True(gif.DuracaoTotalMs <= 30000, "GIF com " + gif.DuracaoTotalMs + " ms");
            gif.Salvar(Path.Combine(_destino, "demo.gif"));
        }

        private static Bitmap Reduzir(Image imagem, int largura)
        {
            var altura = imagem.Height * largura / imagem.Width;
            var reduzida = new Bitmap(largura, altura);
            using (var g = Graphics.FromImage(reduzida))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(imagem, 0, 0, largura, altura);
            }
            return reduzida;
        }

        private static Bitmap ComLegenda(Image tela, int largura, int alturaTela, string legenda)
        {
            var quadro = new Bitmap(largura, alturaTela + 40);
            using (var g = Graphics.FromImage(quadro))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(tela, 0, 0, largura, alturaTela);
                DesenharLegenda(g, largura, alturaTela, legenda);
            }
            return quadro;
        }

        private static Bitmap ImagemFinal(Image orcamento, int largura, int alturaTela, string legenda)
        {
            var quadro = new Bitmap(largura, alturaTela + 40);
            using (var g = Graphics.FromImage(quadro))
            {
                g.Clear(Color.FromArgb(0xE6, 0xE6, 0xE6));
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                var altura = alturaTela - 16;
                var larguraImagem = orcamento.Width * altura / orcamento.Height;
                g.DrawImage(orcamento, (largura - larguraImagem) / 2, 8, larguraImagem, altura);
                DesenharLegenda(g, largura, alturaTela, legenda);
            }
            return quadro;
        }

        private static void DesenharLegenda(Graphics g, int largura, int topo, string legenda)
        {
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            using (var fundo = new SolidBrush(Color.FromArgb(0xC5, 0x5A, 0x11)))
                g.FillRectangle(fundo, 0, topo, largura, 40);
            using (var fonte = new Font("Segoe UI", 13F, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var formato = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString(legenda, fonte, Brushes.White, new RectangleF(0, topo, largura, 40), formato);
        }

        private class PrincipalDemo : FormPrincipal
        {
            private readonly Func<Banco> _criar;

            public PrincipalDemo(Banco banco, Func<Banco> criar) : base(banco)
            {
                _criar = criar;
            }

            protected override Banco CriarBancoDemonstracao() => _criar();
        }

        private class OrcamentoSemAvisos : FormOrcamento
        {
            public string UltimoAviso { get; private set; }

            public OrcamentoSemAvisos(Banco banco, Orcamento orcamento = null) : base(banco, orcamento)
            {
            }

            protected override void Avisar(string mensagem) => UltimoAviso = mensagem;

            protected override bool Confirmar(string pergunta) => false;

            protected override DialogResult PerguntarSalvar(string pergunta) => DialogResult.No;
        }
    }
}
