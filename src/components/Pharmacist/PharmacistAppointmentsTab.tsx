import React, { useState, useEffect, useCallback } from 'react';
import { toast } from 'react-toastify';
import { appointmentsService, AppointmentDto } from '../../services/appointments';
import { useDrugiStore } from '../../store/useDrugiStore';
import { DragDropContext, Droppable, Draggable, DropResult } from '@hello-pangea/dnd';

const COLUMNS = [
  { id: 1, title: 'Recibido', color: '#3b82f6', bg: '#eff6ff' },
  { id: 2, title: 'En Proceso', color: '#f59e0b', bg: '#fffbeb' },
  { id: 3, title: 'Entregado', color: '#10b981', bg: '#ecfdf5' },
  { id: 4, title: 'Cancelado', color: '#ef4444', bg: '#fef2f2' },
];

const PharmacistAppointmentsTab: React.FC = () => {
  const [appointments, setAppointments] = useState<AppointmentDto[]>([]);
  const [loading, setLoading] = useState(true);
  const { showMessage } = useDrugiStore();

  const loadAppointments = useCallback(async () => {
    try {
      setLoading(true);
      const data = await appointmentsService.getAll();
      setAppointments(data);
    } catch { toast.error('Error cargando turnos'); }
    finally { setLoading(false); }
  }, []);

  useEffect(() => {
    loadAppointments();
  }, [loadAppointments]);

  const onDragEnd = async (result: DropResult) => {
    const { destination, source, draggableId } = result;

    if (!destination) return;
    if (destination.droppableId === source.droppableId && destination.index === source.index) return;

    const aptId = Number(draggableId);
    const newStatusId = Number(destination.droppableId);

    // Optimistic UI update
    const previousAppointments = [...appointments];
    setAppointments(prev => prev.map(a => a.id === aptId ? { ...a, status: newStatusId } : a));

    try {
      await appointmentsService.updateStatus(aptId, newStatusId);
      toast.success('Estado actualizado');
      if (newStatusId === 3) {
        showMessage(`¡Excelente! Has entregado el turno #${aptId}. 🎉`, 'feliz');
      }
    } catch (err: any) {
      toast.error('Error al actualizar estado');
      setAppointments(previousAppointments); // Revert
    }
  };

  if (loading) return <p style={{ color: '#64748b' }}>Cargando tablero...</p>;

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
        <h2 style={{ fontSize: 24, fontWeight: 700, color: '#1e293b', margin: 0 }}>Tablero de Despacho</h2>
        <button onClick={loadAppointments} style={{ background: '#fff', border: '1px solid #cbd5e1', borderRadius: 8, padding: '8px 16px', cursor: 'pointer', fontWeight: 600 }}>
          🔄 Actualizar
        </button>
      </div>

      <DragDropContext onDragEnd={onDragEnd}>
        <div style={{ display: 'flex', gap: 16, overflowX: 'auto', paddingBottom: 16 }}>
          {COLUMNS.map(col => {
            const colAppointments = appointments.filter(a => a.status === col.id);
            return (
              <div key={col.id} style={{ flex: '1 1 300px', minWidth: 280, display: 'flex', flexDirection: 'column', background: '#f8fafc', borderRadius: 12, padding: 12, border: '1px solid #e2e8f0' }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 12 }}>
                  <span style={{ width: 12, height: 12, borderRadius: '50%', background: col.color }} />
                  <h3 style={{ margin: 0, fontSize: 16, fontWeight: 700, color: '#334155' }}>{col.title}</h3>
                  <span style={{ marginLeft: 'auto', background: col.bg, color: col.color, padding: '2px 8px', borderRadius: 12, fontSize: 12, fontWeight: 700 }}>
                    {colAppointments.length}
                  </span>
                </div>
                
                <Droppable droppableId={String(col.id)}>
                  {(provided, snapshot) => (
                    <div
                      ref={provided.innerRef}
                      {...provided.droppableProps}
                      style={{
                        flexGrow: 1, minHeight: 150, padding: 8, borderRadius: 8,
                        background: snapshot.isDraggingOver ? col.bg : 'transparent',
                        transition: 'background 0.2s ease',
                      }}
                    >
                      {colAppointments.map((apt, index) => (
                        <Draggable key={apt.id} draggableId={String(apt.id)} index={index}>
                          {(provided, snapshot) => (
                            <div
                              ref={provided.innerRef}
                              {...provided.draggableProps}
                              {...provided.dragHandleProps}
                              style={{
                                userSelect: 'none',
                                padding: 16,
                                margin: '0 0 12px 0',
                                background: snapshot.isDragging ? '#ffffff' : '#ffffff',
                                borderRadius: 10,
                                boxShadow: snapshot.isDragging ? '0 10px 25px rgba(0,0,0,0.1)' : '0 1px 3px rgba(0,0,0,0.05)',
                                border: '1px solid #e2e8f0',
                                transform: snapshot.isDragging ? 'scale(1.02)' : 'none',
                                ...provided.draggableProps.style,
                              }}
                            >
                              <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 8 }}>
                                <span style={{ fontWeight: 700, color: '#1e293b', fontSize: 14 }}>Turno #{apt.id}</span>
                                <span style={{ fontSize: 12, color: '#64748b' }}>{new Date(apt.createdAt).toLocaleDateString()}</span>
                              </div>
                              <p style={{ margin: '0 0 8px', fontSize: 13, color: '#475569' }}>👤 {apt.userName}</p>
                              <div style={{ fontSize: 12, color: '#64748b', background: '#f1f5f9', padding: 8, borderRadius: 6 }}>
                                {apt.details.map(d => `${d.drugName} (x${d.quantity})`).join(', ')}
                              </div>
                            </div>
                          )}
                        </Draggable>
                      ))}
                      {provided.placeholder}
                    </div>
                  )}
                </Droppable>
              </div>
            );
          })}
        </div>
      </DragDropContext>
    </div>
  );
};

export default PharmacistAppointmentsTab;
