using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace PontoDoGesso.Testes
{
    /// <summary>
    /// Grava um GIF animado só com o .NET Framework (o GDI+ não grava GIF com vários quadros), para o GIF do README.
    /// Paleta única de 256 cores tirada das cores mais usadas nos quadros (telas têm poucas cores) e,
    /// a partir do segundo quadro, só o retângulo que mudou: o arquivo fica pequeno.
    /// </summary>
    public sealed class GifAnimado
    {
        private readonly List<(int[] Pixels, int DuracaoMs)> _quadros = new List<(int[] Pixels, int DuracaoMs)>();
        private readonly int _largura;
        private readonly int _altura;

        public GifAnimado(int largura, int altura)
        {
            _largura = largura;
            _altura = altura;
        }

        public int Quadros => _quadros.Count;

        public int DuracaoTotalMs => _quadros.Sum(quadro => quadro.DuracaoMs);

        /// <summary>A imagem é redimensionada para o tamanho do GIF.</summary>
        public void Adicionar(Bitmap imagem, int duracaoMs)
        {
            using (var quadro = new Bitmap(_largura, _altura, PixelFormat.Format32bppArgb))
            {
                using (var g = Graphics.FromImage(quadro))
                {
                    g.Clear(Color.White);
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(imagem, new Rectangle(0, 0, _largura, _altura));
                }

                var dados = quadro.LockBits(new Rectangle(0, 0, _largura, _altura), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                var pixels = new int[_largura * _altura];
                Marshal.Copy(dados.Scan0, pixels, 0, pixels.Length);
                quadro.UnlockBits(dados);
                _quadros.Add((pixels, duracaoMs));
            }
        }

        public void Salvar(string arquivo)
        {
            var (paleta, indiceDaCor) = MontarPaleta();

            using (var saida = new BinaryWriter(File.Create(arquivo)))
            {
                saida.Write(new[] { (byte)'G', (byte)'I', (byte)'F', (byte)'8', (byte)'9', (byte)'a' });
                saida.Write((ushort)_largura);
                saida.Write((ushort)_altura);
                saida.Write((byte)0xF7); // paleta global de 256 cores
                saida.Write((byte)0);
                saida.Write((byte)0);
                foreach (var cor in paleta)
                {
                    saida.Write((byte)(cor >> 16));
                    saida.Write((byte)(cor >> 8));
                    saida.Write((byte)cor);
                }

                // Repetir para sempre
                saida.Write(new byte[] { 0x21, 0xFF, 0x0B });
                saida.Write(System.Text.Encoding.ASCII.GetBytes("NETSCAPE2.0"));
                saida.Write(new byte[] { 0x03, 0x01, 0x00, 0x00, 0x00 });

                int[] anterior = null;
                foreach (var (pixels, duracao) in _quadros)
                {
                    var area = anterior == null ? new Rectangle(0, 0, _largura, _altura) : AreaQueMudou(anterior, pixels);
                    if (area.IsEmpty)
                        area = new Rectangle(0, 0, 1, 1);

                    saida.Write(new byte[] { 0x21, 0xF9, 0x04, 0x04 }); // disposição: manter o quadro anterior
                    saida.Write((ushort)Math.Max(2, duracao / 10));
                    saida.Write((byte)0);
                    saida.Write((byte)0);

                    saida.Write((byte)0x2C);
                    saida.Write((ushort)area.X);
                    saida.Write((ushort)area.Y);
                    saida.Write((ushort)area.Width);
                    saida.Write((ushort)area.Height);
                    saida.Write((byte)0);

                    var indices = new byte[area.Width * area.Height];
                    for (var y = 0; y < area.Height; y++)
                        for (var x = 0; x < area.Width; x++)
                            indices[y * area.Width + x] = indiceDaCor[Reduzir(pixels[(area.Y + y) * _largura + area.X + x])];

                    GravarLzw(saida, indices);
                    anterior = pixels;
                }

                saida.Write((byte)0x3B);
            }
        }

        private Rectangle AreaQueMudou(int[] antes, int[] depois)
        {
            int esquerda = _largura, topo = _altura, direita = -1, baixo = -1;
            for (var y = 0; y < _altura; y++)
                for (var x = 0; x < _largura; x++)
                {
                    var i = y * _largura + x;
                    if (antes[i] == depois[i])
                        continue;
                    if (x < esquerda) esquerda = x;
                    if (x > direita) direita = x;
                    if (y < topo) topo = y;
                    if (y > baixo) baixo = y;
                }

            return direita < 0 ? Rectangle.Empty : Rectangle.FromLTRB(esquerda, topo, direita + 1, baixo + 1);
        }

        /// <summary>Cor em 5 bits por canal (32.768 grupos): junta tons quase iguais do texto suavizado.</summary>
        private static int Reduzir(int argb) => ((argb >> 9) & 0x7C00) | ((argb >> 6) & 0x03E0) | ((argb >> 3) & 0x001F);

        private (int[] Paleta, byte[] IndiceDaCor) MontarPaleta()
        {
            var contagem = new long[32768];
            var somaR = new long[32768];
            var somaG = new long[32768];
            var somaB = new long[32768];
            foreach (var (pixels, _) in _quadros)
                foreach (var argb in pixels)
                {
                    var grupo = Reduzir(argb);
                    contagem[grupo]++;
                    somaR[grupo] += (argb >> 16) & 0xFF;
                    somaG[grupo] += (argb >> 8) & 0xFF;
                    somaB[grupo] += argb & 0xFF;
                }

            var escolhidos = Enumerable.Range(0, 32768).Where(g => contagem[g] > 0).OrderByDescending(g => contagem[g]).Take(256).ToArray();
            var paleta = new int[256];
            for (var i = 0; i < escolhidos.Length; i++)
            {
                var g = escolhidos[i];
                paleta[i] = (int)(somaR[g] / contagem[g]) << 16 | (int)(somaG[g] / contagem[g]) << 8 | (int)(somaB[g] / contagem[g]);
            }

            // Grupo que entrou na paleta usa a própria cor; os outros vão para a cor da paleta mais próxima da média deles
            var indiceDaCor = new byte[32768];
            for (var i = 0; i < escolhidos.Length; i++)
                indiceDaCor[escolhidos[i]] = (byte)i;

            var naPaleta = new System.Collections.Generic.HashSet<int>(escolhidos);
            for (var grupo = 0; grupo < 32768; grupo++)
            {
                if (contagem[grupo] == 0 || naPaleta.Contains(grupo))
                    continue;
                int r = (int)(somaR[grupo] / contagem[grupo]), gr = (int)(somaG[grupo] / contagem[grupo]), b = (int)(somaB[grupo] / contagem[grupo]);
                var melhor = 0;
                var menor = int.MaxValue;
                for (var i = 0; i < escolhidos.Length; i++)
                {
                    int dr = r - ((paleta[i] >> 16) & 0xFF), dg = gr - ((paleta[i] >> 8) & 0xFF), db = b - (paleta[i] & 0xFF);
                    var distancia = dr * dr * 3 + dg * dg * 4 + db * db * 2;
                    if (distancia < menor)
                    {
                        menor = distancia;
                        melhor = i;
                    }
                }
                indiceDaCor[grupo] = (byte)melhor;
            }

            return (paleta, indiceDaCor);
        }

        /// <summary>Compressão LZW do GIF (códigos de tamanho variável, até 12 bits, gravados em blocos de até 255 bytes).</summary>
        private static void GravarLzw(BinaryWriter saida, byte[] indices)
        {
            const int TamanhoMinimo = 8;
            const int Limpar = 1 << TamanhoMinimo;
            const int Fim = Limpar + 1;

            saida.Write((byte)TamanhoMinimo);
            var blocos = new List<byte>();
            int acumulado = 0, bitsAcumulados = 0;
            var tamanhoCodigo = TamanhoMinimo + 1;

            void Emitir(int codigo)
            {
                acumulado |= codigo << bitsAcumulados;
                bitsAcumulados += tamanhoCodigo;
                while (bitsAcumulados >= 8)
                {
                    blocos.Add((byte)acumulado);
                    acumulado >>= 8;
                    bitsAcumulados -= 8;
                }
            }

            var dicionario = new Dictionary<int, int>();
            var proximo = Fim + 1;
            Emitir(Limpar);

            var atual = (int)indices[0];
            for (var i = 1; i < indices.Length; i++)
            {
                var simbolo = indices[i];
                var chave = (atual << 8) | simbolo;
                if (dicionario.TryGetValue(chave, out var codigo))
                {
                    atual = codigo;
                    continue;
                }

                Emitir(atual);
                if (proximo < 4096)
                {
                    dicionario[chave] = proximo++;
                    if (proximo > (1 << tamanhoCodigo) && tamanhoCodigo < 12)
                        tamanhoCodigo++;
                }
                else
                {
                    Emitir(Limpar);
                    dicionario.Clear();
                    proximo = Fim + 1;
                    tamanhoCodigo = TamanhoMinimo + 1;
                }
                atual = simbolo;
            }

            Emitir(atual);
            Emitir(Fim);
            if (bitsAcumulados > 0)
                blocos.Add((byte)acumulado);

            for (var i = 0; i < blocos.Count; i += 255)
            {
                var tamanho = Math.Min(255, blocos.Count - i);
                saida.Write((byte)tamanho);
                saida.Write(blocos.GetRange(i, tamanho).ToArray());
            }
            saida.Write((byte)0);
        }
    }
}
