using System;

namespace PontoDoGesso.Dominio
{
    /// <summary>Endereço de entrega já usado num orçamento anterior do cliente (vem do histórico, sem cadastro à parte).</summary>
    public class EnderecoEntrega
    {
        public string Endereco { get; set; }
        public string Numero { get; set; }
        public string Bairro { get; set; }
        public string Cidade { get; set; }
        public string Referencia { get; set; }
        public string ReceberCom { get; set; }
        public string TelefoneLocal { get; set; }

        /// <summary>Data do orçamento mais recente com este endereço.</summary>
        public DateTime UltimoUso { get; set; }

        /// <summary>Uma linha para a lista: "Rua X, 123 – Bairro – Cidade (último uso 07/11/2026)".</summary>
        public string Descricao
        {
            get
            {
                var texto = Endereco + (string.IsNullOrEmpty(Numero) ? "" : ", " + Numero);
                if (!string.IsNullOrEmpty(Bairro))
                    texto += " – " + Bairro;
                if (!string.IsNullOrEmpty(Cidade))
                    texto += " – " + Cidade;
                return texto + " (último uso " + UltimoUso.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture) + ")";
            }
        }
    }
}
