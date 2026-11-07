using System;

namespace PontoDoGesso.Dados
{
    public enum PeriodoHistorico
    {
        Todos,
        Hoje,
        Ultimos7Dias,
        Ultimos30Dias,
        EsteMes,
        MesPassado,
        Personalizado
    }

    public enum TipoClienteHistorico
    {
        Todos,
        Cadastrados,
        Avulsos
    }

    /// <summary>Todos / com / sem: usado nos filtros de frete e de desconto.</summary>
    public enum PresencaHistorico
    {
        Todos,
        Com,
        Sem
    }

    public enum ColunaHistorico
    {
        Numero,
        Data,
        Cliente,
        Tipo,
        Telefone,
        Frete,
        Desconto,
        Total,
        Entrega,
        Validade
    }

    /// <summary>Filtros do histórico; todos se combinam entre si.</summary>
    public class FiltroHistorico
    {
        public PeriodoHistorico Periodo { get; set; } = PeriodoHistorico.Todos;

        /// <summary>Só para o período Personalizado.</summary>
        public DateTime? De { get; set; }

        public DateTime? Ate { get; set; }

        public TipoClienteHistorico TipoCliente { get; set; } = TipoClienteHistorico.Todos;

        /// <summary>Cliente cadastrado específico; ignorado quando o filtro é "Só avulsos".</summary>
        public long? ClienteId { get; set; }

        /// <summary>Só para mostrar na tela o cliente escolhido.</summary>
        public string ClienteDescricao { get; set; }

        public PresencaHistorico Frete { get; set; } = PresencaHistorico.Todos;
        public PresencaHistorico Desconto { get; set; } = PresencaHistorico.Todos;

        /// <summary>Nº do orçamento, código do cliente, nome (sem acento) ou telefone.</summary>
        public string Texto { get; set; }

        public ColunaHistorico OrdenarPor { get; set; } = ColunaHistorico.Data;
        public bool Crescente { get; set; }

        public FiltroHistorico Copiar() => (FiltroHistorico)MemberwiseClone();

        /// <summary>Datas inicial e final (inclusive) do período; nulo = sem limite.</summary>
        public (DateTime? De, DateTime? Ate) Intervalo(DateTime hoje)
        {
            hoje = hoje.Date;
            var inicioDoMes = new DateTime(hoje.Year, hoje.Month, 1);

            switch (Periodo)
            {
                case PeriodoHistorico.Hoje:
                    return (hoje, hoje);
                case PeriodoHistorico.Ultimos7Dias:
                    return (hoje.AddDays(-6), hoje);
                case PeriodoHistorico.Ultimos30Dias:
                    return (hoje.AddDays(-29), hoje);
                case PeriodoHistorico.EsteMes:
                    return (inicioDoMes, inicioDoMes.AddMonths(1).AddDays(-1));
                case PeriodoHistorico.MesPassado:
                    return (inicioDoMes.AddMonths(-1), inicioDoMes.AddDays(-1));
                case PeriodoHistorico.Personalizado:
                    return (De?.Date, Ate?.Date);
                default:
                    return (null, null);
            }
        }
    }
}
