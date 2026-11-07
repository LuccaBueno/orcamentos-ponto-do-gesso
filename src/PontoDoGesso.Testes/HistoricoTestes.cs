using System;
using System.Diagnostics;
using System.Linq;
using Dapper;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;
using Xunit;
using Xunit.Abstractions;

namespace PontoDoGesso.Testes
{
    public class HistoricoTestes : IDisposable
    {
        private static readonly DateTime Hoje = new DateTime(2026, 11, 7);

        private readonly BancoTemporario _temporario = new BancoTemporario();
        private readonly OrcamentoRepositorio _orcamentos;
        private readonly ClienteRepositorio _clientes;

        private readonly ITestOutputHelper _saida;

        public HistoricoTestes(ITestOutputHelper saida)
        {
            _saida = saida;
            _temporario.Banco.Preparar();
            _orcamentos = new OrcamentoRepositorio(_temporario.Banco);
            _clientes = new ClienteRepositorio(_temporario.Banco);
        }

        public void Dispose() => _temporario.Dispose();

        private Orcamento Novo(string nome, DateTime data, Cliente cliente = null, long frete = 0, decimal descontoPercentual = 0, string telefone = null, long preco = 10000)
        {
            var orcamento = new Orcamento
            {
                Data = data,
                CliNome = nome,
                CliTelefone = telefone,
                FreteCentavos = frete,
                DescontoTipo = descontoPercentual > 0 ? TipoDesconto.Percentual : (TipoDesconto?)null,
                DescontoValor = descontoPercentual,
                Itens = { new ItemOrcamento { Quantidade = 1, Descricao = "Placa", UnitarioCentavos = preco } }
            };

            if (cliente != null)
            {
                orcamento.CopiarCliente(cliente);
                orcamento.CliNome = nome;
            }

            _orcamentos.Salvar(orcamento);
            return orcamento;
        }

        private Cliente NovoCliente(string nome)
        {
            var cliente = new Cliente { Nome = nome, Telefone = "12992445402" };
            _clientes.Inserir(cliente);
            return cliente;
        }

        private long[] Numeros(FiltroHistorico filtro) =>
            _orcamentos.Buscar(filtro, Hoje).Select(o => o.Numero).OrderBy(n => n).ToArray();

        [Theory]
        [InlineData(PeriodoHistorico.Hoje, "2026-11-07", "2026-11-07")]
        [InlineData(PeriodoHistorico.Ultimos7Dias, "2026-11-01", "2026-11-07")]
        [InlineData(PeriodoHistorico.Ultimos30Dias, "2026-10-09", "2026-11-07")]
        [InlineData(PeriodoHistorico.EsteMes, "2026-11-01", "2026-11-30")]
        [InlineData(PeriodoHistorico.MesPassado, "2026-10-01", "2026-10-31")]
        public void IntervaloDoPeriodo(PeriodoHistorico periodo, string de, string ate)
        {
            var (inicio, fim) = new FiltroHistorico { Periodo = periodo }.Intervalo(Hoje.AddHours(15));

            Assert.Equal(DateTime.Parse(de), inicio);
            Assert.Equal(DateTime.Parse(ate), fim);
        }

        [Fact]
        public void MesPassadoEmJaneiroEDezembroDoAnoAnterior()
        {
            var (inicio, fim) = new FiltroHistorico { Periodo = PeriodoHistorico.MesPassado }.Intervalo(new DateTime(2026, 1, 15));

            Assert.Equal(new DateTime(2025, 12, 1), inicio);
            Assert.Equal(new DateTime(2025, 12, 31), fim);
        }

        [Fact]
        public void TodosNaoTemLimiteEPersonalizadoUsaDeAte()
        {
            Assert.Equal((null, null), new FiltroHistorico().Intervalo(Hoje));
            Assert.Equal(((DateTime?)new DateTime(2026, 1, 1), (DateTime?)null), new FiltroHistorico { Periodo = PeriodoHistorico.Personalizado, De = new DateTime(2026, 1, 1, 10, 0, 0) }.Intervalo(Hoje));
        }

