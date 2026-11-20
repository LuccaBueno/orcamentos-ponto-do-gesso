using PontoDoGesso.Dominio;

namespace PontoDoGesso.Servicos
{
    /// <summary>
    /// Quando salvar cria uma versão nova: só se o orçamento já estava salvo e a versão atual já foi enviada
    /// (exportada, copiada, impressa ou mandada pelo WhatsApp). Antes de enviar, salvar só sobrescreve.
    /// Marcar saídas nunca cria versão; duplicar cria um número novo.
    /// </summary>
    public static class VersaoOrcamento
    {
        public static bool CriaNovaVersao(bool salvo, bool enviado) => salvo && enviado;

        public static bool CriaNovaVersao(Orcamento orcamento) => CriaNovaVersao(orcamento.Id != 0, orcamento.EnviadoEm != null);

        public static string AvisoNovaVersao(Orcamento orcamento) =>
            "Este orçamento já foi enviado; as alterações viram a versão " + Orcamento.NumeroComVersao(orcamento.Numero, orcamento.Versao + 1) + ".";
    }
}
