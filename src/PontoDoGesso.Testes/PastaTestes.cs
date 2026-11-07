using System;
using System.IO;

namespace PontoDoGesso.Testes
{
    /// <summary>Tudo que os testes gravam fica em .tmp, na raiz do repositório (ignorada pelo git).</summary>
    public static class PastaTestes
    {
        public static readonly string Raiz = Path.Combine(AcharRepositorio(), ".tmp", "testes");

        public static string Nova(string prefixo)
        {
            Directory.CreateDirectory(Raiz);
            return Path.Combine(Raiz, prefixo + Guid.NewGuid().ToString("N"));
        }

        private static string AcharRepositorio()
        {
            var pasta = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            while (!File.Exists(Path.Combine(pasta.FullName, "PontoDoGesso.sln")))
                pasta = pasta.Parent ?? throw new InvalidOperationException("Raiz do repositório não encontrada.");

            return pasta.FullName;
        }
    }
}
