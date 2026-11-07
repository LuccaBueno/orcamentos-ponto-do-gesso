using System;
using System.IO;
using PontoDoGesso.Dados;

namespace PontoDoGesso.Testes
{
    /// <summary>Banco descartável numa pasta temporária com acento, como pode acontecer no perfil do usuário.</summary>
    public sealed class BancoTemporario : IDisposable
    {
        public string Pasta { get; } = Path.Combine(Path.GetTempPath(), "PontoDoGesso-banco-ção-" + Guid.NewGuid().ToString("N"));
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
