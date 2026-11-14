using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Forms
{
    public class FormClientes : Form
    {
        private readonly Banco _banco;
        private readonly ClienteRepositorio _clientes;
        private readonly TextBox _busca;
        private readonly CheckBox _mostrarInativos;
        private readonly DataGridView _lista;
        private readonly Button _editar;
        private readonly Button _inativar;
        private readonly Button _novoOrcamento;
        private readonly Button _orcamentosDoCliente;

        public FormClientes(Banco banco)
        {
            _banco = banco;
            _clientes = new ClienteRepositorio(banco);

            Text = "Clientes";
            Estilo.Aplicar(this, 760, 480);
            MinimumSize = new Size(620, 360);

            _busca = new TextBox { Name = "busca", Dock = DockStyle.Fill };
            _busca.TextChanged += (s, e) => Carregar();
            Estilo.LigarBusca(this, _busca);

            _mostrarInativos = new CheckBox { Name = "mostrarInativos", Text = "Mostrar inativos", AutoSize = true, Anchor = AnchorStyles.Left };
            _mostrarInativos.CheckedChanged += (s, e) => Carregar();

            var topo = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 3, Padding = new Padding(6) };
            topo.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            topo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            topo.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            topo.Controls.Add(new Label { Text = "Buscar (código, nome ou telefone):", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
            topo.Controls.Add(_busca, 1, 0);
            topo.Controls.Add(_mostrarInativos, 2, 0);

            _lista = Estilo.CriarLista();
            _lista.Name = "lista";
            _lista.Columns.Add(Estilo.CriarColuna("codigo", "Código", 12));
            _lista.Columns.Add(Estilo.CriarColuna("nome", "Nome", 40));
            _lista.Columns.Add(Estilo.CriarColuna("telefone", "Telefone", 20));
            _lista.Columns.Add(Estilo.CriarColuna("bairro", "Bairro", 24));
            _lista.Columns.Add(Estilo.CriarColuna("cidade", "Cidade", 24));
            _lista.SelectionChanged += (s, e) => AtualizarBotoes();
            _lista.CurrentCellChanged += (s, e) => AtualizarBotoes();
            _lista.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                    EditarSelecionado();
            };

            _editar = Estilo.CriarBotao("Editar", (s, e) => EditarSelecionado());
            _editar.Name = "editar";
            _inativar = Estilo.CriarBotao("Inativar", (s, e) => AlternarAtivo());
            _inativar.Name = "inativar";
            _novoOrcamento = Estilo.CriarBotao("Novo orçamento para este cliente", (s, e) => NovoOrcamento());
            _novoOrcamento.Name = "novoOrcamento";
            _orcamentosDoCliente = Estilo.CriarBotao("Orçamentos deste cliente", (s, e) => VerOrcamentos());
            _orcamentosDoCliente.Name = "orcamentosDoCliente";
            var fechar = Estilo.CriarBotao("Fechar", (s, e) => Close());
            CancelButton = fechar;

            var botoes = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(6) };
            botoes.Controls.Add(Estilo.CriarBotao("Novo", (s, e) => Novo()));
            botoes.Controls.Add(_editar);
            botoes.Controls.Add(_inativar);
            botoes.Controls.Add(_novoOrcamento);
            botoes.Controls.Add(_orcamentosDoCliente);
            botoes.Controls.Add(fechar);

            Controls.Add(_lista);
            Controls.Add(topo);
            Controls.Add(botoes);
            Estilo.OrdemTab(topo, _lista, botoes);

            Carregar();
        }

        private Cliente Selecionado => _lista.CurrentRow?.Tag as Cliente;

        /// <summary>Enter na busca abre o primeiro resultado.</summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter && _busca.Focused && _lista.Rows.Count > 0)
            {
                _lista.CurrentCell = _lista.Rows[0].Cells[0];
                EditarSelecionado();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void Carregar(long? manterSelecionado = null)
        {
            var selecionar = manterSelecionado ?? Selecionado?.Id;

            _lista.Rows.Clear();
            foreach (var cliente in _clientes.Buscar(_busca.Text, _mostrarInativos.Checked))
            {
                var linha = _lista.Rows[_lista.Rows.Add(Formatacao.Codigo(cliente.Id), cliente.Nome, Formatacao.Telefone(cliente.Telefone), cliente.Bairro, cliente.Cidade)];
                linha.Tag = cliente;

                if (!cliente.Ativo)
                    linha.DefaultCellStyle.ForeColor = Color.Gray;
            }

            var linhaSelecionada = _lista.Rows.Cast<DataGridViewRow>().FirstOrDefault(linha => ((Cliente)linha.Tag).Id == selecionar);
            if (linhaSelecionada != null)
                _lista.CurrentCell = linhaSelecionada.Cells[0];

            AtualizarBotoes();
        }

        private void AtualizarBotoes()
        {
            var cliente = Selecionado;

            _editar.Enabled = cliente != null;
            _inativar.Enabled = cliente != null;
            _inativar.Text = cliente != null && !cliente.Ativo ? "Reativar" : "Inativar";
            _novoOrcamento.Enabled = cliente != null && cliente.Ativo;
            _orcamentosDoCliente.Enabled = cliente != null;
        }

        private void Novo()
        {
            var cliente = new Cliente();

            if (AbrirEdicao(cliente))
                Carregar(cliente.Id);
        }

        private void EditarSelecionado()
        {
            var cliente = Selecionado;

            if (cliente != null && AbrirEdicao(cliente))
                Carregar(cliente.Id);
        }

        protected virtual bool AbrirEdicao(Cliente cliente)
        {
            using (var form = new FormClienteEditar(_banco, cliente))
                return form.ShowDialog(this) == DialogResult.OK;
        }

        private void NovoOrcamento()
        {
            var cliente = Selecionado;

            if (cliente != null && cliente.Ativo)
                AbrirOrcamento(cliente);
        }

        protected virtual void AbrirOrcamento(Cliente cliente)
        {
            using (var form = new FormOrcamento(_banco, null, cliente))
                form.ShowDialog(this);
        }

        private void VerOrcamentos()
        {
            var cliente = Selecionado;

            if (cliente != null)
                AbrirHistorico(FormHistorico.FiltroDoCliente(cliente));
        }

        protected virtual void AbrirHistorico(FiltroHistorico filtro)
        {
            using (var form = new FormHistorico(_banco, filtro))
                form.ShowDialog(this);
        }

        private void AlternarAtivo()
        {
            var cliente = Selecionado;
            if (cliente == null)
                return;

            _clientes.DefinirAtivo(cliente.Id, !cliente.Ativo);
            Carregar(cliente.Id);
        }
    }
}
