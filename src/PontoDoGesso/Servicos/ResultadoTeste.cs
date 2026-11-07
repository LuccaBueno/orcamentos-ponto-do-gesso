namespace PontoDoGesso.Servicos
{
    public class ResultadoTeste
    {
        public bool Ok { get; }
        public string Mensagem { get; }

        private ResultadoTeste(bool ok, string mensagem)
        {
            Ok = ok;
            Mensagem = mensagem;
        }

        public static ResultadoTeste Sucesso(string mensagem) => new ResultadoTeste(true, mensagem);
        public static ResultadoTeste Falha(string mensagem) => new ResultadoTeste(false, mensagem);

        public override string ToString() => (Ok ? "OK: " : "FALHOU: ") + Mensagem;
    }
}
