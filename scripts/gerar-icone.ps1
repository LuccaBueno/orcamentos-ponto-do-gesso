# Gera C:\dev\orcamentos-ponto-do-gesso\assets\icone.ico (ícone do app) a partir de desenho em código.
# Quadrado arredondado laranja da loja, moldura branca fina (como a do orçamento) e "PG" em Times New Roman.
# Tamanhos 16, 24, 32 e 48 em bitmap (lidos por qualquer programa) e 256 em PNG (Windows Vista ou mais novo).
# Uso: powershell -ExecutionPolicy Bypass -File C:\dev\orcamentos-ponto-do-gesso\scripts\gerar-icone.ps1

$ErrorActionPreference = 'Stop'
$destino = Join-Path (Split-Path $PSScriptRoot -Parent) 'assets\icone.ico'

Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

public static class GeradorIcone
{
    static readonly Color Laranja = Color.FromArgb(0xC5, 0x5A, 0x11);

    static GraphicsPath Arredondado(RectangleF r, float raio)
    {
        var d = raio * 2;
        var p = new GraphicsPath();
        p.AddArc(r.X, r.Y, d, d, 180, 90);
        p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        p.CloseFigure();
        return p;
    }

    public static Bitmap Desenhar(int t)
    {
        var bmp = new Bitmap(t, t, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.Clear(Color.Transparent);

            var margem = t <= 16 ? 0f : t * 0.03f;
            var fundo = new RectangleF(margem, margem, t - 2 * margem, t - 2 * margem);
            using (var caminho = Arredondado(fundo, t * 0.18f))
            using (var pincel = new SolidBrush(Laranja))
                g.FillPath(pincel, caminho);

            // Moldura só a partir de 32 px: nos pequenos ela vira borrão
            if (t >= 32)
            {
                var recuo = t * 0.10f;
                var moldura = new RectangleF(recuo, recuo, t - 2 * recuo, t - 2 * recuo);
                using (var caminho = Arredondado(moldura, t * 0.11f))
                using (var caneta = new Pen(Color.FromArgb(235, Color.White), Math.Max(1f, t * 0.03f)))
                    g.DrawPath(caneta, caminho);
            }

            var tamanhoFonte = t <= 16 ? t * 0.62f : t * 0.48f;
            using (var fonte = new Font("Times New Roman", tamanhoFonte, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var formato = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString("PG", fonte, Brushes.White, new RectangleF(0, t * 0.03f, t, t), formato);
        }
        return bmp;
    }

    static byte[] Dib(Bitmap bmp)
    {
        int t = bmp.Width;
        var ms = new MemoryStream();
        var w = new BinaryWriter(ms);
        w.Write(40); w.Write(t); w.Write(t * 2); w.Write((short)1); w.Write((short)32);
        w.Write(0); w.Write(0); w.Write(0); w.Write(0); w.Write(0); w.Write(0);
        for (int y = t - 1; y >= 0; y--)
            for (int x = 0; x < t; x++)
            {
                var c = bmp.GetPixel(x, y);
                w.Write(c.B); w.Write(c.G); w.Write(c.R); w.Write(c.A);
            }
        // Máscara AND (1 bit por pixel, linhas de 4 em 4 bytes): tudo zero, a transparência vem do alfa
        int linha = ((t + 31) / 32) * 4;
        w.Write(new byte[linha * t]);
        return ms.ToArray();
    }

    static byte[] Png(Bitmap bmp)
    {
        var ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }

    public static void Gerar(string destino, string pastaPrevia)
    {
        int[] tamanhos = { 16, 24, 32, 48, 256 };
        var imagens = new byte[tamanhos.Length][];
        for (int i = 0; i < tamanhos.Length; i++)
            using (var bmp = Desenhar(tamanhos[i]))
            {
                imagens[i] = tamanhos[i] == 256 ? Png(bmp) : Dib(bmp);
                if (pastaPrevia != null)
                    bmp.Save(Path.Combine(pastaPrevia, "icone-" + tamanhos[i] + ".png"), ImageFormat.Png);
            }

        using (var w = new BinaryWriter(File.Create(destino)))
        {
            w.Write((short)0); w.Write((short)1); w.Write((short)tamanhos.Length);
            int deslocamento = 6 + 16 * tamanhos.Length;
            for (int i = 0; i < tamanhos.Length; i++)
            {
                byte lado = (byte)(tamanhos[i] == 256 ? 0 : tamanhos[i]);
                w.Write(lado); w.Write(lado); w.Write((byte)0); w.Write((byte)0);
                w.Write((short)1); w.Write((short)32);
                w.Write(imagens[i].Length); w.Write(deslocamento);
                deslocamento += imagens[i].Length;
            }
            foreach (var imagem in imagens)
                w.Write(imagem);
        }
    }
}
'@

# Prévia em PNG (fora do Git) para conferir o desenho
$previa = Join-Path (Split-Path $PSScriptRoot -Parent) '.tmp\icone'
New-Item -ItemType Directory -Force $previa | Out-Null
[GeradorIcone]::Gerar($destino, $previa)
Write-Host "Ícone gerado: $destino"
