using System;

namespace PontoDoGesso.Dominio
{
    public class Cliente
    {
        public long Id { get; set; }
        public string Nome { get; set; }
        public string Telefone { get; set; }
        public string Telefone2 { get; set; }
        public string Endereco { get; set; }
        public string Numero { get; set; }
        public string Bairro { get; set; }
        public string Cidade { get; set; } = "Pindamonhangaba";
        public string PontoReferencia { get; set; }

        /// <summary>Nota interna: não sai no orçamento.</summary>
        public string Observacoes { get; set; }

        public bool Ativo { get; set; } = true;
        public DateTime CriadoEm { get; set; }
        public DateTime AtualizadoEm { get; set; }
    }
}
