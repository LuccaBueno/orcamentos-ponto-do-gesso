using System;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using PontoDoGesso.Controles;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;
using PontoDoGesso.Exportacao;
using Xunit;

namespace PontoDoGesso.Testes
{
    /// <summary>Observação do cliente como caixa, versão antiga só para leitura na tela de orçamento e o bloco novo abaixo dos itens.</summary>
    public partial class TelaOrcamentoTestes
    {

        private Orcamento Salvo(Cliente cliente = null, bool incluirObs = false)
        {
            var orcamento = new Orcamento { Data = new DateTime(2026, 11, 20), CliNome = cliente?.Nome ?? "Ana", ClienteId = cliente?.Id, Observacoes = "Pagamento na entrega." };
            orcamento.Itens.Add(new ItemOrcamento { Quantidade = 2, Descricao = "Placa", UnitarioCentavos = 1000 });
            if (incluirObs)
            {
                orcamento.IncluirObsCliente = true;
                orcamento.ObsCliente = cliente.Observacoes;
            }
            _orcamentos.Salvar(orcamento);
            return orcamento;
        }

        private static void RodarSalvo(Banco banco, Orcamento orcamento, Action<OrcamentoSemDialogos> teste)
        {
            Tela.EmSta(() =>
            {
                using (var form = new OrcamentoSemDialogos(banco, orcamento, null))
                {
                    Tela.Exibir(form);
                    teste(form);
                    form.Descartar();
                }
            });
        }

        [Fact]
        public void ObservacaoDoClienteSoAparecePorCaixaDesmarcadaEGuardaCopia()
        {
            var cliente = new Cliente { Nome = "João", Cidade = "Pinda", Observacoes = "Portão azul" };
            _clientes.Inserir(cliente);
            var orcamento = Salvo(cliente);

            RodarSalvo(_temporario.Banco, _orcamentos.ObterPorId(orcamento.Id), form =>
            {
                var caixa = Tela.Achar<CheckBox>(form, "incluirObsCliente");
                Assert.True(caixa.Visible);
                Assert.False(caixa.Checked);
                Assert.Equal("Incluir observação do cliente: 'Portão azul'", caixa.Text);
                caixa.Checked = true;
                Assert.True(form.Salvar(), form.UltimoAviso);
                Assert.Equal("Pagamento na entrega.", Tela.Achar<TextBox>(form, "observacoes").Text);
            });

            // Editar o cadastro depois não muda o orçamento
            cliente.Observacoes = "Outra coisa";
            _clientes.Atualizar(cliente);
            var gravado = _orcamentos.ObterPorId(orcamento.Id);
            Assert.True(gravado.IncluirObsCliente);
            Assert.Equal("Portão azul", gravado.ObsClienteImpressa);

            RodarSalvo(_temporario.Banco, gravado, form =>
            {
                Assert.Equal("Incluir observação do cliente: 'Portão azul'", Tela.Achar<CheckBox>(form, "incluirObsCliente").Text);
                form.Duplicar();
                Assert.False(Tela.Achar<CheckBox>(form, "incluirObsCliente").Checked);
            });
        }

        [Fact]
        public void SemObservacaoNoCadastroACaixaNaoAparece()
        {
            RodarSalvo(_temporario.Banco, _orcamentos.ObterPorId(Salvo().Id), form =>
                Assert.False(Tela.Achar<CheckBox>(form, "incluirObsCliente").Visible));
        }

