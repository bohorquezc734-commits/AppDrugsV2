import React, { useEffect, useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { drugsService, Drug } from '../services/drugs';
import { authService } from '../services/auth';
import {
  appointmentsService,
  AppointmentDto,
  CreateAppointmentDetailRequest,
} from '../services/appointments';
import { gestoresService, GestorDto } from '../services/gestores';
import api from '../services/api';
import { inventoriesService, InventoryDto } from '../services/inventories';
import { toast } from 'react-toastify';
import MainLayout from '../components/Layout/MainLayout';
import type { AnyTab } from '../components/Layout/Sidebar';
import Configuracion from '../components/Profile/Configuracion';
import { AppointmentQrCard } from '../components/Appointments/AppointmentQrCard';

interface CartItem {
  inventoryId: number;
  drugName: string;
  quantity: number;
}

type TabType = 'medicamentos' | 'nuevo-turno' | 'mis-turnos' | 'configuracion';

const STATUS_LABELS: Record<number, { label: string; color: string }> = {
  1: { label: 'Recibido',   color: '#3b82f6' },
  2: { label: 'En Proceso', color: '#f59e0b' },
  3: { label: 'Entregado',  color: '#10b981' },
  4: { label: 'Cancelado',  color: '#ef4444' },
};

const UserDashboard: React.FC = () => {
  const navigate    = useNavigate();
  const user        = authService.getUser();
  const [activeTab, setActiveTab] = useState<TabType>('medicamentos');

  // ─── Medicamentos ────────────────────────────────────────────────────
  const [drugs, setDrugs]           = useState<Drug[]>([]);
  const [loadingDrugs, setLoadingDrugs] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 10;

  // ─── Nuevo turno ─────────────────────────────────────────────────────
  const [gestores, setGestores]         = useState<GestorDto[]>([]);
  const [selectedGestor, setSelectedGestor] = useState<number>(0);
  const [sedeInventories, setSedeInventories] = useState<InventoryDto[]>([]);
  const [cart, setCart]                 = useState<CartItem[]>([]);
  const [selectedInventory, setSelectedInventory] = useState<number>(0);
  const [qty, setQty]                   = useState<number>(1);
  const [archivo, setArchivo]           = useState<File | null>(null);
  const [submitting, setSubmitting]     = useState(false);
  const [loadingInventory, setLoadingInventory] = useState(false);

  // ─── Mis turnos ──────────────────────────────────────────────────────
  const [appointments, setAppointments]     = useState<AppointmentDto[]>([]);
  const [loadingAppts, setLoadingAppts]     = useState(false);

  // Verificar rol
  useEffect(() => {
    if (!authService.isUser()) {
      navigate('/dashboard');
    }
  }, [navigate]);

  // Cargar medicamentos
  const loadDrugs = useCallback(async (page = 1) => {
    try {
      setLoadingDrugs(true);
      const data = await drugsService.getAll({ searchTerm: searchTerm || undefined, page, pageSize });
      setDrugs(data);
      setCurrentPage(page);
    } catch {
      toast.error('Error cargando medicamentos');
    } finally {
      setLoadingDrugs(false);
    }
  }, [searchTerm]);

  useEffect(() => { loadDrugs(1); }, []);

  // Cargar gestores cuando se abre "Nuevo Turno"
  useEffect(() => {
    if (activeTab === 'nuevo-turno' && gestores.length === 0) {
      gestoresService.getAll()
        .then(data => setGestores(data))
        .catch(() => toast.error('Error cargando sedes farmacéuticas'));
    }
  }, [activeTab, gestores.length]);

  // Cargar inventario cuando cambia la sede seleccionada
  useEffect(() => {
    if (selectedGestor > 0) {
      setLoadingInventory(true);
      inventoriesService.getAll({ gestorFarmaceuticoId: selectedGestor, onlyActive: true })
        .then(data => {
          setSedeInventories(data);
          if (data.length === 0) {
            toast.warn('Esta sede no tiene medicamentos disponibles en inventario');
          }
        })
        .catch((err) => {
          console.error('Error cargando inventario:', err);
          toast.error('Error cargando inventario de la sede');
        })
        .finally(() => setLoadingInventory(false));
    } else {
      setSedeInventories([]);
    }
  }, [selectedGestor]);

  // Cargar mis turnos
  const loadAppointments = useCallback(async () => {
    setLoadingAppts(true);
    try {
      const data = await appointmentsService.getMyAppointments();
      setAppointments(data);
    } catch {
      toast.error('Error cargando tus turnos');
    } finally {
      setLoadingAppts(false);
    }
  }, []);

  useEffect(() => {
    if (activeTab === 'mis-turnos') loadAppointments();
  }, [activeTab, loadAppointments]);

  // ─── Carrito ─────────────────────────────────────────────────────────
  const addToCart = () => {
    const inv = sedeInventories.find(i => i.id === selectedInventory);
    if (!inv) { toast.warn('Selecciona un medicamento'); return; }
    if (qty < 1)  { toast.warn('La cantidad debe ser al menos 1'); return; }
    if (qty > inv.quantity) { toast.warn(`Solo hay ${inv.quantity} en stock para esta sede`); return; }
    if (cart.find(c => c.inventoryId === inv.id)) {
      toast.warn('Ese medicamento ya está en el pedido');
      return;
    }
    setCart(prev => [...prev, { inventoryId: inv.id, drugName: inv.drugName, quantity: qty }]);
    setSelectedInventory(0);
    setQty(1);
  };

  const removeFromCart = (inventoryId: number) =>
    setCart(prev => prev.filter(c => c.inventoryId !== inventoryId));

  // ─── Crear turno ──────────────────────────────────────────────────────
  const handleCreateAppointment = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedGestor) { toast.warn('Selecciona una sede'); return; }
    if (cart.length === 0) { toast.warn('Agrega al menos un medicamento al pedido'); return; }

    setSubmitting(true);
    try {
      const details: CreateAppointmentDetailRequest[] = cart.map(c => ({
        inventoryId: c.inventoryId,
        quantity: c.quantity,
      }));

      await appointmentsService.create(selectedGestor, details, archivo || undefined);
      toast.success('¡Turno creado exitosamente!');
      setCart([]);
      setSelectedGestor(0);
      setArchivo(null);
      setActiveTab('mis-turnos');
    } catch (err: any) {
      const msg = err.response?.data?.error || 'Error al crear el turno';
      toast.error(msg);
    } finally {
      setSubmitting(false);
    }
  };

  // ─── UI ───────────────────────────────────────────────────────────────
  const SECTION_LABELS: Record<TabType, string> = {
    medicamentos: 'Catálogo',
    'nuevo-turno': 'Agendar Turno',
    'mis-turnos': 'Mis Turnos',
    configuracion: 'Configuración de Perfil'
  };

  return (
    <MainLayout
      activeTab={activeTab as AnyTab}
      onTabChange={(tab) => setActiveTab(tab as TabType)}
      role="user"
      sectionLabel={SECTION_LABELS[activeTab]}
    >
      <div style={{ maxWidth: 1000, margin: '0 auto' }}>

        {/* ── TAB: Configuracion ───────────────────────────────────────── */}
        {activeTab === 'configuracion' && (
          <Configuracion />
        )}

        {/* ── TAB: Medicamentos ───────────────────────────────────────── */}
        {activeTab === 'medicamentos' && (
          <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20, flexWrap: 'wrap', gap: 12 }}>
              <h2 style={{ fontSize: 24, fontWeight: 700, color: '#1e293b', margin: 0 }}>📋 Lista de Medicamentos</h2>
              <form onSubmit={e => { e.preventDefault(); loadDrugs(1); }} style={{ display: 'flex', gap: 8 }}>
                <input
                  type="text"
                  value={searchTerm}
                  onChange={e => setSearchTerm(e.target.value)}
                  placeholder="Buscar medicamento..."
                  style={{ padding: '8px 14px', borderRadius: 8, border: '1px solid #cbd5e1', fontSize: 14, width: 220 }}
                />
                <button type="submit" style={{ background: '#2563eb', color: '#fff', border: 'none', borderRadius: 8, padding: '8px 18px', cursor: 'pointer', fontWeight: 600 }}>
                  🔍 Buscar
                </button>
              </form>
            </div>

            <div style={{ background: '#eff6ff', border: '1px solid #bfdbfe', borderRadius: 10, padding: '12px 16px', marginBottom: 20 }}>
              <p style={{ margin: 0, fontSize: 14, color: '#1e40af' }}>
                <strong>📌 Rol: Afiliado</strong> — Puedes ver medicamentos y crear turnos de entrega.
              </p>
            </div>

            {loadingDrugs ? (
              <div style={{ textAlign: 'center', padding: 60, color: '#64748b' }}>⏳ Cargando medicamentos...</div>
            ) : drugs.length === 0 ? (
              <div style={{ textAlign: 'center', padding: 60, background: '#fff', borderRadius: 12, color: '#64748b' }}>No hay medicamentos disponibles</div>
            ) : (
              <>
                <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(240px, 1fr))', gap: 16 }}>
                  {drugs.map(drug => (
                    <div key={drug.id} style={{ background: '#fff', borderRadius: 12, padding: 18, boxShadow: '0 2px 8px rgba(0,0,0,0.07)', transition: 'box-shadow 0.2s' }}>
                      <h3 style={{ fontWeight: 700, fontSize: 16, color: '#1e293b', margin: '0 0 4px' }}>{drug.name}</h3>
                      <p style={{ fontSize: 13, color: '#94a3b8', margin: '0 0 6px' }}>{drug.genericName}</p>
                      <p style={{ fontSize: 13, color: '#64748b', margin: '0 0 4px' }}>🧪 {drug.laboratory}</p>
                      <p style={{ fontSize: 20, fontWeight: 700, color: '#16a34a', margin: '8px 0 4px' }}>${drug.price.toFixed(2)}</p>
                      <p style={{ fontSize: 13, color: '#64748b', margin: '0 0 2px' }}>📦 Stock: {drug.stock}</p>
                      <p style={{ fontSize: 13, color: '#64748b', margin: 0 }}>📂 {drug.category}</p>
                      {drug.requiresPrescription && (
                        <span style={{ display: 'inline-block', marginTop: 8, background: '#fef9c3', color: '#854d0e', borderRadius: 6, padding: '2px 8px', fontSize: 12, fontWeight: 600 }}>📋 Requiere receta</span>
                      )}
                      <div style={{ marginTop: 12, background: '#f8fafc', borderRadius: 8, padding: '8px 10px', textAlign: 'center' }}>
                        <span style={{ fontSize: 12, color: '#94a3b8' }}>📖 Solo visualización</span>
                      </div>
                    </div>
                  ))}
                </div>

                <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', gap: 16, marginTop: 24 }}>
                  <button onClick={() => loadDrugs(currentPage - 1)} disabled={currentPage === 1}
                    style={{ padding: '8px 18px', borderRadius: 8, border: '1px solid #e2e8f0', background: currentPage === 1 ? '#f1f5f9' : '#fff', cursor: currentPage === 1 ? 'not-allowed' : 'pointer', color: '#475569' }}>
                    ← Anterior
                  </button>
                  <span style={{ fontSize: 14, color: '#64748b' }}>Página {currentPage}</span>
                  <button onClick={() => loadDrugs(currentPage + 1)} disabled={drugs.length < pageSize}
                    style={{ padding: '8px 18px', borderRadius: 8, border: '1px solid #e2e8f0', background: drugs.length < pageSize ? '#f1f5f9' : '#fff', cursor: drugs.length < pageSize ? 'not-allowed' : 'pointer', color: '#475569' }}>
                    Siguiente →
                  </button>
                </div>
              </>
            )}
          </div>
        )}

        {/* ── TAB: Crear Turno ────────────────────────────────────────── */}
        {activeTab === 'nuevo-turno' && (
          <div>
            <h2 style={{ fontSize: 24, fontWeight: 700, color: '#1e293b', marginBottom: 24 }}>📋 Crear Nuevo Turno</h2>

            <form onSubmit={handleCreateAppointment}>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 24 }}>
                {/* Columna izquierda */}
                <div>
                  {/* Seleccionar sede */}
                  <div style={{ background: '#fff', borderRadius: 12, padding: 20, boxShadow: '0 2px 8px rgba(0,0,0,0.07)', marginBottom: 20 }}>
                    <h3 style={{ fontSize: 16, fontWeight: 700, color: '#1e293b', marginBottom: 14 }}>🏪 1. Selecciona la Sede Farmacéutica</h3>
                    {gestores.length === 0 ? (
                      <p style={{ color: '#94a3b8', fontSize: 14 }}>⏳ Cargando sedes...</p>
                    ) : (
                      <select
                        value={selectedGestor}
                        onChange={e => {
                          setSelectedGestor(Number(e.target.value));
                          setCart([]); // Reset cart if Sede changes
                          setSelectedInventory(0);
                        }}
                        style={{ width: '100%', padding: '10px 14px', borderRadius: 8, border: '1px solid #cbd5e1', fontSize: 14, color: '#1e293b', background: '#fff' }}
                        required
                      >
                        <option value={0}>-- Selecciona una sede --</option>
                        {gestores.map(g => (
                          <option key={g.id} value={g.id}>{g.nombreSede} — {g.direccion}</option>
                        ))}
                      </select>
                    )}
                  </div>

                  {/* Archivo de autorización */}
                  <div style={{ background: '#fff', borderRadius: 12, padding: 20, boxShadow: '0 2px 8px rgba(0,0,0,0.07)' }}>
                    <h3 style={{ fontSize: 16, fontWeight: 700, color: '#1e293b', marginBottom: 6 }}>📎 2. Archivo de Autorización <span style={{ fontWeight: 400, color: '#94a3b8', fontSize: 13 }}>(opcional)</span></h3>
                    <p style={{ fontSize: 13, color: '#64748b', marginBottom: 12 }}>Adjunta la autorización médica si algún medicamento la requiere.</p>
                    <input
                      type="file"
                      accept=".pdf,.jpg,.jpeg,.png"
                      onChange={e => setArchivo(e.target.files?.[0] || null)}
                      style={{ fontSize: 14, color: '#475569' }}
                    />
                    {archivo && <p style={{ fontSize: 13, color: '#16a34a', marginTop: 6 }}>✅ {archivo.name}</p>}
                  </div>
                </div>

                {/* Columna derecha: agregar medicamentos */}
                <div>
                  <div style={{ background: '#fff', borderRadius: 12, padding: 20, boxShadow: '0 2px 8px rgba(0,0,0,0.07)', marginBottom: 20 }}>
                    <h3 style={{ fontSize: 16, fontWeight: 700, color: '#1e293b', marginBottom: 14 }}>💊 3. Medicamentos del Pedido</h3>

                    {loadingInventory && (
                      <p style={{ fontSize: 13, color: '#3b82f6', marginBottom: 8 }}>⏳ Cargando medicamentos de la sede...</p>
                    )}
                    {!loadingInventory && selectedGestor > 0 && sedeInventories.length === 0 && (
                      <div style={{ background: '#fef9c3', border: '1px solid #fde047', borderRadius: 8, padding: '10px 14px', marginBottom: 8 }}>
                        <p style={{ margin: 0, fontSize: 13, color: '#854d0e' }}>
                          ⚠️ Esta sede no tiene medicamentos en inventario. Contacta al administrador.
                        </p>
                      </div>
                    )}
                    <div style={{ display: 'flex', gap: 8, marginBottom: 12 }}>
                      <select
                        value={selectedInventory}
                        onChange={e => setSelectedInventory(Number(e.target.value))}
                        disabled={selectedGestor === 0 || loadingInventory || sedeInventories.length === 0}
                        style={{ flex: 1, padding: '8px 12px', borderRadius: 8, border: '1px solid #cbd5e1', fontSize: 13, background: (selectedGestor === 0 || sedeInventories.length === 0) ? '#f1f5f9' : '#fff' }}
                      >
                        <option value={0}>-- Selecciona medicamento --</option>
                        {sedeInventories.map(inv => (
                          <option key={inv.id} value={inv.id}>{inv.drugName} (Stock: {inv.quantity})</option>
                        ))}
                      </select>
                      <input
                        type="number"
                        min={1}
                        value={qty}
                        onChange={e => setQty(Number(e.target.value))}
                        style={{ width: 70, padding: '8px 10px', borderRadius: 8, border: '1px solid #cbd5e1', fontSize: 13 }}
                      />
                      <button type="button" onClick={addToCart}
                        style={{ background: '#2563eb', color: '#fff', border: 'none', borderRadius: 8, padding: '8px 14px', cursor: 'pointer', fontWeight: 600, fontSize: 13 }}>
                        + Agregar
                      </button>
                    </div>

                    {cart.length === 0 ? (
                      <div style={{ textAlign: 'center', padding: '24px 0', color: '#94a3b8', fontSize: 14, border: '2px dashed #e2e8f0', borderRadius: 8 }}>
                        Ningún medicamento agregado aún
                      </div>
                    ) : (
                      <div>
                        {cart.map(item => (
                          <div key={item.inventoryId} style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', padding: '10px 12px', background: '#f8fafc', borderRadius: 8, marginBottom: 6 }}>
                            <div>
                              <p style={{ margin: 0, fontWeight: 600, fontSize: 14, color: '#1e293b' }}>{item.drugName}</p>
                              <p style={{ margin: 0, fontSize: 13, color: '#64748b' }}>Cantidad: {item.quantity}</p>
                            </div>
                            <button type="button" onClick={() => removeFromCart(item.inventoryId)}
                              style={{ background: '#fee2e2', color: '#ef4444', border: 'none', borderRadius: 6, padding: '4px 10px', cursor: 'pointer', fontWeight: 600 }}>
                              ✕
                            </button>
                          </div>
                        ))}
                      </div>
                    )}
                  </div>

                  <button
                    type="submit"
                    disabled={submitting}
                    style={{ width: '100%', padding: '14px', background: submitting ? '#93c5fd' : '#2563eb', color: '#fff', border: 'none', borderRadius: 10, fontSize: 16, fontWeight: 700, cursor: submitting ? 'not-allowed' : 'pointer', boxShadow: '0 4px 12px rgba(37,99,235,0.3)', transition: 'background 0.2s' }}
                  >
                    {submitting ? '⏳ Enviando...' : '📋 Crear Turno'}
                  </button>
                </div>
              </div>
            </form>
          </div>
        )}

        {/* ── TAB: Mis Turnos ─────────────────────────────────────────── */}
        {activeTab === 'mis-turnos' && (
          <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
              <h2 style={{ fontSize: 24, fontWeight: 700, color: '#1e293b', margin: 0 }}>📁 Mis Turnos</h2>
              <button onClick={loadAppointments} style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '8px 16px', cursor: 'pointer', fontSize: 14, color: '#475569', fontWeight: 600 }}>
                🔄 Actualizar
              </button>
            </div>

            {loadingAppts ? (
              <div style={{ textAlign: 'center', padding: 60, color: '#64748b' }}>⏳ Cargando tus turnos...</div>
            ) : appointments.length === 0 ? (
              <div style={{ textAlign: 'center', padding: 60, background: '#fff', borderRadius: 12, color: '#64748b' }}>
                <p style={{ fontSize: 18, marginBottom: 10 }}>📭 No tienes turnos aún</p>
                <button onClick={() => setActiveTab('nuevo-turno')}
                  style={{ background: '#2563eb', color: '#fff', border: 'none', borderRadius: 8, padding: '10px 22px', cursor: 'pointer', fontWeight: 600, fontSize: 14 }}>
                  + Crear mi primer turno
                </button>
              </div>
            ) : (
              <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
                {appointments.map(apt => {
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
                            {apt.details.map(d => (
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
            )}
          </div>
        )}
      </div>
    </MainLayout>
  );
};

export default UserDashboard;
