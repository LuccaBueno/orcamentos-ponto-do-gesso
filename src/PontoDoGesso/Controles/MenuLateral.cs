using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PontoDoGesso.Controles
{
    /// <summary>
    /// Menu fixo à esquerda da janela principal: botões grandes com ícone; o da tela atual fica destacado em laranja.
    /// Os de cima trocam de tela, os de baixo são ações (abrir pasta, verificar atualizações).
    /// </summary>
    public class MenuLateral : Panel
    {
        public static readonly Color Laranja = Color.FromArgb(0xC5, 0x5A, 0x11);
        public static readonly Color Fundo = Color.FromArgb(0xF4, 0xF1, 0xEE);
        private static readonly Color FundoPassando = Color.FromArgb(0xFB, 0xE4, 0xD5);
        private static readonly Color Texto = Color.FromArgb(0x33, 0x33, 0x33);
        private const int TamanhoIcone = 24;

        private readonly FlowLayoutPanel _cima;
        private readonly FlowLayoutPanel _baixo;
        private readonly Dictionary<Button, string> _icones = new Dictionary<Button, string>();
        private Button _destacado;

        public MenuLateral()
        {
            Name = "menuLateral";
            Dock = DockStyle.Left;
            Width = 210;
            BackColor = Fundo;
            Padding = new Padding(0, 0, 0, 6);

            _cima = Coluna(DockStyle.Fill);
            _cima.AutoScroll = true;
            _baixo = Coluna(DockStyle.Bottom);
            _baixo.AutoSize = true;

            Controls.Add(_cima);
            Controls.Add(_baixo);
        }

        private static FlowLayoutPanel Coluna(DockStyle dock) => new FlowLayoutPanel
        {
            Dock = dock,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Margin = new Padding(0),
            Padding = new Padding(6, 4, 6, 0)
        };

        /// <summary>Título no alto do menu (nome da loja).</summary>
        public void AdicionarTitulo(Control titulo) => _cima.Controls.Add(titulo);

        public Button Adicionar(string nome, string texto, string icone, Action aoClicar, bool embaixo = false)
        {
            var botao = new Button
            {
                Name = nome,
                Text = "  " + texto,
                Font = new Font("Segoe UI", embaixo ? 9.75F : 11F),
                ForeColor = Texto,
                BackColor = Fundo,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                ImageAlign = ContentAlignment.MiddleLeft,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Size = new Size(Width - 12, embaixo ? 38 : 46),
                Margin = new Padding(0, 0, 0, 3),
                Padding = new Padding(8, 0, 4, 0),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            botao.FlatAppearance.BorderSize = 0;
            botao.FlatAppearance.MouseOverBackColor = FundoPassando;
            botao.FlatAppearance.MouseDownBackColor = FundoPassando;
            botao.Click += (s, e) => aoClicar();

            _icones[botao] = icone;
            Pintar(botao, false);
            (embaixo ? _baixo : _cima).Controls.Add(botao);
            return botao;
        }

        public void AdicionarEmbaixo(Control controle) => _baixo.Controls.Add(controle);

        /// <summary>Botão destacado agora (tela atual); nulo = nenhum.</summary>
        public Button Destacado => _destacado;

        public void Destacar(string nome)
        {
            if (_destacado != null)
                Pintar(_destacado, false);

            _destacado = null;
            foreach (var botao in _icones.Keys)
                if (botao.Name == nome)
                    _destacado = botao;

            if (_destacado != null)
                Pintar(_destacado, true);
        }

        private void Pintar(Button botao, bool destacado)
        {
            botao.BackColor = destacado ? Laranja : Fundo;
            botao.ForeColor = destacado ? Color.White : Texto;
            botao.FlatAppearance.MouseOverBackColor = destacado ? Laranja : FundoPassando;
            botao.Font = new Font(botao.Font, destacado ? FontStyle.Bold : FontStyle.Regular);

            var anterior = botao.Image;
            botao.Image = Icones.Desenhar(_icones[botao], TamanhoIcone, destacado ? Color.White : Laranja);
            anterior?.Dispose();
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            foreach (var botao in _icones.Keys)
                botao.Width = Width - 12;
        }
    }
}