        [Fact]
        public void VersaoAntigaAbreNaMesmaTelaSoParaLeitura()
        {
            var orcamento = Salvo();
            _orcamentos.MarcarEnviado(orcamento);
            orcamento.FreteCentavos = 500;
            _orcamentos.Salvar(orcamento);

            RodarSalvo(_temporario.Banco, _orcamentos.ObterPorId(orcamento.Id), form =>
            {
                Assert.True(Tela.Achar<ComboBox>(form, "seletorVersao").Visible);
                Assert.Equal(new[] { "0001-2 (atual)", "0001" }, form.VersoesNoSeletor);
                Assert.False(form.SomenteLeitura);

                form.EscolherVersao(1);
                Assert.True(form.SomenteLeitura);
                Assert.True(Tela.Achar<Control>(form, "faixaVersaoAntiga").Visible);
                Assert.Equal("Você está vendo a versão antiga 0001. A atual é a 0001-2.", Tela.Achar<Label>(form, "textoVersaoAntiga").Text);
                Assert.False(Tela.Achar<Button>(form, "salvar").Enabled);
                Assert.True(Tela.Achar<Button>(form, "png").Enabled);
                Assert.False(Tela.Achar<TextBox>(form, "nome").Enabled);
                Assert.True(Tela.Achar<GradeItens>(form, "itens").SomenteLeitura);
                Assert.False(form.Salvar());
                Assert.False(form.Alterado);

                Tela.Achar<Button>(form, "voltarAtual").PerformClick();
                Assert.False(form.SomenteLeitura);
                Assert.Equal("Orçamento Nº 0001-2", form.Text);
            });

            // Aberta pelo histórico (linha da versão antiga), já vem só para leitura
            var antiga = Assert.Single(_orcamentos.VersoesAntigas(orcamento.Id));
            RodarSalvo(_temporario.Banco, antiga, form => Assert.True(form.SomenteLeitura));
        }

        [Fact]
        public void DesenhoTemSubtotalSempreEObservacaoDoClienteSoQuandoIncluida()
        {
            var orcamento = Salvo();
            orcamento.ValidadeDias = 7;
            orcamento.ObsCliente = "Portão azul";
            Assert.Null(orcamento.ObsClienteImpressa);
            orcamento.IncluirObsCliente = true;
            Assert.Equal("Portão azul", orcamento.ObsClienteImpressa);

            using (var imagem = ExportadorPng.CriarImagem(orcamento, DadosLoja.Padrao))
                Assert.True(imagem.Height < imagem.Width * 297 / 210);

            // Exemplos para conferir o layout a olho (só quando pedido)
            var pasta = Environment.GetEnvironmentVariable("PONTODOGESSO_EXEMPLOS");
            if (pasta == null)
                return;

            Directory.CreateDirectory(pasta);
            orcamento.Numero = 2;
            orcamento.Versao = 3;
            orcamento.CliNome = "Maria Aparecida Souza";
            orcamento.CliTelefone = "12992445402";
            orcamento.EntEndereco = "Rua das Palmeiras";
            orcamento.EntNumero = "120";
            orcamento.EntBairro = "Centro";
            orcamento.EntCidade = "Pindamonhangaba";
            orcamento.EntReferencia = "Perto da padaria";
            orcamento.EntReceberCom = "Sr. José";
            orcamento.EntTelefoneLocal = "12988887777";
            orcamento.Itens.Add(new ItemOrcamento { Quantidade = 10, Unidade = "m²", Descricao = "Placa de gesso 60x60", UnitarioCentavos = 1250 });
            orcamento.Itens.Add(new ItemOrcamento { Quantidade = 3, Unidade = "sc", Descricao = "Gesso cola 20 kg", UnitarioCentavos = 3490 });
            orcamento.FreteCentavos = 3000;
            orcamento.DescontoTipo = TipoDesconto.Percentual;
            orcamento.DescontoValor = 5;
            CalculoAplicar(orcamento);
            using (var imagem = ExportadorPng.CriarImagem(orcamento, DadosLoja.Padrao))
                imagem.Save(Path.Combine(pasta, "exemplo-1-com-observacoes.png"), ImageFormat.Png);

            orcamento.Observacoes = null;
            orcamento.IncluirObsCliente = false;
            orcamento.ValidadeDias = null;
            orcamento.DescontoTipo = null;
            orcamento.TipoEntrega = TipoEntrega.Retirada;
            CalculoAplicar(orcamento);
            using (var imagem = ExportadorPng.CriarImagem(orcamento, DadosLoja.Padrao))
                imagem.Save(Path.Combine(pasta, "exemplo-2-sem-observacoes.png"), ImageFormat.Png);
        }

        private static void CalculoAplicar(Orcamento orcamento) => PontoDoGesso.Servicos.CalculoOrcamento.Aplicar(orcamento);
    }
}
