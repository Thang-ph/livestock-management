using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace LivestockManagementSystemAPI.Helper.QRCodeGeneratorHelper
{
    public class QRCodeGeneratorHelper : IQRCodeGeneratorHelper
    {
        public byte[] GenerateQRCode(string text)
        {
            byte[] QRCode = null;
            if (!string.IsNullOrEmpty(text))
            {
                QRCodeGenerator qRCodeGenerator = new QRCodeGenerator();
                QRCodeData data = qRCodeGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
                BitmapByteQRCode bitmap = new BitmapByteQRCode(data);
                QRCode = bitmap.GetGraphic(20);
            }
            return QRCode;
        }

    }
}
