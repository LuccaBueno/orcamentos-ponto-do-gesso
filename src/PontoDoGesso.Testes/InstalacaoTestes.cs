using System;
using System.IO;
using System.IO.Compression;
using PontoDoGesso.Atualizador;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class InstalacaoTestes : IDisposable
    {
        private readonly string _pasta = PastaTestes.Nova("PontoDoGesso-instalação-");
        private readonly string _destino;

        public InstalacaoTestes()
        {
            _destino = Path.Combine(_pasta, "PontoDoGesso");
            Directory.CreateDirectory(Path.Combine(_destino, "x86"));
            File.WriteAllText(Path.Combine(_destino, "PontoDoGesso.exe"), "versão 0.9.0");
            File.WriteAllText(Path.Combine(_destino, "x86", "SQLite.Interop.dll"), "interop 0.9.0");
            File.WriteAllText(Path.Combine(_destino, "so-na-antiga.dll"), "removida na 0.9.1");
        }

        public void Dispose()
        {
            if (Directory.Exists(_pasta))
                Directory.Delete(_pasta, true);
        }

        /// <summary>Zip no formato do empacotar.ps1: tudo dentro de PontoDoGesso/.</summary>
        private string CriarZip(bool comExecutavel = true)
        {
            var zip = Path.Combine(_pasta, "PontoDoGesso-v0.9.1.zip");
            using (var arquivo = ZipFile.Open(zip, ZipArchiveMode.Create))
            {
                if (comExecutavel)
                    Escrever(arquivo, "PontoDoGesso/PontoDoGesso.exe", "versão 0.9.1");
                Escrever(arquivo, "PontoDoGesso/x86/SQLite.Interop.dll", "interop 0.9.1");
                Escrever(arquivo, "PontoDoGesso/assets/icone.png", "ícone");
            }
            return zip;
        }

        private static void Escrever(ZipArchive arquivo, string nome, string conteudo)
        {
            using (var escrita = new StreamWriter(arquivo.CreateEntry(nome).Open()))
                escrita.Write(conteudo);
        }

        private string Ler(params string[] partes) => File.ReadAllText(Path.Combine(partes));

        private Instalacao Nova(string zip) =>
            new Instalacao(zip, _destino + "\\", null) { Tentativas = 2, Intervalo = TimeSpan.FromMilliseconds(10) };

        [Fact]
        public void TrocaAPastaDoAppEGuardaAAnterior()
        {
            var instalacao = Nova(CriarZip());

            instalacao.Instalar();

            Assert.Equal("versão 0.9.1", Ler(_destino, "PontoDoGesso.exe"));
            Assert.Equal("interop 0.9.1", Ler(_destino, "x86", "SQLite.Interop.dll"));
            Assert.Equal("ícone", Ler(_destino, "assets", "icone.png"));
            Assert.False(File.Exists(Path.Combine(_destino, "so-na-antiga.dll")));

            Assert.Equal(_destino + ".anterior", instalacao.Anterior);
            Assert.Equal("versão 0.9.0", Ler(instalacao.Anterior, "PontoDoGesso.exe"));
            Assert.False(Directory.Exists(instalacao.Nova));
        }

        [Fact]
        public void DesfazerVoltaAVersaoAnterior()
        {
            var instalacao = Nova(CriarZip());
            instalacao.Instalar();

            instalacao.Desfazer();

            Assert.Equal("versão 0.9.0", Ler(_destino, "PontoDoGesso.exe"));
            Assert.Equal("removida na 0.9.1", Ler(_destino, "so-na-antiga.dll"));
            Assert.False(Directory.Exists(instalacao.Anterior));
        }

        [Fact]
        public void AnteriorDeUmaAtualizacaoPassadaEhSubstituida()
        {
            Directory.CreateDirectory(_destino + ".anterior");
            File.WriteAllText(Path.Combine(_destino + ".anterior", "PontoDoGesso.exe"), "versão 0.8.0");

            Nova(CriarZip()).Instalar();

            Assert.Equal("versão 0.9.0", Ler(_destino + ".anterior", "PontoDoGesso.exe"));
        }

        [Fact]
        public void PacoteSemOExecutavelNaoMudaNada()
        {
            var instalacao = Nova(CriarZip(comExecutavel: false));

            Assert.Throws<InvalidDataException>(() => instalacao.Instalar());

            Assert.Equal("versão 0.9.0", Ler(_destino, "PontoDoGesso.exe"));
            Assert.False(Directory.Exists(instalacao.Anterior));
            Assert.False(Directory.Exists(instalacao.Nova));
        }

        [Fact]
        public void ZipDanificadoNaoMudaNada()
        {
            var zip = Path.Combine(_pasta, "danificado.zip");
            File.WriteAllText(zip, "isto não é um zip");
            var instalacao = Nova(zip);

            Assert.ThrowsAny<Exception>(() => instalacao.Instalar());

            Assert.Equal("versão 0.9.0", Ler(_destino, "PontoDoGesso.exe"));
            Assert.False(Directory.Exists(instalacao.Anterior));
        }

        [Fact]
        public void ArquivoPresoNaPastaDoAppNaoMudaNada()
        {
            var instalacao = Nova(CriarZip());

            // Arquivo aberto sem permitir exclusão: o Windows não deixa renomear a pasta
            using (new FileStream(Path.Combine(_destino, "PontoDoGesso.exe"), FileMode.Open, FileAccess.Read, FileShare.Read))
                Assert.ThrowsAny<IOException>(() => instalacao.Instalar());

            Assert.Equal("versão 0.9.0", Ler(_destino, "PontoDoGesso.exe"));
            Assert.False(Directory.Exists(instalacao.Anterior));
            Assert.False(Directory.Exists(instalacao.Nova));
        }

        [Fact]
        public void FalhaAoColocarAVersaoNovaVoltaAAnterior()
        {
            var instalacao = new FalhaNaSegundaMudanca(CriarZip(), _destino);

            Assert.Throws<IOException>(() => instalacao.Instalar());

            Assert.Equal("versão 0.9.0", Ler(_destino, "PontoDoGesso.exe"));
            Assert.Equal("removida na 0.9.1", Ler(_destino, "so-na-antiga.dll"));
            Assert.False(Directory.Exists(instalacao.Anterior));
            Assert.False(Directory.Exists(instalacao.Nova));
        }

        [Fact]
        public void SemPastaDoAppNaoInstala()
        {
            Directory.Delete(_destino, true);
            Assert.Throws<DirectoryNotFoundException>(() => Nova(CriarZip()).Instalar());
        }

        /// <summary>Simula o antivírus segurando um arquivo da versão nova bem na hora de mover.</summary>
        private class FalhaNaSegundaMudanca : Instalacao
        {
            private int _mudancas;

            public FalhaNaSegundaMudanca(string zip, string destino) : base(zip, destino, null)
            {
                Tentativas = 2;
                Intervalo = TimeSpan.FromMilliseconds(10);
            }

            protected override void Mover(string de, string para)
            {
                if (++_mudancas == 2 || _mudancas == 3)
                    throw new IOException("O processo não pode acessar o arquivo porque ele está sendo usado por outro processo.");
                base.Mover(de, para);
            }
        }
    }
}