        [Fact]
        public void CadaFiltroIsoladoECombinado()
        {
            var cliente = NovoCliente("Ana Cadastrada");
            Novo("Ana Cadastrada", Hoje, cliente, frete: 2000);                        // 1
            Novo("Bruno Avulso", Hoje.AddDays(-3), frete: 1500);                       // 2
            Novo("Carla Avulsa", new DateTime(2026, 10, 15), frete: 3000);             // 3: mês passado, avulso, com frete
            Novo("Davi Avulso", new DateTime(2026, 10, 20), descontoPercentual: 10);   // 4: mês passado, avulso, sem frete
            Novo("Ana Cadastrada", new DateTime(2026, 10, 25), cliente, descontoPercentual: 5); // 5: mês passado, cadastrado
            Novo("Élio Antigo", new DateTime(2025, 1, 10), telefone: "11987654321");   // 6

            Assert.Equal(new long[] { 1, 2, 3, 4, 5, 6 }, Numeros(new FiltroHistorico()));
            Assert.Equal(new long[] { 1 }, Numeros(new FiltroHistorico { Periodo = PeriodoHistorico.Hoje }));
            Assert.Equal(new long[] { 1, 2 }, Numeros(new FiltroHistorico { Periodo = PeriodoHistorico.Ultimos7Dias }));
            Assert.Equal(new long[] { 1, 2, 3, 4, 5 }, Numeros(new FiltroHistorico { Periodo = PeriodoHistorico.Ultimos30Dias }));
            Assert.Equal(new long[] { 1, 2 }, Numeros(new FiltroHistorico { Periodo = PeriodoHistorico.EsteMes }));
            Assert.Equal(new long[] { 3, 4, 5 }, Numeros(new FiltroHistorico { Periodo = PeriodoHistorico.MesPassado }));
            Assert.Equal(new long[] { 4, 5 }, Numeros(new FiltroHistorico { Periodo = PeriodoHistorico.Personalizado, De = new DateTime(2026, 10, 20), Ate = new DateTime(2026, 10, 31) }));

            Assert.Equal(new long[] { 1, 5 }, Numeros(new FiltroHistorico { TipoCliente = TipoClienteHistorico.Cadastrados }));
            Assert.Equal(new long[] { 2, 3, 4, 6 }, Numeros(new FiltroHistorico { TipoCliente = TipoClienteHistorico.Avulsos }));
            Assert.Equal(new long[] { 1, 2, 3 }, Numeros(new FiltroHistorico { Frete = PresencaHistorico.Com }));
            Assert.Equal(new long[] { 4, 5, 6 }, Numeros(new FiltroHistorico { Frete = PresencaHistorico.Sem }));
            Assert.Equal(new long[] { 4, 5 }, Numeros(new FiltroHistorico { Desconto = PresencaHistorico.Com }));
            Assert.Equal(new long[] { 1, 2, 3, 6 }, Numeros(new FiltroHistorico { Desconto = PresencaHistorico.Sem }));

            // Avulsos + com frete + mês passado
            Assert.Equal(new long[] { 3 }, Numeros(new FiltroHistorico { TipoCliente = TipoClienteHistorico.Avulsos, Frete = PresencaHistorico.Com, Periodo = PeriodoHistorico.MesPassado }));
            Assert.Equal(new long[] { 5 }, Numeros(new FiltroHistorico { TipoCliente = TipoClienteHistorico.Cadastrados, Desconto = PresencaHistorico.Com, Frete = PresencaHistorico.Sem }));
        }

        [Fact]
        public void ClienteEspecificoTrazSoOsOrcamentosDaqueleCadastro()
        {
            var ana = NovoCliente("Ana");
            var outraAna = NovoCliente("Ana");
            Novo("Ana", Hoje, ana);
            Novo("Ana", Hoje, outraAna);
            Novo("Ana", Hoje);
            Novo("Ana (obra da praia)", Hoje, ana);

            Assert.Equal(new long[] { 1, 4 }, Numeros(new FiltroHistorico { ClienteId = ana.Id }));
            Assert.Equal(new long[] { 1, 4 }, Numeros(new FiltroHistorico { ClienteId = ana.Id, TipoCliente = TipoClienteHistorico.Cadastrados }));

            // "Só avulsos" ignora o cliente específico
            Assert.Equal(new long[] { 3 }, Numeros(new FiltroHistorico { ClienteId = ana.Id, TipoCliente = TipoClienteHistorico.Avulsos }));
        }

        [Fact]
        public void TextoAchaPorNumeroCodigoDoClienteNomeSemAcentoETelefone()
        {
            var cliente = NovoCliente("Zé");
            Novo("José Avulso", Hoje, telefone: "11987654321");   // 1
            Novo("Maria", Hoje);                                   // 2
            Novo("Zé", Hoje, cliente);                             // 3 (cliente 0001)

            Assert.Equal(new long[] { 1 }, Numeros(new FiltroHistorico { Texto = "jose" }));
            Assert.Equal(new long[] { 1 }, Numeros(new FiltroHistorico { Texto = "(11) 98765" }));
            Assert.Equal(new long[] { 2 }, Numeros(new FiltroHistorico { Texto = "MARÍA" }));
            Assert.Equal(new long[] { 1, 3 }, Numeros(new FiltroHistorico { Texto = "1" }));
            Assert.Empty(Numeros(new FiltroHistorico { Texto = "%" }));
        }

