using System.Windows.Forms;

namespace PontoDoGesso.Forms
{
    /// <summary>
    /// Mensagens das telas (avisos, confirmações, Salvar / Não salvar / Cancelar). A janela principal implementa; nos testes
    /// ela é trocada por uma que responde sozinha, para nenhuma caixa de mensagem travar o teste.
    /// </summary>
    public interface IDialogos
    {
        void Avisar(Control tela, string mensagem);

        void Informar(Control tela, string mensagem);

        bool Confirmar(Control tela, string pergunta, MessageBoxIcon icone, MessageBoxDefaultButton padrao);

        /// <summary>Yes (Salvar), No (Não salvar) ou Cancel.</summary>
        DialogResult PerguntarSalvar(Control tela, string pergunta);
    }
}
