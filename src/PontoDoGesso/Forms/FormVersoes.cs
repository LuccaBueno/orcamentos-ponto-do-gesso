using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;
using PontoDoGesso.Exportacao;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Forms
{
    /// <summary>
    /// Versões de um orçamento: a atual e as já enviadas que foram substituídas. Só para ver (prévia da imagem) e exportar de novo;
    /// exportar uma versão antiga não conta como envio.
    /// </summary>
    public class FormVersoes : Form
    {
        private readonly List<Orcamento> _versoes;
        private readonly ListBox _lista = new ListBox { Name = "listaVersoes", Dock = DockStyle.Fill, IntegralHeight = false, Font = new Font("Segoe UI", 9.5F) };
        private readonly PictureBox _previa = new PictureBox { Name = "previa", SizeMode = PictureBoxSizeMode.AutoSize, Location = new Point(0, 0) };
        private readonly AcoesExportacao _acoes;
        private readonly DadosLoja _loja;

        public FormVersoes(Banco banco, long orcamentoId)
        {
            var orcamentos = new OrcamentoRepositorio(banco);
            _versoes = new List<Orcamento>();
            var atual = orcamentos.ObterPorId(orcamentoId);
            if (atual != null)
                _versoes.Add(atual);
            _versoes.AddRange(orcamentos.VersoesAntigas(orcamentoId));
            _acoes = new AcoesExportacao(banco, this);
            _loja = new ExportacaoOrcamento(banco).Loja;

            Estilo.Aplicar(this, 900, 640);
            Text = "Versões do orçamento Nº " + (atual?.NumeroExibido ?? "");
            StartPosition = FormStartPosition.CenterParent;

            var dicas = new ToolTip();
            var esquerda = new TableLayoutPanel { Dock = DockStyle.Left, Width = 300, ColumnCount = 1, Padding = new Padding(8) };
            esquerda.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            esquerda.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            esquerda.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            esquerda.Controls.Add(new Label
            {
                Text = "A versão atual é a que abre na tela de orçamento. As antigas ficam guardadas só para ver e exportar de novo.",
                AutoSize = true,
                MaximumSize = new Size(280, 0),
                ForeColor = Color.DimGray,
                Margin = new Padding(3, 0, 3, 6)
            }, 0, 0);
            esquerda.Controls.Add(_lista, 0, 1);

            var botoes = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(0, 6, 0, 0) };
            void Botao(string nome, string texto, string dica, Action<Orcamento> acao)
            {
                var botao = Estilo.CriarBotao(texto, (s, e) =>
                {
                    if (Selecionada != null)
                        acao(Selecionada);
                });
                botao.Name = nome;
                dicas.SetToolTip(botao, dica);
                botoes.Controls.Add(botao);
            }

            Botao("copiarImagem", "Copiar imagem", "Copia a imagem desta versão para colar no WhatsApp Web com Ctrl+V.", o => _acoes.CopiarImagem(o));
            Botao("png", "PNG", "Salva a imagem desta versão (PNG) na pasta de orçamentos.", o => _acoes.Png(o));
            Botao("pdf", "PDF", "Salva esta versão em PDF (A4) na pasta de orçamentos.", o => _acoes.Pdf(o));
            Botao("imprimir", "Imprimir", "Mostra a prévia e imprime esta versão em A4.", o => _acoes.Imprimir(o));
            var fechar = Estilo.CriarBotao("Fechar", (s, e) => Close());
            fechar.Name = "fechar";
            botoes.Controls.Add(fechar);
            esquerda.Controls.Add(botoes, 0, 2);

            var rolagem = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(0xE8, 0xE8, 0xE8) };
            rolagem.Controls.Add(_previa);

            Controls.Add(rolagem);
            Controls.Add(esquerda);

            foreach (var versao in _versoes)
                _lista.Items.Add(Descrever(versao, versao == atual));

            _lista.SelectedIndexChanged += (s, e) => MostrarPrevia();
            if (_lista.Items.Count > 0)
                _lista.SelectedIndex = 0;
        }

        public IReadOnlyList<Orcamento> Versoes => _versoes;

        private Orcamento Selecionada => _lista.SelectedIndex < 0 ? null : _versoes[_lista.SelectedIndex];

        private static string Descrever(Orcamento orcamento, bool atual) =>
            "Nº " + orcamento.NumeroExibido + (atual ? " (atual)" : "") + " · " + orcamento.Data.ToString("dd/MM/yyyy") + " · " + Formatacao.Moeda(orcamento.TotalCentavos)
            + (orcamento.EnviadoEm == null ? " · não enviada" : " · enviada em " + orcamento.EnviadoEm.Value.ToString("dd/MM/yyyy"));

        /// <summary>Prévia só para leitura: a mesma imagem do PNG, em tamanho menor.</summary>
        private void MostrarPrevia()
        {
            var anterior = _previa.Image;
            _previa.Image = Selecionada == null ? null : ExportadorPng.CriarImagem(Selecionada, _loja, 72);
            anterior?.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _previa.Image?.Dispose();
            base.Dispose(disposing);
        }
    }
}
