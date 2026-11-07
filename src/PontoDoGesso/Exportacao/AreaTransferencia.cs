using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Forms;

namespace PontoDoGesso.Exportacao
{
    /// <summary>Coloca a imagem do orçamento na área de transferência, para colar no WhatsApp Web com Ctrl+V.</summary>
    public static class AreaTransferencia
    {
        /// <summary>Bitmap comum e também PNG: alguns navegadores só aceitam colar um dos dois.</summary>
        public static DataObject CriarDados(Bitmap imagem)
        {
            var dados = new DataObject();
            dados.SetImage(imagem);

            var png = new MemoryStream();
            imagem.Save(png, ImageFormat.Png);
            png.Position = 0;
            dados.SetData("PNG", false, png);
            return dados;
        }

        /// <summary>A área de transferência só funciona em thread STA; fora dela, abre uma só para isso.</summary>
        public static void CopiarImagem(Bitmap imagem)
        {
            var dados = CriarDados(imagem);

            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                Clipboard.SetDataObject(dados, true, 5, 100);
                return;
            }

            ExceptionDispatchInfo falha = null;
            var thread = new Thread(() =>
            {
                try
                {
                    Clipboard.SetDataObject(dados, true, 5, 100);
                }
                catch (Exception erro)
                {
                    falha = ExceptionDispatchInfo.Capture(erro);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            falha?.Throw();
        }
    }
}
