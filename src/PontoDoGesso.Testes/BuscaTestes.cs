using System;
using System.Linq;
using Dapper;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;
using PontoDoGesso.Servicos;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class BuscaTestes : IDisposable
    {
        private readonly BancoTemporario _temporario = new BancoTemporario();
        private readonly ClienteRepositorio _clientes;
        private readonly ProdutoRepositorio _produtos;

        public BuscaTestes()
        {
            _temporario.Banco.Preparar();
            _clientes = new ClienteRepositorio(_temporario.Banco);
            _produtos = new ProdutoRepositorio(_temporario.Banco);
        }

        public void Dispose() => _temporario.Dispose();

        private Cliente NovoCliente(string nome, string telefone = null, bool ativo = true)
        {
            var cliente = new Cliente { Nome = nome, Telefone = telefone, Ativo = ativo };
            _clientes.Inserir(cliente);
            return cliente;
        }

        [Theory]
        [InlineData("João Conceição", "joao conceicao")]
        [InlineData("ÁÉÍÓÚ Ç ñ", "aeiou c n")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void NormalizarTiraAcentosEMaiusculas(string texto, string esperado)
        {
            Assert.Equal(esperado, Formatacao.Normalizar(texto));
        }

        [Theory]
        [InlineData("joao")]
        [InlineData("JOÃO")]
        [InlineData("conceicao")]
        public void BuscaDeClienteIgnoraAcentoEMaiuscula(string texto)
        {
            NovoCliente("João da Conceição");
            NovoCliente("Maria");

            Assert.Equal("João da Conceição", Assert.Single(_clientes.Buscar(texto)).Nome);
        }

        [Theory]
        [InlineData("99244")]
        [InlineData("992445402")]
        [InlineData("(12) 99244")]
        public void BuscaDeClienteAchaPeloTelefone(string texto)
        {
            NovoCliente("João", "(12) 99244-5402");
            NovoCliente("Maria", "(11) 98765-4321");

            Assert.Equal("João", Assert.Single(_clientes.Buscar(texto)).Nome);
        }

        [Theory]
        [InlineData("45")]
        [InlineData("0045")]
        public void BuscaPorCodigoAchaOMesmoRegistro(string texto)
        {
            using (var conexao = _temporario.Banco.Abrir())
            {
                conexao.Execute("INSERT INTO sqlite_sequence (name, seq) VALUES ('clientes', 44)");
                conexao.Execute("INSERT INTO sqlite_sequence (name, seq) VALUES ('produtos', 44)");
            }

            var cliente = NovoCliente("João");
            var produto = new Produto { Nome = "Placa" };
            _produtos.Inserir(produto);

            Assert.Equal(45, cliente.Id);
            Assert.Equal(45, Assert.Single(_clientes.Buscar(texto)).Id);
            Assert.Equal(45, Assert.Single(_produtos.Buscar(texto)).Id);
        }

        [Fact]
        public void CodigoExatoVemAntesDeTelefoneParecido()
        {
            var primeiro = NovoCliente("Zeca");
            NovoCliente("Ana", "(12) 91111-1111");

            Assert.Equal(primeiro.Id, _clientes.Buscar("1").First().Id);
        }

        [Fact]
        public void InativoSoApareceQuandoPedido()
        {
            NovoCliente("João", ativo: false);
            var produto = new Produto { Nome = "Placa", Ativo = false };
            _produtos.Inserir(produto);

            Assert.Empty(_clientes.Buscar("joao"));
            Assert.Single(_clientes.Buscar("joao", incluirInativos: true));
            Assert.Empty(_produtos.Buscar("placa"));
            Assert.Single(_produtos.Buscar("placa", incluirInativos: true));
        }

        [Fact]
        public void BuscaVaziaTrazTodosEmOrdemDeNome()
        {
            NovoCliente("Álvaro Z");
            NovoCliente("Bruno");
            NovoCliente("Alberto");

            Assert.Equal(new[] { "Alberto", "Álvaro Z", "Bruno" }, _clientes.Buscar("").Select(cliente => cliente.Nome));
        }

        [Fact]
        public void CuringasDigitadosSaoTextoComum()
        {
            NovoCliente("João");
            NovoCliente("100% Gesso");

            Assert.Equal("100% Gesso", Assert.Single(_clientes.Buscar("%")).Nome);
            Assert.Empty(_clientes.Buscar("_"));
        }

        [Fact]
        public void BuscaDeProdutoIgnoraAcento()
        {
            _produtos.Inserir(new Produto { Nome = "Massa para Junção" });
            _produtos.Inserir(new Produto { Nome = "Placa" });

            Assert.Equal("Massa para Junção", Assert.Single(_produtos.Buscar("juncao")).Nome);
        }
    }
}
