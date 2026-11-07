using System;
using System.Linq;
using System.Windows.Forms;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;
using PontoDoGesso.Forms;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class TelaClientesTestes : IDisposable
    {
        private readonly BancoTemporario _temporario = new BancoTemporario();
        private readonly ClienteRepositorio _clientes;

        public TelaClientesTestes()
        {
            _temporario.Banco.Preparar();
            _clientes = new ClienteRepositorio(_temporario.Banco);
        }

        public void Dispose() => _temporario.Dispose();

        private Cliente Novo(string nome, string telefone = null, bool ativo = true)
        {
            var cliente = new Cliente { Nome = nome, Telefone = telefone, Bairro = "Centro", Ativo = ativo };
            _clientes.Inserir(cliente);
            return cliente;
        }

        private static string[] Nomes(FormClientes form) =>
            Tela.Achar<DataGridView>(form, "lista").Rows.Cast<DataGridViewRow>().Select(linha => (string)linha.Cells["nome"].Value).ToArray();

        [Fact]
        public void ListaMostraCodigoNomeTelefoneBairroECidade()
        {
            Novo("João", "11987654321");

            Tela.EmSta(() =>
            {
                using (var form = new FormClientes(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    Tela.Capturar(form, "clientes");
                    var linha = Tela.Achar<DataGridView>(form, "lista").Rows[0];

                    Assert.Equal("0001", linha.Cells["codigo"].Value);
                    Assert.Equal("João", linha.Cells["nome"].Value);
                    Assert.Equal("(11) 98765-4321", linha.Cells["telefone"].Value);
                    Assert.Equal("Centro", linha.Cells["bairro"].Value);
                    Assert.Equal("Pindamonhangaba", linha.Cells["cidade"].Value);
                }
            });
        }

        [Fact]
        public void BuscaFiltraEnquantoDigita()
        {
            Novo("João");
            Novo("Maria");

            Tela.EmSta(() =>
            {
                using (var form = new FormClientes(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    Tela.Achar<TextBox>(form, "busca").Text = "joao";

                    Assert.Equal(new[] { "João" }, Nomes(form));
                }
            });
        }

        [Fact]
        public void InativarTiraDaListaEMostrarInativosPermiteReativar()
        {
            var cliente = Novo("João");

            Tela.EmSta(() =>
            {
                using (var form = new FormClientes(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    var inativar = Tela.Achar<Button>(form, "inativar");

                    inativar.PerformClick();
                    Assert.Empty(Nomes(form));
                    Assert.False(_clientes.ObterPorId(cliente.Id).Ativo);

                    Tela.Achar<CheckBox>(form, "mostrarInativos").Checked = true;
                    Assert.Equal("Reativar", inativar.Text);

                    inativar.PerformClick();
                    Assert.True(_clientes.ObterPorId(cliente.Id).Ativo);
                    Assert.Equal("Inativar", inativar.Text);
                }
            });
        }

        [Fact]
        public void EdicaoMostraOCodigoNoTitulo()
        {
            var cliente = Novo("João");

            Tela.EmSta(() =>
            {
                using (var form = new FormClienteEditar(_temporario.Banco, cliente))
                    Assert.Equal("Cliente Cód. 0001", form.Text);
            });
        }

        [Fact]
        public void NovoClienteVemComPindamonhangabaEExigeNome()
        {
            Tela.EmSta(() =>
            {
                using (var form = new EdicaoSemDialogos(_temporario.Banco, new Cliente()))
                {
                    Assert.Equal("Pindamonhangaba", Tela.Achar<TextBox>(form, "cidade").Text);

                    Assert.False(form.Salvar());
                    Assert.Equal("Informe o nome do cliente.", form.UltimoAviso);

                    Tela.Achar<TextBox>(form, "nome").Text = "  João  ";
                    Assert.True(form.Salvar());
                }
            });

            Assert.Equal("João", Assert.Single(_clientes.Buscar("")).Nome);
        }

        private class EdicaoSemDialogos : FormClienteEditar
        {
            public string UltimoAviso { get; private set; }

            public EdicaoSemDialogos(Banco banco, Cliente cliente) : base(banco, cliente)
            {
            }

            protected override void Avisar(string mensagem) => UltimoAviso = mensagem;
        }
    }
}
