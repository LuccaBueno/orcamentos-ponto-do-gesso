using System;

namespace PontoDoGesso.Servicos
{
    /// <summary>Endereços do projeto no GitHub: repositório e manual do usuário.</summary>
    public static class Ajuda
    {
        public const string Repositorio = "https://github.com/LuccaBueno/orcamentos-ponto-do-gesso";
        public const string Manual = Repositorio + "/blob/main/docs/MANUAL.md";

        /// <summary>Abre o link no navegador padrão; trocado nos testes para não abrir nada.</summary>
        public static Action<string> AbrirLink { get; set; } = Sistema.AbrirLink;
    }
}
