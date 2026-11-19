using System;
using System.Collections.Generic;
using System.Windows.Forms;
using PontoDoGesso.Dados;
using PontoDoGesso.Forms;

namespace PontoDoGesso.Testes
{
    /// <summary>Janela principal que não abre nenhuma janela de verdade: diálogos, pastas e mensagens ficam anotados.</summary>
    public class PrincipalDeTeste : FormPrincipal
    {
        public List<Form> Abertas { get; } = new List<Form>();
        public List<string> Pastas { get; } = new List<string>();
        public List<string> Avisos { get; } = new List<string>();
        public List<string> Informacoes { get; } = new List<string>();
        public List<string> Perguntas { get; } = new List<string>();
        public DialogResult RespostaSalvar { get; set; } = DialogResult.No;
        public bool RespostaConfirmar { get; set; }
        public int Manuais { get; private set; }

        public PrincipalDeTeste(Banco banco) : base(banco)
        {
        }

        public bool Teclar(Keys tecla)
        {
            var mensagem = new Message();
            return ProcessCmdKey(ref mensagem, tecla);
        }

        protected override void AbrirPasta(string pasta) => Pastas.Add(pasta);

        protected override void AbrirManual() => Manuais++;

        protected override void AbrirDialogo(Func<Form> abrir)
        {
            var form = abrir();
            Abertas.Add(form);
            form.Dispose();
        }

        public override void Avisar(Control tela, string mensagem) => Avisos.Add(mensagem);

        public override void Informar(Control tela, string mensagem) => Informacoes.Add(mensagem);

        public override bool Confirmar(Control tela, string pergunta, MessageBoxIcon icone, MessageBoxDefaultButton padrao)
        {
            Perguntas.Add(pergunta);
            return RespostaConfirmar;
        }

        public override DialogResult PerguntarSalvar(Control tela, string pergunta)
        {
            Perguntas.Add(pergunta);
            return RespostaSalvar;
        }

        /// <summary>Fecha no fim do teste sem perguntar nada.</summary>
        public void Descartar()
        {
            RespostaSalvar = DialogResult.No;
            if (!IsDisposed)
                Close();
            Application.DoEvents();
        }
    }
}
