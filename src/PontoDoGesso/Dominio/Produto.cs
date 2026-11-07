using System;

namespace PontoDoGesso.Dominio
{
    public class Produto
    {
        public long Id { get; set; }
        public string Nome { get; set; }
        public string Unidade { get; set; }
        public long PrecoCentavos { get; set; }

        /// <summary>Última vez que o preço mudou (no cadastro, na grade ou no reajuste).</summary>
        public DateTime PrecoAtualizadoEm { get; set; }

        public bool Ativo { get; set; } = true;
        public DateTime CriadoEm { get; set; }
        public DateTime AtualizadoEm { get; set; }
    }
}
