using System;
using System.IO;
using PontoDoGesso.Servicos;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class AutotesteTestes : IDisposable
    {
        private readonly string _pasta = PastaTestes.Nova("PontoDoGesso-autoteste-");

        public void Dispose()
        {
            if (Directory.Exists(_pasta))
                Directory.Delete(_pasta, true);
        }

        [Fact]
        public void ConfereSqliteBancoEPdfEGravaOResultado()
        {
            var resultado = Path.Combine(_pasta, "saida", "resultado.txt");

            var codigo = Autoteste.Executar(resultado, Path.Combine(_pasta, "dados"));

            var texto = File.ReadAllText(resultado);
            Assert.True(codigo == 0, texto);
            Assert.Contains("SQLite: OK", texto);
            Assert.Contains("Banco do app: OK", texto);
            Assert.Contains("PDF e PNG: OK", texto);
            Assert.Contains("System.Data.SQLite carregado de: ", texto);
            Assert.EndsWith("RESULTADO: OK", texto.TrimEnd());
            Assert.Empty(Directory.GetFiles(Path.Combine(_pasta, "dados"), "*.db"));
        }
    }
}
