using System;
using System.ComponentModel;
using System.Data.SQLite;
using System.IO;
using System.Net;
using PontoDoGesso.Dominio;

namespace PontoDoGesso.Servicos
{
    /// <summary>Erro previsto pelo próprio app, com a mensagem já escrita para quem usa a loja.</summary>
    public class ErroConhecidoException : Exception
    {
        public ErroConhecidoException(string mensagem) : base(mensagem)
        {
        }
    }

    /// <summary>
    /// Traduz um erro em português claro, sem termos técnicos, para mostrar na tela.
    /// O detalhe técnico (mensagem original e pilha) vai só para o log, pelo Log.RegistrarErro.
    /// </summary>
    public static class MensagemErro
    {
        private const int ArquivoEmUso = unchecked((int)0x80070020);
        private const int TrechoTravado = unchecked((int)0x80070021);
        private const int DiscoCheio = unchecked((int)0x80070070);
        private const int DiscoCheio2 = unchecked((int)0x80070027);

        public static string Explicar(Exception erro)
        {
            switch (erro)
            {
                case null:
                    return "Aconteceu um erro inesperado.";
                case ErroConhecidoException _:
                case OrcamentoInvalidoException _:
                    return erro.Message;
                case AggregateException agregado when agregado.InnerException != null:
                    return Explicar(agregado.GetBaseException());
                case UnauthorizedAccessException _:
                    return "O Windows não deu permissão para usar o arquivo ou a pasta.";
                case DirectoryNotFoundException _:
                    return "A pasta não existe (pode ser um pendrive ou disco que foi removido).";
                case FileNotFoundException _:
                    return "O arquivo não foi encontrado.";
                case PathTooLongException _:
                    return "O caminho da pasta é comprido demais. Escolha uma pasta mais curta, como C:\\Orçamentos.";
                case IOException io when io.HResult == ArquivoEmUso || io.HResult == TrechoTravado:
                    return "O arquivo está aberto em outro programa. Feche o outro programa e tente de novo.";
                case IOException io when io.HResult == DiscoCheio || io.HResult == DiscoCheio2:
                    return "O disco está cheio.";
                case IOException _:
                    return "Não foi possível ler ou gravar o arquivo no disco.";
                case SQLiteException sqlite when sqlite.ResultCode == SQLiteErrorCode.Busy || sqlite.ResultCode == SQLiteErrorCode.Locked:
                    return "Os dados estão sendo usados por outro programa. Feche o outro programa e tente de novo.";
                case SQLiteException _:
                    return "Não foi possível ler ou gravar os dados (o arquivo do banco pode estar danificado).";
                case WebException web:
                    return Explicar(web);
                case Win32Exception _:
                    return "O Windows não conseguiu abrir o programa (pode ter sido bloqueado pelo antivírus).";
                case OperationCanceledException _:
                    return "A operação foi cancelada.";
                default:
                    return "Aconteceu um erro inesperado.";
            }
        }

        private static string Explicar(WebException erro)
        {
            switch (erro.Status)
            {
                case WebExceptionStatus.Timeout:
                    return "A internet demorou demais para responder.";
                case WebExceptionStatus.NameResolutionFailure:
                case WebExceptionStatus.ConnectFailure:
                case WebExceptionStatus.ProxyNameResolutionFailure:
                    return "Sem conexão com a internet.";
                case WebExceptionStatus.SecureChannelFailure:
                case WebExceptionStatus.TrustFailure:
                    return "Não foi possível fazer uma conexão segura com o GitHub (confira a data e a hora do Windows).";
                case WebExceptionStatus.ProtocolError:
                    return "O GitHub não respondeu como esperado. Tente de novo mais tarde.";
                default:
                    return "A conexão com a internet falhou.";
            }
        }
    }
}
