using System;
using System.Drawing;
using System.IO;
using System.Linq;
using PdfSharp.Drawing;
using PdfSharp.Pdf.IO;
using PontoDoGesso.Dominio;
using PontoDoGesso.Exportacao;
using PontoDoGesso.Servicos;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class ExportacaoTestes : IDisposable
    {
        private readonly string _pasta = PastaTestes.Nova("PontoDoGesso-exportacao-");

        public void Dispose()
        {
            if (Directory.Exists(_pasta))
                Directory.Delete(_pasta, true);
        }

        /// <summary>Orçamento com cliente e produtos fictícios.</summary>
        public static Orcamento Exemplo(int quantidadeItens, bool desconto = false, bool validade = false, bool retirada = false, string observacoes = null)
        {
            var produtos = new[]
            {
                ("Placa de gesso 60x60", "m²", 1250L),
                ("Gesso em pó 40 kg", "saco", 3290L),
                ("Perfil F530 3 m", "un", 1590L),
                ("Massa para drywall pronta 15 kg — balde com tampa, uso interno, alta aderência e secagem rápida", "un", 8990L),
                ("Parafuso ponta agulha 25 mm", "cx", 2450L),
                ("Fita telada 50 m", "un", 1875L),
                ("Moldura de gesso decorativa 8 cm", "m", 690L)
            };

            var orcamento = new Orcamento
            {
                Numero = 123,
                Data = new DateTime(2026, 11, 6),
                ValidadeDias = validade ? 7 : (int?)null,
                ClienteId = 1,
                CliNome = "Maria Fictícia de Souza",
                CliTelefone = "12987654321",
                TipoEntrega = retirada ? TipoEntrega.Retirada : TipoEntrega.Entrega,
                EntEndereco = retirada ? null : "Rua das Flores Imaginárias",
                EntNumero = retirada ? null : "250",
                EntBairro = retirada ? null : "Jardim Exemplo",
                EntCidade = retirada ? null : "Pindamonhangaba",
                EntReferencia = retirada ? null : "Portão azul, ao lado da padaria",
                Observacoes = observacoes,
                DescontoTipo = desconto ? TipoDesconto.Percentual : (TipoDesconto?)null,
                DescontoValor = desconto ? 5 : 0,
                FreteCentavos = retirada ? 0 : 2000
            };

            for (var i = 0; i < quantidadeItens; i++)
            {
                var (nome, unidade, preco) = produtos[i % produtos.Length];
                var quantidade = i % 3 == 0 ? 10 : i % 3 == 1 ? 2.5m : 1;
                orcamento.Itens.Add(new ItemOrcamento { Quantidade = quantidade, Unidade = unidade, Descricao = nome, UnitarioCentavos = preco });
            }

            CalculoOrcamento.Aplicar(orcamento);
            return orcamento;
        }

        [Fact]
        public void NomeDoArquivoSemAcentosNemEspacos()
        {
            var orcamento = new Orcamento { Numero = 123, CliNome = "João  da Silva & Filhos", Data = new DateTime(2026, 11, 6) };

            Assert.Equal("Orc-0123_Joao-da-Silva-Filhos_06-11-2026.pdf", ArquivosOrcamento.NomeArquivo(orcamento, ".pdf"));
        }

        [Fact]
        public void NomeSemLetrasFicaSoComNumeroEData()
        {
            var orcamento = new Orcamento { Numero = 7, CliNome = "!!!", Data = new DateTime(2026, 1, 2) };

            Assert.Equal("Orc-0007_02-01-2026.png", ArquivosOrcamento.NomeArquivo(orcamento, ".png"));
        }

        [Fact]
        public void NomeMuitoLongoECortado()
        {
            var nome = ArquivosOrcamento.SemAcentosNemEspacos(new string('a', 30) + " " + new string('b', 30));

            Assert.True(nome.Length <= 40);
            Assert.False(nome.EndsWith("-"));
        }

        [Fact]
        public void PastaPorAnoEMes()
        {
            var arquivo = ArquivosOrcamento.Preparar(_pasta, Exemplo(1), ".pdf");

            Assert.Equal(Path.Combine(_pasta, "2026", "11", "Orc-0123_Maria-Ficticia-de-Souza_06-11-2026.pdf"), arquivo);
            Assert.True(Directory.Exists(Path.Combine(_pasta, "2026", "11")));
        }

        [Theory]
        [InlineData(3, 1)]
        [InlineData(15, 1)]
        [InlineData(20, 2)]
        [InlineData(35, 2)]
        [InlineData(80, 3)]
        public void PaginasDoPdf(int itens, int paginas)
        {
            var orcamento = Exemplo(itens, desconto: true, validade: true);
            var desenho = new DesenhoOrcamento(orcamento, DadosLoja.Padrao);

            Assert.Equal(paginas, desenho.Paginas.Count);
            Assert.Equal(itens, desenho.Paginas.Sum(p => p.Item2));
            Assert.All(desenho.Paginas, p => Assert.True(p.Item2 > 0));

            Directory.CreateDirectory(_pasta);
            var arquivo = Path.Combine(_pasta, "orcamento.pdf");
            ExportadorPdf.Gerar(orcamento, DadosLoja.Padrao, arquivo);

            using (var documento = PdfReader.Open(arquivo, PdfDocumentOpenMode.Import))
            {
                Assert.Equal(paginas, documento.PageCount);
                Assert.Equal(595, documento.Pages[0].Width.Point, 0);
                Assert.Equal(842, documento.Pages[0].Height.Point, 0);
            }
        }

        [Fact]
        public void PaginasSeguemAOrdemDosItens()
        {
            var desenho = new DesenhoOrcamento(Exemplo(60), DadosLoja.Padrao);
            var esperado = 0;

            foreach (var (inicio, quantidade) in desenho.Paginas)
            {
                Assert.Equal(esperado, inicio);
                esperado += quantidade;
            }

            Assert.Equal(60, esperado);
        }

        [Fact]
        public void NenhumItemSomeNaPaginacao()
        {
            foreach (var (desconto, validade, observacoes) in new[] { (false, false, (string)null), (true, true, "Entregar pela manhã. Cliente vai conferir as medidas.") })
                for (var itens = 1; itens <= 90; itens++)
                {
                    var desenho = new DesenhoOrcamento(Exemplo(itens, desconto, validade, observacoes: observacoes), DadosLoja.Padrao);

                    Assert.Equal(itens, desenho.Paginas.Sum(p => p.Item2));
                    Assert.All(desenho.Paginas, p => Assert.True(p.Item2 > 0));
                }
        }

        [Fact]
        public void PngNaLarguraDoA4A150DpiTerminandoLogoAbaixoDoConteudo()
        {
            using (var imagem = ExportadorPng.CriarImagem(Exemplo(3), DadosLoja.Padrao))
            {
                Assert.Equal(1240, imagem.Width);
                Assert.InRange(imagem.Height, 800, 1200);
                Assert.Equal(Color.White.ToArgb(), imagem.GetPixel(2, 2).ToArgb());
            }
        }

        [Fact]
        public void PngComMuitosItensViraUmaImagemMaisAlta()
        {
            using (var imagem = ExportadorPng.CriarImagem(Exemplo(35), DadosLoja.Padrao))
            {
                Assert.Equal(1240, imagem.Width);
                Assert.True(imagem.Height > 1754);
            }
        }

        [Fact]
        public void DescricaoLongaQuebraEmDuasLinhasComReticencias()
        {
            using (var medida = XGraphics.CreateMeasureContext(new XSize(595, 842), XGraphicsUnit.Point, XPageDirection.Downwards))
            {
                var fonte = new XFont("Arial", 10);

                var curta = DesenhoOrcamento.Quebrar(medida, "Placa de gesso", fonte, 90, 2);
                var longa = DesenhoOrcamento.Quebrar(medida, string.Join(" ", Enumerable.Repeat("Placa de gesso acartonado", 20)), fonte, 90, 2);

                Assert.Single(curta);
                Assert.Equal(2, longa.Length);
                Assert.EndsWith("…", longa[1]);
            }
        }

        [Fact]
        public void IconeDoWhatsAppEDesenhadoQuandoExiste()
        {
            Directory.CreateDirectory(_pasta);
            var icone = Path.Combine(_pasta, "icone.png");
            using (var verde = new Bitmap(32, 32))
            {
                using (var g = Graphics.FromImage(verde))
                    g.Clear(Color.FromArgb(0x25, 0xD3, 0x66));
                verde.Save(icone, System.Drawing.Imaging.ImageFormat.Png);
            }

            var desenho = new DesenhoOrcamento(Exemplo(1), DadosLoja.Padrao, paginado: false, arquivoIcone: icone);
            using (var imagem = new Bitmap(1240, 1754))
            using (var superficie = Graphics.FromImage(imagem))
            using (var grafico = XGraphics.FromGraphics(superficie, new XSize(595, 842)))
            {
                superficie.Clear(Color.White);
                grafico.ScaleTransform(150 / 72.0);
                desenho.DesenharPagina(grafico, 0);
                grafico.Dispose();

                var achouVerde = false;
                for (var x = 0; x < 700 && !achouVerde; x += 2)
                    for (var y = 200; y < 360 && !achouVerde; y += 2)
                        achouVerde = imagem.GetPixel(x, y).ToArgb() == Color.FromArgb(0x25, 0xD3, 0x66).ToArgb();

                Assert.True(achouVerde);
            }
        }

        [Fact]
        public void SemIconeOOrcamentoSaiMesmoAssim()
        {
            var desenho = new DesenhoOrcamento(Exemplo(1), DadosLoja.Padrao, arquivoIcone: Path.Combine(_pasta, "nao-existe.png"));
            Directory.CreateDirectory(_pasta);
            ExportadorPdf.Gerar(Exemplo(1), DadosLoja.Padrao, Path.Combine(_pasta, "sem-icone.pdf"));
            Assert.Single(desenho.Paginas);
        }

        [Theory]
        [InlineData("(12) 99244-5402", "(12)", "99244-5402")]
        [InlineData("99244-5402", "", "99244-5402")]
        public void TelefoneDaLojaSeparaODdd(string telefone, string ddd, string numero)
        {
            Assert.Equal((ddd, numero), DesenhoOrcamento.SepararDdd(telefone));
        }

        /// <summary>
        /// Gera as imagens de comparação com a folha em papel em docs\referencia\comparacoes.
        /// Só roda com PONTODOGESSO_COMPARACOES definida, para não mexer no repositório a cada teste.
        /// </summary>
        [Fact]
        public void GerarComparacoes()
        {
            if (Environment.GetEnvironmentVariable("PONTODOGESSO_COMPARACOES") == null)
                return;

            var pasta = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(PastaTestes.Raiz)), "docs", "referencia", "comparacoes");
            Directory.CreateDirectory(pasta);

            var casos = new[]
            {
                ("03-itens-sem-desconto", Exemplo(3)),
                ("03-itens-retirada-com-validade", Exemplo(3, validade: true, retirada: true)),
                ("15-itens-com-desconto-e-validade", Exemplo(15, desconto: true, validade: true, observacoes: "Entregar pela manhã. Cliente vai conferir as medidas antes de confirmar o pedido.")),
                ("20-itens-com-desconto", Exemplo(20, desconto: true)),
                ("35-itens-duas-paginas", Exemplo(35, validade: true))
            };

            foreach (var (nome, orcamento) in casos)
            {
                ExportadorPng.Gerar(orcamento, DadosLoja.Padrao, Path.Combine(pasta, nome + ".png"));
                ExportadorPdf.Gerar(orcamento, DadosLoja.Padrao, Path.Combine(pasta, nome + ".pdf"));
            }

            // Lado a lado: primeira via da folha em papel (ampliada para A4) | orçamento novo em PNG
            var pastaLadoALado = Path.Combine(pasta, "lado-a-lado");
            Directory.CreateDirectory(pastaLadoALado);
            var ladoALado = new[]
            {
                ("03-itens", Exemplo(3)),
                ("15-itens", Exemplo(15)),
                ("35-itens", Exemplo(35)),
                ("com-desconto-e-validade", Exemplo(8, desconto: true, validade: true, observacoes: "Entregar pela manhã. Cliente vai conferir as medidas antes de confirmar o pedido."))
            };

            using (var folha = new Bitmap(Path.Combine(Path.GetDirectoryName(pasta), "folha-orcamento-atual-via1.png")))
                foreach (var (nome, orcamento) in ladoALado)
                    using (var novo = ExportadorPng.CriarImagem(orcamento, DadosLoja.Padrao))
                    using (var imagem = LadoALado(folha, novo))
                        imagem.Save(Path.Combine(pastaLadoALado, nome + ".png"), System.Drawing.Imaging.ImageFormat.Png);

            // CNPJ: o mesmo orçamento com a caixa desmarcada (igual a antes) e marcada; CNPJ fictício
            var pastaCnpj = Path.Combine(pasta, "cnpj");
            Directory.CreateDirectory(pastaCnpj);
            var semCnpj = Exemplo(3);
            var comCnpj = Exemplo(3);
            comCnpj.MostrarCnpj = true;
            comCnpj.EmpresaCnpj = CnpjTestes.Ficticio;

            ExportadorPng.Gerar(semCnpj, DadosLoja.Padrao, Path.Combine(pastaCnpj, "sem-cnpj.png"));
            ExportadorPng.Gerar(comCnpj, DadosLoja.Padrao, Path.Combine(pastaCnpj, "com-cnpj.png"));
            ExportadorPdf.Gerar(comCnpj, DadosLoja.Padrao, Path.Combine(pastaCnpj, "com-cnpj.pdf"));
            using (var sem = ExportadorPng.CriarImagem(semCnpj, DadosLoja.Padrao))
            using (var com = ExportadorPng.CriarImagem(comCnpj, DadosLoja.Padrao))
            using (var imagem = LadoALado(sem, com, "Sem CNPJ (caixa desmarcada, igual a antes)", "Com CNPJ (caixa marcada)"))
                imagem.Save(Path.Combine(pastaCnpj, "sem-e-com-cnpj.png"), System.Drawing.Imaging.ImageFormat.Png);
        }

        private static Bitmap LadoALado(Image folha, Image novo, string tituloEsquerda = "Folha em papel (1ª via, ampliada para A4)", string tituloDireita = "Orçamento novo (PNG)")
        {
            const int topo = 60, espaco = 40;
            var largura = folha.Width + espaco + novo.Width;
            var imagem = new Bitmap(largura, topo + Math.Max(folha.Height, novo.Height));

            using (var g = Graphics.FromImage(imagem))
            using (var fonte = new Font("Arial", 22, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var borda = new Pen(Color.FromArgb(0xBF, 0xBF, 0xBF)))
            {
                g.Clear(Color.White);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                g.DrawString(tituloEsquerda, fonte, Brushes.DimGray, 10, 18);
                g.DrawString(tituloDireita, fonte, Brushes.DimGray, folha.Width + espaco + 10, 18);
                g.DrawImage(folha, 0, topo, folha.Width, folha.Height);
                g.DrawImage(novo, folha.Width + espaco, topo, novo.Width, novo.Height);
                g.DrawRectangle(borda, 0, topo, folha.Width - 1, folha.Height - 1);
                g.DrawRectangle(borda, folha.Width + espaco, topo, novo.Width - 1, novo.Height - 1);
            }

            return imagem;
        }
    }
}
