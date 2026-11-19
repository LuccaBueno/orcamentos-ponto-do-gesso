using System.Linq;
using System.Windows.Forms;
using PontoDoGesso.Controles;

namespace PontoDoGesso.Forms
{
    /// <summary>
    /// Enter nos campos de formulário vai para o próximo campo, como o Tab. Num diálogo, Enter no último campo aciona o Salvar.
    /// Ficam de fora: campo de várias linhas (Enter quebra linha), barra "Adicionar item", grade e campos com lista de sugestões.
    /// </summary>
    internal static class EnterAvanca
    {
        /// <summary>Chamado no ProcessDialogKey da tela ou do diálogo; devolve verdadeiro se usou a tecla.</summary>
        public static bool Tratar(ContainerControl dono, Keys tecla, IButtonControl salvar = null)
        {
            if (tecla != Keys.Enter)
                return false;

            var campo = Campo(dono);

            if (campo == null)
                return false;

            var proximo = salvar == null ? null : Proximo(dono, campo);

            if (salvar != null && (proximo == null || proximo is IButtonControl))
            {
                salvar.PerformClick();
                return true;
            }

            dono.SelectNextControl(campo, true, true, true, true);
            return true;
        }

        private static Control Campo(ContainerControl dono)
        {
            Control ativo = dono.ActiveControl;

            while (ativo is ContainerControl recipiente && !Folha(ativo) && recipiente.ActiveControl != null)
                ativo = recipiente.ActiveControl;

            if (ativo == null)
                return null;

            for (var pai = ativo; pai != null && pai != dono; pai = pai.Parent)
                if (pai is BarraItem || pai is DataGridView)
                    return null;

            switch (ativo)
            {
                case TextBox texto when texto.Multiline && texto.AcceptsReturn:
                    return null;
                case TextBox texto when TemSugestoes(dono, texto):
                    return null;
                case ComboBox combo when combo.DroppedDown:
                    return null;
                case TextBoxBase _:
                case ComboBox _:
                case CheckBox _:
                case RadioButton _:
                case DateTimePicker _:
                    return ativo;
                default:
                    return Folha(ativo) ? ativo : null;
            }
        }

        /// <summary>Controles compostos tratados como um campo só.</summary>
        private static bool Folha(Control controle) =>
            controle is UpDownBase;

        private static bool TemSugestoes(Control dono, TextBox campo)
        {
            var raiz = dono.TopLevelControl ?? dono;
            return Descendentes(raiz).OfType<ListaSugestoes>().Any(lista => lista.Campo == campo);
        }

        private static System.Collections.Generic.IEnumerable<Control> Descendentes(Control raiz)
        {
            foreach (Control filho in raiz.Controls)
            {
                yield return filho;

                foreach (var neto in Descendentes(filho))
                    yield return neto;
            }
        }

        /// <summary>Próximo controle na ordem do Tab, sem voltar ao começo; nulo = era o último.</summary>
        private static Control Proximo(Control dono, Control campo)
        {
            for (var controle = dono.GetNextControl(campo, true); controle != null; controle = dono.GetNextControl(controle, true))
                if (!campo.Contains(controle) && controle.CanSelect && controle.TabStop)
                    return controle;

            return null;
        }
    }
}
