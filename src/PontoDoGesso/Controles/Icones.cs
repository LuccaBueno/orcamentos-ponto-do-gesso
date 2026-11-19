using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PontoDoGesso.Controles
{
    /// <summary>
    /// Ícones simples desenhados em código (traço de uma cor só), para o menu lateral e os botões dos itens.
    /// Não dependem de arquivo nem de fonte de símbolos (a Segoe UI Symbol do Windows 7 não tem todos).
    /// </summary>
    public static class Icones
    {
        public const string Inicio = "inicio";
        public const string Orcamento = "orcamento";
        public const string Historico = "historico";
        public const string Clientes = "clientes";
        public const string Produtos = "produtos";
        public const string Empresa = "empresa";
        public const string Configuracoes = "configuracoes";
        public const string Pasta = "pasta";
        public const string Atualizar = "atualizar";
        public const string Lixeira = "lixeira";

        public static Bitmap Desenhar(string nome, int tamanho, Color cor)
        {
            var imagem = new Bitmap(tamanho, tamanho);
            using (var g = Graphics.FromImage(imagem))
            using (var caneta = new Pen(cor, Math.Max(1.5f, tamanho / 13f)) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round })
            using (var pincel = new SolidBrush(cor))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                // Desenho numa grade de 24×24, ampliada para o tamanho pedido
                var escala = tamanho / 24f;
                g.ScaleTransform(escala, escala);
                caneta.Width /= escala;
                Desenhar(g, nome, caneta, pincel);
            }

            return imagem;
        }

        private static void Desenhar(Graphics g, string nome, Pen caneta, Brush pincel)
        {
            switch (nome)
            {
                case Inicio:
                    g.DrawLines(caneta, new[] { new PointF(3, 11.5f), new PointF(12, 3.5f), new PointF(21, 11.5f) });
                    g.DrawLines(caneta, new[] { new PointF(5.5f, 9.5f), new PointF(5.5f, 20.5f), new PointF(18.5f, 20.5f), new PointF(18.5f, 9.5f) });
                    g.DrawRectangle(caneta, 10, 14, 4, 6.5f);
                    break;
                case Orcamento:
                    g.DrawLines(caneta, new[] { new PointF(14, 2.5f), new PointF(5, 2.5f), new PointF(5, 21.5f), new PointF(19, 21.5f), new PointF(19, 7.5f), new PointF(14, 2.5f), new PointF(14, 7.5f), new PointF(19, 7.5f) });
                    g.DrawLine(caneta, 8.5f, 12, 15.5f, 12);
                    g.DrawLine(caneta, 8.5f, 15.5f, 15.5f, 15.5f);
                    g.DrawLine(caneta, 8.5f, 19, 12.5f, 19);
                    break;
                case Historico:
                    g.DrawEllipse(caneta, 3, 3, 18, 18);
                    g.DrawLines(caneta, new[] { new PointF(12, 7), new PointF(12, 12), new PointF(15.5f, 14.5f) });
                    break;
                case Clientes:
                    g.DrawEllipse(caneta, 8, 3, 8, 8);
                    g.DrawArc(caneta, 4, 13, 16, 14, 180, 180);
                    break;
                case Produtos:
                    g.DrawPolygon(caneta, new[] { new PointF(12, 2.5f), new PointF(20.5f, 7), new PointF(20.5f, 17), new PointF(12, 21.5f), new PointF(3.5f, 17), new PointF(3.5f, 7) });
                    g.DrawLines(caneta, new[] { new PointF(3.5f, 7), new PointF(12, 11.5f), new PointF(20.5f, 7) });
                    g.DrawLine(caneta, 12, 11.5f, 12, 21.5f);
                    break;
                case Empresa:
                    g.DrawRectangle(caneta, 4, 3, 10, 18);
                    g.DrawLines(caneta, new[] { new PointF(14, 9), new PointF(20, 9), new PointF(20, 21), new PointF(2.5f, 21) });
                    foreach (var y in new[] { 7f, 11f, 15f })
                    {
                        g.DrawLine(caneta, 7, y, 7.5f, y);
                        g.DrawLine(caneta, 10.5f, y, 11, y);
                    }
                    g.DrawLine(caneta, 17, 13, 17, 13.5f);
                    g.DrawLine(caneta, 17, 17, 17, 17.5f);
                    break;
                case Configuracoes:
                    for (var i = 0; i < 8; i++)
                    {
                        var angulo = i * Math.PI / 4;
                        g.DrawLine(caneta, (float)(12 + 6.5 * Math.Cos(angulo)), (float)(12 + 6.5 * Math.Sin(angulo)), (float)(12 + 9.5 * Math.Cos(angulo)), (float)(12 + 9.5 * Math.Sin(angulo)));
                    }
                    g.DrawEllipse(caneta, 5.5f, 5.5f, 13, 13);
                    g.DrawEllipse(caneta, 9.5f, 9.5f, 5, 5);
                    break;
                case Pasta:
                    g.DrawPolygon(caneta, new[] { new PointF(2.5f, 5), new PointF(9, 5), new PointF(11, 7.5f), new PointF(21.5f, 7.5f), new PointF(21.5f, 19.5f), new PointF(2.5f, 19.5f) });
                    g.DrawLine(caneta, 2.5f, 10.5f, 21.5f, 10.5f);
                    break;
                case Atualizar:
                    g.DrawArc(caneta, 4, 4, 16, 16, -60, 150);
                    g.DrawArc(caneta, 4, 4, 16, 16, 120, 150);
                    g.FillPolygon(pincel, new[] { new PointF(17.5f, 1.5f), new PointF(19.5f, 7.5f), new PointF(13.5f, 7) });
                    g.FillPolygon(pincel, new[] { new PointF(6.5f, 22.5f), new PointF(4.5f, 16.5f), new PointF(10.5f, 17) });
                    break;
                case Lixeira:
                    g.DrawLine(caneta, 3.5f, 6, 20.5f, 6);
                    g.DrawLines(caneta, new[] { new PointF(9, 6), new PointF(9, 3), new PointF(15, 3), new PointF(15, 6) });
                    g.DrawLines(caneta, new[] { new PointF(5.5f, 6), new PointF(7, 21), new PointF(17, 21), new PointF(18.5f, 6) });
                    g.DrawLine(caneta, 10, 10, 10, 17);
                    g.DrawLine(caneta, 14, 10, 14, 17);
                    break;
                default:
                    throw new ArgumentException("Ícone desconhecido: " + nome, nameof(nome));
            }
        }
    }
}
