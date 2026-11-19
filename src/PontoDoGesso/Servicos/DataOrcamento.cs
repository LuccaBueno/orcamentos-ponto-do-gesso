using System;

namespace PontoDoGesso.Servicos
{
    /// <summary>
    /// Data do orçamento: o novo (ainda não salvo) acompanha o dia de hoje do Windows até alguém mudar a data à mão;
    /// o já salvo fica com a data dele para sempre (reabrir ou exportar de novo não muda nada).
    /// </summary>
    public static class DataOrcamento
    {
        /// <summary>Data que o orçamento deve ter agora.</summary>
        /// <param name="salvo">Já tem número (foi salvo alguma vez).</param>
        /// <param name="alteradaAMao">O usuário mudou a data na tela.</param>
        /// <param name="dataNaTela">Data mostrada no campo.</param>
        /// <param name="hoje">Dia de hoje no relógio do Windows.</param>
        public static DateTime Atual(bool salvo, bool alteradaAMao, DateTime dataNaTela, DateTime hoje) =>
            salvo || alteradaAMao ? dataNaTela.Date : hoje.Date;
    }
}
