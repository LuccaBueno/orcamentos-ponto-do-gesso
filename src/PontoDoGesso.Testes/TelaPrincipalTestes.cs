using System;
using System.Collections.Generic;
using System.Windows.Forms;
using PontoDoGesso.Dados;
using PontoDoGesso.Forms;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class TelaPrincipalTestes : IDisposable
    {
        private readonly BancoTemporario _temporario = new BancoTemporario();

        public TelaPrincipalTestes()
        {
            _temporario.Banco.Preparar();
        }

        public void Dispose() => _temporario.Dispose();

        [Theory]
        [InlineData("clientes", typeof(FormClientes))]
        [InlineData("produtos", typeof(FormProdutos))]
        public void BotaoHabilitadoAbreATela(string botao, Type tela)
        {
            Tela.EmSta(() =>
            {
                using (var form = new PrincipalSemDialogos(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    var alvo = Tela.Achar<Button>(form, botao);
                    Assert.True(alvo.Enabled);

                    alvo.PerformClick();
                    Assert.IsType(tela, Assert.Single(form.Abertas));
                }
            });
        }

        [Theory]
        [InlineData("novoOrcamento")]
        [InlineData("historico")]
        [InlineData("configuracoes")]
        public void BotaoDeFaseFuturaFicaDesabilitado(string botao)
        {
            Tela.EmSta(() =>
            {
                using (var form = new FormPrincipal(_temporario.Banco))
                    Assert.False(Tela.Achar<Button>(form, botao).Enabled);
            });
        }

        private class PrincipalSemDialogos : FormPrincipal
        {
            public List<Form> Abertas { get; } = new List<Form>();

            public PrincipalSemDialogos(Banco banco) : base(banco)
            {
            }

            protected override void AbrirTela(Func<Form> abrirTela)
            {
                var form = abrirTela();
                Abertas.Add(form);
                form.Dispose();
            }
        }
    }
}
