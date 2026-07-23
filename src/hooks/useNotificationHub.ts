import { useEffect, useRef, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';

/** Payload del evento "QrReady" emitido por el Hub */
export interface QrReadyPayload {
  appointmentId: number;
  message: string;
}

interface UseNotificationHubOptions {
  /** URL del Hub: "/hubs/notifications" */
  hubUrl: string;
  /** JWT del usuario autenticado (se envía en la query string para SignalR) */
  token: string | null;
  /** Callback invocado cada vez que el Hub emite el evento "QrReady" */
  onQrReady?: (payload: QrReadyPayload) => void;
}

/**
 * Paso 3 – Custom Hook para SignalR.
 *
 * - Crea y gestiona la conexión al Hub de notificaciones.
 * - Reconexión automática con backoff exponencial.
 * - Limpia la conexión cuando el componente se desmonta.
 * - Expone `subscribe` para registrar handlers en caliente.
 */
export function useNotificationHub({
  hubUrl,
  token,
  onQrReady,
}: UseNotificationHubOptions) {
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  // Guardamos el callback en un ref para no recrear el efecto al cambiar el handler
  const onQrReadyRef = useRef(onQrReady);
  useEffect(() => { onQrReadyRef.current = onQrReady; }, [onQrReady]);

  useEffect(() => {
    // Si no hay token todavía (usuario no autenticado), no conectamos
    if (!token) return;

    const apiBase = process.env.REACT_APP_API_URL
      ? process.env.REACT_APP_API_URL.replace('/api', '')
      : 'https://localhost:7068';

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${apiBase}${hubUrl}`, {
        // SignalR necesita el token vía query string porque WS no soporta headers
        accessTokenFactory: () => token,
        transport: signalR.HttpTransportType.WebSockets |
                   signalR.HttpTransportType.ServerSentEvents |
                   signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000]) // backoff manual
      .configureLogging(
        process.env.NODE_ENV === 'development'
          ? signalR.LogLevel.Information
          : signalR.LogLevel.Error
      )
      .build();

    // Registrar listener del evento "QrReady"
    connection.on('QrReady', (payload: QrReadyPayload) => {
      onQrReadyRef.current?.(payload);
    });

    // Estado de reconexión (logging)
    connection.onreconnecting(() =>
      console.info('[SignalR] Reconectando...')
    );
    connection.onreconnected(() =>
      console.info('[SignalR] Reconexión exitosa.')
    );
    connection.onclose(() =>
      console.warn('[SignalR] Conexión cerrada.')
    );

    // Iniciar
    connection
      .start()
      .then(() => console.info('[SignalR] Conectado al hub:', hubUrl))
      .catch(err => console.error('[SignalR] Error al conectar:', err));

    connectionRef.current = connection;

    // Cleanup: detener la conexión al desmontar
    return () => {
      connection.stop().catch(() => {});
      connectionRef.current = null;
    };
  }, [hubUrl, token]); // Se reconecta si cambia la URL o el token

  /**
   * Función utilitaria para suscribirse a cualquier evento adicional del Hub.
   * Se puede usar si en el futuro el Hub emite más eventos.
   */
  const subscribe = useCallback(
    <T>(eventName: string, handler: (payload: T) => void) => {
      connectionRef.current?.on(eventName, handler);
      return () => connectionRef.current?.off(eventName, handler);
    },
    []
  );

  return { subscribe };
}
