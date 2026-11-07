using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Dapper;
using PontoDoGesso.Controles;
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
            using (var conexao = _temporario.Banco.Abrir())
                conexao.Execute("INSERT INTO sqlite_sequence (name, seq) VALUES ('clientes', 44)");

            var cliente = Novo("João");

            Tela.EmSta(() =>
            {
                using (var form = new FormClienteEditar(_temporario.Banco, cliente))
                    Assert.Equal("Cliente Cód. 0045", form.Text);
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

        [Fact]
        public void TelefoneVemComDddPadraoEGravaCompleto()
        {
            Tela.EmSta(() =>
            {
                using (var form = new EdicaoSemDialogos(_temporario.Banco, new Cliente()))
                {
                    var telefone = Tela.Achar<CampoTelefone>(form, "telefone");
                    Assert.Equal("12", telefone.Ddd);

                    Tela.Achar<TextBox>(form, "nome").Text = "João";
                    telefone.Numero = "99244-5402";
                    Assert.True(form.Salvar());
                }
            });

            Assert.Equal("12992445402", Assert.Single(_clientes.Buscar("")).Telefone);
        }

        [Fact]
        public void ClienteComDdd11SalvaEExibeFormatado()
        {
            Tela.EmSta(() =>
            {
                using (var form = new EdicaoSemDialogos(_temporario.Banco, new Cliente()))
                {
                    Tela.Achar<TextBox>(form, "nome").Text = "João";
                    var telefone = Tela.Achar<CampoTelefone>(form, "telefone");
                    telefone.Ddd = "11";
                    telefone.Numero = "987654321";
                    Assert.True(form.Salvar());
                }

                using (var lista = new FormClientes(_temporario.Banco))
                {
                    Tela.Exibir(lista);
                    Assert.Equal("(11) 98765-4321", Tela.Achar<DataGridView>(lista, "lista").Rows[0].Cells["telefone"].Value);
                }
            });

            Assert.Equal("11987654321", Assert.Single(_clientes.Buscar("")).Telefone);
        }

        [Fact]
        public void Ddd00EhRecusadoAoSalvar()
        {
            Tela.EmSta(() =>
            {
                using (var form = new EdicaoSemDialogos(_temporario.Banco, new Cliente()))
                {
                    Tela.Achar<TextBox>(form, "nome").Text = "João";
                    var telefone = Tela.Achar<CampoTelefone>(form, "telefone2");
                    telefone.Ddd = "00";
                    telefone.Numero = "987654321";

                    Assert.False(form.Salvar());
                    Assert.StartsWith("Telefone 2: O DDD \"00\" não existe", form.UltimoAviso);
                }
            });

            Assert.Empty(_clientes.Buscar(""));
        }

        [Fact]
        public void TelefoneGravadoApareceSeparadoNaEdicao()
        {
            var cliente = Novo("João", "11987654321");

            Tela.EmSta(() =>
            {
                using (var form = new FormClienteEditar(_temporario.Banco, cliente))
                {
                    Tela.Exibir(form);
                    Tela.Capturar(form, "cliente-editar");
                    var telefone = Tela.Achar<CampoTelefone>(form, "telefone");
                    Assert.Equal("11", telefone.Ddd);
                    Assert.Equal("98765-4321", telefone.Numero);
                }
            });
        }

        [Fact]
        public void AvisaDuplicadoPeloTelefoneENaoSalvaSeARespostaForNao()
        {
            Novo("Maria", "12992445402");

            Tela.EmSta(() =>
            {
                using (var form = new EdicaoSemDialogos(_temporario.Banco, new Cliente()))
                {
                    Tela.Achar<TextBox>(form, "nome").Text = "Maria Souza";
                    Tela.Achar<CampoTelefone>(form, "telefone2").Numero = "99244-5402";

                    Assert.False(form.Salvar());
                    Assert.Contains("Cód. 0001 – Maria – (12) 99244-5402", form.UltimaPergunta);

                    form.Resposta = true;
                    Assert.True(form.Salvar());
                }
            });

            Assert.Equal(2, _clientes.Buscar("").Count);
        }

        [Fact]
        public void AvisaDuplicadoPeloNomeSemAcento()
        {
            Novo("João Silva");

            Tela.EmSta(() =>
            {
                using (var form = new EdicaoSemDialogos(_temporario.Banco, new Cliente()))
                {
                    Tela.Achar<TextBox>(form, "nome").Text = "JOAO SILVA";

                    Assert.False(form.Salvar());
                    Assert.Contains("João Silva", form.UltimaPergunta);
                }
            });
        }

        [Fact]
        public void EditarOProprioClienteNaoContaComoDuplicado()
        {
            var cliente = Novo("João", "12992445402");

            Tela.EmSta(() =>
            {
                using (var form = new EdicaoSemDialogos(_temporario.Banco, cliente))
                {
                    Assert.True(form.Salvar());
                    Assert.Null(form.UltimaPergunta);
                }
            });
        }

        [Fact]
        public void EnterNaBuscaAbreOPrimeiroResultado()
        {
            Novo("Ana");
            var joao = Novo("João");

            Tela.EmSta(() =>
            {
                using (var form = new ListaSemDialogos(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    var busca = Tela.Achar<TextBox>(form, "busca");
                    busca.Text = "jo";
                    busca.Focus();

                    Assert.True(form.Teclar(Keys.Enter));
                    Assert.Equal(joao.Id, Assert.Single(form.Abertos).Id);
                }
            });
        }

        [Fact]
        public void DuploCliqueAbreEdicaoEEscFecha()
        {
            var joao = Novo("João");

            Tela.EmSta(() =>
            {
                using (var form = new ListaSemDialogos(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    var lista = Tela.Achar<DataGridView>(form, "lista");
                    typeof(DataGridView).GetMethod("OnCellDoubleClick", BindingFlags.NonPublic | BindingFlags.Instance)
                        .Invoke(lista, new object[] { new DataGridViewCellEventArgs(1, 0) });

                    Assert.Equal(joao.Id, Assert.Single(form.Abertos).Id);

                    ((Button)form.CancelButton).PerformClick();
                    Assert.False(form.Visible);
                }
            });
        }

        [Fact]
        public void NovoOrcamentoParaEsteClienteAbreOOrcamentoComOCliente()
        {
            var joao = Novo("João");
            Novo("Zeca", ativo: false);

            Tela.EmSta(() =>
            {
                using (var form = new ListaSemDialogos(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    var botao = Tela.Achar<Button>(form, "novoOrcamento");
                    Assert.True(botao.Enabled);

                    botao.PerformClick();
                    Assert.Equal(joao.Id, Assert.Single(form.Orcamentos).Id);

                    // Cliente inativo não recebe orçamento novo
                    Tela.Achar<CheckBox>(form, "mostrarInativos").Checked = true;
                    var lista = Tela.Achar<DataGridView>(form, "lista");
                    lista.CurrentCell = lista.Rows.Cast<DataGridViewRow>().Single(linha => ((Cliente)linha.Tag).Nome == "Zeca").Cells[0];
                    Assert.False(botao.Enabled);
                }
            });
        }

        private class ListaSemDialogos : FormClientes
        {
            public List<Cliente> Abertos { get; } = new List<Cliente>();
            public List<Cliente> Orcamentos { get; } = new List<Cliente>();

            protected override void AbrirOrcamento(Cliente cliente) => Orcamentos.Add(cliente);

            public ListaSemDialogos(Banco banco) : base(banco)
            {
            }

            protected override bool AbrirEdicao(Cliente cliente)
            {
                Abertos.Add(cliente);
                return false;
            }

            public bool Teclar(Keys tecla)
            {
                var mensagem = new Message();
                return ProcessCmdKey(ref mensagem, tecla);
            }
        }

        private class EdicaoSemDialogos : FormClienteEditar
        {
            public string UltimoAviso { get; private set; }

            public EdicaoSemDialogos(Banco banco, Cliente cliente) : base(banco, cliente)
            {
            }

            protected override void Avisar(string mensagem) => UltimoAviso = mensagem;

            public bool Resposta { get; set; }
            public string UltimaPergunta { get; private set; }

            protected override bool Confirmar(string pergunta)
            {
                UltimaPergunta = pergunta;
                return Resposta;
            }
        }
    }
}
