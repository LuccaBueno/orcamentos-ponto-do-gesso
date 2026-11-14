using System;
using System.ComponentModel;
using System.Data.SQLite;
using System.IO;
using System.Net;
using PontoDoGesso.Dominio;
using PontoDoGesso.Servicos;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class MensagemErroTestes
    {
        [Fact]
        public void ErroDoProprioAppMostraAMensagemDele()
        {
            Assert.Equal("Inclua ao menos um item.", MensagemErro.Explicar(new OrcamentoInvalidoException("Inclua ao menos um item.")));
            Assert.Equal("Backup inválido.", MensagemErro.Explicar(new ErroConhecidoException("Backup inválido.")));
            Assert.Equal("Não confere.", MensagemErro.Explicar(new PacoteInvalidoException("Não confere.")));
        }

        [Fact]
        public void ArquivoAbertoEmOutroPrograma()
        {
            var erro = new IOException("The process cannot access the file because it is being used by another process.", unchecked((int)0x80070020));
            Assert.Equal("O arquivo está aberto em outro programa. Feche o outro programa e tente de novo.", MensagemErro.Explicar(erro));
        }

        [Fact]
        public void DiscoCheio() =>
            Assert.Equal("O disco está cheio.", MensagemErro.Explicar(new IOException("There is not enough space on the disk.", unchecked((int)0x80070070))));

        [Theory]
        [InlineData(WebExceptionStatus.NameResolutionFailure, "Sem conexão com a internet.")]
        [InlineData(WebExceptionStatus.Timeout, "A internet demorou demais para responder.")]
        [InlineData(WebExceptionStatus.SecureChannelFailure, "Não foi possível fazer uma conexão segura com o GitHub (confira a data e a hora do Windows).")]
        public void ErrosDeInternet(WebExceptionStatus situacao, string esperado) =>
            Assert.Equal(esperado, MensagemErro.Explicar(new WebException("The remote name could not be resolved: 'api.github.com'", situacao)));

        [Fact]
        public void NenhumaMensagemTemTextoTecnicoEmIngles()
        {
            var erros = new Exception[]
            {
                new UnauthorizedAccessException(@"Access to the path 'C:\x' is denied."),
                new DirectoryNotFoundException(@"Could not find a part of the path 'E:\'."),
                new FileNotFoundException("Could not find file 'x.db'."),
                new IOException("Unknown error"),
                new SQLiteException(SQLiteErrorCode.Busy, "database is locked"),
                new SQLiteException(SQLiteErrorCode.Corrupt, "database disk image is malformed"),
                new Win32Exception(5),
                new AggregateException(new WebException("Unable to connect", WebExceptionStatus.ConnectFailure)),
                new InvalidOperationException("Collection was modified"),
                null
            };

            foreach (var erro in erros)
            {
                var mensagem = MensagemErro.Explicar(erro);
                Assert.False(string.IsNullOrWhiteSpace(mensagem));
                Assert.DoesNotContain("the", mensagem.ToLowerInvariant().Split(' ', '.', '\''));
                Assert.DoesNotContain("Exception", mensagem);
                Assert.EndsWith(".", mensagem);
            }
        }
    }
}
