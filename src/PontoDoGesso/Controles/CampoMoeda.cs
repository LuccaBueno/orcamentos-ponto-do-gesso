using System;
using System.Windows.Forms;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Controles
{
    /// <summary>Campo de valor em reais: aceita 12,5, 12,50 ou 1.250,00 e formata ao sair.</summary>
    public class CampoMoeda : TextBox
    {
        public CampoMoeda()
        {
            TextAlign = HorizontalAlignment.Right;
            Width = 100;
        }

        public bool Vazio => Text.Trim().Length == 0;

        /// <summary>Vazio também é válido (quem usa decide se vazio é zero).</summary>
        public bool Valido => Vazio || Formatacao.TentarLerCentavos(Text, out _);

        /// <summary>Nulo quando vazio ou inválido.</summary>
        public long? Centavos
        {
            get => !Vazio && Formatacao.TentarLerCentavos(Text, out var centavos) ? centavos : (long?)null;
            set => Text = value == null ? "" : Formatacao.Valor(value.Value);
        }

        /// <summary>Reescreve o valor no formato 1.250,00; texto inválido fica como está para ser corrigido.</summary>
        public void Formatar()
        {
            var centavos = Centavos;
            if (centavos != null)
                Text = Formatacao.Valor(centavos.Value);
        }

        protected override void OnLeave(EventArgs e)
        {
            Formatar();
            base.OnLeave(e);
        }
    }
}
