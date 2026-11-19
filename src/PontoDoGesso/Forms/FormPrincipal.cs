using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using PontoDoGesso.Controles;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;
using PontoDoGesso.Exportacao;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Forms
{
    /// <summary>
    /// A única janela do app: menu lateral fixo à esquerda e, à direita, a tela escolhida (Início, Orçamento, Histórico...).
    /// Trocar de tela pergunta antes se há alterações não salvas. Só os diálogos pequenos abrem janelas por cima.
    /// </summary>
    public class FormPrincipal : Form, INavegacao, IDialogos
    {
        private static readonly Color Laranja = MenuLateral.Laranja;

        public const string TituloJanela = "Ponto do Gesso – Orçamentos";

        private static readonly Dictionary<Destino, string> BotoesDoMenu = new Dictionary<Destino, string>
        {
            [Destino.Inicio] = "inicio",
            [Destino.Orcamento] = "novoOrcamento",
            [Destino.Historico] = "historico",
            [Destino.Clientes] = "clientes",
            [Destino.Produtos] = "produtos",
            [Destino.Configuracoes] = "configuracoes"
        };

        private readonly Banco _bancoReal;
        private Banco _banco;
        private readonly MenuLateral _menu = new MenuLateral();
        private readonly Panel _conteudo = new Panel { Name = "conteudo", Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White };
        private LinkLabel _avisoAtualizacao;
        private Label _avisoDemonstracao;
        private ToolStripMenuItem _demonstracao;
        private VersaoPublicada _versaoDisponivel;

        public FormPrincipal() : this(new Banco())
        {
        }

        public FormPrincipal(Banco banco)
        {
            _bancoReal = banco;
            _banco = banco;

            Text = TituloJanela;
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96F, 96F);
            Font = new Font("Segoe UI", 9.5F);
            BackColor = Color.White;
            ClientSize = new Size(1000, 680);
            MinimumSize = new Size(800, 560);
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;

            MontarMenuLateral();

            // Tab: primeiro a tela atual, depois o menu lateral
            _conteudo.TabIndex = 0;
            _menu.TabIndex = 1;
            Controls.Add(_conteudo);
            Controls.Add(_menu);
            Controls.Add(CriarAvisoDemonstracao());
            var menu = CriarMenu();
            Controls.Add(menu);
            MainMenuStrip = menu;

            MostrarInicio();
        }

        // ---------- menu lateral ----------

        private void MontarMenuLateral()
        {
            var titulo = new TableLayoutPanel { AutoSize = true, ColumnCount = 1, Margin = new Padding(0, 6, 0, 10), Width = _menu.Width - 12 };
            titulo.Controls.Add(new Label { Text = "PONTO DO GESSO", Font = new Font("Times New Roman", 15F), ForeColor = Laranja, AutoSize = true, Anchor = AnchorStyles.None, Margin = new Padding(0) });
            titulo.Controls.Add(new Label { Text = "Orçamentos", Font = new Font("Segoe UI", 9F), ForeColor = Laranja, AutoSize = true, Anchor = AnchorStyles.None, Margin = new Padding(0) });
            _menu.AdicionarTitulo(titulo);

            _menu.Adicionar("inicio", "Início", Icones.Inicio, () => Ir(Destino.Inicio));
            _menu.Adicionar("novoOrcamento", "Novo orçamento", Icones.Orcamento, () => Ir(Destino.Orcamento));
            _menu.Adicionar("historico", "Histórico", Icones.Historico, () => Ir(Destino.Historico));
            _menu.Adicionar("clientes", "Clientes", Icones.Clientes, () => Ir(Destino.Clientes));
            _menu.Adicionar("produtos", "Produtos", Icones.Produtos, () => Ir(Destino.Produtos));

            _menu.Adicionar("configuracoes", "Configurações", Icones.Configuracoes, () => Ir(Destino.Configuracoes), true);
            _menu.Adicionar("pastaOrcamentos", "Abrir pasta de orçamentos", Icones.Pasta, AbrirPastaOrcamentos, true);
            _menu.Adicionar("verificarAtualizacoes", "Verificar atualizações", Icones.Atualizar, () => AbrirDialogo(() => new FormAtualizacao(_bancoReal)), true);

            // Aviso da verificação automática: só aparece quando há versão nova
            _avisoAtualizacao = new LinkLabel { Name = "avisoAtualizacao", AutoSize = true, Visible = false, LinkColor = Laranja, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Margin = new Padding(8, 2, 0, 2) };
            _avisoAtualizacao.LinkClicked += (s, e) => AbrirDialogo(() => new FormAtualizacao(_bancoReal, new Atualizacao(), _versaoDisponivel));
            _menu.AdicionarEmbaixo(_avisoAtualizacao);

            _menu.AdicionarEmbaixo(new Label { Name = "versao", Text = "Versão " + Diagnostico.VersaoApp, AutoSize = true, ForeColor = Color.Gray, Margin = new Padding(8, 2, 0, 0) });
        }

        public MenuLateral Menu => _menu;

        // ---------- troca de tela ----------

        /// <summary>Tela que aparece agora na área principal.</summary>
        public TelaBase TelaAtual { get; private set; }

        public bool Ir(Destino destino)
        {
            switch (destino)
            {
                case Destino.Orcamento:
                    return NovoOrcamento();
                case Destino.Historico:
                    return Trocar(() => new TelaHistorico(_banco));
                case Destino.Clientes:
                    return Trocar(() => new TelaClientes(_banco));
                case Destino.Produtos:
                    return Trocar(() => new TelaProdutos(_banco));
                case Destino.Configuracoes:
                    return Trocar(() => new TelaConfiguracoes(_banco));
                default:
                    return Trocar(() => new TelaInicio(_banco));
            }
        }

        /// <summary>Já na tela de orçamento, sem cliente: a mesma tela passa a ser um orçamento em branco (como o Ctrl+N).</summary>
        public bool NovoOrcamento(Cliente cliente = null)
        {
            if (cliente == null && TelaAtual is TelaOrcamento atual)
                return atual.NovoOrcamento();

            return Trocar(() => new TelaOrcamento(_banco, null, cliente));
        }

        public bool AbrirOrcamento(Orcamento orcamento, bool duplicar) =>
            Trocar(() =>
            {
                var tela = new TelaOrcamento(_banco, orcamento);
                if (duplicar)
                    tela.Duplicar();
                return tela;
            });

        public bool AbrirHistorico(FiltroHistorico filtro) => Trocar(() => new TelaHistorico(_banco, filtro));

        /// <summary>Pergunta à tela atual se ela pode sair (alterações não salvas) e só então mostra a nova.</summary>
        protected virtual bool Trocar(Func<TelaBase> criar)
        {
            if (TelaAtual != null && !TelaAtual.PodeSair())
                return false;

            Mostrar(criar());
            return true;
        }

        private void Mostrar(TelaBase nova)
        {
            var anterior = TelaAtual;
            TelaAtual = nova;
            nova.Navegacao = this;
            nova.Dialogos = this;

            _conteudo.SuspendLayout();
            _conteudo.AutoScrollPosition = Point.Empty;
            _conteudo.AutoScrollMinSize = LogicalToDeviceUnits(nova.TamanhoMinimo);
            _conteudo.Controls.Add(nova);
            if (anterior != null)
                _conteudo.Controls.Remove(anterior);
            _conteudo.ResumeLayout(true);

            _menu.Destacar(BotoesDoMenu[nova.Destino]);
            ActiveControl = nova.ControleInicial != null && nova.ControleInicial.Enabled ? nova.ControleInicial : nova;
            nova.MostrarInicioDosCampos();
            nova.AoAtivar();

            if (anterior != null)
                Descartar(anterior);
        }

        /// <summary>
        /// A tela que saiu pode estar no meio do próprio clique (ex.: botão Abrir do histórico): é descartada depois que a
        /// mensagem atual termina.
        /// </summary>
        private void Descartar(TelaBase tela)
        {
            if (IsHandleCreated && !IsDisposed)
                BeginInvoke((Action)tela.Dispose);
            else
                tela.Dispose();
        }

        private void MostrarInicio()
        {
            TelaBase inicio;
            try
            {
                inicio = new TelaInicio(_banco);
            }
            catch (Exception erro) when (erro is System.Data.SQLite.SQLiteException || erro is System.IO.IOException || erro is UnauthorizedAccessException || erro is InvalidOperationException)
            {
                // Banco com problema: a janela abre mesmo assim, para dar acesso ao Diagnóstico (Ajuda)
                Log.RegistrarErro(erro);
                inicio = new TelaBase { Text = "Início" };
                inicio.Controls.Add(new Label { Name = "erroBanco", Dock = DockStyle.Fill, Padding = new Padding(16), Text = "Não foi possível abrir os dados do app. " + MensagemErro.Explicar(erro) + "\n\nUse Ajuda → Diagnóstico para investigar." });
            }

            Mostrar(inicio);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            TelaAtual?.AoAtivar();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && TelaAtual != null && !TelaAtual.PodeSair())
                e.Cancel = true;

            base.OnFormClosing(e);
        }

        /// <summary>Atalhos que a tela atual não usou: Ctrl+N abre um orçamento novo e Ctrl+F o histórico.</summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.N:
                    NovoOrcamento();
                    return true;
                case Keys.Control | Keys.F:
                    Ir(Destino.Historico);
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ---------- mensagens das telas ----------

        public virtual void Avisar(Control tela, string mensagem) =>
            MessageBox.Show(this, mensagem, tela.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);

        public virtual void Informar(Control tela, string mensagem) =>
            MessageBox.Show(this, mensagem, tela.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);

        public virtual bool Confirmar(Control tela, string pergunta, MessageBoxIcon icone, MessageBoxDefaultButton padrao) =>
            MessageBox.Show(this, pergunta, tela.Text, MessageBoxButtons.YesNo, icone, padrao) == DialogResult.Yes;

        public virtual DialogResult PerguntarSalvar(Control tela, string pergunta) => PerguntaSalvar.Perguntar(this, pergunta, tela.Text);

        // ---------- atualização ----------

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
            var verificacao = new VerificacaoAutomatica(_bancoReal);
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

        // ---------- pasta e diálogos ----------

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
                MessageBox.Show(this, "Não foi possível abrir a pasta de orçamentos:\n" + pasta + "\n\n" + MensagemErro.Explicar(erro), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        protected virtual void AbrirPasta(string pasta) => Sistema.AbrirPasta(pasta);

        /// <summary>Janela por cima da principal (Verificar atualizações, Sobre); a tela atual continua como estava.</summary>
        protected virtual void AbrirDialogo(Func<Form> abrir)
        {
            using (var form = abrir())
                form.ShowDialog(this);
        }

        protected virtual void AbrirManual() => Estilo.AbrirManual(this);

        private MenuStrip CriarMenu()
        {
            var manual = new ToolStripMenuItem("Manual do usuário") { Name = "manual", ShortcutKeys = Keys.F1 };
            manual.Click += (s, e) => AbrirManual();

            var diagnostico = new ToolStripMenuItem("Diagnóstico");
            diagnostico.Click += (s, e) => AbrirDialogo(() => new FormDiagnostico());

            var sobre = new ToolStripMenuItem("Sobre") { Name = "sobre" };
            sobre.Click += (s, e) => AbrirDialogo(() => new FormSobre(Caminhos.PastaDados));

            _demonstracao = new ToolStripMenuItem("Dados de demonstração") { Name = "demonstracao" };
            _demonstracao.Click += (s, e) => AlternarDemonstracao();

            var ajuda = new ToolStripMenuItem("Ajuda");
            ajuda.DropDownItems.Add(manual);
            ajuda.DropDownItems.Add(diagnostico);
            ajuda.DropDownItems.Add(_demonstracao);
            ajuda.DropDownItems.Add(new ToolStripSeparator());
            ajuda.DropDownItems.Add(sobre);

            var menu = new MenuStrip();
            menu.Items.Add(ajuda);
            return menu;
        }

        // ---------- modo de demonstração ----------

        private Control CriarAvisoDemonstracao()
        {
            _avisoDemonstracao = new Label
            {
                Name = "avisoDemonstracao",
                Text = "MODO DEMONSTRAÇÃO – dados fictícios  (Ajuda → Sair da demonstração)",
                Dock = DockStyle.Top,
                AutoSize = false,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(0xFF, 0xE0, 0x82),
                ForeColor = Color.FromArgb(0x5C, 0x45, 0x00),
                Visible = false
            };
            return _avisoDemonstracao;
        }

        /// <summary>Banco usado pelas telas agora: o real ou o de demonstração.</summary>
        public Banco BancoAtual => _banco;

        public bool EmDemonstracao => _banco != _bancoReal;

        /// <summary>Troca de banco e volta para o Início (antes, a tela atual pode pedir para salvar).</summary>
        public void AlternarDemonstracao()
        {
            if (TelaAtual != null && !TelaAtual.PodeSair())
                return;

            if (EmDemonstracao)
            {
                _banco = _bancoReal;
            }
            else
            {
                try
                {
                    _banco = CriarBancoDemonstracao();
                }
                catch (Exception erro) when (erro is System.IO.IOException || erro is UnauthorizedAccessException || erro is System.Data.SQLite.SQLiteException)
                {
                    Log.RegistrarErro(erro);
                    MessageBox.Show(this, "Não foi possível criar os dados de demonstração. " + MensagemErro.Explicar(erro), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            _avisoDemonstracao.Visible = EmDemonstracao;
            _demonstracao.Text = EmDemonstracao ? "Sair da demonstração" : "Dados de demonstração";
            Text = EmDemonstracao ? TituloJanela + " – DEMONSTRAÇÃO" : TituloJanela;
            MostrarInicio();
        }

        /// <summary>Recriado a cada entrada, com datas a partir de hoje; nunca toca no banco real.</summary>
        protected virtual Banco CriarBancoDemonstracao() => Demonstracao.Criar(DateTime.Today);
    }
}
