using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Dapper;
using PontoDoGesso.Controles;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;
using PontoDoGesso.Forms;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class TelaOrcamentoTestes : IDisposable
    {
        private readonly BancoTemporario _temporario = new BancoTemporario();
        private readonly ClienteRepositorio _clientes;
        private readonly ProdutoRepositorio _produtos;
        private readonly OrcamentoRepositorio _orcamentos;

        public TelaOrcamentoTestes()
        {
            _temporario.Banco.Preparar();
            _clientes = new ClienteRepositorio(_temporario.Banco);
            _produtos = new ProdutoRepositorio(_temporario.Banco);
            _orcamentos = new OrcamentoRepositorio(_temporario.Banco);
        }

        public void Dispose() => _temporario.Dispose();

        private Cliente NovoCliente(string nome = "João Silva", string observacoes = null)
        {
            var cliente = new Cliente
            {
                Nome = nome,
                Telefone = "12992445402",
                Endereco = "Rua A",
                Numero = "10",
                Bairro = "Centro",
                PontoReferencia = "Perto da praça",
                Observacoes = observacoes
            };
            _clientes.Inserir(cliente);
            return cliente;
        }

        private Produto NovoProduto(string nome = "Placa de gesso 60x60", long preco = 1250, string unidade = "m²")
        {
            var produto = new Produto { Nome = nome, PrecoCentavos = preco, Unidade = unidade };
            _produtos.Inserir(produto);
            return produto;
        }

        private static GradeItens Grade(Form form) => Tela.Achar<GradeItens>(form, "itens");

        private static string Texto(Form form, string nome) => Tela.Achar<Control>(form, nome).Text;

        private static int AdicionarItem(Form form, string quantidade, string unidade, string descricao, string unitario)
        {
            var grade = Grade(form);
            var indice = grade.Rows.Add();
            var linha = grade.Rows[indice];
            linha.Cells["quantidade"].Value = quantidade;
            linha.Cells["unidade"].Value = unidade;
            linha.Cells["descricao"].Value = descricao;
            linha.Cells["unitario"].Value = unitario;
            return indice;
        }

        private static void Avulso(Form form, string nome, string numeroTelefone = null)
        {
            Tela.Achar<RadioButton>(form, "avulso").Checked = true;
            Tela.Achar<TextBox>(form, "nome").Text = nome;
            if (numeroTelefone != null)
                Tela.Achar<CampoTelefone>(form, "telefone").Numero = numeroTelefone;
        }

        private static void Rodar(Banco banco, Action<OrcamentoSemDialogos> teste, Cliente cliente = null, Orcamento orcamento = null)
        {
            Tela.EmSta(() =>
            {
                using (var form = new OrcamentoSemDialogos(banco, orcamento, cliente))
                {
                    Tela.Exibir(form);
                    teste(form);
                    form.Descartar();
                }
            });
        }

        [Fact]
        public void OrcamentoComClienteCadastradoGuardaCopiaDosDados()
        {
            var cliente = NovoCliente(observacoes: "Pede tudo discriminado");

            Rodar(_temporario.Banco, form =>
            {
                Tela.Capturar(form, "orcamento-vazio");
                Assert.Equal("Orçamento Nº — (novo)", form.Text);
                Assert.Equal("João Silva", Texto(form, "nome"));
                Assert.Equal("Rua A", Texto(form, "endereco"));
                Assert.Equal("Atenção: Pede tudo discriminado", Texto(form, "avisoCliente"));
                Assert.True(Tela.Achar<Label>(form, "avisoCliente").Visible);

                AdicionarItem(form, "2", "m²", "Placa", "12,50");
                Tela.Achar<TextBox>(form, "endereco").Text = "Rua da obra";

                Assert.True(form.Salvar(), form.UltimoAviso);
                Assert.Equal("Orçamento Nº 0001", form.Text);
                Assert.False(form.Alterado);
            }, cliente);

            var salvo = _orcamentos.ObterPorId(1);
            Assert.Equal(cliente.Id, salvo.ClienteId);
            Assert.Equal("João Silva", salvo.CliNome);
            Assert.Equal("12992445402", salvo.CliTelefone);
            Assert.Equal("Rua da obra", salvo.EntEndereco);
            Assert.Equal("Centro", salvo.EntBairro);
            Assert.Equal("Pindamonhangaba", salvo.EntCidade);
            Assert.Equal(TipoEntrega.Entrega, salvo.TipoEntrega);
            Assert.Equal(2500, salvo.TotalCentavos);

            // Só o orçamento mudou: o cadastro continua com o endereço antigo
            Assert.Equal("Rua A", _clientes.ObterPorId(cliente.Id).Endereco);
        }

        [Fact]
        public void BuscaDeClienteMostraCodigoNomeETelefoneEPreencheAoEscolher()
        {
            var cliente = NovoCliente();

            Rodar(_temporario.Banco, form =>
            {
                var busca = Tela.Achar<TextBox>(form, "buscaCliente");
                busca.Text = "joao";

                var sugestoes = form.Controls.OfType<ListaSugestoes>().First(lista => lista.Visible);
                Assert.Equal("0001 – João Silva – (12) 99244-5402", sugestoes.Items[0].ToString());

                Assert.True(sugestoes.Teclar(Keys.Enter));
                Assert.Equal("0001 – João Silva", busca.Text);
                Assert.Equal("João Silva", Texto(form, "nome"));
                Assert.Equal("99244-5402", Tela.Achar<CampoTelefone>(form, "telefone").Numero);
                Assert.Equal("Perto da praça", Texto(form, "referencia"));
                Assert.False(sugestoes.Visible);
            });
        }

        [Fact]
        public void CadastradoSemEscolherClienteNaoSalva()
        {
            Rodar(_temporario.Banco, form =>
            {
                Tela.Achar<TextBox>(form, "nome").Text = "Fulano";
                AdicionarItem(form, "1", "un", "Placa", "10");

                Assert.False(form.Salvar());
                Assert.Equal("Busque e escolha o cliente cadastrado, ou marque Avulso.", form.UltimoAviso);
            });
        }

        [Fact]
        public void NovoClientePelaTelaDoOrcamentoJaFicaEscolhido()
        {
            Rodar(_temporario.Banco, form =>
            {
                Tela.Achar<TextBox>(form, "buscaCliente").Text = "Maria Souza";
                form.ClienteParaCadastrar = cliente =>
                {
                    Assert.Equal("Maria Souza", cliente.Nome);
                    cliente.Telefone = "11987654321";
                    _clientes.Inserir(cliente);
                };

                Tela.Achar<Button>(form, "novoCliente").PerformClick();

                Assert.Equal("Maria Souza", Texto(form, "nome"));
                Assert.Equal("11", Tela.Achar<CampoTelefone>(form, "telefone").Ddd);
                AdicionarItem(form, "1", "un", "Placa", "10");
                Assert.True(form.Salvar(), form.UltimoAviso);
            });

            Assert.Equal(1, _orcamentos.ObterPorId(1).ClienteId);
        }

        [Fact]
        public void AvulsoNaoCriaCadastroQuandoARespostaENaoEPerguntaUmaVezSo()
        {
            Rodar(_temporario.Banco, form =>
            {
                Avulso(form, "Cliente de balcão", "98765-4321");
                Tela.Achar<CampoTelefone>(form, "telefone").Ddd = "11";
                AdicionarItem(form, "1", "un", "Placa", "10");

                Assert.True(form.Salvar(), form.UltimoAviso);
                Assert.Equal("Deseja cadastrar este cliente?\n\nCliente de balcão – (11) 98765-4321", Assert.Single(form.Perguntas));

                Tela.Achar<TextBox>(form, "observacoes").Text = "mudou";
                Assert.True(form.Salvar());
                Assert.Single(form.Perguntas);
            });

            var salvo = _orcamentos.ObterPorId(1);
            Assert.Null(salvo.ClienteId);
            Assert.Equal("11987654321", salvo.CliTelefone);
            Assert.Empty(_clientes.Buscar("", incluirInativos: true));
        }

        [Fact]
        public void AvulsoViraCadastroQuandoARespostaESim()
        {
            Rodar(_temporario.Banco, form =>
            {
                form.Resposta = true;
                Avulso(form, "Pedro");
                Tela.Achar<TextBox>(form, "bairro").Text = "Vila";
                AdicionarItem(form, "1", "un", "Placa", "10");

                Assert.True(form.Salvar(), form.UltimoAviso);
                Assert.True(Tela.Achar<RadioButton>(form, "cadastrado").Checked);
            });

            var cliente = Assert.Single(_clientes.Buscar("pedro"));
            Assert.Equal("Vila", cliente.Bairro);
            Assert.Equal(cliente.Id, _orcamentos.ObterPorId(1).ClienteId);
        }

        [Fact]
        public void ConfiguracaoDesligaAPerguntaDoAvulso()
        {
            new ConfiguracaoRepositorio(_temporario.Banco).Definir("orcamento.perguntar_cadastro_avulso", "0");

            Rodar(_temporario.Banco, form =>
            {
                Avulso(form, "Pedro");
                AdicionarItem(form, "1", "un", "Placa", "10");

                Assert.True(form.Salvar(), form.UltimoAviso);
                Assert.Empty(form.Perguntas);
            });
        }

        [Fact]
        public void RetiradaEscondeELimpaAEntrega()
        {
            var cliente = NovoCliente();

            Rodar(_temporario.Banco, form =>
            {
                Tela.Achar<RadioButton>(form, "retirada").Checked = true;
                Assert.False(Tela.Achar<Control>(form, "painelEntrega").Visible);
                Assert.Equal("", Texto(form, "endereco"));

                AdicionarItem(form, "1", "un", "Placa", "10");
                Assert.True(form.Salvar(), form.UltimoAviso);

                Tela.Achar<RadioButton>(form, "entrega").Checked = true;
                Assert.True(Tela.Achar<Control>(form, "painelEntrega").Visible);
                Assert.Equal("Rua A", Texto(form, "endereco"));
            }, cliente);

            var salvo = _orcamentos.ObterPorId(1);
            Assert.Equal(TipoEntrega.Retirada, salvo.TipoEntrega);
            Assert.Null(salvo.EntEndereco);
            Assert.Null(salvo.EntCidade);
        }

        [Fact]
        public void QuantidadeFracionadaEFrete()
        {
            Rodar(_temporario.Banco, form =>
            {
                Avulso(form, "Ana");
                AdicionarItem(form, "2,5", "m²", "Placa", "12,50");
                AdicionarItem(form, "3", "un", "Cantoneira", "4,9");
                Tela.Achar<TextBox>(form, "frete").Text = "50";

                Assert.Equal("31,25", Grade(form).Rows[0].Cells["total"].Value);
                Assert.Equal("14,70", Grade(form).Rows[1].Cells["total"].Value);
                Assert.Equal("R$ 45,95", Texto(form, "subtotal"));
                Assert.Equal("R$ 50,00", Texto(form, "freteExibido"));
                Assert.Equal("R$ 95,95", Texto(form, "total"));

                Assert.True(form.Salvar(), form.UltimoAviso);
                Assert.Equal("50,00", Texto(form, "frete"));
            });

            var salvo = _orcamentos.ObterPorId(1);
            Assert.Equal(2.5m, salvo.Itens[0].Quantidade);
            Assert.Equal(5000, salvo.FreteCentavos);
            Assert.Equal(9595, salvo.TotalCentavos);
        }

        [Fact]
        public void SemFreteQuandoVazioOuZero()
        {
            Rodar(_temporario.Banco, form =>
            {
                Avulso(form, "Ana");
                AdicionarItem(form, "1", "un", "Placa", "10");
                Tela.Achar<TextBox>(form, "frete").Text = "0";

                Assert.True(form.Salvar(), form.UltimoAviso);
            });

            Assert.Equal(0, _orcamentos.ObterPorId(1).FreteCentavos);
        }

        [Fact]
        public void DescontoEmReais()
        {
            Rodar(_temporario.Banco, form =>
            {
                Avulso(form, "Ana");
                AdicionarItem(form, "4", "un", "Placa", "25");
                Tela.Achar<TextBox>(form, "frete").Text = "10";
                Assert.False(Tela.Achar<TextBox>(form, "descontoValor").Enabled);

                Tela.Achar<CheckBox>(form, "desconto").Checked = true;
                Tela.Achar<TextBox>(form, "descontoValor").Text = "15";

                Assert.Equal("− R$ 15,00", Texto(form, "descontoCalculado"));
                Assert.Equal("R$ 95,00", Texto(form, "total"));
                Assert.True(form.Salvar(), form.UltimoAviso);
            });

            var salvo = _orcamentos.ObterPorId(1);
            Assert.Equal(TipoDesconto.Valor, salvo.DescontoTipo);
            Assert.Equal(1500, salvo.DescontoCentavos);
            Assert.Equal(9500, salvo.TotalCentavos);
        }

        [Fact]
        public void DescontoEmPercentualSoSobreOsProdutos()
        {
            Rodar(_temporario.Banco, form =>
            {
                Avulso(form, "Ana");
                AdicionarItem(form, "1", "un", "Placa", "33,33");
                Tela.Achar<TextBox>(form, "frete").Text = "10";
                Tela.Achar<CheckBox>(form, "desconto").Checked = true;
                Tela.Achar<RadioButton>(form, "descontoPercentual").Checked = true;
                Tela.Achar<TextBox>(form, "descontoValor").Text = "5,5";

                Assert.Equal("− R$ 1,83", Texto(form, "descontoCalculado"));
                Assert.Equal("R$ 41,50", Texto(form, "total"));
                Assert.True(form.Salvar(), form.UltimoAviso);
            });

            var salvo = _orcamentos.ObterPorId(1);
            Assert.Equal(TipoDesconto.Percentual, salvo.DescontoTipo);
            Assert.Equal(5.5m, salvo.DescontoValor);
            Assert.Equal(183, salvo.DescontoCentavos);
        }

        [Fact]
        public void DescontoMaiorQueOSubtotalEhBloqueado()
        {
            Rodar(_temporario.Banco, form =>
            {
                Avulso(form, "Ana");
                AdicionarItem(form, "1", "un", "Placa", "10");
                Tela.Achar<CheckBox>(form, "desconto").Checked = true;
                Tela.Achar<TextBox>(form, "descontoValor").Text = "10,01";

                Assert.Equal("O desconto não pode ser maior que o subtotal.", Texto(form, "erroTotais"));
                Assert.False(form.Salvar());
                Assert.Equal("O desconto não pode ser maior que o subtotal.", form.UltimoAviso);
            });

            Assert.Null(_orcamentos.ObterPorId(1));
        }

        [Fact]
        public void SemDescontoESemValidadePorPadrao()
        {
            Rodar(_temporario.Banco, form =>
            {
                Assert.False(Tela.Achar<CheckBox>(form, "desconto").Checked);
                Assert.False(Tela.Achar<CheckBox>(form, "validade").Checked);
                Avulso(form, "Ana");
                AdicionarItem(form, "1", "un", "Placa", "10");
                Assert.True(form.Salvar(), form.UltimoAviso);
            });

            var salvo = _orcamentos.ObterPorId(1);
            Assert.Null(salvo.DescontoTipo);
            Assert.Null(salvo.ValidadeDias);
        }

        [Fact]
        public void ValidadeSugereOsDiasDaConfiguracaoEMostraADataFinal()
        {
            new ConfiguracaoRepositorio(_temporario.Banco).Definir("orcamento.validade_sugerida_dias", "10");

            Rodar(_temporario.Banco, form =>
            {
                Avulso(form, "Ana");
                AdicionarItem(form, "1", "un", "Placa", "10");
                Tela.Achar<DateTimePicker>(form, "data").Value = new DateTime(2026, 11, 6);
                Tela.Achar<CheckBox>(form, "validade").Checked = true;

                Assert.Equal(10, Tela.Achar<NumericUpDown>(form, "validadeDias").Value);
                Assert.Equal("(até 16/11/2026)", Texto(form, "validadeAte"));

                Tela.Achar<NumericUpDown>(form, "validadeDias").Value = 7;
                Assert.Equal("(até 13/11/2026)", Texto(form, "validadeAte"));
                Assert.True(form.Salvar(), form.UltimoAviso);
            });

            var salvo = _orcamentos.ObterPorId(1);
            Assert.Equal(7, salvo.ValidadeDias);
            Assert.Equal(new DateTime(2026, 11, 6), salvo.Data);
        }

        [Fact]
        public void ProdutoDoCatalogoPreencheUnidadePrecoECodigo()
        {
            using (var conexao = _temporario.Banco.Abrir())
                conexao.Execute("INSERT INTO sqlite_sequence (name, seq) VALUES ('produtos', 11)");
            var produto = NovoProduto();

            Rodar(_temporario.Banco, form =>
            {
                var grade = Grade(form);
                var indice = grade.Rows.Add();
                form.EscolherProduto(indice, produto);

                var linha = grade.Rows[indice];
                Assert.Equal("0012", linha.Cells["codigo"].Value);
                Assert.Equal("1", linha.Cells["quantidade"].Value);
                Assert.Equal("m²", linha.Cells["unidade"].Value);
                Assert.Equal("Placa de gesso 60x60", linha.Cells["descricao"].Value);
                Assert.Equal("12,50", linha.Cells["unitario"].Value);
                Assert.Equal("12,50", linha.Cells["total"].Value);
                Assert.Equal(indice + 1, grade.CurrentCell.RowIndex);
                Assert.Equal("quantidade", grade.Columns[grade.CurrentCell.ColumnIndex].Name);

                Avulso(form, "Ana");
                Assert.True(form.Salvar(), form.UltimoAviso);
            });

            Assert.Equal(produto.Id, _orcamentos.ObterPorId(1).Itens[0].ProdutoId);
        }

        [Fact]
        public void AutocompleteDaDescricaoAchaPeloCodigoEPeloNome()
        {
            using (var conexao = _temporario.Banco.Abrir())
                conexao.Execute("INSERT INTO sqlite_sequence (name, seq) VALUES ('produtos', 11)");
            NovoProduto();
            NovoProduto("Massa corrida 25 kg", 4590, "saco");

            Rodar(_temporario.Banco, form =>
            {
                var grade = Grade(form);
                grade.CurrentCell = grade.Rows[0].Cells["descricao"];
                Assert.True(grade.BeginEdit(false));
                var campo = (TextBox)grade.EditingControl;
                var sugestoes = form.Controls.OfType<ListaSugestoes>().Single(lista => lista.Campo == campo);

                campo.Text = "12";
                Assert.True(sugestoes.Visible);
                Assert.Equal("0012 – Placa de gesso 60x60 – R$ 12,50 / m²", sugestoes.Items[0].ToString());

                campo.Text = "corrida";
                Assert.Equal("0013 – Massa corrida 25 kg – R$ 45,90 / saco", Assert.Single(sugestoes.Items.Cast<object>()).ToString());

                Assert.True(sugestoes.Teclar(Keys.Enter));
                Tela.Capturar(form, "orcamento-item");

                var linha = grade.Rows[0];
                Assert.Equal("0013", linha.Cells["codigo"].Value);
                Assert.Equal("Massa corrida 25 kg", linha.Cells["descricao"].Value);
                Assert.Equal("45,90", linha.Cells["unitario"].Value);
            });
        }

        [Fact]
        public void AlterarPrecoDoItemNaoMudaOCatalogo()
        {
            var produto = NovoProduto();

            Rodar(_temporario.Banco, form =>
            {
                var indice = Grade(form).Rows.Add();
                form.EscolherProduto(indice, produto);
                Grade(form).Rows[indice].Cells["unitario"].Value = "11,00";
                Avulso(form, "Ana");

                Assert.True(form.Salvar(), form.UltimoAviso);
            });

            Assert.Equal(1100, _orcamentos.ObterPorId(1).Itens[0].UnitarioCentavos);
            Assert.Equal(1250, _produtos.ObterPorId(produto.Id).PrecoCentavos);
        }

        [Fact]
        public void TrocarADescricaoAMaoTiraOProdutoDoItem()
        {
            var produto = NovoProduto();

            Rodar(_temporario.Banco, form =>
            {
                var grade = Grade(form);
                var indice = grade.Rows.Add();
                form.EscolherProduto(indice, produto);

                grade.CurrentCell = grade.Rows[indice].Cells["descricao"];
                grade.BeginEdit(true);
                grade.EditingControl.Text = "Placa especial";
                grade.EndEdit();

                Assert.Null(grade.Rows[indice].Cells["codigo"].Value);
                Avulso(form, "Ana");
                Assert.True(form.Salvar(), form.UltimoAviso);
            });

            Assert.Null(_orcamentos.ObterPorId(1).Itens[0].ProdutoId);
        }

        [Fact]
        public void EnterAvancaPelasColunasEDescePraProximaLinha()
        {
            Rodar(_temporario.Banco, form =>
            {
                var grade = Grade(form);
                grade.CurrentCell = grade.Rows[0].Cells["quantidade"];

                var visitadas = new List<string>();
                for (var i = 0; i < 4; i++)
                {
                    grade.Avancar();
                    visitadas.Add(grade.Columns[grade.CurrentCell.ColumnIndex].Name + grade.CurrentCell.RowIndex);
                }

                // Linha 0 ainda é a linha nova (vazia): não desce
                Assert.Equal(new[] { "unidade0", "descricao0", "unitario0", "unitario0" }, visitadas);

                AdicionarItem(form, "1", "un", "Placa", "10");
                grade.CurrentCell = grade.Rows[0].Cells["unitario"];
                grade.Avancar();
                Assert.Equal(1, grade.CurrentCell.RowIndex);
                Assert.Equal("quantidade", grade.Columns[grade.CurrentCell.ColumnIndex].Name);
            });
        }

        [Fact]
        public void LinhasNovasDeleteECtrlSetasReordenam()
        {
            Rodar(_temporario.Banco, form =>
            {
                var grade = Grade(form);
                AdicionarItem(form, "1", "un", "A", "1");
                AdicionarItem(form, "1", "un", "B", "2");
                AdicionarItem(form, "1", "un", "C", "3");
                Assert.True(grade.Rows[grade.Rows.Count - 1].IsNewRow);

                grade.CurrentCell = grade.Rows[2].Cells["descricao"];
                grade.MoverLinhaAtual(-1);
                grade.MoverLinhaAtual(-1);
                grade.MoverLinhaAtual(-1);
                Assert.Equal(new[] { "C", "A", "B" }, Descricoes(grade));

                grade.MoverLinhaAtual(1);
                grade.MoverLinhaAtual(1);
                grade.MoverLinhaAtual(1);
                Assert.Equal(new[] { "A", "B", "C" }, Descricoes(grade));

                grade.CurrentCell = grade.Rows[1].Cells["descricao"];
                grade.RemoverLinhaAtual();
                Assert.Equal(new[] { "A", "C" }, Descricoes(grade));
                Assert.Equal("R$ 4,00", Texto(form, "total"));

                Avulso(form, "Ana");
                Assert.True(form.Salvar(), form.UltimoAviso);
            });

            Assert.Equal(new[] { "A", "C" }, _orcamentos.ObterPorId(1).Itens.Select(item => item.Descricao));
        }

        private static string[] Descricoes(DataGridView grade) =>
            grade.Rows.Cast<DataGridViewRow>().Where(linha => !linha.IsNewRow).Select(linha => (string)linha.Cells["descricao"].Value).ToArray();

        [Theory]
        [InlineData("", "1", "10", "Informe o nome do cliente.")]
        [InlineData("Ana", null, null, "Inclua ao menos um item.")]
        [InlineData("Ana", "0", "10", "A quantidade de \"Placa\" precisa ser um número maior que zero (ex.: 2 ou 2,5).")]
        [InlineData("Ana", "x", "10", "A quantidade de \"Placa\" precisa ser um número maior que zero (ex.: 2 ou 2,5).")]
        [InlineData("Ana", "1", "", "Informe o valor unitário de \"Placa\" (ex.: 12,50).")]
        public void SalvarValidaNomeItensEQuantidades(string nome, string quantidade, string unitario, string aviso)
        {
            Rodar(_temporario.Banco, form =>
            {
                Avulso(form, nome);
                if (quantidade != null)
                    AdicionarItem(form, quantidade, "un", "Placa", unitario);

                Assert.False(form.Salvar());
                Assert.Equal(aviso, form.UltimoAviso);
            });

            Assert.Null(_orcamentos.ObterPorId(1));
        }

        [Fact]
        public void CtrlSSalva()
        {
            Rodar(_temporario.Banco, form =>
            {
                Avulso(form, "Ana");
                AdicionarItem(form, "1", "un", "Placa", "10");

                Assert.True(form.Teclar(Keys.Control | Keys.S));
                Assert.Equal("Orçamento Nº 0001", form.Text);
            });
        }

        [Fact]
        public void LinhaSemDescricaoEhIgnorada()
        {
            Rodar(_temporario.Banco, form =>
            {
                Avulso(form, "Ana");
                AdicionarItem(form, "1", "un", "Placa", "10");
                AdicionarItem(form, "5", "un", "", "");
                Assert.True(form.Salvar(), form.UltimoAviso);
            });

            Assert.Single(_orcamentos.ObterPorId(1).Itens);
        }

        [Fact]
        public void ObservacoesTemLimiteEContador()
        {
            Rodar(_temporario.Banco, form =>
            {
                var observacoes = Tela.Achar<TextBox>(form, "observacoes");
                Assert.Equal(300, observacoes.MaxLength);
                Assert.Equal("0/300", Texto(form, "contador"));

                observacoes.Text = "Entregar pela manhã";
                Assert.Equal("19/300", Texto(form, "contador"));
            });
        }

        [Fact]
        public void AvisaAoFecharComAlteracoesNaoSalvas()
        {
            Tela.EmSta(() =>
            {
                using (var form = new OrcamentoSemDialogos(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    Assert.False(form.Alterado);

                    Tela.Achar<TextBox>(form, "observacoes").Text = "x";
                    Assert.True(form.Alterado);

                    form.RespostaSalvar = DialogResult.Cancel;
                    form.Close();
                    Assert.True(form.Visible);
                    Assert.Single(form.Perguntas);

                    form.RespostaSalvar = DialogResult.No;
                    form.Close();
                    Assert.False(form.Visible);
                }
            });

            Assert.Null(_orcamentos.ObterPorId(1));
        }

        [Fact]
        public void FecharSemAlteracoesNaoPergunta()
        {
            var cliente = NovoCliente();

            Tela.EmSta(() =>
            {
                using (var form = new OrcamentoSemDialogos(_temporario.Banco, null, cliente))
                {
                    Tela.Exibir(form);
                    form.Close();
                    Assert.Empty(form.Perguntas);
                }
            });
        }

        [Fact]
        public void DuplicarCriaOrcamentoNovoComOsMesmosDadosEDataDeHoje()
        {
            var cliente = NovoCliente();

            Rodar(_temporario.Banco, form =>
            {
                Tela.Achar<DateTimePicker>(form, "data").Value = new DateTime(2026, 1, 5);
                AdicionarItem(form, "2", "un", "Placa", "10");
                Assert.True(form.Salvar(), form.UltimoAviso);

                Assert.True(Tela.Achar<Button>(form, "duplicar").Enabled);
                Assert.True(form.Duplicar());
                Assert.Equal("Orçamento Nº — (novo)", form.Text);
                Assert.Equal(DateTime.Today, Tela.Achar<DateTimePicker>(form, "data").Value.Date);
                Assert.True(form.Alterado);

                Assert.True(form.Salvar(), form.UltimoAviso);
                Assert.Equal("Orçamento Nº 0002", form.Text);
            }, cliente);

            var original = _orcamentos.ObterPorId(1);
            var copia = _orcamentos.ObterPorId(2);
            Assert.Equal(new DateTime(2026, 1, 5), original.Data);
            Assert.Equal(DateTime.Today, copia.Data);
            Assert.Equal(original.ClienteId, copia.ClienteId);
            Assert.Equal(original.TotalCentavos, copia.TotalCentavos);
            Assert.Equal("Placa", Assert.Single(copia.Itens).Descricao);
        }

        [Fact]
        public void AbrirOrcamentoSalvoMostraOsDados()
        {
            var cliente = NovoCliente();
            var orcamento = new Orcamento
            {
                Data = new DateTime(2026, 11, 6),
                ValidadeDias = 5,
                Observacoes = "Obs",
                DescontoTipo = TipoDesconto.Percentual,
                DescontoValor = 10,
                FreteCentavos = 3000
            };
            orcamento.CopiarCliente(cliente);
            orcamento.Itens.Add(new ItemOrcamento { Quantidade = 2.5m, Unidade = "m²", Descricao = "Placa", UnitarioCentavos = 1000 });
            _orcamentos.Salvar(orcamento);

            Rodar(_temporario.Banco, form =>
            {
                Assert.Equal("Orçamento Nº 0001", form.Text);
                Assert.Equal("0001 – João Silva", Texto(form, "buscaCliente"));
                Assert.Equal("2,5", Grade(form).Rows[0].Cells["quantidade"].Value);
                Assert.Equal("10", Texto(form, "descontoValor"));
                Assert.True(Tela.Achar<RadioButton>(form, "descontoPercentual").Checked);
                Assert.Equal("30,00", Texto(form, "frete"));
                Assert.Equal("R$ 52,50", Texto(form, "total"));
                Assert.Equal("(até 11/11/2026)", Texto(form, "validadeAte"));
                Assert.False(form.Alterado);
            }, orcamento: _orcamentos.ObterPorId(orcamento.Id));
        }

        private string PastaExportacao()
        {
            var pasta = System.IO.Path.Combine(_temporario.Pasta, "exportados");
            new ConfiguracaoRepositorio(_temporario.Banco).Definir("exportacao.pasta", pasta);
            return pasta;
        }

        [Fact]
        public void ExportarOrcamentoNovoSalvaAntesEAvisa()
        {
            var pasta = PastaExportacao();

            Rodar(_temporario.Banco, form =>
            {
                Avulso(form, "José Fictício");
                AdicionarItem(form, "2", "m²", "Placa", "12,50");

                Tela.Achar<Button>(form, "png").PerformClick();

                Assert.Equal("Orçamento Nº 0001", form.Text);
                Assert.False(form.Alterado);
                var arquivo = Assert.Single(form.Acoes.Salvos);
                var hoje = DateTime.Today;
                Assert.Equal(System.IO.Path.Combine(pasta, hoje.ToString("yyyy"), hoje.ToString("MM"), "Orc-0001_Jose-Ficticio_" + hoje.ToString("dd-MM-yyyy") + ".png"), arquivo);
                Assert.True(System.IO.File.Exists(arquivo));
                Assert.StartsWith("O orçamento foi salvo (Nº 0001) antes de exportar.", form.Acoes.Mensagens[0]);

                // Já salvo e sem alterações: exporta direto, sem aviso
                Tela.Achar<Button>(form, "pdf").PerformClick();
                Assert.Equal("PDF salvo.", form.Acoes.Mensagens[1]);
                Assert.True(System.IO.File.Exists(form.Acoes.Salvos[1]));
                Assert.Single(_orcamentos.ObterPorId(1).Itens);
            });
        }

        [Fact]
        public void ExportarComErroNoOrcamentoNaoGeraArquivo()
        {
            PastaExportacao();

            Rodar(_temporario.Banco, form =>
            {
                Avulso(form, "José Fictício");
                Tela.Achar<Button>(form, "pdf").PerformClick();

                Assert.Equal("Inclua ao menos um item.", form.UltimoAviso);
                Assert.Empty(form.Acoes.Salvos);
            });
        }

        [Fact]
        public void CopiarImagemEImprimirUsamOOrcamentoSalvo()
        {
            PastaExportacao();

            Rodar(_temporario.Banco, form =>
            {
                Avulso(form, "José Fictício");
                AdicionarItem(form, "2", "m²", "Placa", "12,50");

                Tela.Achar<Button>(form, "copiarImagem").PerformClick();
                Assert.Equal(new System.Drawing.Size(1240, 1754), Assert.Single(form.Acoes.Copias));
                Assert.Contains("Ctrl+V", form.Acoes.Informacoes.Single());

                Tela.Achar<Button>(form, "imprimir").PerformClick();
                Assert.Equal("Orçamento Nº 0001", Assert.Single(form.Acoes.Impressoes));
            });
        }

        [Fact]
        public void WhatsAppSoComTelefoneEAbreAConversaComAImagemNaPasta()
        {
            PastaExportacao();

            Rodar(_temporario.Banco, form =>
            {
                Avulso(form, "José Fictício");
                AdicionarItem(form, "2", "m²", "Placa", "12,50");
                var whatsapp = Tela.Achar<Button>(form, "whatsapp");
                Assert.False(whatsapp.Enabled);

                Tela.Achar<CampoTelefone>(form, "telefone").Ddd = "11";
                Tela.Achar<CampoTelefone>(form, "telefone").Numero = "987654321";
                Assert.True(whatsapp.Enabled);
                Assert.Equal("Abrir conversa com (11) 98765-4321", form.DicaDe(whatsapp));

                whatsapp.PerformClick();

                Assert.Equal("https://wa.me/5511987654321", Assert.Single(form.Acoes.Links));
                var arquivo = Assert.Single(form.Acoes.Selecionados);
                Assert.EndsWith(".png", arquivo);
                Assert.True(System.IO.File.Exists(arquivo));
            });
        }

        [Fact]
        public void WhatsAppComTelefoneInvalidoMostraAvisoEmVezDeAbrir()
        {
            PastaExportacao();
            var orcamento = new Orcamento { Data = DateTime.Today, CliNome = "Antigo", CliTelefone = "99244540", Itens = { new ItemOrcamento { Quantidade = 1, Descricao = "Placa", UnitarioCentavos = 100 } } };
            _orcamentos.Salvar(orcamento);

            Rodar(_temporario.Banco, form =>
            {
                form.Acoes.WhatsApp(form.Orcamento);

                Assert.Empty(form.Acoes.Links);
                Assert.Contains("não está completo", form.Acoes.Avisos.Single());
            }, orcamento: _orcamentos.ObterPorId(orcamento.Id));
        }

        private class OrcamentoSemDialogos : FormOrcamento
        {
            public List<string> Avisos { get; } = new List<string>();
            public List<string> Perguntas { get; } = new List<string>();
            public string UltimoAviso => Avisos.LastOrDefault();
            public bool Resposta { get; set; }
            public DialogResult RespostaSalvar { get; set; } = DialogResult.No;
            public Action<Cliente> ClienteParaCadastrar { get; set; }

            public OrcamentoSemDialogos(Banco banco, Orcamento orcamento = null, Cliente cliente = null) : base(banco, orcamento, cliente)
            {
            }

            public AcoesFalsas Acoes { get; private set; }

            protected override AcoesExportacao CriarAcoes(Banco banco) => Acoes = new AcoesFalsas(banco, this);


            public bool Teclar(Keys tecla)
            {
                var mensagem = new Message();
                return ProcessCmdKey(ref mensagem, tecla);
            }

            /// <summary>Fecha sem perguntar nada, no fim de cada teste.</summary>
            public void Descartar()
            {
                RespostaSalvar = DialogResult.No;
                Close();
            }

            protected override void Avisar(string mensagem) => Avisos.Add(mensagem);

            protected override bool Confirmar(string pergunta)
            {
                Perguntas.Add(pergunta);
                return Resposta;
            }

            protected override DialogResult PerguntarSalvar(string pergunta)
            {
                Perguntas.Add(pergunta);
                return RespostaSalvar;
            }

            protected override bool AbrirCadastroCliente(Cliente cliente)
            {
                if (ClienteParaCadastrar == null)
                    return false;

                ClienteParaCadastrar(cliente);
                return true;
            }
        }
    }
}
