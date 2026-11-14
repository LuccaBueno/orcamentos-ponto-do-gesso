using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using PontoDoGesso.Dados;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Forms
{
    /// <summary>Verificar atualizações: mostra a versão nova e as notas, baixa, confere, faz backup e chama o Atualizador.</summary>
    public class FormAtualizacao : Form
    {
        private readonly Banco _banco;
        private readonly Atualizacao _atualizacao;
        private VersaoPublicada _publicada;
        private CancellationTokenSource _cancelamento;
        private bool _fechada;

        private readonly Label _nova;
        private readonly Label _situacao;
        private readonly TextBox _notas;
        private readonly ProgressBar _progresso;
        private readonly Button _atualizar;
        private readonly Button _downloads;
        private readonly Button _depois;

        public FormAtualizacao(Banco banco) : this(banco, new Atualizacao(), null)
        {
        }

        /// <param name="jaConsultada">Versão encontrada pela verificação automática; nulo = consulta ao abrir.</param>
        public FormAtualizacao(Banco banco, Atualizacao atualizacao, VersaoPublicada jaConsultada)
        {
            _banco = banco;
            _atualizacao = atualizacao;
            _publicada = jaConsultada;

            Estilo.Aplicar(this, 560, 400);
            Text = "Verificar atualizações";
            MinimumSize = new Size(480, 360);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(12) };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            layout.Controls.Add(new Label { Name = "atual", Text = "Versão instalada: " + Atualizacao.VersaoAtual.ToString(3), AutoSize = true });
            _nova = new Label { Name = "nova", AutoSize = true, Font = new Font(Font, FontStyle.Bold), Margin = new Padding(3, 6, 3, 0) };
            layout.Controls.Add(_nova);
            _situacao = new Label { Name = "situacao", AutoSize = true, MaximumSize = new Size(530, 0), Margin = new Padding(3, 6, 3, 6) };
            layout.Controls.Add(_situacao);
            _notas = new TextBox
            {
                Name = "notas",
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                TabStop = false,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            layout.Controls.Add(_notas);
            _progresso = new ProgressBar { Name = "progresso", Dock = DockStyle.Fill, Height = 18, Visible = false, Margin = new Padding(3, 8, 3, 0) };
            layout.Controls.Add(_progresso);

            _atualizar = Estilo.CriarBotao("Atualizar agora", (s, e) => Atualizar());
            _atualizar.Name = "atualizar";
            _atualizar.Enabled = false;
            _downloads = Estilo.CriarBotao("Abrir página de downloads", (s, e) => AbrirDownloads());
            _downloads.Name = "downloads";
            _depois = Estilo.CriarBotao("Depois", (s, e) => Close());
            _depois.Name = "depois";

            var botoes = new FlowLayoutPanel { AutoSize = true, Anchor = AnchorStyles.Right, Margin = new Padding(0, 10, 0, 0) };
            botoes.Controls.Add(_atualizar);
            botoes.Controls.Add(_downloads);
            botoes.Controls.Add(_depois);
            layout.Controls.Add(botoes);

            Controls.Add(layout);
            AcceptButton = _atualizar;
            CancelButton = _depois;

            if (_publicada != null)
                Mostrar(_publicada);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (_publicada == null)
                Consultar();
            else
                (_atualizar.Enabled ? _atualizar : _depois).Focus();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _cancelamento?.Cancel();
            base.OnFormClosing(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _fechada = true;
            base.OnFormClosed(e);
        }

        /// <summary>
        /// O download e a consulta terminam em outra thread e voltam para a tela depois.
        /// Se a janela já foi fechada (ou está sendo descartada), não mexe mais nos controles.
        /// </summary>
        private bool PodeAtualizarTela => !_fechada && !IsDisposed && !Disposing && IsHandleCreated;

        /// <summary>
        /// Roda a ação na thread da tela, via BeginInvoke (não depende do SynchronizationContext da thread),
        /// e só se a janela ainda estiver aberta quando a ação chegar.
        /// </summary>
        private void NaTela(Action acao)
        {
            if (!PodeAtualizarTela)
                return;

            try
            {
                BeginInvoke((Action)(() =>
                {
                    if (PodeAtualizarTela)
                        acao();
                }));
            }
            catch (InvalidOperationException)
            {
                // A janela fechou entre a conferência e o BeginInvoke
            }
        }

        private void Consultar()
        {
            _situacao.Text = "Verificando no GitHub…";
            UseWaitCursor = true;

            Task.Run(() => _atualizacao.ConsultarUltima()).ContinueWith(tarefa => NaTela(() =>
            {
                UseWaitCursor = false;
                if (tarefa.IsFaulted)
                    MostrarErroConsulta(tarefa.Exception.GetBaseException());
                else
                    Mostrar(tarefa.Result);
            }));
        }

        public void Mostrar(VersaoPublicada publicada)
        {
            _publicada = publicada;
            _notas.Text = Atualizacao.TextoSimples(publicada.Notas);

            if (Atualizacao.EhMaisNova(publicada.Versao, Atualizacao.VersaoAtual))
            {
                _nova.Text = "Versão nova: " + publicada.Versao.ToString(3);
                _situacao.Text = "O que mudou:";
                _atualizar.Enabled = true;
                if (IsHandleCreated)
                    _atualizar.Focus();
            }
            else
            {
                _nova.Text = "Você já está com a versão mais nova.";
                _situacao.Text = "Última versão publicada: " + publicada.Versao.ToString(3) + ".";
                _atualizar.Enabled = false;
                _depois.Text = "Fechar";
            }
        }

        public void MostrarErroConsulta(Exception erro)
        {
            Log.RegistrarErro(erro);
            _nova.Text = "Não foi possível verificar.";
            _situacao.Text = "Não foi possível falar com o GitHub. " + MensagemErro.Explicar(erro) + "\n\n"
                + "Para atualizar à mão, clique em Abrir página de downloads, baixe o .zip da versão mais nova e siga o docs\\INSTALACAO-WINDOWS7.md.";
            _atualizar.Enabled = false;
            _depois.Text = "Fechar";
        }

        private void Atualizar()
        {
            // O foco vai para o Cancelar, que continua habilitado durante o download
            _depois.Focus();
            _atualizar.Enabled = false;
            _downloads.Enabled = false;
            _depois.Text = "Cancelar";
            _progresso.Value = 0;
            _progresso.Visible = true;
            _situacao.Text = "Baixando " + _publicada.NomeZip + "…";

            _cancelamento = new CancellationTokenSource();
            var cancelamento = _cancelamento.Token;
            var publicada = _publicada;

            Task.Run(() => _atualizacao.Baixar(publicada, p => NaTela(() => _progresso.Value = p), cancelamento))
                .ContinueWith(tarefa => NaTela(() =>
                {
                    if (tarefa.IsCanceled || (tarefa.IsFaulted && tarefa.Exception.GetBaseException() is OperationCanceledException))
                        return;

                    if (tarefa.IsFaulted)
                        FalharDownload(tarefa.Exception.GetBaseException());
                    else
                        Instalar(tarefa.Result);
                }));
        }

        private void FalharDownload(Exception erro)
        {
            Log.RegistrarErro(erro);
            _progresso.Visible = false;
            _downloads.Enabled = true;
            _depois.Text = "Fechar";
            _atualizar.Enabled = true;
            _situacao.Text = erro is PacoteInvalidoException
                ? erro.Message
                : "Não foi possível baixar a versão nova. " + MensagemErro.Explicar(erro) + " Nada foi alterado. Tente de novo ou use a página de downloads.";
        }

        private void Instalar(string zip)
        {
            _situacao.Text = "Arquivo conferido. Fazendo backup dos dados…";
            Refresh();

            try
            {
                new Backup(_banco).AntesDeAtualizar(DateTime.Now);
            }
            catch (Exception erro)
            {
                Log.RegistrarErro(erro);
                _situacao.Text = "Não foi possível fazer o backup dos dados antes de atualizar. " + MensagemErro.Explicar(erro) + " A atualização foi cancelada; nada foi alterado.";
                _progresso.Visible = false;
                _downloads.Enabled = true;
                _depois.Text = "Fechar";
                return;
            }

            try
            {
                IniciarAtualizador(zip);
            }
            catch (AtualizadorAusenteException erro)
            {
                Log.RegistrarErro(erro);
                Avisar(erro.Message + "\n\nA atualização automática não vai funcionar. A página de downloads vai abrir para você baixar e instalar a versão nova à mão.");
                AbrirDownloads();
                Close();
                return;
            }

            _situacao.Text = "O app vai fechar e abrir de novo já atualizado.";
            Refresh();
            EncerrarApp();
        }

        protected virtual void IniciarAtualizador(string zip)
        {
            using (var processo = Process.GetCurrentProcess())
                Atualizacao.IniciarAtualizador(Application.StartupPath, zip, processo.Id, Caminhos.PastaLogs);
        }

        protected virtual void EncerrarApp() => Application.Exit();

        private void AbrirDownloads()
        {
            try
            {
                AbrirLink(_publicada?.Pagina ?? Atualizacao.PaginaDownloads);
            }
            catch (Exception erro)
            {
                Log.RegistrarErro(erro);
                Avisar("Não foi possível abrir o navegador. Abra este endereço à mão:\n\n" + Atualizacao.PaginaDownloads);
            }
        }

        protected virtual void AbrirLink(string link) => Sistema.AbrirLink(link);

        protected virtual void Avisar(string mensagem) =>
            MessageBox.Show(this, mensagem, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
