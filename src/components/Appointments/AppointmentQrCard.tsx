import React, { useState, useCallback } from 'react';
import { toast } from 'react-toastify';
import { appointmentsService, AppointmentDto } from '../../services/appointments';
import { useNotificationHub, QrReadyPayload } from '../../hooks/useNotificationHub';
import { authService } from '../../services/auth';

interface AppointmentQrCardProps {
  appointment: AppointmentDto;
}

const HUB_URL = '/hubs/notifications';

/**
 * Paso 4 – Tarjeta de turno con generación de QR y notificaciones en tiempo real.
 *
 * Flujo:
 * 1. El usuario pulsa "Generar QR" → llama a POST /appointments/{id}/qr.
 * 2. El backend genera el QR, lo guarda y emite "QrReady" vía SignalR.
 * 3. El hook useNotificationHub recibe el evento y actualiza el estado local
 *    SIN necesidad de recargar la página.
 */
export const AppointmentQrCard: React.FC<AppointmentQrCardProps> = ({ appointment }) => {
  const [qrBase64, setQrBase64]   = useState<string | undefined>(appointment.qrCodeBase64);
  const [isLoading, setIsLoading] = useState(false);

  // Obtenemos el token del localStorage (ya gestionado por authService)
  const token = localStorage.getItem('token');

  // Callback estable que actualiza el QR cuando llega el evento en tiempo real
  const handleQrReady = useCallback((payload: QrReadyPayload) => {
    if (payload.appointmentId === appointment.id) {
      toast.success(`✅ ${payload.message}`);
      // Recargamos el QR desde el servidor para obtener el Base64 fresco
      appointmentsService
        .generateQr(appointment.id)
        .then(base64 => setQrBase64(base64))
        .catch(() => {/* ya notificamos via toast */});
    }
  }, [appointment.id]);

  // Conectar al Hub y suscribirse a "QrReady"
  useNotificationHub({
    hubUrl: HUB_URL,
    token,
    onQrReady: handleQrReady,
  });

  /** Genera el QR de forma explícita mediante el botón */
  const handleGenerateQr = async () => {
    try {
      setIsLoading(true);
      const base64 = await appointmentsService.generateQr(appointment.id);
      setQrBase64(base64);
      toast.success('QR generado exitosamente 🎉');
    } catch (err: any) {
      const msg = err?.response?.data?.message ?? 'Error al generar el QR';
      toast.error(msg);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="bg-white rounded-2xl shadow-md border border-gray-100 overflow-hidden">
      
      {/* Cabecera */}
      <div className="bg-gradient-to-r from-blue-600 to-blue-500 px-5 py-4 flex items-center justify-between">
        <div>
          <p className="text-white font-bold text-base">Turno #{appointment.id}</p>
          <p className="text-blue-100 text-xs mt-0.5">{appointment.sedeName}</p>
        </div>
        <span className={`text-xs font-semibold px-3 py-1 rounded-full ${
          appointment.status === 3
            ? 'bg-green-100 text-green-700'
            : appointment.status === 4
            ? 'bg-red-100 text-red-700'
            : 'bg-yellow-100 text-yellow-700'
        }`}>
          {appointment.statusName}
        </span>
      </div>

      {/* Cuerpo */}
      <div className="p-5 flex flex-col items-center gap-4">

        {qrBase64 ? (
          /* ── QR generado: mostrar imagen ── */
          <div className="flex flex-col items-center gap-3">
            <div className="border-4 border-blue-100 rounded-xl p-2 shadow-inner bg-gray-50">
              <img
                src={`data:image/png;base64,${qrBase64}`}
                alt={`QR Turno #${appointment.id}`}
                className="w-48 h-48 rounded-lg"
              />
            </div>
            <p className="text-xs text-gray-400 text-center">
              Presenta este QR en la sede para retirar tu pedido
            </p>
            <button
              onClick={handleGenerateQr}
              disabled={isLoading}
              className="text-xs text-blue-500 hover:text-blue-700 underline underline-offset-2 transition-colors disabled:opacity-50"
            >
              {isLoading ? 'Regenerando...' : 'Regenerar QR'}
            </button>
          </div>
        ) : (
          /* ── QR no generado: mostrar botón ── */
          <div className="flex flex-col items-center gap-4 py-4">
            {/* Placeholder visual */}
            <div className="w-48 h-48 bg-gray-50 border-2 border-dashed border-gray-200 rounded-xl flex flex-col items-center justify-center gap-2">
              <svg className="w-12 h-12 text-gray-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
                  d="M12 4v1m6 11h2m-6 0h-2v4m0-11v3m0 0h.01M12 12h4.01M16 20h4M4 12h4m12 0h.01M5 8h2a1 1 0 001-1V5a1 1 0 00-1-1H5a1 1 0 00-1 1v2a1 1 0 001 1zm12 0h2a1 1 0 001-1V5a1 1 0 00-1-1h-2a1 1 0 00-1 1v2a1 1 0 001 1zM5 20h2a1 1 0 001-1v-2a1 1 0 00-1-1H5a1 1 0 00-1 1v2a1 1 0 001 1z"
                />
              </svg>
              <p className="text-xs text-gray-400 text-center px-4">
                El QR de tu turno no ha sido generado aún
              </p>
            </div>

            <button
              onClick={handleGenerateQr}
              disabled={isLoading}
              className="w-full flex items-center justify-center gap-2 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-300 text-white font-semibold py-3 px-6 rounded-xl transition-all duration-200 shadow-md hover:shadow-lg active:scale-95 disabled:cursor-not-allowed"
            >
              {isLoading ? (
                <>
                  <svg className="w-4 h-4 animate-spin" viewBox="0 0 24 24" fill="none">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"/>
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8z"/>
                  </svg>
                  Generando...
                </>
              ) : (
                <>
                  <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                      d="M12 4v1m6 11h2m-6 0h-2v4m0-11v3m0 0h.01M12 12h4.01M16 20h4M4 12h4m12 0h.01M5 8h2a1 1 0 001-1V5a1 1 0 00-1-1H5a1 1 0 00-1-1zm12 0h2a1 1 0 001-1V5a1 1 0 00-1-1h-2a1 1 0 00-1 1v2a1 1 0 001 1zM5 20h2a1 1 0 001-1v-2a1 1 0 00-1-1H5a1 1 0 00-1 1v2a1 1 0 001 1z"
                    />
                  </svg>
                  Generar QR
                </>
              )}
            </button>
          </div>
        )}
      </div>
    </div>
  );
};

export default AppointmentQrCard;
