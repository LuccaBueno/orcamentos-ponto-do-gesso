using System;
using System.Data.SQLite;

namespace PontoDoGesso.Dados
{
    /// <summary>Função de texto do app registrada no SQLite; valores nulos continuam nulos.</summary>
    internal class FuncaoTexto : SQLiteFunction
    {
        private readonly Func<string, string> _funcao;

        private FuncaoTexto(Func<string, string> funcao)
        {
            _funcao = funcao;
        }

        public static void Registrar(SQLiteConnection conexao, string nome, Func<string, string> funcao) =>
            conexao.BindFunction(new SQLiteFunctionAttribute(nome, 1, FunctionType.Scalar), new FuncaoTexto(funcao));

        public override object Invoke(object[] argumentos) =>
            argumentos[0] is string texto ? _funcao(texto) : argumentos[0];
    }
}
