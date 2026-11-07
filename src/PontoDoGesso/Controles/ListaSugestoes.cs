using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PontoDoGesso.Controles
{
    /// <summary>
    /// Autocomplete que procura em qualquer parte do texto (código, nome, telefone): uma lista que aparece
    /// embaixo do campo enquanto se digita. ↑/↓ escolhem, Enter confirma, Esc fecha mantendo o que foi digitado.
    /// </summary>
    public class ListaSugestoes : ListBox
    {
        private const int MaximoDeLinhas = 8;

        private readonly Form _janela;
        private readonly Func<string, IEnumerable<object>> _buscar;
        private readonly Func<object, string> _exibir;
        private TextBox _campo;
        private bool _ignorarTexto;

        public event EventHandler<SugestaoEscolhidaEventArgs> Escolhido;

        public ListaSugestoes(Form janela, Func<string, IEnumerable<object>> buscar, Func<object, string> exibir)
        {
            _janela = janela;
            _buscar = buscar;
            _exibir = exibir;

            Visible = false;
            IntegralHeight = true;
            TabStop = false;
            BorderStyle = BorderStyle.FixedSingle;
            janela.Controls.Add(this);

            MouseClick += (s, e) => Escolher();
        }

        public TextBox Campo => _campo;

        /// <summary>Passa a acompanhar o campo (a grade reaproveita a mesma caixa de texto em várias colunas).</summary>
        public void Ligar(TextBox campo)
        {
            Desligar();
            _campo = campo;
            _campo.TextChanged += AoMudarTexto;
            _campo.KeyDown += AoTeclar;
            _campo.Leave += AoSairDoCampo;
        }

        public void Desligar()
        {
            Esconder();

            if (_campo == null)
                return;

            _campo.TextChanged -= AoMudarTexto;
            _campo.KeyDown -= AoTeclar;
            _campo.Leave -= AoSairDoCampo;
            _campo = null;
        }

        /// <summary>Troca o texto do campo sem abrir a lista.</summary>
        public void DefinirTextoSemBuscar(string texto)
        {
            _ignorarTexto = true;
            try
            {
                _campo.Text = texto;
                _campo.SelectionStart = _campo.Text.Length;
            }
            finally
            {
                _ignorarTexto = false;
            }
        }

        public void Esconder() => Visible = false;

        /// <summary>Procura e mostra a lista com o primeiro item já marcado; texto vazio ou sem resultado esconde.</summary>
        public void Atualizar()
        {
            if (_campo == null)
                return;

            var texto = _campo.Text.Trim();
            var itens = texto.Length == 0 ? new List<Sugestao>() : _buscar(texto).Select(item => new Sugestao(item, _exibir(item))).ToList();

            BeginUpdate();
            Items.Clear();
            Items.AddRange(itens.Cast<object>().ToArray());
            EndUpdate();

            if (itens.Count == 0)
            {
                Esconder();
                return;
            }

            SelectedIndex = 0;
            Posicionar(itens.Count);
            Visible = true;
            BringToFront();
        }

        /// <summary>Trata ↑, ↓, Enter e Esc enquanto a lista está aberta; devolve verdadeiro se usou a tecla.</summary>
        public bool Teclar(Keys tecla)
        {
            if (!Visible || Items.Count == 0)
                return false;

            switch (tecla)
            {
                case Keys.Down:
                    SelectedIndex = Math.Min(SelectedIndex + 1, Items.Count - 1);
                    return true;
                case Keys.Up:
                    SelectedIndex = Math.Max(SelectedIndex - 1, 0);
                    return true;
                case Keys.Enter:
                    Escolher();
                    return true;
                case Keys.Escape:
                    Esconder();
                    return true;
                default:
                    return false;
            }
        }

        public void Escolher()
        {
            if (!(SelectedItem is Sugestao sugestao))
                return;

            Esconder();
            Escolhido?.Invoke(this, new SugestaoEscolhidaEventArgs(sugestao.Valor));
        }

        private void AoMudarTexto(object sender, EventArgs e)
        {
            if (!_ignorarTexto)
                Atualizar();
        }

        private void AoTeclar(object sender, KeyEventArgs e)
        {
            if (Teclar(e.KeyData))
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void AoSairDoCampo(object sender, EventArgs e)
        {
            // O clique na lista tira o foco do campo antes do MouseClick: só esconde se o foco foi para outro lugar
            if (!Focused)
                Esconder();
        }

        private void Posicionar(int quantidade)
        {
            var embaixoDoCampo = _janela.PointToClient(_campo.PointToScreen(new Point(0, _campo.Height)));
            var largura = Math.Max(_campo.Width, 420);
            var altura = ItemHeight * Math.Min(quantidade, MaximoDeLinhas) + 4;

            // Se não couber embaixo, abre para cima
            if (embaixoDoCampo.Y + altura > _janela.ClientSize.Height)
                embaixoDoCampo.Y = Math.Max(0, embaixoDoCampo.Y - _campo.Height - altura);

            if (embaixoDoCampo.X + largura > _janela.ClientSize.Width)
                embaixoDoCampo.X = Math.Max(0, _janela.ClientSize.Width - largura);

            Bounds = new Rectangle(embaixoDoCampo, new Size(largura, altura));
        }

        private class Sugestao
        {
            public object Valor { get; }
            private readonly string _texto;

            public Sugestao(object valor, string texto)
            {
                Valor = valor;
                _texto = texto;
            }

            public override string ToString() => _texto;
        }
    }

    public class SugestaoEscolhidaEventArgs : EventArgs
    {
        public object Valor { get; }

        public SugestaoEscolhidaEventArgs(object valor)
        {
            Valor = valor;
        }
    }
}
