using System;

namespace PontoDoGesso.Dominio
{
    /// <summary>Regra do orçamento não atendida; a mensagem é para mostrar ao usuário.</summary>
    public class OrcamentoInvalidoException : Exception
    {
        public OrcamentoInvalidoException(string mensagem) : base(mensagem)
        {
        }
    }
}
