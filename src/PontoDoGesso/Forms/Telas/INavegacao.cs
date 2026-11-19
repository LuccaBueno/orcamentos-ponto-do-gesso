using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;

namespace PontoDoGesso.Forms
{
    /// <summary>Itens do menu lateral.</summary>
    public enum Destino
    {
        Inicio,
        Orcamento,
        Historico,
        Clientes,
        Produtos,
        Configuracoes
    }

    /// <summary>
    /// Troca a tela da área principal (a janela principal implementa). Cada troca pergunta antes à tela atual se ela pode sair
    /// (alterações não salvas); por isso tudo devolve falso quando o usuário cancela.
    /// </summary>
    public interface INavegacao
    {
        bool Ir(Destino destino);

        /// <summary>Orçamento em branco, já com o cliente escolhido se ele vier.</summary>
        bool NovoOrcamento(Cliente cliente = null);

        /// <summary>Orçamento salvo; <paramref name="duplicar"/> abre uma cópia nova (sem número, data de hoje).</summary>
        bool AbrirOrcamento(Orcamento orcamento, bool duplicar);

        bool AbrirHistorico(FiltroHistorico filtro);
    }
}
