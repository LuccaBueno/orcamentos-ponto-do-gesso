using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PontoDoGesso.Dados
{
    /// <summary>Uma alteração do esquema; a versão vem do início do nome do arquivo (001_inicial.sql).</summary>
    public class Migracao
    {
        private const string PrefixoRecurso = "Migracoes.";

        public int Versao { get; }
        public string Sql { get; }

        public Migracao(int versao, string sql)
        {
            Versao = versao;
            Sql = sql;
        }

        /// <summary>Migrações embutidas no executável, em ordem de versão.</summary>
        public static IReadOnlyList<Migracao> Embutidas()
        {
            var assembly = typeof(Migracao).Assembly;

            return assembly.GetManifestResourceNames()
                .Where(nome => nome.StartsWith(PrefixoRecurso) && nome.EndsWith(".sql"))
                .Select(nome => new Migracao(VersaoDoRecurso(nome), LerRecurso(assembly, nome)))
                .OrderBy(migracao => migracao.Versao)
                .ToList();
        }

        private static int VersaoDoRecurso(string nome) =>
            int.Parse(nome.Substring(PrefixoRecurso.Length, 3));

        private static string LerRecurso(Assembly assembly, string nome)
        {
            using (var fluxo = assembly.GetManifestResourceStream(nome))
            using (var leitor = new StreamReader(fluxo, Encoding.UTF8))
                return leitor.ReadToEnd();
        }
    }
}
