import React, { useEffect, useState } from 'react';
import { useMyAppointments } from '../../hooks/useAppointments';
import { AppointmentDto } from '../../services/appointments';
import { AppointmentSkeleton } from '../Common/AppointmentSkeleton';
import { ErrorBoundary } from '../Common/ErrorBoundary';
import { AppointmentQrCard } from '../Appointments/AppointmentQrCard';
import Lottie from 'lottie-react';

const STATUS_LABELS: Record<number, { label: string; color: string }> = {
  1: { label: 'Recibido',   color: '#3b82f6' },
  2: { label: 'En Proceso', color: '#f59e0b' },
  3: { label: 'Entregado',  color: '#10b981' },
  4: { label: 'Cancelado',  color: '#ef4444' },
};

interface MyAppointmentsTabProps {
  onCreateNewClick: () => void;
  isActive: boolean;
}

const MyAppointmentsTab: React.FC<MyAppointmentsTabProps> = ({ onCreateNewClick, isActive }) => {
  const { 
    data: appointments = [], 
    isLoading: loadingAppts, 
    isError: isErrorAppts,
    error: apptsError,
    refetch: refetchAppts
  } = useMyAppointments();

  const [filter, setFilter] = useState<'ALL' | 'PENDING' | 'COMPLETED' | 'CANCELLED'>('ALL');
  const [emptyAnimData, setEmptyAnimData] = useState<any>(null);

  useEffect(() => {
    // Usamos una URL pública confiable para la animación Lottie (calendario vacío)
    fetch('https://assets3.lottiefiles.com/packages/lf20_r0d3m5z3.json')
      .then(res => res.json())
      .then(data => setEmptyAnimData(data))
      .catch(err => console.error("Error al cargar animación Lottie", err));
  }, []);

  useEffect(() => {
    if (isActive) {
      refetchAppts();
    }
  }, [isActive, refetchAppts]);

  const filteredAppointments = appointments.filter((apt: AppointmentDto) => {
    if (filter === 'ALL') return true;
    if (filter === 'PENDING') return apt.status === 1 || apt.status === 2; // Recibido o En Proceso
    if (filter === 'COMPLETED') return apt.status === 3; // Entregado
    if (filter === 'CANCELLED') return apt.status === 4; // Cancelado
    return true;
  });

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20, flexWrap: 'wrap', gap: 10 }}>
        <h2 style={{ fontSize: 24, fontWeight: 700, color: '#1e293b', margin: 0 }}>📁 Mis Turnos</h2>
        
        <div style={{ display: 'flex', gap: 8, background: '#f8fafc', padding: '4px', borderRadius: '12px', border: '1px solid #e2e8f0' }}>
          {[
            { id: 'ALL', label: 'Todos' },
            { id: 'PENDING', label: 'Pendientes' },
            { id: 'COMPLETED', label: 'Entregados' },
            { id: 'CANCELLED', label: 'Cancelados' }
          ].map(f => (
            <button
              key={f.id}
              onClick={() => setFilter(f.id as any)}
              style={{
                padding: '6px 14px',
                borderRadius: '8px',
                border: 'none',
                background: filter === f.id ? '#fff' : 'transparent',
                color: filter === f.id ? '#2563eb' : '#64748b',
                fontWeight: filter === f.id ? 700 : 500,
                fontSize: 13,
                cursor: 'pointer',
                boxShadow: filter === f.id ? '0 2px 4px rgba(0,0,0,0.05)' : 'none',
                transition: 'all 0.2s'
              }}
            >
              {f.label}
            </button>
          ))}
        </div>

        <button onClick={() => refetchAppts()} style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '8px 16px', cursor: 'pointer', fontSize: 14, color: '#475569', fontWeight: 600 }}>
          🔄 Actualizar
        </button>
      </div>

      <ErrorBoundary>
        {(() => {
          if (loadingAppts) {
            return (
              <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
                <AppointmentSkeleton />
                <AppointmentSkeleton />
                <AppointmentSkeleton />
              </div>
            );
          }

          if (isErrorAppts) {
            throw apptsError || new Error('Ocurrió un error al cargar tus turnos');
          }

          if (filteredAppointments.length === 0) {
            return (
              <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', padding: '60px 20px', background: '#fff', borderRadius: 16, border: '1px dashed #cbd5e1', boxShadow: '0 4px 6px -1px rgba(0, 0, 0, 0.05)' }}>
                {emptyAnimData ? (
                  <Lottie animationData={emptyAnimData} style={{ width: 180, height: 180, marginBottom: 10 }} />
                ) : (
                  <div style={{ fontSize: 60, marginBottom: 10 }}>📭</div>
                )}
                <h3 style={{ margin: '0 0 8px', fontSize: 20, color: '#1e293b', fontWeight: 700 }}>
                  {filter === 'ALL' ? 'No tienes turnos registrados' : 'No hay turnos en este estado'}
                </h3>
                <p style={{ margin: '0 0 24px', color: '#64748b', fontSize: 14, textAlign: 'center', maxWidth: 400 }}>
                  {filter === 'ALL' 
                    ? 'Explora el catálogo de medicamentos y crea tu primer turno para recibirlos en tu sede preferida.'
                    : 'Intenta cambiar los filtros para ver otros turnos en tu historial.'}
                </p>
                {filter === 'ALL' && (
                  <button onClick={onCreateNewClick}
                    style={{ background: '#2563eb', color: '#fff', border: 'none', borderRadius: 10, padding: '12px 24px', cursor: 'pointer', fontWeight: 600, fontSize: 14, boxShadow: '0 4px 12px rgba(37,99,235,0.3)', transition: 'transform 0.2s' }}
                    onMouseEnter={e => (e.currentTarget as HTMLElement).style.transform = 'scale(1.05)'}
                    onMouseLeave={e => (e.currentTarget as HTMLElement).style.transform = 'scale(1)'}
                  >
                    + Crear mi primer turno
                  </button>
                )}
              </div>
            );
          }

          return (
            <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
              {filteredAppointments.map((apt: AppointmentDto) => {
                const statusInfo = STATUS_LABELS[apt.status] || { label: apt.statusName, color: '#64748b' };
                return (
                  <div key={apt.id} style={{ background: '#fff', borderRadius: 12, padding: 20, boxShadow: '0 2px 8px rgba(0,0,0,0.07)' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: 10 }}>
                      <div>
                        <p style={{ margin: '0 0 4px', fontWeight: 700, fontSize: 16, color: '#1e293b' }}>Turno #{apt.id}</p>
                        <p style={{ margin: '0 0 2px', fontSize: 14, color: '#64748b' }}>🏪 {apt.sedeName}</p>
                        <p style={{ margin: 0, fontSize: 13, color: '#94a3b8' }}>
                          📅 Creado: {new Date(apt.createdAt).toLocaleDateString('es-CO', { day: '2-digit', month: 'long', year: 'numeric' })}
                        </p>
                      </div>
                      <span style={{ background: statusInfo.color + '20', color: statusInfo.color, borderRadius: 20, padding: '4px 14px', fontWeight: 700, fontSize: 13, border: `1px solid ${statusInfo.color}40` }}>
                        {statusInfo.label}
                      </span>
                    </div>

                    {apt.details && apt.details.length > 0 && (
                      <div style={{ marginTop: 14, borderTop: '1px solid #f1f5f9', paddingTop: 12 }}>
                        <p style={{ margin: '0 0 8px', fontSize: 13, fontWeight: 600, color: '#475569' }}>Medicamentos solicitados:</p>
                        <div style={{ display: 'flex', flexWrap: 'wrap', gap: 6 }}>
                          {apt.details.map((d: any) => (
                            <span key={d.id} style={{ background: '#eff6ff', color: '#1d4ed8', borderRadius: 6, padding: '3px 10px', fontSize: 13 }}>
                              {d.drugName} × {d.quantity}
                            </span>
                          ))}
                        </div>
                      </div>
                    )}

                    {apt.observaciones && (
                      <div style={{ marginTop: 10, background: '#fefce8', borderRadius: 8, padding: '8px 12px' }}>
                        <p style={{ margin: 0, fontSize: 13, color: '#854d0e' }}>💬 {apt.observaciones}</p>
                      </div>
                    )}

                    {apt.fechaEntrega && (
                      <p style={{ marginTop: 8, fontSize: 13, color: '#16a34a', fontWeight: 600 }}>
                        📦 Fecha de entrega: {new Date(apt.fechaEntrega).toLocaleDateString('es-CO')}
                      </p>
                    )}

                    {/* ── QR del turno ── */}
                    <div style={{ marginTop: 16, borderTop: '1px solid #f1f5f9', paddingTop: 16 }}>
                      <AppointmentQrCard appointment={apt} />
                    </div>
                  </div>
                );
              })}
            </div>
          );
        })()}
      </ErrorBoundary>
    </div>
  );
};

export default MyAppointmentsTab;
