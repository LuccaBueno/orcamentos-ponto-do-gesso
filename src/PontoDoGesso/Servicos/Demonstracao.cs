using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;

namespace PontoDoGesso.Servicos
{
    /// <summary>
    /// Modo de demonstração: um banco separado (demo.db) com clientes, produtos e orçamentos fictícios,
    /// usado para mostrar o app e tirar os prints do README. O banco real nunca é aberto nem alterado por aqui.
    /// </summary>
    public static class Demonstracao
    {
        public static string ArquivoBanco => Path.Combine(Caminhos.PastaDados, "demo.db");
        public static string PastaBackups => Path.Combine(Caminhos.PastaDados, "demo-backups");
        public static string PastaExportacao => Path.Combine(Caminhos.PastaDados, "demo-exportados");

        /// <summary>Recria o banco de demonstração do zero, com as datas contadas a partir de hoje.</summary>
        public static Banco Criar(DateTime hoje) => Criar(ArquivoBanco, PastaBackups, PastaExportacao, hoje);

        public static Banco Criar(string arquivo, string pastaBackups, string pastaExportacao, DateTime hoje)
        {
            if (string.Equals(Path.GetFullPath(arquivo), Path.GetFullPath(Caminhos.ArquivoBanco), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("O banco de demonstração não pode ser o banco real.");

            foreach (var sobra in new[] { arquivo, arquivo + "-wal", arquivo + "-shm" })
            {
                if (File.Exists(sobra))
                    File.Delete(sobra);
            }

            var banco = new Banco(arquivo, pastaBackups);
            banco.Preparar();

            // Exportações da demonstração não se misturam com os orçamentos de verdade
            new ConfiguracaoRepositorio(banco).Definir("exportacao.pasta", pastaExportacao);

            var produtos = InserirProdutos(banco);
            var clientes = InserirClientes(banco);
            InserirOrcamentos(banco, produtos, clientes, hoje);
            return banco;
        }

        private static Dictionary<string, Produto> InserirProdutos(Banco banco)
        {
            var repositorio = new ProdutoRepositorio(banco);
            var lista = new[]
            {
                ("Placa de gesso 60x60", "un", 1250L),
                ("Gesso em pó 40 kg", "saco", 3890L),
                ("Placa drywall ST 1,20x1,80", "un", 4590L),
                ("Placa drywall RU (verde) 1,20x1,80", "un", 5890L),
                ("Perfil montante 48 mm x 3 m", "un", 2190L),
                ("Perfil guia 48 mm x 3 m", "un", 1790L),
                ("Parafuso drywall 25 mm (cx 1000)", "cx", 4500L),
                ("Fita telada 50 m", "un", 1590L),
                ("Massa para drywall 30 kg", "balde", 8990L),
                ("Sanca de gesso 10 cm", "m", 890L),
                ("Moldura de gesso 5 cm", "m", 650L),
                ("Cola de gesso 5 kg", "saco", 1490L),
                ("Arame galvanizado nº 18", "kg", 2690L),
                ("Tabica metálica 3 m", "un", 1390L),
                ("Espátula de aço 10 cm", "un", 1850L),
                ("Desempenadeira de PVC", "un", 2490L)
            };

            var produtos = new Dictionary<string, Produto>();
            foreach (var (nome, unidade, preco) in lista)
            {
                var produto = new Produto { Nome = nome, Unidade = unidade, PrecoCentavos = preco };
                repositorio.Inserir(produto);
                produtos[nome] = produto;
            }
            return produtos;
        }

        private static List<Cliente> InserirClientes(Banco banco)
        {
            // Nomes e endereços inventados; telefones na faixa (12) 90000-00xx, que não é usada por celulares de verdade
            var repositorio = new ClienteRepositorio(banco);
            var clientes = new List<Cliente>
            {
                new Cliente { Nome = "Ana Paula Moreira", Telefone = "12900000001", Endereco = "Rua das Acácias", Numero = "120", Bairro = "Jardim das Flores", PontoReferencia = "Portão verde" },
                new Cliente { Nome = "Construtora Exemplo Ltda", Telefone = "12900000002", Telefone2 = "1236000002", Endereco = "Avenida Principal", Numero = "1500", Bairro = "Distrito Industrial", Observacoes = "Pede nota com o CNPJ da obra" },
                new Cliente { Nome = "Carlos Eduardo Lima", Telefone = "12900000003", Endereco = "Rua dos Ipês", Numero = "45", Bairro = "Vila Nova" },
                new Cliente { Nome = "Fernanda Souza", Telefone = "12900000004", Endereco = "Rua Sete de Setembro", Numero = "300", Bairro = "Centro", PontoReferencia = "Ao lado da padaria" },
                new Cliente { Nome = "José Ricardo Alves", Telefone = "12900000005", Endereco = "Rua das Palmeiras", Numero = "78", Bairro = "Bosque" },
                new Cliente { Nome = "Mariana Costa", Telefone = "12900000006", Endereco = "Travessa do Sol", Numero = "12", Bairro = "Jardim América" },
                new Cliente { Nome = "Gesseiro Paulo Santos", Telefone = "12900000007", Endereco = "Rua do Comércio", Numero = "910", Bairro = "Centro", Observacoes = "Gesseiro parceiro: sempre retira na loja" },
                new Cliente { Nome = "Roberta Nunes", Telefone = "12900000008", Endereco = "Rua Beija-Flor", Numero = "33", Bairro = "Parque das Águas" }
            };
            foreach (var cliente in clientes)
                repositorio.Inserir(cliente);
            return clientes;
        }

        private static void InserirOrcamentos(Banco banco, Dictionary<string, Produto> p, List<Cliente> c, DateTime hoje)
        {
            var repositorio = new OrcamentoRepositorio(banco);

            void Salvar(int diasAtras, Cliente cliente, string avulso, Action<Orcamento> ajustar, params (string Produto, decimal Quantidade)[] itens)
            {
                var orcamento = new Orcamento { Data = hoje.Date.AddDays(-diasAtras) };
                if (cliente != null)
                    orcamento.CopiarCliente(cliente);
                else
                    orcamento.CliNome = avulso;

                orcamento.Itens = itens.Select(item => ItemOrcamento.DoProduto(p[item.Produto], item.Quantidade)).ToList();
                ajustar?.Invoke(orcamento);
                repositorio.Salvar(orcamento);
            }

            Salvar(38, c[1], null, o => { o.FreteCentavos = 8000; o.ValidadeDias = 7; },
                ("Placa drywall ST 1,20x1,80", 40), ("Perfil montante 48 mm x 3 m", 60), ("Perfil guia 48 mm x 3 m", 30), ("Parafuso drywall 25 mm (cx 1000)", 3), ("Fita telada 50 m", 4), ("Massa para drywall 30 kg", 4));
            Salvar(31, c[0], null, o => o.FreteCentavos = 3000,
                ("Placa de gesso 60x60", 45), ("Arame galvanizado nº 18", 2), ("Gesso em pó 40 kg", 2));
            Salvar(26, null, "Seu Antônio (obra da esquina)", o => { o.TipoEntrega = TipoEntrega.Retirada; },
                ("Gesso em pó 40 kg", 5), ("Cola de gesso 5 kg", 2));
            Salvar(21, c[2], null, o => { o.FreteCentavos = 3500; o.DescontoTipo = TipoDesconto.Percentual; o.DescontoValor = 5; },
                ("Sanca de gesso 10 cm", 32), ("Moldura de gesso 5 cm", 18.5m), ("Cola de gesso 5 kg", 3));
            Salvar(17, c[6], null, o => { o.TipoEntrega = TipoEntrega.Retirada; o.EntEndereco = o.EntNumero = o.EntBairro = o.EntCidade = o.EntReferencia = null; },
                ("Placa de gesso 60x60", 120), ("Gesso em pó 40 kg", 6), ("Arame galvanizado nº 18", 5), ("Tabica metálica 3 m", 20));
            Salvar(12, c[3], null, o => { o.FreteCentavos = 3000; o.ValidadeDias = 10; o.Observacoes = "Entregar pela manhã."; },
                ("Placa drywall RU (verde) 1,20x1,80", 8), ("Perfil montante 48 mm x 3 m", 12), ("Perfil guia 48 mm x 3 m", 6), ("Parafuso drywall 25 mm (cx 1000)", 1), ("Massa para drywall 30 kg", 1));
            Salvar(9, null, "Cláudia (indicação do Paulo)", o => { o.CliTelefone = "12900000009"; o.EntEndereco = "Rua Primavera"; o.EntNumero = "8"; o.EntBairro = "Jardim Sul"; o.EntCidade = "Pindamonhangaba"; o.FreteCentavos = 2500; },
                ("Placa de gesso 60x60", 30), ("Tabica metálica 3 m", 8));
            Salvar(6, c[4], null, o => { o.FreteCentavos = 3000; o.DescontoTipo = TipoDesconto.Valor; o.DescontoValor = 2000; },
                ("Placa drywall ST 1,20x1,80", 12), ("Perfil montante 48 mm x 3 m", 18), ("Perfil guia 48 mm x 3 m", 9), ("Parafuso drywall 25 mm (cx 1000)", 1), ("Fita telada 50 m", 2));
            Salvar(4, c[5], null, o => o.FreteCentavos = 2500,
                ("Moldura de gesso 5 cm", 42), ("Cola de gesso 5 kg", 2), ("Espátula de aço 10 cm", 1));
            Salvar(2, c[7], null, o => { o.TipoEntrega = TipoEntrega.Retirada; o.EntEndereco = o.EntNumero = o.EntBairro = o.EntCidade = o.EntReferencia = null; },
                ("Gesso em pó 40 kg", 3), ("Desempenadeira de PVC", 1));
            Salvar(1, c[1], null, o => { o.FreteCentavos = 8000; o.ValidadeDias = 7; o.Observacoes = "Material para o 2º andar. Entregas só até o térreo."; },
                ("Placa drywall ST 1,20x1,80", 25), ("Placa drywall RU (verde) 1,20x1,80", 10), ("Perfil montante 48 mm x 3 m", 40), ("Perfil guia 48 mm x 3 m", 20), ("Parafuso drywall 25 mm (cx 1000)", 2), ("Massa para drywall 30 kg", 3));
            Salvar(0, c[0], null, o => { o.FreteCentavos = 3000; o.DescontoTipo = TipoDesconto.Percentual; o.DescontoValor = 3; },
                ("Placa de gesso 60x60", 38), ("Sanca de gesso 10 cm", 24), ("Gesso em pó 40 kg", 2), ("Arame galvanizado nº 18", 2));
        }
    }
}