        [Fact]
        public void NumeroDoOrcamentoECodigoDoClienteNaBusca()
        {
            var cliente = NovoCliente("Carlos");                 // cliente 0001
            Novo("Maria", Hoje);                                  // orçamento 0001
            Novo("Carlos", Hoje, cliente);                        // orçamento 0002, cliente 0001
            Novo("Pedro", Hoje);                                  // orçamento 0003

            Assert.Equal(new long[] { 1, 2 }, Numeros(new FiltroHistorico { Texto = "0001" }));
            Assert.Equal(new long[] { 3 }, Numeros(new FiltroHistorico { Texto = "3" }));
        }

        [Fact]
        public void MaisRecentesPrimeiroEOrdenacaoPorColuna()
        {
            Novo("Bia", new DateTime(2026, 10, 1), preco: 300);
            Novo("Ana", new DateTime(2026, 11, 1), preco: 100);
            Novo("Caio", new DateTime(2026, 10, 15), preco: 200);

            Assert.Equal(new long[] { 2, 3, 1 }, _orcamentos.Buscar(new FiltroHistorico(), Hoje).Select(o => o.Numero));
            Assert.Equal(new[] { "Ana", "Bia", "Caio" }, _orcamentos.Buscar(new FiltroHistorico { OrdenarPor = ColunaHistorico.Cliente, Crescente = true }, Hoje).Select(o => o.CliNome));
            Assert.Equal(new long[] { 300, 200, 100 }, _orcamentos.Buscar(new FiltroHistorico { OrdenarPor = ColunaHistorico.Total }, Hoje).Select(o => o.TotalCentavos));
        }

        [Fact]
        public void PaginacaoESomaDoFiltroInteiro()
        {
            for (var i = 0; i < 7; i++)
                Novo("Cliente " + i, Hoje.AddDays(-i), preco: 1000);

            var filtro = new FiltroHistorico();
            Assert.Equal(new long[] { 1, 2, 3 }, _orcamentos.Buscar(filtro, Hoje, 3, 0).Select(o => o.Numero));
            Assert.Equal(new long[] { 7 }, _orcamentos.Buscar(filtro, Hoje, 3, 6).Select(o => o.Numero));
            Assert.Equal((7, 7000L), _orcamentos.Somar(filtro, Hoje));
            Assert.Equal((0, 0L), _orcamentos.Somar(new FiltroHistorico { Texto = "ninguém" }, Hoje));
        }

        [Fact]
        public void BuscaRapidaComCincoMilOrcamentos()
        {
            using (var conexao = _temporario.Banco.Abrir())
            using (var transacao = conexao.BeginTransaction())
            {
                for (var i = 1; i <= 5000; i++)
                {
                    conexao.Execute(
                        @"INSERT INTO orcamentos (numero, data, cli_nome, cli_telefone, tipo_entrega, subtotal_centavos, desconto_centavos, frete_centavos, total_centavos, criado_em, atualizado_em)
                          VALUES (@i, @data, @nome, @telefone, 'ENTREGA', 10000, @desconto, @frete, 10000, '2026-01-01T00:00:00', '2026-01-01T00:00:00')",
                        new
                        {
                            i,
                            data = Hoje.AddDays(-(i % 700)).ToString("yyyy-MM-dd"),
                            nome = "Cliente Fictício " + i,
                            telefone = "1299" + (1000000 + i),
                            desconto = i % 4 == 0 ? 500 : 0,
                            frete = i % 3 == 0 ? 2000 : 0
                        }, transacao);
                }

                transacao.Commit();
            }

            var relogio = Stopwatch.StartNew();
            var todos = _orcamentos.Buscar(new FiltroHistorico(), Hoje);
            var soma = _orcamentos.Somar(new FiltroHistorico(), Hoje);
            var texto = _orcamentos.Buscar(new FiltroHistorico { Texto = "ficticio 4999" }, Hoje);
            var combinado = _orcamentos.Somar(new FiltroHistorico { Periodo = PeriodoHistorico.MesPassado, Frete = PresencaHistorico.Com, TipoCliente = TipoClienteHistorico.Avulsos, Texto = "cliente" }, Hoje);
            relogio.Stop();
            _saida.WriteLine("Busca com 5.000 orçamentos: " + relogio.ElapsedMilliseconds + " ms");

            Assert.Equal(200, todos.Count);
            Assert.Equal(5000, soma.Quantidade);
            Assert.Equal(4999, Assert.Single(texto).Numero);
            Assert.True(combinado.Quantidade > 0);
            // Meta no Windows 7: menos de 1 s. Aqui fica bem abaixo; folga para o CI.
            Assert.True(relogio.ElapsedMilliseconds < 1500, "Busca levou " + relogio.ElapsedMilliseconds + " ms");
        }
    }
}
