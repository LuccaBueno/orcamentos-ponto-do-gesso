using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PontoDoGesso.Controles;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Forms
{
    public class FormClienteEditar : Form
    {
        private readonly ClienteRepositorio _clientes;
        private readonly Cliente _cliente;
        private readonly TextBox _nome = new TextBox { Name = "nome", Dock = DockStyle.Fill, MaxLength = 120 };
        private readonly CampoTelefone _telefone = new CampoTelefone { Name = "telefone" };
        private readonly CampoTelefone _telefone2 = new CampoTelefone { Name = "telefone2" };
        private readonly TextBox _endereco = new TextBox { Name = "endereco", Dock = DockStyle.Fill };
        private readonly TextBox _numero = new TextBox { Name = "numero", Width = 90 };
        private readonly TextBox _bairro = new TextBox { Name = "bairro", Dock = DockStyle.Fill };
        private readonly TextBox _cidade = new TextBox { Name = "cidade", Dock = DockStyle.Fill };
        private readonly TextBox _referencia = new TextBox { Name = "referencia", Dock = DockStyle.Fill };
        private readonly TextBox _observacoes = new TextBox { Name = "observacoes", Dock = DockStyle.Fill, Multiline = true, AcceptsReturn = true, ScrollBars = ScrollBars.Vertical };

        public FormClienteEditar(Banco banco, Cliente cliente)
        {
            _clientes = new ClienteRepositorio(banco);
            _cliente = cliente;

            var configuracoes = new ConfiguracaoRepositorio(banco);
            var dddPadrao = configuracoes.Obter("cliente.ddd_padrao") ?? "12";

            // Cliente novo começa com a cidade padrão das configurações
            if (cliente.Id == 0 && cliente.Cidade == new Cliente().Cidade)
                cliente.Cidade = configuracoes.Obter("cliente.cidade_padrao") ?? cliente.Cidade;

            _telefone.DddPadrao = dddPadrao;
            _telefone2.DddPadrao = dddPadrao;

            Text = cliente.Id == 0 ? "Novo cliente" : "Cliente Cód. " + Formatacao.Codigo(cliente.Id);
            Estilo.Aplicar(this, 520, 420);
            MinimumSize = new Size(460, 400);
            MaximizeBox = false;

            var campos = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(10) };
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            Adicionar(campos, "Nome *", _nome);
            Adicionar(campos, "Telefone", _telefone);
            Adicionar(campos, "Telefone 2", _telefone2);
            Adicionar(campos, "Endereço", _endereco);
            Adicionar(campos, "Número", _numero);
            Adicionar(campos, "Bairro", _bairro);
            Adicionar(campos, "Cidade", _cidade);
            Adicionar(campos, "Ponto de referência", _referencia);
            Adicionar(campos, "Observações", _observacoes);
            campos.RowStyles[campos.RowCount - 1] = new RowStyle(SizeType.Percent, 100F);
            campos.Controls.Add(new Label { Text = "Observações são internas: não saem no orçamento.", AutoSize = true, ForeColor = Color.DimGray }, 1, campos.RowCount);
            campos.RowCount++;
            campos.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var salvar = Estilo.CriarBotao("Salvar", (s, e) => SalvarEFechar());
            salvar.Name = "salvar";
            var cancelar = Estilo.CriarBotao("Cancelar", (s, e) => Close());
            cancelar.DialogResult = DialogResult.Cancel;

            var botoes = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(6) };
            botoes.Controls.Add(cancelar);
            botoes.Controls.Add(salvar);
            Estilo.OrdemTab(salvar, cancelar);

            Controls.Add(campos);
            Controls.Add(botoes);
            AcceptButton = salvar;
            CancelButton = cancelar;

            Preencher();
        }

        private static void Adicionar(TableLayoutPanel campos, string titulo, Control campo)
        {
            var linha = campos.RowCount;
            campos.RowCount++;
            campos.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            campos.Controls.Add(new Label { Text = titulo, AutoSize = true, Anchor = AnchorStyles.Left }, 0, linha);
            campos.Controls.Add(campo, 1, linha);
        }

        private void Preencher()
        {
            _nome.Text = _cliente.Nome;
            _telefone.Exibir(_cliente.Telefone);
            _telefone2.Exibir(_cliente.Telefone2);
            _endereco.Text = _cliente.Endereco;
            _numero.Text = _cliente.Numero;
            _bairro.Text = _cliente.Bairro;
            _cidade.Text = _cliente.Cidade;
            _referencia.Text = _cliente.PontoReferencia;
            _observacoes.Text = _cliente.Observacoes;
        }

        private void SalvarEFechar()
        {
            if (Salvar())
                DialogResult = DialogResult.OK;
        }

        /// <summary>Confere e grava; devolve falso se algo impediu.</summary>
        public bool Salvar()
        {
            var nome = Estilo.Limpo(_nome.Text);
            if (nome == null)
            {
                Avisar("Informe o nome do cliente.");
                _nome.Focus();
                return false;
            }

            if (!LerTelefone(_telefone, "Telefone", out var telefone) || !LerTelefone(_telefone2, "Telefone 2", out var telefone2))
                return false;

            if (!ConfirmarDuplicados(new Cliente { Id = _cliente.Id, Nome = nome, Telefone = telefone, Telefone2 = telefone2 }))
                return false;

            _cliente.Nome = nome;
            _cliente.Telefone = telefone;
            _cliente.Telefone2 = telefone2;
            _cliente.Endereco = Estilo.Limpo(_endereco.Text);
            _cliente.Numero = Estilo.Limpo(_numero.Text);
            _cliente.Bairro = Estilo.Limpo(_bairro.Text);
            _cliente.Cidade = Estilo.Limpo(_cidade.Text);
            _cliente.PontoReferencia = Estilo.Limpo(_referencia.Text);
            _cliente.Observacoes = Estilo.Limpo(_observacoes.Text);

            if (_cliente.Id == 0)
                _clientes.Inserir(_cliente);
            else
                _clientes.Atualizar(_cliente);

            return true;
        }

        private bool LerTelefone(CampoTelefone campo, string titulo, out string telefone)
        {
            if (campo.TentarLer(out telefone, out var erro))
                return true;

            Avisar(titulo + ": " + erro);
            campo.FocarErro();
            return false;
        }

        private bool ConfirmarDuplicados(Cliente dados)
        {
            var parecidos = _clientes.PossiveisDuplicados(dados);
            if (parecidos.Count == 0)
                return true;

            var lista = string.Join("\n", parecidos.Take(5).Select(cliente =>
                "Cód. " + Formatacao.Codigo(cliente.Id) + " – " + cliente.Nome +
                (cliente.Telefone == null ? "" : " – " + Formatacao.Telefone(cliente.Telefone)) +
                (cliente.Ativo ? "" : " (inativo)")));

            return Confirmar("Já existe cliente com o mesmo nome ou telefone:\n\n" + lista + "\n\nSalvar mesmo assim?");
        }

        protected virtual void Avisar(string mensagem) =>
            MessageBox.Show(this, mensagem, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);

        protected virtual bool Confirmar(string pergunta) =>
            MessageBox.Show(this, pergunta, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
    }
}
