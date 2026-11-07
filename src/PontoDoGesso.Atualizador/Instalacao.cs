using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace PontoDoGesso.Atualizador
{
    /// <summary>
    /// Troca a pasta do app pelo conteúdo do zip, guardando a versão atual em &lt;destino&gt;.anterior.
    /// Se qualquer passo falhar, a pasta do app volta a ser a de antes.
    /// </summary>
    public class Instalacao
    {
        public const string NomeApp = "PontoDoGesso.exe";

        private readonly string _zip;
        private readonly Action<string> _log;

        public Instalacao(string zip, string destino, Action<string> log)
        {
            _zip = zip;
            Destino = destino.TrimEnd('\\', '/');
            _log = log ?? (texto => { });
        }

        public string Destino { get; }
        public string Anterior => Destino + ".anterior";
        public string Nova => Destino + ".nova";
        public string ExecutavelApp => Path.Combine(Destino, NomeApp);

        /// <summary>Quantas vezes tenta mover uma pasta (o antivírus pode estar lendo um arquivo).</summary>
        public int Tentativas { get; set; } = 10;
        public TimeSpan Intervalo { get; set; } = TimeSpan.FromSeconds(1);

        public void Instalar()
        {
            if (!Directory.Exists(Destino))
                throw new DirectoryNotFoundException("A pasta do app não existe: " + Destino);

            // 1. Extrai ao lado da pasta do app (mesmo disco: mover pasta é instantâneo)
            ApagarPasta(Nova);
            _log("Extraindo " + _zip + " em " + Nova);
            ZipFile.ExtractToDirectory(_zip, Nova);

            var origem = Directory.Exists(Path.Combine(Nova, "PontoDoGesso")) ? Path.Combine(Nova, "PontoDoGesso") : Nova;
            if (!File.Exists(Path.Combine(origem, NomeApp)))
            {
                ApagarPasta(Nova);
                throw new InvalidDataException("O pacote não tem o " + NomeApp + ". Nada foi alterado.");
            }

            // 2. Guarda a versão atual (antes disso, nada mudou na pasta do app)
            ApagarPasta(Anterior);
            _log("Guardando a versão atual em " + Anterior);
            try
            {
                ComTentativas(() => Mover(Destino, Anterior));
            }
            catch (Exception)
            {
                ApagarPasta(Nova);
                throw;
            }

            // 3. Coloca a versão nova no lugar
            try
            {
                _log("Colocando a versão nova em " + Destino);
                ComTentativas(() => Mover(origem, Destino));
            }
            catch (Exception erro)
            {
                _log("Falhou ao colocar a versão nova: " + erro.Message);
                Desfazer();
                ApagarPasta(Nova);
                throw;
            }

            ApagarPasta(Nova);
            _log("Versão nova instalada");
        }

        /// <summary>Volta a versão guardada em &lt;destino&gt;.anterior.</summary>
        public void Desfazer()
        {
            if (!Directory.Exists(Anterior))
                throw new DirectoryNotFoundException("Não há versão anterior para voltar: " + Anterior);

            _log("Voltando a versão anterior de " + Anterior);
            ComTentativas(() =>
            {
                if (Directory.Exists(Destino))
                    Directory.Delete(Destino, true);
                Mover(Anterior, Destino);
            });
            _log("Versão anterior restaurada");
        }

        protected virtual void Mover(string de, string para) => Directory.Move(de, para);

        private void ComTentativas(Action acao)
        {
            for (var tentativa = 1; ; tentativa++)
            {
                try
                {
                    acao();
                    return;
                }
                catch (Exception erro) when ((erro is IOException || erro is UnauthorizedAccessException) && tentativa < Tentativas)
                {
                    _log("Tentativa " + tentativa + " falhou (" + erro.Message + "); tentando de novo");
                    Thread.Sleep(Intervalo);
                }
            }
        }

        private void ApagarPasta(string pasta)
        {
            try
            {
                if (Directory.Exists(pasta))
                    Directory.Delete(pasta, true);
            }
            catch (Exception erro) when (erro is IOException || erro is UnauthorizedAccessException)
            {
                _log("Não foi possível apagar " + pasta + ": " + erro.Message);
            }
        }
    }
}
