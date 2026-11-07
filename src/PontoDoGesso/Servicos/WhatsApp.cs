namespace PontoDoGesso.Servicos
{
    /// <summary>Link para abrir a conversa no WhatsApp Web; nada é enviado automaticamente.</summary>
    public static class WhatsApp
    {
        /// <summary>Telefone gravado (DDD + número, só dígitos) → https://wa.me/55DDDNUMERO.</summary>
        public static bool TentarMontarLink(string telefone, out string link, out string erro)
        {
            link = null;
            erro = null;
            var digitos = Formatacao.SomenteDigitos(telefone);

            if (digitos.Length == 0)
            {
                erro = "Este orçamento não tem telefone.";
                return false;
            }

            if ((digitos.Length != 10 && digitos.Length != 11) || !Telefone.DddValido(digitos.Substring(0, 2)) || !Telefone.NumeroValido(digitos.Substring(2)))
            {
                erro = "O telefone " + Formatacao.Telefone(telefone) + " não está completo (DDD + 8 ou 9 dígitos). Corrija o telefone no orçamento.";
                return false;
            }

            link = "https://wa.me/55" + digitos;
            return true;
        }

        public static string Dica(string telefone) =>
            TentarMontarLink(telefone, out _, out var erro) ? "Abrir conversa com " + Formatacao.Telefone(telefone) : erro;
    }
}
