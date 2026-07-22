namespace AppDrugsV2.Application.Common.Interfaces
{
    /// <summary>
    /// Contrato de servicio de notificaciones en tiempo real.
    /// La capa Application depende de esta interfaz; la implementación
    /// (SignalR) vive en Infrastructure y nunca cruza esta frontera.
    /// </summary>
    public interface INotificationHubService
    {
        /// <summary>
        /// Envía una notificación a todos los clientes del usuario especificado.
        /// </summary>
        /// <param name="userId">ID del usuario destino.</param>
        /// <param name="eventName">Nombre del evento que escucha el cliente (ej. "QrReady").</param>
        /// <param name="payload">Objeto serializable con los datos del evento.</param>
        Task SendToUserAsync(int userId, string eventName, object payload, CancellationToken cancellationToken = default);

        /// <summary>
        /// Envía una notificación broadcast a todos los clientes conectados.
        /// </summary>
        Task SendToAllAsync(string eventName, object payload, CancellationToken cancellationToken = default);
    }
}
