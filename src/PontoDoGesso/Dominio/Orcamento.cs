using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using PontoDoGesso.Servicos;

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

        /// <summary>1 = primeira versão (sem sufixo); alterar e salvar depois de enviado cria a próxima.</summary>
        public int Versao { get; set; } = 1;

        /// <summary>Quando esta versão foi exportada, copiada, impressa ou mandada pelo WhatsApp; nulo = ainda não enviada.</summary>
        public DateTime? EnviadoEm { get; set; }

        /// <summary>Só no app: a lista de itens mostra o que já saiu da loja e o que falta (nunca sai no orçamento).</summary>
        public bool ControlarSaidas { get; set; }

        /// <summary>Versão antiga guardada: abre só para leitura e, exportada de novo, não conta como envio.</summary>
        [XmlIgnore]
        public bool VersaoAntiga { get; set; }

        /// <summary>Lidos no histórico: itens com tudo já saído e total de itens.</summary>
        [XmlIgnore]
        public int SaidasCompletas { get; set; }

        [XmlIgnore]
        public int SaidasItens { get; set; }

        /// <summary>Lido no histórico: quantas versões antigas estão guardadas.</summary>
        [XmlIgnore]
        public int QuantidadeVersoesAntigas { get; set; }

        /// <summary>"0001" na primeira versão, "0001-2" na segunda...</summary>
        public string NumeroExibido => NumeroComVersao(Numero, Versao);

        public static string NumeroComVersao(long numero, int versao) =>
            Formatacao.Codigo(numero) + (versao > 1 ? "-" + versao : "");

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

        /// <summary>Quem recebe no local da entrega (opcional).</summary>
        public string EntReceberCom { get; set; }

        /// <summary>Telefone de quem recebe, só dígitos com DDD (opcional).</summary>
        public string EntTelefoneLocal { get; set; }

        public TipoEntrega TipoEntrega { get; set; } = TipoEntrega.Entrega;

        /// <summary>O CNPJ da empresa sai no cabeçalho deste orçamento (desligado por padrão).</summary>
        public bool MostrarCnpj { get; set; }

        /// <summary>Cópia (snapshot) do CNPJ da empresa, só dígitos, quando <see cref="MostrarCnpj"/> está ligado.</summary>
        public string EmpresaCnpj { get; set; }

        /// <summary>Sai impresso no orçamento.</summary>
        public string Observacoes { get; set; }

        /// <summary>A observação do cadastro do cliente sai no orçamento, logo abaixo das observações (desligado por padrão).</summary>
        public bool IncluirObsCliente { get; set; }

        /// <summary>Cópia (snapshot) da observação do cadastro usada quando <see cref="IncluirObsCliente"/> está ligado.</summary>
        public string ObsCliente { get; set; }

        /// <summary>Observação do cliente que sai no orçamento; nulo se não foi incluída.</summary>
        public string ObsClienteImpressa => IncluirObsCliente && !string.IsNullOrWhiteSpace(ObsCliente) ? ObsCliente.Trim() : null;

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
