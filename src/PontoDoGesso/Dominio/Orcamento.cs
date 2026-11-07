using System;
using System.Collections.Generic;

namespace PontoDoGesso.Dominio
{
    public enum TipoEntrega
    {
        Entrega,
        Retirada
    }

    public enum TipoDesconto
    {
        Valor,
        Percentual
    }

    public class Orcamento
    {
        public long Id { get; set; }

        /// <summary>Atribuído ao salvar pela primeira vez; 0 = ainda não salvo.</summary>
        public long Numero { get; set; }

        public DateTime Data { get; set; }

        /// <summary>Nulo = sem validade (não imprime).</summary>
        public int? ValidadeDias { get; set; }

        /// <summary>Nulo = cliente avulso.</summary>
        public long? ClienteId { get; set; }

        // Cópia (snapshot) do cliente e da entrega no momento do orçamento
        public string CliNome { get; set; }
        public string CliTelefone { get; set; }
        public string EntEndereco { get; set; }
        public string EntNumero { get; set; }
        public string EntBairro { get; set; }
        public string EntCidade { get; set; }
        public string EntReferencia { get; set; }

        public TipoEntrega TipoEntrega { get; set; } = TipoEntrega.Entrega;

        /// <summary>Sai impresso no orçamento.</summary>
        public string Observacoes { get; set; }

        public long SubtotalCentavos { get; set; }

        /// <summary>Nulo = sem desconto.</summary>
        public TipoDesconto? DescontoTipo { get; set; }

        /// <summary>Centavos se o tipo for Valor; percentual (ex.: 5,5) se for Percentual.</summary>
        public decimal DescontoValor { get; set; }

        public long DescontoCentavos { get; set; }
        public long FreteCentavos { get; set; }
        public long TotalCentavos { get; set; }
        public DateTime CriadoEm { get; set; }
        public DateTime AtualizadoEm { get; set; }

        public List<ItemOrcamento> Itens { get; set; } = new List<ItemOrcamento>();

        public void CopiarCliente(Cliente cliente)
        {
            ClienteId = cliente.Id;
            CliNome = cliente.Nome;
            CliTelefone = cliente.Telefone;
            EntEndereco = cliente.Endereco;
            EntNumero = cliente.Numero;
            EntBairro = cliente.Bairro;
            EntCidade = cliente.Cidade;
            EntReferencia = cliente.PontoReferencia;
        }
    }
}
