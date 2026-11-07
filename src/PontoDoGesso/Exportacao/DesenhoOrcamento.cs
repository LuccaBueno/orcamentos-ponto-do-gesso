using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using PdfSharp.Drawing;
using PontoDoGesso.Dominio;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Exportacao
{
    /// <summary>
    /// Desenha o orçamento no visual da folha em papel (A4 retrato). O mesmo desenho serve para PDF, PNG e impressão.
    /// As medidas estão em milímetros e são convertidas para pontos (1/72") na hora de desenhar.
    /// </summary>
    public sealed class DesenhoOrcamento
    {
        public const double LarguraMm = 210;
        public const double AlturaMm = 297;

        private const double Margem = 12;
        private const double Esquerda = Margem;
        private const double Direita = LarguraMm - Margem;
        private const double Largura = Direita - Esquerda;

        // Topo da moldura do cabeçalho, linhas de campos e tabela
        private const double AlturaMoldura = 40;
        private const double TopoCampos = Margem + AlturaMoldura + 7;
        private const double EspacoCampos = 7.5;
        private const double TopoTabela = TopoCampos + 5 * EspacoCampos;
        private const double TopoTabelaContinuacao = Margem + 12;

        private const double AlturaCabecalhoTabela = 6.5;
        private const double AlturaLinha = 6;
        private const double AlturaLinhaDupla = 9.5;
        private const double AlturaTotal = 6.5;
        private const double AlturaLinhaTexto = 4.3;
        private const double Folga = 1.5;

        /// <summary>Abaixo disso só fica o número do orçamento.</summary>
        private const double LimiteConteudo = AlturaMm - Margem - 4;

        private static readonly double[] Colunas = { 0.14, 0.52, 0.17, 0.17 };

        public static readonly XColor Laranja = XColor.FromArgb(0xC5, 0x5A, 0x11);
        public static readonly XColor FundoClaro = XColor.FromArgb(0xFB, 0xE4, 0xD5);
        private static readonly XColor CinzaNumero = XColor.FromArgb(0xA6, 0xA6, 0xA6);

        private static readonly XPen Moldura = new XPen(Laranja, 1);
        private static readonly XPen Linha = new XPen(Laranja, 0.6);
        private static readonly XBrush PincelLaranja = new XSolidBrush(Laranja);
        private static readonly XBrush PincelFundo = new XSolidBrush(FundoClaro);
        private static readonly XBrush PincelNumero = new XSolidBrush(CinzaNumero);

        /// <summary>Calibri não vem no Windows 7 sem o Office: nesse caso usa Arial.</summary>
        public static readonly string FonteTabela = FonteInstalada("Calibri") ? "Calibri" : "Arial";

        private static readonly XFont FonteTitulo = new XFont("Times New Roman", 30);
        private static readonly XFont FonteSubtitulo = new XFont("Arial", 14);
        private static readonly XFont FonteDdd = new XFont("Arial", 11);
        private static readonly XFont FonteTelefone = new XFont("Arial", 15);
        private static readonly XFont FonteEndereco = new XFont("Arial", 11);
        private static readonly XFont FonteRotulo = new XFont(FonteTabela, 10);
        private static readonly XFont FonteValor = new XFont(FonteTabela, 10.5);
        private static readonly XFont FonteCabecalhoTabela = new XFont(FonteTabela, 9.5, XFontStyleEx.Bold);
        private static readonly XFont FonteItem = new XFont(FonteTabela, 10);
        private static readonly XFont FonteTotalRotulo = new XFont(FonteTabela, 10, XFontStyleEx.Bold);
        private static readonly XFont FonteTotalValor = new XFont(FonteTabela, 11, XFontStyleEx.Bold);
        private static readonly XFont FonteRodape = new XFont(FonteTabela, 9.5, XFontStyleEx.Bold);
        private static readonly XFont FonteValidade = new XFont(FonteTabela, 9.5);
        private static readonly XFont FonteContinuacaoTitulo = new XFont("Times New Roman", 15);
        private static readonly XFont FonteNumero = new XFont("Arial", 7);

        private readonly Orcamento _orcamento;
        private readonly DadosLoja _loja;
        private readonly string _arquivoIcone;
        private readonly List<string[]> _descricoes = new List<string[]>();
        private readonly List<double> _alturas = new List<double>();
        private readonly List<string> _observacoes;
        private readonly List<string> _linhasRodape = new List<string>();
        private readonly List<Tuple<string, string>> _totais = new List<Tuple<string, string>>();

        /// <summary>
        /// Monta o desenho. <paramref name="paginado"/> = falso (PNG): tudo numa página só, mais alta que o A4 se precisar.
        /// </summary>
        public DesenhoOrcamento(Orcamento orcamento, DadosLoja loja, bool paginado = true, string arquivoIcone = null)
        {
            _orcamento = orcamento;
            _loja = loja;
            _arquivoIcone = arquivoIcone ?? ArquivoIconePadrao;

            var medida = XGraphics.CreateMeasureContext(new XSize(Pt(LarguraMm), Pt(AlturaMm)), XGraphicsUnit.Point, XPageDirection.Downwards);

            var larguraDescricao = Largura * Colunas[1] - 2 * Folga;
            foreach (var item in orcamento.Itens)
            {
                var linhas = Quebrar(medida, item.Descricao, FonteItem, larguraDescricao, 2);
                _descricoes.Add(linhas);
                _alturas.Add(linhas.Length > 1 ? AlturaLinhaDupla : AlturaLinha);
            }

            if (orcamento.DescontoTipo != null && orcamento.DescontoCentavos > 0)
            {
                _totais.Add(Tuple.Create("SUBTOTAL:", Formatacao.Valor(orcamento.SubtotalCentavos)));
                var rotulo = orcamento.DescontoTipo == TipoDesconto.Percentual ? "DESC. " + Formatacao.Quantidade(orcamento.DescontoValor) + "%:" : "DESC.:";
                _totais.Add(Tuple.Create(rotulo, "-" + Formatacao.Valor(orcamento.DescontoCentavos)));
            }
            _totais.Add(Tuple.Create("FRETE:", orcamento.FreteCentavos > 0 ? Formatacao.Valor(orcamento.FreteCentavos) : "—"));

            var observacoes = (orcamento.Observacoes ?? "").Trim();
            _observacoes = observacoes.Length == 0 ? new List<string>()
                : Quebrar(medida, "OBS: " + observacoes, FonteValidade, Largura * (Colunas[0] + Colunas[1]) - 2 * Folga, 12).ToList();

            if (!string.IsNullOrWhiteSpace(loja.Rodape))
                _linhasRodape.AddRange(Quebrar(medida, loja.Rodape.Trim(), FonteRodape, Largura - 2 * Folga, 3));

            var validade = CalculoOrcamento.DataValidade(orcamento.Data, orcamento.ValidadeDias);
            if (validade != null)
            {
                var dias = orcamento.ValidadeDias.Value;
                _linhasRodape.Add("Orçamento válido por " + dias + (dias == 1 ? " dia" : " dias") + " (até " + validade.Value.ToString("dd/MM/yyyy") + ").");
            }

            medida.Dispose();
            Paginar(paginado);
        }

        /// <summary>Ícone do WhatsApp copiado junto do executável; opcional.</summary>
        public static string ArquivoIconePadrao => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "icone-whatsapp.png");

        /// <summary>Itens de cada página (início e quantidade).</summary>
        public IReadOnlyList<Tuple<int, int>> Paginas { get; private set; }

        /// <summary>Altura da página em mm: A4 no PDF e na impressão; no PNG, cresce com a quantidade de itens.</summary>
        public double AlturaPaginaMm { get; private set; }

        public static double Pt(double mm) => mm * 72 / 25.4;

        // ---------- paginação ----------

        private double AlturaTotais => Math.Max(_totais.Count * AlturaLinha + AlturaTotal, _observacoes.Count * AlturaLinhaTexto + 2 * Folga);

        private double AlturaRodape => _linhasRodape.Count == 0 ? 0 : _linhasRodape.Count * AlturaLinhaTexto + 2.5;

        private double AlturaFinal => AlturaTotais + AlturaRodape;

        private void Paginar(bool paginado)
        {
            var paginas = new List<Tuple<int, int>>();

            if (!paginado)
            {
                paginas.Add(Tuple.Create(0, _alturas.Count));
                Paginas = paginas;
                var fim = TopoTabela + AlturaCabecalhoTabela + _alturas.Sum() + AlturaFinal;
                AlturaPaginaMm = Math.Max(AlturaMm, fim + (AlturaMm - LimiteConteudo));
                return;
            }

            AlturaPaginaMm = AlturaMm;
            var indice = 0;
            var topo = TopoTabela;

            while (true)
            {
                var inicio = indice;
                var y = topo + AlturaCabecalhoTabela;

                while (indice < _alturas.Count && y + _alturas[indice] <= LimiteConteudo)
                    y += _alturas[indice++];

                if (indice == _alturas.Count)
                {
                    // O fecho (totais e rodapé) precisa ficar com pelo menos um item na última página
                    while (y + AlturaFinal > LimiteConteudo && indice > inicio + 1)
                        y -= _alturas[--indice];

                    if (y + AlturaFinal <= LimiteConteudo)
                    {
                        paginas.Add(Tuple.Create(inicio, indice - inicio));
                        break;
                    }
                }

                paginas.Add(Tuple.Create(inicio, indice - inicio));
                topo = TopoTabelaContinuacao;
            }

            Paginas = paginas;
        }

        // ---------- desenho ----------

        /// <summary>Desenha a página indicada num XGraphics em pontos, com a origem no canto superior esquerdo do papel.</summary>
        public void DesenharPagina(XGraphics grafico, int pagina)
        {
            var (inicio, quantidade) = Paginas[pagina];
            var ultima = pagina == Paginas.Count - 1;
            double y;

            if (pagina == 0)
            {
                DesenharCabecalho(grafico);
                DesenharCampos(grafico);
                y = TopoTabela;
            }
            else
            {
                DesenharCabecalhoContinuacao(grafico);
                y = TopoTabelaContinuacao;
            }

            y = DesenharTabela(grafico, y, inicio, quantidade, ultima);

            if (ultima)
                DesenharFecho(grafico, y);

            DesenharNumero(grafico, pagina);
        }

        private void DesenharCabecalho(XGraphics grafico)
        {
            var moldura = Retangulo(Esquerda, Margem, Largura, AlturaMoldura);
            grafico.DrawRoundedRectangle(Moldura, moldura, new XSize(Pt(9), Pt(9)));

            Texto(grafico, _loja.Nome, FonteTitulo, PincelLaranja, Esquerda, Margem + 2, Largura, 14, XStringFormats.Center);
            Texto(grafico, _loja.Subtitulo, FonteSubtitulo, PincelLaranja, Esquerda, Margem + 15, Largura, 8, XStringFormats.Center);

            // Telefone como na folha: DDD pequeno e número grande, com o ícone do WhatsApp ao lado
            var linhaTelefone = Margem + 26.5;
            var x = Pt(Esquerda + 7);
            var (ddd, numero) = SepararDdd(_loja.Telefone ?? "");
            if (ddd.Length > 0)
            {
                grafico.DrawString(ddd + " ", FonteDdd, PincelLaranja, new XRect(x, Pt(linhaTelefone), 400, Pt(8)), XStringFormats.CenterLeft);
                x += grafico.MeasureString(ddd + " ", FonteDdd).Width;
            }
            grafico.DrawString(numero, FonteTelefone, PincelLaranja, new XRect(x, Pt(linhaTelefone), 400, Pt(8)), XStringFormats.CenterLeft);
            x += grafico.MeasureString(numero, FonteTelefone).Width + Pt(1.2);
            DesenharIcone(grafico, x, Pt(linhaTelefone + 4 - 2.6), Pt(5.2));

            var xEndereco = Esquerda + Largura * 0.51;
            var larguraEndereco = Direita - 6 - xEndereco;
            Texto(grafico, _loja.EnderecoLinha1, FonteEndereco, PincelLaranja, xEndereco, Margem + 24.5, larguraEndereco, 7, XStringFormats.CenterLeft);
            Texto(grafico, _loja.EnderecoLinha2, FonteEndereco, PincelLaranja, xEndereco, Margem + 31, larguraEndereco, 7, XStringFormats.CenterLeft);
        }

        private void DesenharIcone(XGraphics grafico, double x, double y, double tamanho)
        {
            if (string.IsNullOrEmpty(_arquivoIcone) || !File.Exists(_arquivoIcone))
                return;

            try
            {
                using (var icone = XImage.FromFile(_arquivoIcone))
                    grafico.DrawImage(icone, x, y, tamanho, tamanho);
            }
            catch (Exception erro)
            {
                // Ícone com defeito não impede o orçamento de sair
                Log.RegistrarErro(erro);
            }
        }

        private void DesenharCampos(XGraphics grafico)
        {
            var o = _orcamento;
            var retirada = o.TipoEntrega == TipoEntrega.Retirada;
            var y = TopoCampos;

            Campo(grafico, "Nome:", o.CliNome, Esquerda, y, Esquerda + Largura * 0.72);
            Campo(grafico, "Data:", o.Data.ToString("dd/MM/yyyy"), Esquerda + Largura * 0.75, y, Direita);

            y += EspacoCampos;
            Campo(grafico, "Tel:", string.IsNullOrEmpty(o.CliTelefone) ? "" : Formatacao.Telefone(o.CliTelefone), Esquerda, y, Esquerda + Largura * 0.45);
            var xOpcao = Esquerda + Largura * 0.52;
            xOpcao = Opcao(grafico, "Entrega", !retirada, xOpcao, y);
            Opcao(grafico, "Retirada na loja", retirada, xOpcao + 6, y);

            y += EspacoCampos;
            Campo(grafico, "End:", retirada ? "Retirada na loja" : o.EntEndereco, Esquerda, y, Esquerda + Largura * 0.78);
            Campo(grafico, "Nº", retirada ? "" : o.EntNumero, Esquerda + Largura * 0.80, y, Direita);

            y += EspacoCampos;
            Campo(grafico, "Bairro:", retirada ? "" : o.EntBairro, Esquerda, y, Esquerda + Largura * 0.50);
            Campo(grafico, "Cidade:", retirada ? "" : o.EntCidade, Esquerda + Largura * 0.53, y, Direita);

            y += EspacoCampos;
            Campo(grafico, "P.Ref:", retirada ? "" : o.EntReferencia, Esquerda, y, Direita);
        }

        /// <summary>Rótulo laranja, valor em preto sobre a linha (como na folha preenchida à mão).</summary>
        private static void Campo(XGraphics grafico, string rotulo, string valor, double x, double y, double fim)
        {
            var linhaBase = y + 5;
            grafico.DrawString(rotulo, FonteRotulo, PincelLaranja, Pt(x), Pt(linhaBase - 0.6));
            var inicioLinha = Pt(x) + grafico.MeasureString(rotulo, FonteRotulo).Width + Pt(0.6);
            grafico.DrawLine(Linha, inicioLinha, Pt(linhaBase), Pt(fim), Pt(linhaBase));

            if (!string.IsNullOrEmpty(valor))
            {
                var largura = Pt(fim) - inicioLinha - Pt(2);
                grafico.DrawString(Cortar(grafico, valor.Replace("\r", " ").Replace("\n", " "), FonteValor, largura), FonteValor, XBrushes.Black, inicioLinha + Pt(1.5), Pt(linhaBase - 0.9));
            }
        }

        /// <summary>Quadradinho marcado com X; devolve onde termina.</summary>
        private static double Opcao(XGraphics grafico, string texto, bool marcado, double x, double y)
        {
            var linhaBase = y + 5;
            grafico.DrawString(texto, FonteRotulo, PincelLaranja, Pt(x), Pt(linhaBase - 0.6));
            var caixaX = Pt(x) + grafico.MeasureString(texto, FonteRotulo).Width + Pt(1.5);
            var lado = Pt(3.4);
            var caixa = new XRect(caixaX, Pt(linhaBase - 0.6) - lado + Pt(0.4), lado, lado);
            grafico.DrawRectangle(Linha, caixa);

            if (marcado)
            {
                var pena = new XPen(XColors.Black, 1.1);
                var recuo = Pt(0.7);
                grafico.DrawLine(pena, caixa.Left + recuo, caixa.Top + recuo, caixa.Right - recuo, caixa.Bottom - recuo);
                grafico.DrawLine(pena, caixa.Right - recuo, caixa.Top + recuo, caixa.Left + recuo, caixa.Bottom - recuo);
            }

            return (caixa.Right) * 25.4 / 72;
        }

        private void DesenharCabecalhoContinuacao(XGraphics grafico)
        {
            Texto(grafico, _loja.Nome, FonteContinuacaoTitulo, PincelLaranja, Esquerda, Margem, Largura, 8, XStringFormats.CenterLeft);
            Texto(grafico, "Orçamento Nº " + Formatacao.Codigo(_orcamento.Numero) + " — continuação", FonteRotulo, PincelLaranja, Esquerda, Margem, Largura, 8, XStringFormats.CenterRight);
            grafico.DrawLine(Linha, Pt(Esquerda), Pt(Margem + 9.5), Pt(Direita), Pt(Margem + 9.5));
        }

        /// <summary>Cabeçalho da tabela e as linhas dos itens; devolve onde a tabela terminou.</summary>
        private double DesenharTabela(XGraphics grafico, double y, int inicio, int quantidade, bool ultima)
        {
            var x = PosicoesColunas();

            grafico.DrawRectangle(PincelFundo, Retangulo(Esquerda, y, Largura, AlturaCabecalhoTabela));
            var titulos = new[] { "QUANT", "DISCRIMINAÇÃO", "UNITÁRIO", "TOTAL R$" };
            for (var c = 0; c < 4; c++)
                Texto(grafico, titulos[c], FonteCabecalhoTabela, PincelLaranja, x[c], y, x[c + 1] - x[c], AlturaCabecalhoTabela, XStringFormats.Center);

            var topo = y;
            y += AlturaCabecalhoTabela;
            grafico.DrawLine(Linha, Pt(Esquerda), Pt(y), Pt(Direita), Pt(y));

            for (var i = inicio; i < inicio + quantidade; i++)
            {
                var item = _orcamento.Itens[i];
                var altura = _alturas[i];

                var qtd = Formatacao.Quantidade(item.Quantidade) + (string.IsNullOrWhiteSpace(item.Unidade) ? "" : " " + item.Unidade.Trim());
                Texto(grafico, Cortar(grafico, qtd, FonteItem, Pt(x[1] - x[0] - 2 * Folga)), FonteItem, XBrushes.Black, x[0], y, x[1] - x[0], altura, XStringFormats.Center);

                var linhas = _descricoes[i];
                var topoTexto = y + (altura - linhas.Length * AlturaLinhaTexto) / 2;
                for (var l = 0; l < linhas.Length; l++)
                    Texto(grafico, linhas[l], FonteItem, XBrushes.Black, x[1] + Folga, topoTexto + l * AlturaLinhaTexto, x[2] - x[1] - 2 * Folga, AlturaLinhaTexto, XStringFormats.CenterLeft);

                Texto(grafico, Formatacao.Valor(item.UnitarioCentavos), FonteItem, XBrushes.Black, x[2] + Folga, y, x[3] - x[2] - 2 * Folga, altura, XStringFormats.CenterRight);
                Texto(grafico, Formatacao.Valor(item.TotalCentavos), FonteItem, XBrushes.Black, x[3] + Folga, y, x[4] - x[3] - 2 * Folga, altura, XStringFormats.CenterRight);

                y += altura;
                grafico.DrawLine(Linha, Pt(Esquerda), Pt(y), Pt(Direita), Pt(y));
            }

            // Bordas verticais (na última página descem pelos totais, desenhados depois)
            for (var c = 0; c <= 4; c++)
                grafico.DrawLine(Linha, Pt(x[c]), Pt(topo), Pt(x[c]), Pt(y));

            if (!ultima)
                grafico.DrawRectangle(Linha, Retangulo(Esquerda, topo, Largura, y - topo));

            return y;
        }

        /// <summary>Totais com as observações à esquerda, e o rodapé da loja com a validade.</summary>
        private void DesenharFecho(XGraphics grafico, double y)
        {
            var x = PosicoesColunas();
            var alturaTotais = AlturaTotais;
            var topo = y;

            // Observações à esquerda
            var yTexto = topo + Folga;
            foreach (var linha in _observacoes)
            {
                Texto(grafico, linha, FonteValidade, XBrushes.Black, Esquerda + Folga, yTexto, x[2] - Esquerda - 2 * Folga, AlturaLinhaTexto, XStringFormats.CenterLeft);
                yTexto += AlturaLinhaTexto;
            }

            // Linhas de SUBTOTAL, DESC. e FRETE
            foreach (var (rotulo, valor) in _totais)
            {
                Texto(grafico, rotulo, FonteTotalRotulo, PincelLaranja, x[2] + Folga, y, x[3] - x[2] - Folga, AlturaLinha, XStringFormats.CenterLeft);
                Texto(grafico, valor, FonteItem, XBrushes.Black, x[3] + Folga, y, x[4] - x[3] - 2 * Folga, AlturaLinha, XStringFormats.CenterRight);
                y += AlturaLinha;
                grafico.DrawLine(Linha, Pt(x[2]), Pt(y), Pt(Direita), Pt(y));
            }
            grafico.DrawLine(Linha, Pt(x[3]), Pt(topo), Pt(x[3]), Pt(y));

            // TOTAL ocupa o resto da altura, com fundo claro
            var alturaTotal = topo + alturaTotais - y;
            var celulaTotal = Retangulo(x[2], y, Direita - x[2], alturaTotal);
            grafico.DrawRectangle(PincelFundo, celulaTotal);
            grafico.DrawRectangle(Linha, celulaTotal);
            Texto(grafico, "TOTAL:", FonteTotalRotulo, PincelLaranja, x[2] + Folga, y, x[3] - x[2], AlturaTotal, XStringFormats.CenterLeft);
            Texto(grafico, Formatacao.Moeda(_orcamento.TotalCentavos), FonteTotalValor, XBrushes.Black, x[2] + Folga, y, Direita - x[2] - 2 * Folga, AlturaTotal, XStringFormats.CenterRight);

            var fimTotais = topo + alturaTotais;
            grafico.DrawLine(Linha, Pt(x[2]), Pt(topo), Pt(x[2]), Pt(fimTotais));

            // Rodapé: "As entregas..." e a validade, numa faixa na largura toda
            var fim = fimTotais;
            if (_linhasRodape.Count > 0)
            {
                var yRodape = fimTotais + 1.2;
                var temRodapeLoja = !string.IsNullOrWhiteSpace(_loja.Rodape);
                for (var l = 0; l < _linhasRodape.Count; l++)
                {
                    var daLoja = temRodapeLoja && l < _linhasRodape.Count - (_orcamento.ValidadeDias != null ? 1 : 0);
                    Texto(grafico, _linhasRodape[l], daLoja ? FonteRodape : FonteValidade, daLoja ? PincelLaranja : XBrushes.Black,
                        Esquerda + Folga, yRodape, Largura - 2 * Folga, AlturaLinhaTexto, XStringFormats.CenterLeft);
                    yRodape += AlturaLinhaTexto;
                }
                fim += AlturaRodape;
            }

            grafico.DrawRectangle(Linha, Retangulo(Esquerda, TopoDaTabelaNaPagina(), Largura, fim - TopoDaTabelaNaPagina()));
            if (_linhasRodape.Count > 0)
                grafico.DrawLine(Linha, Pt(Esquerda), Pt(fimTotais), Pt(Direita), Pt(fimTotais));
        }

        private double TopoDaTabelaNaPagina() => Paginas.Count == 1 ? TopoTabela : TopoTabelaContinuacao;

        /// <summary>Número discreto no canto inferior direito; com mais de uma página, junto de "Página X de Y".</summary>
        private void DesenharNumero(XGraphics grafico, int pagina)
        {
            var texto = _orcamento.Numero > 0 ? "Nº " + Formatacao.Codigo(_orcamento.Numero) : "";
            if (Paginas.Count > 1)
                texto = (texto.Length > 0 ? texto + " · " : "") + "Página " + (pagina + 1) + " de " + Paginas.Count;

            if (texto.Length > 0)
                Texto(grafico, texto, FonteNumero, PincelNumero, Esquerda, AlturaPaginaMm - Margem - 3, Largura, 3, XStringFormats.BottomRight);
        }

        // ---------- utilidades ----------

        private static double[] PosicoesColunas()
        {
            var x = new double[5];
            x[0] = Esquerda;
            for (var c = 0; c < 4; c++)
                x[c + 1] = x[c] + Largura * Colunas[c];
            x[4] = Direita;
            return x;
        }

        private static XRect Retangulo(double x, double y, double largura, double altura) =>
            new XRect(Pt(x), Pt(y), Pt(largura), Pt(altura));

        private static void Texto(XGraphics grafico, string texto, XFont fonte, XBrush pincel, double x, double y, double largura, double altura, XStringFormat formato)
        {
            if (!string.IsNullOrEmpty(texto))
                grafico.DrawString(texto, fonte, pincel, Retangulo(x, y, largura, altura), formato);
        }

        /// <summary>"(12) 99244-5402" → ("(12)", "99244-5402"); sem DDD entre parênteses, tudo vai para o número.</summary>
        public static (string Ddd, string Numero) SepararDdd(string telefone)
        {
            var limpo = telefone.Trim();
            var fecha = limpo.IndexOf(')');
            return limpo.StartsWith("(") && fecha > 0 ? (limpo.Substring(0, fecha + 1), limpo.Substring(fecha + 1).Trim()) : ("", limpo);
        }

        /// <summary>Corta o texto com "…" para caber na largura (em pontos).</summary>
        private static string Cortar(XGraphics grafico, string texto, XFont fonte, double largura, bool sempreReticencias = false)
        {
            if (!sempreReticencias && grafico.MeasureString(texto, fonte).Width <= largura)
                return texto;

            var fim = texto.Length;
            while (fim > 0 && grafico.MeasureString(texto.Substring(0, fim).TrimEnd() + "…", fonte).Width > largura)
                fim--;

            return texto.Substring(0, fim).TrimEnd() + "…";
        }

        /// <summary>Quebra o texto em linhas que cabem na largura (em mm); o que passar do limite de linhas é cortado com "…".</summary>
        public static string[] Quebrar(XGraphics grafico, string texto, XFont fonte, double larguraMm, int maximoLinhas)
        {
            var largura = Pt(larguraMm);
            var linhas = new List<string>();

            foreach (var paragrafo in (texto ?? "").Replace("\r\n", "\n").Split('\n'))
            {
                var atual = "";
                foreach (var palavra in paragrafo.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var tentativa = atual.Length == 0 ? palavra : atual + " " + palavra;
                    if (grafico.MeasureString(tentativa, fonte).Width <= largura)
                    {
                        atual = tentativa;
                        continue;
                    }

                    if (atual.Length > 0)
                        linhas.Add(atual);

                    // Palavra sozinha maior que a linha: quebra no meio
                    atual = palavra;
                    while (grafico.MeasureString(atual, fonte).Width > largura && atual.Length > 1)
                    {
                        var corte = atual.Length - 1;
                        while (corte > 1 && grafico.MeasureString(atual.Substring(0, corte), fonte).Width > largura)
                            corte--;
                        linhas.Add(atual.Substring(0, corte));
                        atual = atual.Substring(corte);
                    }
                }

                linhas.Add(atual);
            }

            while (linhas.Count > 1 && linhas[linhas.Count - 1].Length == 0)
                linhas.RemoveAt(linhas.Count - 1);

            if (linhas.Count > maximoLinhas)
            {
                linhas.RemoveRange(maximoLinhas, linhas.Count - maximoLinhas);
                linhas[maximoLinhas - 1] = Cortar(grafico, linhas[maximoLinhas - 1], fonte, largura, true);
            }

            return linhas.ToArray();
        }

        private static bool FonteInstalada(string nome)
        {
            using (var fontes = new InstalledFontCollection())
                return fontes.Families.Any(familia => string.Equals(familia.Name, nome, StringComparison.OrdinalIgnoreCase));
        }
    }
}
