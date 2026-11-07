using System;
using System.Data;
using System.Globalization;
using Dapper;

namespace PontoDoGesso.Dados
{
    /// <summary>Quantidades ficam no banco como TEXT invariante, independente da cultura do processo.</summary>
    internal static class ConversoresDapper
    {
        public static void Registrar()
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;
            SqlMapper.AddTypeHandler(new ConversorDecimal());
        }

        public static string Texto(decimal valor) => valor.ToString(CultureInfo.InvariantCulture);

        private class ConversorDecimal : SqlMapper.TypeHandler<decimal>
        {
            public override decimal Parse(object valor) =>
                decimal.Parse(Convert.ToString(valor, CultureInfo.InvariantCulture), NumberStyles.Number, CultureInfo.InvariantCulture);

            // O Dapper não passa por aqui ao gravar decimal; os repositórios enviam o texto pronto
            public override void SetValue(IDbDataParameter parametro, decimal valor) =>
                parametro.Value = Texto(valor);
        }
    }
}
