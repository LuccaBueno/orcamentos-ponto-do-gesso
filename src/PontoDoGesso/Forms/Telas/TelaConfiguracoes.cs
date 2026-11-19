using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;
using PontoDoGesso.Exportacao;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Forms
{
    /// <summary>Dados da empresa, padrões do orçamento, exportação e backup, com um Salvar só para todas as abas.</summary>
    public class TelaConfiguracoes : TelaBase
    {
        public const string ImpressoraDoWindows = "(impressora padrão do Windows)";

        private readonly ConfiguracaoRepositorio _configuracoes;
        private readonly Backup _backup;
        private readonly TelaEmpresa _empresa;
        private readonly TabControl _abas = new TabControl { Name = "abas", Dock = DockStyle.Fill };

        private readonly NumericUpDown _validade = new NumericUpDown { Name = "validade", Minimum = 1, Maximum = 365, Width = 60 };
        private readonly TextBox _cidade = Campo("cidade", 60);
        private readonly TextBox _ddd = new TextBox { Name = "ddd", MaxLength = 2, Width = 40 };
        private readonly CheckBox _perguntarAvulso = new CheckBox { Name = "perguntarAvulso", Text = "Ao salvar orçamento de cliente avulso, perguntar se quer cadastrar o cliente", AutoSize = true };

        private readonly TextBox _pasta = Campo("pasta", 260);
        private readonly ComboBox _impressora = new ComboBox { Name = "impressora", DropDownStyle = ComboBoxStyle.DropDownList, Width = 320 };

        private readonly Label _ultimoBackup = new Label { Name = "ultimoBackup", AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Margin = new Padding(3, 6, 3, 6) };
        private readonly TextBox _pastaExtra = new TextBox { Name = "pastaExtra", ReadOnly = true, Dock = DockStyle.Fill };

        public TelaConfiguracoes(Banco banco)
        {
            _configuracoes = new ConfiguracaoRepositorio(banco);
            _backup = new Backup(banco);

            Text = "Configurações";

            _empresa = new TelaEmpresa(banco, true) { Name = "empresa" };

            _abas.TabPages.Add(Aba("Empresa", _empresa));
            _abas.TabPages.Add(Aba("Orçamento", CriarOrcamento()));
            _abas.TabPages.Add(Aba("Exportação", CriarExportacao()));
            _abas.TabPages.Add(Aba("Backup", CriarBackup()));

            var salvar = Estilo.CriarBotao("Salvar (Ctrl+S)", (s, e) => SalvarEAvisar());
            salvar.Name = "salvar";

            var botoes = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(6) };
            botoes.Controls.Add(salvar);

            Controls.Add(_abas);
            Controls.Add(botoes);
            ControleInicial = _empresa.ControleInicial;

            Exibir(ConfiguracoesApp.Ler(_configuracoes));
            _salvas = Chave(Lidas());
        }

        public override Size TamanhoMinimo => new Size(640, 600);

        private static TextBox Campo(string nome, int limite) => new TextBox { Name = nome, Dock = DockStyle.Fill, MaxLength = limite };

        private static TabPage Aba(string titulo, Control conteudo)
        {
            var aba = new TabPage(titulo) { Padding = new Padding(8), UseVisualStyleBackColor = true };
            aba.Controls.Add(conteudo);
            return aba;
        }

        private static TableLayoutPanel Tabela()
        {
            var tabela = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoScroll = true };
            tabela.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tabela.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            return tabela;
        }

        private static void Linha(TableLayoutPanel tabela, string rotulo, Control controle)
        {
            tabela.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            if (rotulo != null)
                tabela.Controls.Add(new Label { Text = rotulo, AutoSize = true, Anchor = AnchorStyles.Left }, 0, tabela.RowCount);
            tabela.Controls.Add(controle, rotulo == null ? 0 : 1, tabela.RowCount);
            if (rotulo == null)
                tabela.SetColumnSpan(controle, 2);
            tabela.RowCount++;
        }

        private static Label Explicacao(string texto) =>
            new Label { Text = texto, AutoSize = true, ForeColor = Color.DimGray, MaximumSize = new Size(600, 0), Margin = new Padding(3, 2, 3, 10) };

        private static FlowLayoutPanel Fila(params Control[] controles)
        {
            var fila = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
            fila.Controls.AddRange(controles);
            return fila;
        }

        private Button Botao(string nome, string texto, Action acao)
        {
            var botao = Estilo.CriarBotao(texto, (s, e) => acao());
            botao.Name = nome;
            return botao;
        }

        // ---------- abas ----------

        private Control CriarOrcamento()
        {
            var tabela = Tabela();
            Linha(tabela, "Validade sugerida (dias)", _validade);
            Linha(tabela, null, Explicacao("A validade continua desligada por padrão em cada orçamento; este é o número que aparece quando ela é ligada."));
            Linha(tabela, "Cidade padrão", _cidade);
            Linha(tabela, null, Explicacao("Já vem preenchida em clientes novos e na entrega de clientes avulsos."));
            Linha(tabela, "DDD padrão", _ddd);
            Linha(tabela, null, Explicacao("Já vem preenchido nos telefones; sempre dá para trocar."));
            Linha(tabela, null, _perguntarAvulso);
            return tabela;
        }

        private Control CriarExportacao()
        {
            var tabela = Tabela();
            var escolher = Botao("escolherPasta", "Escolher…", () =>
            {
                var pasta = EscolherPasta("Pasta onde os orçamentos (PNG e PDF) serão salvos", _pasta.Text);
                if (pasta != null)
                    _pasta.Text = pasta;
            });
            var abrir = Botao("abrirPasta", "Abrir", () => Abrir(() => AbrirPasta(_pasta.Text.Trim())));

            var linhaPasta = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 3, Margin = new Padding(0) };
            linhaPasta.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            linhaPasta.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            linhaPasta.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            linhaPasta.Controls.Add(_pasta, 0, 0);
            linhaPasta.Controls.Add(escolher, 1, 0);
            linhaPasta.Controls.Add(abrir, 2, 0);

            Linha(tabela, "Pasta dos orçamentos", linhaPasta);
            Linha(tabela, null, Explicacao("Os arquivos ficam separados por ano e mês (ex.: 2026\\11)."));
            Linha(tabela, "Impressora padrão", _impressora);
            Linha(tabela, null, Explicacao("Já vem escolhida ao imprimir um orçamento. Se ela for desinstalada, o app usa a impressora padrão do Windows."));
            return tabela;
        }

        private Control CriarBackup()
        {
            var tabela = Tabela();
            Linha(tabela, null, _ultimoBackup);
            Linha(tabela, null, Explicacao("O app faz um backup automático ao abrir, uma vez por dia, e guarda os últimos " + Backup.AutomaticosMantidos + ". Pasta dos backups: " + _backup.Pasta));

            Linha(tabela, null, Fila(
                Botao("fazerBackup", "Fazer backup agora", FazerBackup),
                Botao("copiarBackup", "Copiar backup para…", CopiarBackup),
                Botao("abrirBackups", "Abrir pasta dos backups", () => Abrir(() => AbrirPasta(_backup.Pasta)))));

            var linhaExtra = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 3, Margin = new Padding(0, 12, 0, 0) };
            linhaExtra.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            linhaExtra.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            linhaExtra.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            linhaExtra.Controls.Add(_pastaExtra, 0, 0);
            linhaExtra.Controls.Add(Botao("escolherPastaExtra", "Escolher…", EscolherPastaExtra), 1, 0);
            linhaExtra.Controls.Add(Botao("removerPastaExtra", "Remover", () => DefinirPastaExtra("")), 2, 0);
            Linha(tabela, null, new Label { Text = "Pasta extra de backup (opcional)", AutoSize = true, Font = new Font(Font, FontStyle.Bold), Margin = new Padding(3, 14, 3, 0) });
            Linha(tabela, null, linhaExtra);
            Linha(tabela, null, Explicacao("Cada backup também é copiado para esta pasta (pendrive, HD externo ou pasta de rede) quando ela estiver acessível. Vale na hora, sem precisar clicar em Salvar."));

            Linha(tabela, null, new Label { Text = "Restaurar", AutoSize = true, Font = new Font(Font, FontStyle.Bold), Margin = new Padding(3, 14, 3, 0) });
            Linha(tabela, null, Botao("restaurar", "Restaurar backup…", Restaurar));
            Linha(tabela, null, Explicacao("Troca todos os dados do app pelos do backup escolhido. Antes, uma cópia dos dados atuais é guardada na pasta dos backups. O app reinicia em seguida."));
            return tabela;
        }

        // ---------- exibir e salvar ----------

        private void Exibir(ConfiguracoesApp configuracoes)
        {
            _validade.Value = configuracoes.ValidadeSugeridaDias;
            _cidade.Text = configuracoes.CidadePadrao;
            _ddd.Text = configuracoes.DddPadrao;
            _perguntarAvulso.Checked = configuracoes.PerguntarCadastroAvulso;

            _pasta.Text = configuracoes.PastaExportacao;
            _impressora.Items.Clear();
            _impressora.Items.Add(ImpressoraDoWindows);
            foreach (var impressora in Impressoras())
                _impressora.Items.Add(impressora);
            if (configuracoes.Impressora.Length > 0 && !_impressora.Items.Contains(configuracoes.Impressora))
                _impressora.Items.Add(configuracoes.Impressora);
            _impressora.SelectedItem = configuracoes.Impressora.Length > 0 ? configuracoes.Impressora : ImpressoraDoWindows;

            _pastaExtra.Text = _backup.PastaExtra;
            AtualizarUltimoBackup();
        }

        protected virtual string[] Impressoras()
        {
            try
            {
                return PrinterSettings.InstalledPrinters.Cast<string>().ToArray();
            }
            catch (Exception erro) when (erro is System.ComponentModel.Win32Exception || erro is InvalidPrinterException)
            {
                // Spooler de impressão parado: a lista fica só com a padrão
                Log.RegistrarErro(erro);
                return new string[0];
            }
        }

        public ConfiguracoesApp Lidas() => new ConfiguracoesApp
        {
            ValidadeSugeridaDias = (int)_validade.Value,
            CidadePadrao = _cidade.Text,
            DddPadrao = _ddd.Text,
            PerguntarCadastroAvulso = _perguntarAvulso.Checked,
            PastaExportacao = _pasta.Text,
            Impressora = Equals(_impressora.SelectedItem, ImpressoraDoWindows) ? "" : Convert.ToString(_impressora.SelectedItem)
        };

        public bool Salvar()
        {
            var configuracoes = Lidas();
            var erro = configuracoes.Validar();
            if (erro != null)
            {
                Avisar(erro);
                return false;
            }

            // A aba Empresa confere os próprios campos e avisa; as outras abas só gravam se ela gravou
            if (_empresa.Alterado && !_empresa.Salvar())
            {
                _abas.SelectedIndex = 0;
                return false;
            }

            configuracoes.Gravar(_configuracoes);
            _salvas = Chave(configuracoes);
            return true;
        }

        private void SalvarEAvisar()
        {
            if (Salvar())
                Informar("Configurações salvas.");
        }

        /// <summary>Configurações como estavam ao abrir a janela (ou no último Salvar), para saber se há alterações.</summary>
        private string _salvas;

        public bool Alterado => _empresa.Alterado || Chave(Lidas()) != _salvas;

        private static string Chave(ConfiguracoesApp c) => string.Join("\u001f",
            c.ValidadeSugeridaDias, c.CidadePadrao, c.DddPadrao, c.PerguntarCadastroAvulso, c.PastaExportacao, c.Impressora);

        public override Destino Destino => Destino.Configuracoes;

        public override bool PodeSair() =>
            PerguntarAntesDeSair(Alterado, "As configurações têm alterações não salvas. Salvar antes de sair?", Salvar);

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.S))
            {
                SalvarEAvisar();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ---------- backup ----------

        private void AtualizarUltimoBackup()
        {
            var ultimo = _backup.Ultimo;
            _ultimoBackup.Text = ultimo == null ? "Nenhum backup feito ainda." : "Último backup: " + ultimo.Value.ToString("dd/MM/yyyy 'às' HH:mm");
        }

        private void FazerBackup()
        {
            var arquivo = Executar(() => _backup.FazerAgora(DateTime.Now));
            AtualizarUltimoBackup();
            if (arquivo != null)
                Informar("Backup feito:\n" + arquivo);
        }

        private void CopiarBackup()
        {
            var pasta = EscolherPasta("Escolha onde guardar a cópia do backup (pendrive, HD externo…)", null);
            if (pasta == null)
                return;

            var arquivo = Executar(() => _backup.CopiarPara(pasta, DateTime.Now));
            if (arquivo != null)
                Informar("Cópia do backup salva em:\n" + arquivo);
        }

        private void EscolherPastaExtra()
        {
            var pasta = EscolherPasta("Pasta extra de backup (pendrive, HD externo ou pasta de rede)", _pastaExtra.Text);
            if (pasta != null)
                DefinirPastaExtra(pasta);
        }

        private void DefinirPastaExtra(string pasta)
        {
            _backup.PastaExtra = pasta;
            _pastaExtra.Text = _backup.PastaExtra;
        }

        private void Restaurar()
        {
            var arquivo = EscolherArquivoBackup(_backup.Pasta);
            if (arquivo == null)
                return;

            var erro = Backup.Validar(arquivo);
            if (erro != null)
            {
                Avisar("Este arquivo não pode ser restaurado.\n\n" + erro);
                return;
            }

            if (!Confirmar("Restaurar o backup\n" + arquivo + "?\n\nTodos os dados atuais (clientes, produtos, orçamentos e configurações) serão trocados pelos do backup. "
                + "Uma cópia dos dados atuais fica guardada na pasta dos backups.\n\nO app vai reiniciar."))
                return;

            var guardado = Executar(() => _backup.Restaurar(arquivo, DateTime.Now));
            if (guardado == null)
                return;

            Informar("Backup restaurado. Os dados de antes ficaram guardados em:\n" + guardado + "\n\nO app vai reiniciar agora.");
            Reiniciar();
        }

        private string Executar(Func<string> acao)
        {
            try
            {
                return acao();
            }
            catch (Exception erro) when (erro is System.IO.IOException || erro is UnauthorizedAccessException || erro is System.Data.SQLite.SQLiteException || erro is ErroConhecidoException)
            {
                Log.RegistrarErro(erro);
                Avisar("Não foi possível concluir. " + MensagemErro.Explicar(erro));
                return null;
            }
        }

        private void Abrir(Action abrir)
        {
            try
            {
                abrir();
            }
            catch (Exception erro) when (erro is System.IO.IOException || erro is UnauthorizedAccessException || erro is System.ComponentModel.Win32Exception || erro is ArgumentException)
            {
                Log.RegistrarErro(erro);
                Avisar("Não foi possível abrir a pasta. " + MensagemErro.Explicar(erro));
            }
        }

        // ---------- janelas e o que sai do app (trocados nos testes) ----------

        protected virtual string EscolherPasta(string descricao, string inicial)
        {
            using (var dialogo = new FolderBrowserDialog { Description = descricao, ShowNewFolderButton = true, SelectedPath = inicial ?? "" })
                return dialogo.ShowDialog(this) == DialogResult.OK ? dialogo.SelectedPath : null;
        }

        protected virtual string EscolherArquivoBackup(string pasta)
        {
            using (var dialogo = new OpenFileDialog { Title = "Escolha o backup para restaurar", Filter = "Backups do Ponto do Gesso (*.db)|*.db|Todos os arquivos (*.*)|*.*", InitialDirectory = pasta })
                return dialogo.ShowDialog(this) == DialogResult.OK ? dialogo.FileName : null;
        }

        protected virtual void AbrirPasta(string pasta) => Sistema.AbrirPasta(pasta);

        protected virtual void Reiniciar() => Application.Restart();

        protected override bool Confirmar(string pergunta) => Confirmar(pergunta, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
    }
}
