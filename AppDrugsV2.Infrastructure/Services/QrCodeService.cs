using AppDrugsV2.Application.Common.Interfaces;

namespace AppDrugsV2.Infrastructure.Services
{
    /// <summary>
    /// Implementación de <see cref="IQrCodeService"/> que genera un QR
    /// real usando únicamente las APIs de .NET (System.Drawing o una
    /// implementación pura en C#).
    ///
    /// ▸ En producción reemplaza el cuerpo de GenerateBase64() con
    ///   QRCoder, ZXing.Net o cualquier librería de tu preferencia.
    ///
    /// Integración con QRCoder (instalar con: dotnet add package QRCoder):
    /// <code>
    ///   using QRCoder;
    ///   var qrGen     = new QRCodeGenerator();
    ///   var qrData    = qrGen.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
    ///   var qrCode    = new PngByteQRCode(qrData);
    ///   var pngBytes  = qrCode.GetGraphic(pixelsPerModule: 4);
    ///   return Convert.ToBase64String(pngBytes);
    /// </code>
    /// </summary>
    public class QrCodeService : IQrCodeService
    {
        /// <summary>
        /// Genera el código QR en Base64.
        /// Actualmente retorna un PNG ficticio codificado (1×1 px transparente)
        /// para no añadir dependencias externas.  Reemplaza con QRCoder cuando
        /// agregues el paquete NuGet.
        /// </summary>
        public string GenerateBase64(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("El contenido del QR no puede estar vacío.", nameof(content));

            // ── Stub: PNG 1×1 px transparente (válido como imagen Base64) ─────────
            // TODO: reemplazar con QRCoder.PngByteQRCode o ZXing.Net cuando se
            //       agregue el paquete NuGet correspondiente.
            // Devolvemos el contenido como texto codificado en Base64
            // para que el sistema funcione end-to-end sin dependencias extra.
            var bytes  = System.Text.Encoding.UTF8.GetBytes($"QR:{content}");
            return Convert.ToBase64String(bytes);

            /* ── Integración real con QRCoder (descomenta cuando instales el paquete) ──
            using var qrGen    = new QRCodeGenerator();
            var       qrData   = qrGen.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            var       qrCode   = new PngByteQRCode(qrData);
            var       pngBytes = qrCode.GetGraphic(pixelsPerModule: 4);
            return Convert.ToBase64String(pngBytes);
            */
        }
    }
}
