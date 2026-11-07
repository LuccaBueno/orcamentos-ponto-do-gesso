using System;
using System.Globalization;
using PontoDoGesso.Dados;

namespace PontoDoGesso.Servicos
{
    /// <summary>Verificação de atualização ao abrir o app: no máximo uma vez a cada 24 h, mesmo sem internet.</summary>
    public class VerificacaoAutomatica
    {
        public static readonly TimeSpan Intervalo = TimeSpan.FromHours(24);

        private const string ChaveUltima = "atualizacao.ultima_verificacao";
        private const string FormatoData = "yyyy-MM-ddTHH:mm:ss";

        private readonly ConfiguracaoRepositorio _configuracoes;

        public VerificacaoAutomatica(Banco banco)
        {
            _configuracoes = new ConfiguracaoRepositorio(banco);
        }

        public DateTime? Ultima =>
            DateTime.TryParseExact(_configuracoes.Obter(ChaveUltima), FormatoData, CultureInfo.InvariantCulture, DateTimeStyles.None, out var data) ? data : (DateTime?)null;

        /// <summary>Relógio voltado para trás (data no futuro) também libera a verificação.</summary>
        public bool Precisa(DateTime agora)
        {
            var ultima = Ultima;
            return ultima == null || ultima.Value > agora || agora - ultima.Value >= Intervalo;
        }

        /// <summary>
        /// Consulta o GitHub se já passou o intervalo. Devolve a versão publicada só se for mais nova que a instalada.
        /// A tentativa é registrada antes da consulta: sem internet, só tenta de novo no dia seguinte.
        /// </summary>
        public VersaoPublicada VerificarSePrecisa(Func<VersaoPublicada> consultar, Version atual, DateTime agora)
        {
            if (!Precisa(agora))
                return null;

            _configuracoes.Definir(ChaveUltima, agora.ToString(FormatoData, CultureInfo.InvariantCulture));
            var publicada = consultar();
            return Atualizacao.EhMaisNova(publicada?.Versao, atual) ? publicada : null;
        }
    }
}
