using System;
using System.Drawing;
using System.Windows.Forms;
using PontoDoGesso.Controles;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;
using PontoDoGesso.Exportacao;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Forms
{
    /// <summary>Dados da empresa que saem no cabeçalho e no rodapé do orçamento, com a pré-visualização do cabeçalho ao vivo.</summary>
    public class TelaEmpresa : TelaBase
    {
        private readonly ConfiguracaoRepositorio _configuracoes;
        private readonly Timer _atrasoPrevia = new Timer { Interval = 300 };

        private readonly TextBox _nome = Campo("lojaNome", 60);
        private readonly TextBox _subtitulo = Campo("lojaSubtitulo", 60);
        private readonly CampoTelefone _telefone = new CampoTelefone { Name = "lojaTelefone", Anchor = AnchorStyles.Left };
        private readonly TextBox _endereco1 = Campo("lojaEndereco1", 60);
        private readonly TextBox _endereco2 = Campo("lojaEndereco2", 60);
        private readonly MaskedTextBox _cnpj = new MaskedTextBox
        {
            Name = "lojaCnpj",
            Mask = Cnpj.MascaraCampo,
            TextMaskFormat = MaskFormat.ExcludePromptAndLiterals,
            CutCopyMaskFormat = MaskFormat.IncludeLiterals,
            Width = 150,
            Anchor = AnchorStyles.Left
        };
        private readonly Label _situacaoCnpj = new Label { Name = "situacaoCnpj", AutoSize = true, Anchor = AnchorStyles.Left };
        private readonly TextBox _rodape = Campo("lojaRodape", 120);
        private readonly PictureBox _previa = new PictureBox { Name = "previa", Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, MinimumSize = new Size(0, 120) };

        /// <summary>Dados como estavam ao abrir a tela (ou no último Salvar), para saber se há alterações.</summary>
        private string _salvos;

        /// <summary>Dentro da aba Empresa de Configurações: sem título nem botão Salvar próprios (o Salvar é o da tela toda).</summary>
        private readonly bool _embutida;

        public TelaEmpresa(Banco banco) : this(banco, false)
        {
        }

        public TelaEmpresa(Banco banco, bool embutida)
        {
            _embutida = embutida;
            _configuracoes = new ConfiguracaoRepositorio(banco);
            Text = "Empresa";
            _telefone.DddPadrao = _configuracoes.Obter("cliente.ddd_padrao") ?? "12";

            var tabela = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(8, 4, 8, 0) };
            tabela.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tabela.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            void Linha(string rotulo, Control controle)
            {
                tabela.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                if (rotulo != null)
                    tabela.Controls.Add(new Label { Text = rotulo, AutoSize = true, Anchor = AnchorStyles.Left }, 0, tabela.RowCount);
                tabela.Controls.Add(controle, rotulo == null ? 0 : 1, tabela.RowCount);
                if (rotulo == null)
                    tabela.SetColumnSpan(controle, 2);
                tabela.RowCount++;
            }

            var cnpj = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
            cnpj.Controls.Add(_cnpj);
            cnpj.Controls.Add(_situacaoCnpj);

            if (!embutida)
                Linha(null, new Label { Text = "Empresa", AutoSize = true, Font = new Font("Segoe UI", 14F, FontStyle.Bold), Margin = new Padding(0, 0, 0, 6) });
            Linha("Nome *", _nome);
            Linha("Subtítulo", _subtitulo);
            Linha("Telefone (com DDD)", _telefone);
            Linha("Endereço (linha 1)", _endereco1);
            Linha("Endereço (linha 2)", _endereco2);
            Linha("CNPJ (opcional)", cnpj);
            Linha("Rodapé", _rodape);
            Linha(null, Explicacao("Estes dados saem no cabeçalho e no rodapé de todo orçamento exportado ou impresso (os já salvos também, ao exportar de novo). "
                + "O CNPJ só sai nos orçamentos em que a caixa \"Mostrar CNPJ da empresa neste orçamento\" for marcada."));
            Linha(null, new Label { Text = "Pré-visualização do cabeçalho", AutoSize = true, Font = new Font(Font, FontStyle.Bold) });
            tabela.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tabela.Controls.Add(_previa, 0, tabela.RowCount);
            tabela.SetColumnSpan(_previa, 2);
            tabela.RowCount++;

            var salvar = Estilo.CriarBotao("Salvar (Ctrl+S)", (s, e) => SalvarEAvisar());
            salvar.Name = "salvar";
            salvar.Font = new Font(Font, FontStyle.Bold);
            var botoes = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(6) };
            botoes.Controls.Add(salvar);

            if (embutida)
                Padding = new Padding(0);
            Controls.Add(tabela);
            if (!embutida)
                Controls.Add(botoes);
            ControleInicial = _nome;

            Exibir(DadosLoja.Ler(_configuracoes));
            _salvos = Chave();

            _atrasoPrevia.Tick += (s, e) =>
            {
                _atrasoPrevia.Stop();
                AtualizarPrevia();
            };
            foreach (var campo in new Control[] { _nome, _subtitulo, _endereco1, _endereco2, _rodape, _cnpj })
                campo.TextChanged += (s, e) => AgendarPrevia();
            _telefone.Alterado += (s, e) => AgendarPrevia();
            _cnpj.TextChanged += (s, e) => AtualizarSituacaoCnpj();
            _cnpj.Enter += (s, e) => BeginInvoke((Action)(() =>
            {
                // Clicar no meio da máscara vazia deixava o cursor perdido no meio dos pontos
                if (!IsDisposed && _cnpj.Text.Length == 0)
                    _cnpj.Select(0, 0);
            }));
        }

        public override Size TamanhoMinimo => new Size(600, 520);

        private static TextBox Campo(string nome, int limite) => new TextBox { Name = nome, Dock = DockStyle.Fill, MaxLength = limite };

        private static Label Explicacao(string texto) =>
            new Label { Text = texto, AutoSize = true, ForeColor = Color.DimGray, MaximumSize = new Size(640, 0), Margin = new Padding(3, 4, 3, 8) };

        private void Exibir(DadosLoja loja)
        {
            _nome.Text = loja.Nome;
            _subtitulo.Text = loja.Subtitulo;
            _telefone.Exibir(Formatacao.SomenteDigitos(loja.Telefone));
            _endereco1.Text = loja.EnderecoLinha1;
            _endereco2.Text = loja.EnderecoLinha2;
            _cnpj.Text = Formatacao.SomenteDigitos(loja.Cnpj);
            _rodape.Text = loja.Rodape;
            AtualizarSituacaoCnpj();
            AtualizarPrevia();
        }

        private void AtualizarSituacaoCnpj()
        {
            var digitos = Formatacao.SomenteDigitos(_cnpj.Text);
            var valido = Cnpj.Valido(digitos);
            _situacaoCnpj.Text = digitos.Length == 0 ? "não sai no orçamento"
                : digitos.Length < 14 ? "incompleto"
                : valido ? "CNPJ válido" : "CNPJ não existe: confira os números";
            _situacaoCnpj.ForeColor = digitos.Length == 0 ? Color.DimGray : valido ? Color.ForestGreen : Color.Firebrick;
        }

        /// <summary>Telefone como sai no orçamento: (12) 99244-5402; se ainda estiver incompleto, como foi digitado.</summary>
        private string TelefoneDigitado() =>
            _telefone.TentarLer(out var telefone, out _) ? Formatacao.Telefone(telefone) ?? "" : "(" + _telefone.Ddd + ") " + _telefone.Numero;

        /// <summary>O que está digitado agora (ainda não salvo).</summary>
        public DadosLoja Digitada() => new DadosLoja
        {
            Nome = _nome.Text.Trim(),
            Subtitulo = _subtitulo.Text.Trim(),
            Telefone = TelefoneDigitado(),
            EnderecoLinha1 = _endereco1.Text.Trim(),
            EnderecoLinha2 = _endereco2.Text.Trim(),
            Rodape = _rodape.Text.Trim(),
            Cnpj = Formatacao.SomenteDigitos(_cnpj.Text)
        };

        public bool Alterado => Chave() != _salvos;

        private string Chave()
        {
            var loja = Digitada();
            return string.Join("\u001f", loja.Nome, loja.Subtitulo, loja.Telefone, loja.EnderecoLinha1, loja.EnderecoLinha2, loja.Rodape, loja.Cnpj);
        }

        public bool Salvar()
        {
            if (!_telefone.TentarLer(out _, out var erroTelefone))
            {
                Avisar("Telefone: " + erroTelefone);
                _telefone.FocarErro();
                return false;
            }

            var loja = Digitada();
            var erro = loja.Validar();
            if (erro != null)
            {
                Avisar(erro);
                (loja.Nome.Length == 0 ? (Control)_nome : _cnpj).Focus();
                return false;
            }

            loja.Gravar(_configuracoes);
            _salvos = Chave();
            return true;
        }

        private void SalvarEAvisar()
        {
            if (Salvar())
                Informar("Dados da empresa salvos.");
        }

        public override bool PodeSair() =>
            PerguntarAntesDeSair(Alterado, "Os dados da empresa têm alterações não salvas. Salvar antes de sair?", Salvar);

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.S) && !_embutida)
            {
                SalvarEAvisar();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void AgendarPrevia()
        {
            _atrasoPrevia.Stop();
            _atrasoPrevia.Start();
        }

        /// <summary>Topo de um orçamento de exemplo com os dados digitados (ainda não salvos); com o CNPJ, se ele for válido.</summary>
        public void AtualizarPrevia()
        {
            var loja = Digitada();
            var exemplo = new Orcamento
            {
                Data = DateTime.Today,
                CliNome = "Nome do cliente",
                MostrarCnpj = loja.TemCnpj,
                EmpresaCnpj = loja.Cnpj,
                Itens = { new ItemOrcamento { Quantidade = 1, Descricao = "Exemplo", UnitarioCentavos = 0 } }
            };

            const int dpi = 96;
            var altura = (int)Math.Round(76 / 25.4 * dpi);

            using (var pagina = ExportadorPng.CriarImagem(exemplo, loja, dpi))
            {
                var topo = pagina.Clone(new Rectangle(0, 0, pagina.Width, altura), pagina.PixelFormat);
                var anterior = _previa.Image;
                _previa.Image = topo;
                anterior?.Dispose();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _atrasoPrevia.Dispose();
                _previa.Image?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
