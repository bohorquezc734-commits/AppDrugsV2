namespace AppDrugsV2.Application.Common.Interfaces
{
    /// <summary>
    /// Contrato para el servicio de generación de códigos QR.
    /// Abstrae la librería concreta (QRCoder, SkiaSharp, etc.)
    /// de la capa Application.
    /// </summary>
    public interface IQrCodeService
    {
        /// <summary>
        /// Genera un código QR a partir del texto proporcionado
        /// y devuelve la imagen en Base64 (PNG).
        /// </summary>
        /// <param name="content">Texto o URL que codificará el QR.</param>
        /// <returns>String Base64 de la imagen PNG del QR.</returns>
        string GenerateBase64(string content);
    }
}
