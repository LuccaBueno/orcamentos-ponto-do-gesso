using System;
using System.Drawing;
using System.Windows.Forms;

namespace PontoDoGesso.Forms
{
    /// <summary>
    /// Tela que aparece na área à direita do menu lateral da janela principal (Orçamento, Histórico, Clientes...).
    /// O título fica em <see cref="Control.Text"/>. Diálogos pequenos (confirmar, editar um cliente) continuam sendo janelas modais.
    /// </summary>
    public class TelaBase : UserControl
    {
        private TextBox _busca;

        public TelaBase()
        {
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96F, 96F);
            Font = new Font("Segoe UI", 9.5F);
            BackColor = Color.White;
            Dock = DockStyle.Fill;
            Padding = new Padding(4, 4, 4, 0);
        }

        /// <summary>Quem troca de tela; definida pela janela principal (nula nos testes que mostram a tela sozinha).</summary>
        public INavegacao Navegacao { get; set; }

        /// <summary>Quem mostra as mensagens; definida pela janela principal. Sem ela (tela sozinha), caixa de mensagem do Windows.</summary>
        public IDialogos Dialogos { get; set; }

        /// <summary>Destino do menu lateral que fica destacado enquanto a tela aparece.</summary>
        public virtual Destino Destino => Destino.Inicio;

        /// <summary>Menor tamanho em que nada fica cortado; abaixo disso a área da tela ganha barras de rolagem.</summary>
        public virtual Size TamanhoMinimo => new Size(600, 420);

        /// <summary>
        /// Chamado antes de trocar de tela ou de fechar o app. Com alterações não salvas, pergunta Salvar / Não salvar / Cancelar;
        /// devolve falso para ficar na tela (Cancelar, ou o salvamento não deu certo).
        /// </summary>
        public virtual bool PodeSair() => true;

        /// <summary>Cada vez que a tela aparece ou a janela volta a ficar ativa (ex.: depois de outro programa).</summary>
        public virtual void AoAtivar()
        {
        }

        /// <summary>Controle que recebe o cursor quando a tela aparece.</summary>
        public Control ControleInicial { get; protected set; }

        /// <summary>
        /// Campo de uma linha criado estreito (antes do layout) e alargado depois continua com o texto rolado para a esquerda e
        /// parece vazio. Chamado pela janela principal depois de mostrar a tela.
        /// </summary>
        public void MostrarInicioDosCampos() => MostrarInicioDosCampos(this);

        private static void MostrarInicioDosCampos(Control raiz)
        {
            foreach (Control filho in raiz.Controls)
            {
                if (filho is TextBoxBase campo && !campo.Multiline && campo.IsHandleCreated)
                {
                    var inicio = campo.SelectionStart;
                    var tamanho = campo.SelectionLength;
                    campo.Select(0, 0);
                    campo.Select(inicio, tamanho);
                }

                MostrarInicioDosCampos(filho);
            }
        }

        /// <summary>Ctrl+F leva o cursor para o campo de busca da tela.</summary>
        protected void LigarBusca(TextBox busca) => _busca = busca;

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.F) && _busca != null && _busca.Enabled)
            {
                _busca.Focus();
                _busca.SelectAll();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>Enter avança para o próximo campo (o campo de busca mantém o comportamento da tela).</summary>
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (!(_busca != null && _busca.Focused) && EnterAvanca.Tratar(this, keyData))
                return true;

            return base.ProcessDialogKey(keyData);
        }

        /// <summary>Pergunta usada pelo <see cref="PodeSair"/> de cada tela; <paramref name="salvar"/> devolve falso se não conseguiu.</summary>
        protected bool PerguntarAntesDeSair(bool alterado, string pergunta, Func<bool> salvar)
        {
            if (!alterado)
                return true;

            var resposta = PerguntarSalvar(pergunta);
            return resposta == DialogResult.No || (resposta == DialogResult.Yes && salvar());
        }

        /// <summary>Os diálogos da tela ou, numa tela embutida em outra (Empresa dentro de Configurações), os da tela de fora.</summary>
        private IDialogos DialogosEfetivos
        {
            get
            {
                for (Control controle = this; controle != null; controle = controle.Parent)
                    if (controle is TelaBase tela && tela.Dialogos != null)
                        return tela.Dialogos;
                return null;
            }
        }

        protected virtual DialogResult PerguntarSalvar(string pergunta) =>
            DialogosEfetivos?.PerguntarSalvar(this, pergunta) ?? PerguntaSalvar.Perguntar(this, pergunta, Text);

        protected virtual void Avisar(string mensagem)
        {
            var dialogos = DialogosEfetivos;
            if (dialogos != null)
                dialogos.Avisar(this, mensagem);
            else
                MessageBox.Show(this, mensagem, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        protected virtual void Informar(string mensagem)
        {
            var dialogos = DialogosEfetivos;
            if (dialogos != null)
                dialogos.Informar(this, mensagem);
            else
                MessageBox.Show(this, mensagem, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>Sim/Não; por padrão o botão Não vem marcado.</summary>
        protected virtual bool Confirmar(string pergunta) => Confirmar(pergunta, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

        protected bool Confirmar(string pergunta, MessageBoxIcon icone, MessageBoxDefaultButton padrao) =>
            DialogosEfetivos?.Confirmar(this, pergunta, icone, padrao) ?? MessageBox.Show(this, pergunta, Text, MessageBoxButtons.YesNo, icone, padrao) == DialogResult.Yes;
    }
}
