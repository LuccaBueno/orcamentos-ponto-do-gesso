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
        [InlineData("novoOrcamento", typeof(FormOrcamento))]
        [InlineData("historico", typeof(FormHistorico))]
        [InlineData("configuracoes", typeof(FormConfiguracoes))]
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

        [Fact]
        public void AbrirPastaDeOrcamentosUsaAPastaDasConfiguracoes()
        {
            var pasta = System.IO.Path.Combine(_temporario.Pasta, "exportados");
            new ConfiguracaoRepositorio(_temporario.Banco).Definir("exportacao.pasta", pasta);

            Tela.EmSta(() =>
            {
                using (var form = new PrincipalSemDialogos(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    Tela.Capturar(form, "principal");
                    Tela.Achar<Button>(form, "pastaOrcamentos").PerformClick();
                    Assert.Equal(pasta, Assert.Single(form.Pastas));
                }
            });
        }

        private class PrincipalSemDialogos : FormPrincipal
        {
            public List<Form> Abertas { get; } = new List<Form>();
            public List<string> Pastas { get; } = new List<string>();

            protected override void AbrirPasta(string pasta) => Pastas.Add(pasta);

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
