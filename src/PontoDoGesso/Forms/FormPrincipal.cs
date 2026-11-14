using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using PontoDoGesso.Dados;
using PontoDoGesso.Exportacao;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Forms
{
    public class FormPrincipal : Form
    {
        private static readonly Color Laranja = Color.FromArgb(0xC5, 0x5A, 0x11);

        public const string TituloJanela = "Ponto do Gesso – Orçamentos";

        private readonly Banco _banco;
        private LinkLabel _avisoAtualizacao;
        private VersaoPublicada _versaoDisponivel;

        public FormPrincipal() : this(new Banco())
        {
        }

        public FormPrincipal(Banco banco)
        {
            _banco = banco;

            Text = TituloJanela;
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96F, 96F);
            Font = new Font("Segoe UI", 10F);
            BackColor = Color.White;
            ClientSize = new Size(640, 480);
            MinimumSize = new Size(560, 460);
            StartPosition = FormStartPosition.CenterScreen;

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(24, 16, 24, 8) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            layout.Controls.Add(CriarCabecalho(), 0, 0);
            layout.Controls.Add(CriarBotoes(), 0, 1);
            layout.Controls.Add(CriarRodape(), 0, 2);

            Controls.Add(layout);
            var menu = CriarMenu();
            Controls.Add(menu);
            MainMenuStrip = menu;
        }

        private static Control CriarCabecalho()
        {
            var cabecalho = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 1, RowCount = 2 };
            cabecalho.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            cabecalho.Controls.Add(new Label
            {
                Text = "PONTO DO GESSO",
                Font = new Font("Times New Roman", 28F),
                ForeColor = Laranja,
                AutoSize = true,
                Anchor = AnchorStyles.None
            }, 0, 0);
            cabecalho.Controls.Add(new Label
            {
                Text = "Gesso e Ferramentas",
                Font = new Font("Segoe UI", 12F),
                ForeColor = Laranja,
                AutoSize = true,
                Anchor = AnchorStyles.None
            }, 0, 1);
            return cabecalho;
        }

        private Control CriarRodape()
        {
            var rodape = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 4, Margin = new Padding(0) };
            rodape.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            rodape.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            rodape.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            rodape.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var pasta = new Button { Name = "pastaOrcamentos", Text = "Abrir pasta de orçamentos", AutoSize = true, Padding = new Padding(6, 2, 6, 2), Anchor = AnchorStyles.Left };
            pasta.Click += (s, e) => AbrirPastaOrcamentos();
            rodape.Controls.Add(pasta, 0, 0);

            // Aviso da verificação automática: só aparece quando há versão nova
            _avisoAtualizacao = new LinkLabel { Name = "avisoAtualizacao", AutoSize = true, Visible = false, Anchor = AnchorStyles.Right, LinkColor = Laranja, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            _avisoAtualizacao.LinkClicked += (s, e) => AbrirTela(() => new FormAtualizacao(_banco, new Atualizacao(), _versaoDisponivel));
            rodape.Controls.Add(_avisoAtualizacao, 1, 0);

            var verificar = new LinkLabel { Name = "verificarAtualizacoes", Text = "Verificar atualizações", AutoSize = true, Anchor = AnchorStyles.Right, Margin = new Padding(12, 3, 6, 3) };
            verificar.LinkClicked += (s, e) => AbrirTela(() => new FormAtualizacao(_banco));
            rodape.Controls.Add(verificar, 2, 0);

            rodape.Controls.Add(new Label
            {
                Name = "versao",
                Text = "Versão " + Diagnostico.VersaoApp,
                AutoSize = true,
                ForeColor = Color.Gray,
                Anchor = AnchorStyles.Right
            }, 3, 0);
            return rodape;
        }

        /// <summary>Ligada só pelo Program.cs: nos testes a tela não acessa a internet.</summary>
        public bool VerificarAtualizacaoAoAbrir { get; set; }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (VerificarAtualizacaoAoAbrir)
                VerificarAtualizacaoEmSegundoPlano();
        }

        private void VerificarAtualizacaoEmSegundoPlano()
        {
            var verificacao = new VerificacaoAutomatica(_banco);
            var atualizacao = new Atualizacao();

            Task.Run(() => verificacao.VerificarSePrecisa(atualizacao.ConsultarUltima, Atualizacao.VersaoAtual, DateTime.Now)).ContinueWith(tarefa =>
            {
                // Sem internet ou GitHub fora do ar: não incomoda; o Verificar atualizações mostra o motivo
                if (tarefa.IsFaulted)
                    Log.RegistrarErro(tarefa.Exception.GetBaseException());
                else if (tarefa.Result != null)
                    NaTela(() => MostrarAvisoAtualizacao(tarefa.Result));
            });
        }

        /// <summary>Volta para a thread da tela por BeginInvoke, só se a janela ainda estiver aberta.</summary>
        private void NaTela(Action acao)
        {
            if (IsDisposed || Disposing || !IsHandleCreated)
                return;

            try
            {
                BeginInvoke((Action)(() =>
                {
                    if (!IsDisposed && !Disposing)
                        acao();
                }));
            }
            catch (InvalidOperationException)
            {
                // A janela fechou entre a conferência e o BeginInvoke
            }
        }

        public void MostrarAvisoAtualizacao(VersaoPublicada versao)
        {
            _versaoDisponivel = versao;
            _avisoAtualizacao.Text = "Nova versão " + versao.Versao.ToString(3) + "!";
            _avisoAtualizacao.Visible = true;
        }

        private void AbrirPastaOrcamentos()
        {
            var pasta = ArquivosOrcamento.PastaBase(new ConfiguracaoRepositorio(_banco));
            try
            {
                AbrirPasta(pasta);
            }
            catch (Exception erro) when (erro is System.IO.IOException || erro is UnauthorizedAccessException || erro is System.ComponentModel.Win32Exception)
            {
                Log.RegistrarErro(erro);
                MessageBox.Show(this, "Não foi possível abrir a pasta de orçamentos:\n" + pasta + "\n\n" + erro.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        protected virtual void AbrirPasta(string pasta) => Sistema.AbrirPasta(pasta);

        private Control CriarBotoes()
        {
            // Botão sem ação = tela de uma fase que ainda não chegou: fica visível, mas desabilitado
            var botoes = new[]
            {
                CriarBotao("novoOrcamento", "Novo orçamento  (Ctrl+N)", () => new FormOrcamento(_banco)),
                CriarBotao("historico", "Histórico  (Ctrl+F)", () => new FormHistorico(_banco)),
                CriarBotao("clientes", "Clientes", () => new FormClientes(_banco)),
                CriarBotao("produtos", "Produtos", () => new FormProdutos(_banco)),
                CriarBotao("configuracoes", "Configurações", () => new FormConfiguracoes(_banco))
            };

            var grade = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3, Padding = new Padding(0, 12, 0, 12) };
            grade.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            grade.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            for (var linha = 0; linha < 3; linha++)
                grade.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / 3));

            grade.Controls.Add(botoes[0], 0, 0);
            grade.SetColumnSpan(botoes[0], 2);

            for (var i = 1; i < botoes.Length; i++)
                grade.Controls.Add(botoes[i], (i - 1) % 2, 1 + (i - 1) / 2);

            return grade;
        }

        private Button CriarBotao(string nome, string texto, Func<Form> abrirTela)
        {
            var botao = new Button
            {
                Name = nome,
                Text = texto,
                Dock = DockStyle.Fill,
                Margin = new Padding(6),
                Font = new Font("Segoe UI", 13F),
                Enabled = abrirTela != null
            };

            if (abrirTela != null)
                botao.Click += (s, e) => AbrirTela(abrirTela);

            return botao;
        }

        protected virtual void AbrirTela(Func<Form> abrirTela)
        {
            using (var form = abrirTela())
                form.ShowDialog(this);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.N:
                    AbrirTela(() => new FormOrcamento(_banco));
                    return true;
                case Keys.Control | Keys.F:
                    AbrirTela(() => new FormHistorico(_banco));
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected virtual void AbrirManual() => Estilo.AbrirManual(this);

        private MenuStrip CriarMenu()
        {
            var manual = new ToolStripMenuItem("Manual do usuário") { Name = "manual", ShortcutKeys = Keys.F1 };
            manual.Click += (s, e) => AbrirManual();

            var diagnostico = new ToolStripMenuItem("Diagnóstico");
            diagnostico.Click += AbrirDiagnostico;

            var ajuda = new ToolStripMenuItem("Ajuda");
            ajuda.DropDownItems.Add(manual);
            ajuda.DropDownItems.Add(diagnostico);

            var menu = new MenuStrip();
            menu.Items.Add(ajuda);
            return menu;
        }

        private void AbrirDiagnostico(object sender, EventArgs e)
        {
            using (var form = new FormDiagnostico())
                form.ShowDialog(this);
        }
    }
}
