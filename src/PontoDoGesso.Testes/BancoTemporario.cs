using System;
using System.IO;
using PontoDoGesso.Dados;

namespace PontoDoGesso.Testes
{
    /// <summary>Banco descartável em .tmp, numa pasta com acento, como pode acontecer no perfil do usuário.</summary>
    public sealed class BancoTemporario : IDisposable
    {
        public string Pasta { get; } = PastaTestes.Nova("PontoDoGesso-banco-ção-");
        public Banco Banco { get; }

        public BancoTemporario()
        {
            Banco = new Banco(Path.Combine(Pasta, "dados.db"), Path.Combine(Pasta, "backups"));
        }

        public void Dispose()
        {
            if (Directory.Exists(Pasta))
                Directory.Delete(Pasta, true);
        }
    }
}
