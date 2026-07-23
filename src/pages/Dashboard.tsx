import React, { useEffect, useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { drugsService, Drug } from '../services/drugs';
import { gestoresService, GestorDto } from '../services/gestores';
import { inventoriesService, InventoryDto } from '../services/inventories';
import { appointmentsService, AppointmentDto } from '../services/appointments';
import { authService } from '../services/auth';
import { toast } from 'react-toastify';
import { reportsService, downloadFile } from '../services/reports';
import ReportModal from '../components/Reports/ReportModal';
import MainLayout from '../components/Layout/MainLayout';
import type { AnyTab } from '../components/Layout/Sidebar';
import Configuracion from '../components/Profile/Configuracion';
import AuditLogs from './AuditLogs';
import { AppointmentQrCard } from '../components/Appointments/AppointmentQrCard';
import { useDrugiStore } from '../store/useDrugiStore';

type TabType = 'medicamentos' | 'sedes' | 'inventarios' | 'turnos' | 'reportes' | 'configuracion' | 'auditoria';

const STATUS_LABELS: Record<number, { label: string; color: string }> = {
  1: { label: 'Recibido', color: '#3b82f6' },
  2: { label: 'En Proceso', color: '#f59e0b' },
  3: { label: 'Entregado', color: '#10b981' },
  4: { label: 'Cancelado', color: '#ef4444' },
};

/* ─── Custom Dialog Component ──────────────────────── */
interface DialogProps {
  isOpen: boolean;
  title: string;
  message?: string;
  icon?: React.ReactNode;
  iconBg?: string;
  children?: React.ReactNode;
  confirmLabel?: string;
  confirmColor?: string;
  cancelLabel?: string;
  onConfirm: () => void;
  onCancel: () => void;
}

const CustomDialog: React.FC<DialogProps> = ({
  isOpen, title, message, icon, iconBg, children,
  confirmLabel = 'Confirmar', confirmColor = '#2563eb',
  cancelLabel = 'Cancelar', onConfirm, onCancel,
}) => {
  if (!isOpen) return null;
  return (
    <div style={{
      position: 'fixed', inset: 0, zIndex: 9999,
      background: 'rgba(15,23,42,0.5)',
      display: 'flex', alignItems: 'center', justifyContent: 'center',
      padding: 20, backdropFilter: 'blur(4px)',
    }} onClick={onCancel}>
      <div style={{
        background: '#fff', borderRadius: 18, padding: '28px 28px 24px',
        maxWidth: 400, width: '100%',
        boxShadow: '0 24px 60px rgba(0,0,0,0.18)',
        animation: 'dialogIn 0.2s ease',
      }} onClick={e => e.stopPropagation()}>
        {icon && (
          <div style={{
            width: 52, height: 52, borderRadius: 14,
            background: iconBg || '#eff6ff',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            margin: '0 auto 16px', fontSize: 24,
          }}>{icon}</div>
        )}
        <h3 style={{ fontWeight: 700, fontSize: 17, color: '#1e293b', textAlign: 'center', marginBottom: message ? 8 : 16 }}>
          {title}
        </h3>
        {message && (
          <p style={{ fontSize: 14, color: '#64748b', textAlign: 'center', marginBottom: 20, lineHeight: 1.5 }}>
            {message}
          </p>
        )}
        {children && <div style={{ marginBottom: 20 }}>{children}</div>}
        <div style={{ display: 'flex', gap: 10 }}>
          <button onClick={onCancel} style={{
            flex: 1, padding: '10px', borderRadius: 10, border: '1.5px solid #e2e8f0',
            background: '#f8fafc', color: '#475569', fontWeight: 600, fontSize: 14, cursor: 'pointer',
          }}>{cancelLabel}</button>
          <button onClick={onConfirm} style={{
            flex: 1, padding: '10px', borderRadius: 10, border: 'none',
            background: confirmColor, color: '#fff', fontWeight: 700, fontSize: 14, cursor: 'pointer',
            boxShadow: `0 4px 12px ${confirmColor}55`,
          }}>{confirmLabel}</button>
        </div>
      </div>
    </div>
  );
};

const Dashboard: React.FC = () => {
  const navigate = useNavigate();
  const user = authService.getUser();
  const [activeTab, setActiveTab] = useState<TabType>('medicamentos');
  const { showMessage } = useDrugiStore();

  // Permisos
  const isAdmin = authService.isAdmin();
  const isPharmacist = authService.isPharmacist();
  const isRegularUser = authService.isUser();

  useEffect(() => {
    if (isRegularUser) {
      navigate('/user-dashboard');
    }
  }, [isRegularUser, navigate]);

  // ==========================================
  // TAB: MEDICAMENTOS
  // ==========================================
  const [drugs, setDrugs] = useState<Drug[]>([]);
  const [loadingDrugs, setLoadingDrugs] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [showCreateDrugModal, setShowCreateDrugModal] = useState(false);
  const [showEditDrugModal, setShowEditDrugModal] = useState(false);
  const [selectedDrug, setSelectedDrug] = useState<Drug | null>(null);
  const [drugCurrentPage, setDrugCurrentPage] = useState(1);
  const pageSize = 10;
  
  const [drugForm, setDrugForm] = useState({
    name: '', genericName: '', laboratory: '', price: 0, stock: 0,
    category: '', requiresPrescription: false, expirationDate: '',
  });

  const loadDrugs = useCallback(async (page = 1) => {
    try {
      setLoadingDrugs(true);
      const data = await drugsService.getAll({ searchTerm: searchTerm || undefined, page, pageSize });
      setDrugs(data);
      setDrugCurrentPage(page);
    } catch {
      toast.error('Error cargando medicamentos');
    } finally {
      setLoadingDrugs(false);
    }
  }, [searchTerm]);

  useEffect(() => {
    if (activeTab === 'medicamentos') loadDrugs(1);
  }, [activeTab, loadDrugs]);

  const handleCreateDrug = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await drugsService.create(drugForm);
      toast.success('Medicamento creado exitosamente');
      showMessage('¡Excelente! El nuevo medicamento ya está en el catálogo. 💊', 'feliz');
      setShowCreateDrugModal(false);
      loadDrugs(1);
    } catch { toast.error('Error al crear medicamento'); }
  };

  const handleUpdateDrug = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedDrug) return;
    try {
      await drugsService.update({ id: selectedDrug.id, ...drugForm });
      toast.success('Medicamento actualizado');
      setShowEditDrugModal(false);
      loadDrugs(drugCurrentPage);
    } catch { toast.error('Error al actualizar medicamento'); }
  };

  const handleDeleteDrug = async (id: number) => {
    if (!window.confirm('¿Eliminar este medicamento?')) return;
    try {
      await drugsService.delete(id);
      toast.success('Medicamento eliminado');
      loadDrugs(drugCurrentPage);
    } catch { toast.error('Error al eliminar'); }
  };

  // ==========================================
  // TAB: SEDES (GESTORES) - Solo Admin
  // ==========================================
  const [gestores, setGestores] = useState<GestorDto[]>([]);
  const [loadingGestores, setLoadingGestores] = useState(false);
  const [showCreateGestorModal, setShowCreateGestorModal] = useState(false);
  const [gestorForm, setGestorForm] = useState({ nombreSede: '', direccion: '', telefono: '', idEps: 1 });

  const loadGestores = useCallback(async () => {
    try {
      setLoadingGestores(true);
      const data = await gestoresService.getAll();
      setGestores(data);
    } catch { toast.error('Error cargando sedes'); }
    finally { setLoadingGestores(false); }
  }, []);

  useEffect(() => {
    if (activeTab === 'sedes' && isAdmin) loadGestores();
  }, [activeTab, isAdmin, loadGestores]);

  const handleCreateGestor = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await gestoresService.create(gestorForm);
      toast.success('Sede creada exitosamente');
      setShowCreateGestorModal(false);
      loadGestores();
    } catch { toast.error('Error al crear sede'); }
  };

  // ==========================================
  // TAB: INVENTARIOS
  // ==========================================
  const [inventories, setInventories] = useState<InventoryDto[]>([]);
  const [loadingInventories, setLoadingInventories] = useState(false);
  const [invFilterSede, setInvFilterSede] = useState<number>(0);
  const [showCreateInvModal, setShowCreateInvModal] = useState(false);
  const [invForm, setInvForm] = useState({ drugId: 0, gestorFarmaceuticoId: 0, quantity: 0 });

  // Modal: stock
  const [stockModal, setStockModal] = useState<{ open: boolean; type: 'add' | 'remove'; invId: number; drugName: string }>({
    open: false, type: 'add', invId: 0, drugName: '',
  });
  const [stockQty, setStockQty] = useState<string>('1');
  const [stockFocus, setStockFocus] = useState(false);

  // Modal: delete inventory confirm
  const [deleteInvModal, setDeleteInvModal] = useState<{ open: boolean; invId: number; drugName: string }>({
    open: false, invId: 0, drugName: '',
  });

  // Modal: change status
  const [statusModal, setStatusModal] = useState<{ open: boolean; aptId: number; current: number }>({
    open: false, aptId: 0, current: 1,
  });
  const [newStatus, setNewStatus] = useState<number>(1);

  const loadInventories = useCallback(async () => {
    try {
      setLoadingInventories(true);
      const params = invFilterSede > 0 ? { gestorFarmaceuticoId: invFilterSede } : undefined;
      const data = await inventoriesService.getAll(params);
      setInventories(data);
    } catch { toast.error('Error cargando inventario'); }
    finally { setLoadingInventories(false); }
  }, [invFilterSede]);

  useEffect(() => {
    if (activeTab === 'inventarios') {
      loadInventories();
      if (gestores.length === 0) loadGestores();
      if (drugs.length === 0) loadDrugs();
    }
  }, [activeTab, invFilterSede, loadInventories, gestores.length, loadGestores, drugs.length, loadDrugs]);

  const handleCreateInventory = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await inventoriesService.create(invForm);
      toast.success('Inventario creado exitosamente');
      setShowCreateInvModal(false);
      loadInventories();
    } catch { toast.error('Error al crear inventario'); }
  };

  const openStockModal = (type: 'add' | 'remove', inv: InventoryDto) => {
    setStockQty('1');
    setStockModal({ open: true, type, invId: inv.id, drugName: inv.drugName });
  };

  const confirmStock = async () => {
    const q = Number(stockQty);
    if (!stockQty || isNaN(q) || q <= 0) { toast.warn('Ingresa una cantidad válida'); return; }
    try {
      if (stockModal.type === 'add') {
        await inventoriesService.addStock(stockModal.invId, q);
        toast.success('Stock agregado exitosamente');
      } else {
        await inventoriesService.removeStock(stockModal.invId, q);
        toast.success('Stock retirado exitosamente');
      }
      setStockModal(s => ({ ...s, open: false }));
      loadInventories();
    } catch { toast.error(stockModal.type === 'add' ? 'Error al agregar stock' : 'Error al retirar stock'); }
  };

  const confirmDeleteInventory = async () => {
    try {
      await inventoriesService.delete(deleteInvModal.invId);
      toast.success('Inventario eliminado');
      setDeleteInvModal(s => ({ ...s, open: false }));
      loadInventories();
    } catch { toast.error('Error al eliminar inventario'); }
  };

  // ==========================================
  // TAB: TURNOS
  // ==========================================
  const [appointments, setAppointments] = useState<AppointmentDto[]>([]);
  const [loadingAppointments, setLoadingAppointments] = useState(false);
  
  const loadAppointments = useCallback(async () => {
    try {
      setLoadingAppointments(true);
      const data = await appointmentsService.getAll();
      setAppointments(data);
    } catch { toast.error('Error cargando turnos'); }
    finally { setLoadingAppointments(false); }
  }, []);

  useEffect(() => {
    if (activeTab === 'turnos') loadAppointments();
  }, [activeTab, loadAppointments]);

  const openStatusModal = (id: number, current: number) => {
    setNewStatus(current);
    setStatusModal({ open: true, aptId: id, current });
  };

  const confirmChangeStatus = async () => {
    if (![1,2,3,4].includes(newStatus)) { toast.warn('Estado inválido'); return; }
    try {
      await appointmentsService.updateStatus(statusModal.aptId, newStatus);
      toast.success('Estado actualizado');
      showMessage(`¡Listo! El turno #${statusModal.aptId} ha sido actualizado correctamente. 👍`, 'feliz');
      setStatusModal(s => ({ ...s, open: false }));
      loadAppointments();
    } catch (err: any) {
      const serverMsg = err?.response?.data?.message || err?.response?.data || err?.message || 'Error desconocido';
      const status = err?.response?.status;
      console.error('[updateStatus] Error:', status, serverMsg, err?.response?.data);
      toast.error(`Error al actualizar estado (${status ?? 'sin respuesta'}): ${typeof serverMsg === 'string' ? serverMsg : JSON.stringify(serverMsg)}`);
    }
  };

  // ==========================================
  // TAB: REPORTES
  // ==========================================
  const [reportLoading, setReportLoading] = useState(false);
  const [showReportModal, setShowReportModal] = useState(false);
  const [reportType, setReportType] = useState<'appointments' | 'inventory'>('appointments');
  const [reportFilters, setReportFilters] = useState<any>({});

  const handleOpenReportModal = async (type: 'appointments' | 'inventory') => {
    setReportType(type);
    setShowReportModal(true);
    if (gestores.length === 0) loadGestores();
  };

  const handleExportReport = async (format: 'excel' | 'pdf') => {
    setReportLoading(true);
    try {
      const params: any = {};
      if (reportFilters.dateFrom) params.dateFrom = reportFilters.dateFrom;
      if (reportFilters.dateTo) params.dateTo = reportFilters.dateTo;
      if (reportFilters.status) params.status = reportFilters.status;
      if (reportFilters.gestorId) params.gestorId = reportFilters.gestorId;
      if (reportFilters.onlyActive !== undefined) params.onlyActive = reportFilters.onlyActive;

      let data: Blob;
      let fileName = '';

      if (reportType === 'appointments') {
        data = format === 'excel' ? await reportsService.exportAppointmentsExcel(params) : await reportsService.exportAppointmentsPDF(params);
        fileName = `Turnos_${new Date().toISOString().slice(0, 10)}.${format === 'excel' ? 'xlsx' : 'pdf'}`;
      } else {
        data = format === 'excel' ? await reportsService.exportInventoryExcel(params) : await reportsService.exportInventoryPDF(params);
        fileName = `Inventario_${new Date().toISOString().slice(0, 10)}.${format === 'excel' ? 'xlsx' : 'pdf'}`;
      }
      downloadFile(data, fileName);
      toast.success('Reporte descargado');
    } catch (err: any) {
      let serverMsg = err?.message || 'Error desconocido';
      if (err?.response?.data instanceof Blob) {
        serverMsg = await err.response.data.text();
      } else if (err?.response?.data) {
        serverMsg = err.response.data.message || err.response.data;
      }
      console.error('[handleExportReport] Error:', err?.response?.status, serverMsg);
      toast.error(`Error al descargar reporte: ${typeof serverMsg === 'string' ? serverMsg : JSON.stringify(serverMsg)}`);
    }
    finally { setReportLoading(false); }
  };

  // ==========================================
  // UI HELPERS
  // ==========================================
  const SECTION_LABELS: Record<TabType, string> = {
    medicamentos: 'Catálogo de Medicamentos',
    sedes: 'Gestión de Sedes',
    inventarios: 'Inventario de Medicamentos',
    turnos: 'Gestión de Turnos',
    reportes: 'Reportes del Sistema',
    auditoria: 'Registros de Auditoría',
    configuracion: 'Configuración de Perfil'
  };

  const role = isAdmin ? 'admin' : isPharmacist ? 'pharmacist' : 'user';

  return (
    <MainLayout
      activeTab={activeTab as AnyTab}
      onTabChange={(tab) => setActiveTab(tab as TabType)}
      role={role}
      sectionLabel={SECTION_LABELS[activeTab]}
    >
      <div style={{ maxWidth: 1100, margin: '0 auto' }}>

        {/* ── TAB: Configuracion ───────────────────────────────────────── */}
        {activeTab === 'configuracion' && (
          <Configuracion />
        )}

        {/* ── TAB: Medicamentos ───────────────────────────────────────── */}
        {activeTab === 'medicamentos' && (
          <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
              <h2 className="text-2xl font-bold text-gray-800">Catálogo de Medicamentos</h2>
              <div className="flex gap-2">
                <input type="text" value={searchTerm} onChange={e => setSearchTerm(e.target.value)} placeholder="Buscar..." className="px-4 py-2 border rounded-lg" />
                <button onClick={() => loadDrugs(1)} className="bg-blue-500 text-white px-4 py-2 rounded-lg hover:bg-blue-600">🔍 Buscar</button>
                {isAdmin && <button onClick={() => { setDrugForm({name:'', genericName:'', laboratory:'', price:0, stock:0, category:'', requiresPrescription:false, expirationDate:''}); setShowCreateDrugModal(true); }} className="bg-green-500 text-white px-4 py-2 rounded-lg hover:bg-green-600">+ Nuevo</button>}
              </div>
            </div>

            {loadingDrugs ? <p>Cargando...</p> : (
              <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
                {drugs.map(drug => (
                  <div key={drug.id} className="bg-white p-4 rounded-lg shadow-md">
                    <h3 className="font-bold text-lg">{drug.name}</h3>
                    <p className="text-sm text-gray-500">{drug.genericName} - {drug.laboratory}</p>
                    <p className="text-green-600 font-bold mt-2">${drug.price.toFixed(2)}</p>
                    <p className="text-sm">Stock Global: {drug.stock}</p>
                    
                    {isAdmin && (
                      <div className="mt-4 flex gap-2">
                        <button onClick={() => { 
                          setSelectedDrug(drug); 
                          setDrugForm({ ...drug, expirationDate: drug.expirationDate.split('T')[0] }); 
                          setShowEditDrugModal(true); 
                        }} className="bg-blue-500 text-white px-2 py-1 rounded text-sm">Editar</button>
                        <button onClick={() => handleDeleteDrug(drug.id)} className="bg-red-500 text-white px-2 py-1 rounded text-sm">Eliminar</button>
                      </div>
                    )}
                  </div>
                ))}
              </div>
            )}
            
            <div className="flex justify-center gap-3 mt-6">
              <button onClick={() => loadDrugs(drugCurrentPage - 1)} disabled={drugCurrentPage === 1} className="px-4 py-2 bg-white border rounded">Anterior</button>
              <button onClick={() => loadDrugs(drugCurrentPage + 1)} disabled={drugs.length < pageSize} className="px-4 py-2 bg-white border rounded">Siguiente</button>
            </div>
          </div>
        )}

        {/* ── TAB: Sedes ───────────────────────────────────────── */}
        {activeTab === 'sedes' && isAdmin && (
          <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
              <h2 className="text-2xl font-bold text-gray-800">Sedes Farmacéuticas (Gestores)</h2>
              <button onClick={() => setShowCreateGestorModal(true)} className="bg-green-500 text-white px-4 py-2 rounded-lg">+ Nueva Sede</button>
            </div>

            {loadingGestores ? <p>Cargando...</p> : (
              <div className="bg-white rounded-lg shadow overflow-hidden">
                <table className="w-full text-left">
                  <thead className="bg-gray-50 border-b">
                    <tr><th className="p-4">ID</th><th className="p-4">Nombre Sede</th><th className="p-4">Dirección</th><th className="p-4">Teléfono</th><th className="p-4">Estado</th></tr>
                  </thead>
                  <tbody>
                    {gestores.map(g => (
                      <tr key={g.id} className="border-b hover:bg-gray-50">
                        <td className="p-4">{g.id}</td>
                        <td className="p-4 font-semibold">{g.nombreSede}</td>
                        <td className="p-4">{g.direccion}</td>
                        <td className="p-4">{g.telefono}</td>
                        <td className="p-4">{g.isActive ? '✅ Activo' : '❌ Inactivo'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        )}

        {/* ── TAB: Inventarios ───────────────────────────────────────── */}
        {activeTab === 'inventarios' && (
          <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
              <h2 className="text-2xl font-bold text-gray-800">Inventario por Sede</h2>
              <div className="flex gap-2">
                <select value={invFilterSede} onChange={e => setInvFilterSede(Number(e.target.value))} className="border rounded-lg px-4 py-2 bg-white">
                  <option value={0}>Todas las sedes</option>
                  {gestores.map(g => <option key={g.id} value={g.id}>{g.nombreSede}</option>)}
                </select>
                <button onClick={() => setShowCreateInvModal(true)} className="bg-green-500 text-white px-4 py-2 rounded-lg">+ Nuevo Inventario</button>
              </div>
            </div>

            {loadingInventories ? <p>Cargando...</p> : (
              <div className="bg-white rounded-lg shadow overflow-hidden">
                <table className="w-full text-left">
                  <thead className="bg-gray-50 border-b">
                    <tr><th className="p-4">Sede</th><th className="p-4">Medicamento</th><th className="p-4">Cantidad</th><th className="p-4">Acciones</th></tr>
                  </thead>
                  <tbody>
                    {inventories.map(inv => (
                      <tr key={inv.id} className="border-b hover:bg-gray-50">
                        <td className="p-4">{inv.sedeName}</td>
                        <td className="p-4 font-semibold">{inv.drugName}</td>
                        <td className="p-4 text-blue-600 font-bold">{inv.quantity}</td>
                        <td className="p-4 flex gap-2">
                           <button onClick={() => openStockModal('add', inv)} style={{ background:'#dcfce7', color:'#15803d', border:'none', borderRadius:6, padding:'4px 10px', fontSize:13, fontWeight:600, cursor:'pointer' }}>+ Stock</button>
                           <button onClick={() => openStockModal('remove', inv)} style={{ background:'#fee2e2', color:'#dc2626', border:'none', borderRadius:6, padding:'4px 10px', fontSize:13, fontWeight:600, cursor:'pointer' }}>- Stock</button>
                           {isAdmin && <button onClick={() => setDeleteInvModal({ open: true, invId: inv.id, drugName: inv.drugName })} style={{ background:'#f1f5f9', color:'#475569', border:'none', borderRadius:6, padding:'4px 10px', fontSize:13, fontWeight:600, cursor:'pointer' }}>Eliminar</button>}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        )}

        {/* ── TAB: Turnos ───────────────────────────────────────── */}
        {activeTab === 'turnos' && (
          <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
              <h2 className="text-2xl font-bold text-gray-800">Gestión de Turnos</h2>
              <button onClick={loadAppointments} className="bg-white border rounded-lg px-4 py-2">🔄 Actualizar</button>
            </div>

            {loadingAppointments ? <p>Cargando...</p> : (
              <div className="grid gap-4">
                {appointments.map(apt => {
                  const statusInfo = STATUS_LABELS[apt.status] || { label: apt.statusName, color: '#64748b' };
                  return (
                    <div key={apt.id} className="bg-white p-4 rounded-lg shadow border-l-4" style={{ borderColor: statusInfo.color }}>
                      <div className="flex justify-between">
                        <div>
                          <h3 className="font-bold text-lg">Turno #{apt.id} - Sede: {apt.sedeName}</h3>
                          <p className="text-sm text-gray-500">Usuario: {apt.userName} | Creado: {new Date(apt.createdAt).toLocaleDateString()}</p>
                        </div>
                        <div className="text-right">
                          <span className="px-3 py-1 rounded-full text-sm font-bold text-white" style={{ background: statusInfo.color }}>
                            {statusInfo.label}
                          </span>
                          <button onClick={() => openStatusModal(apt.id, apt.status)} style={{ display:'block', marginTop:8, background:'none', border:'none', fontSize:13, color:'#2563eb', cursor:'pointer', fontWeight:600, textDecoration:'underline' }}>
                            Cambiar Estado
                          </button>
                        </div>
                      </div>
                      <div className="mt-3 text-sm text-gray-700">
                        <strong>Medicamentos:</strong> {apt.details.map(d => `${d.drugName} (x${d.quantity})`).join(', ')}
                      </div>

                      {/* ── Tarjeta de QR ── */}
                      <div className="mt-4 pt-4 border-t border-gray-100">
                        <AppointmentQrCard appointment={apt} />
                      </div>
                    </div>
                  );
                })}
              </div>
            )}
          </div>
        )}

        {/* ── TAB: Reportes ───────────────────────────────────────── */}
        {activeTab === 'reportes' && (
          <div className="bg-white p-6 rounded-lg shadow">
            <h2 className="text-2xl font-bold text-gray-800 mb-6">Generación de Reportes</h2>
            <div className="flex gap-4">
              <button onClick={() => handleOpenReportModal('appointments')} className="bg-blue-500 text-white px-6 py-3 rounded-lg shadow hover:bg-blue-600 font-semibold text-lg">
                📊 Reporte de Turnos
              </button>
              <button onClick={() => handleOpenReportModal('inventory')} className="bg-green-500 text-white px-6 py-3 rounded-lg shadow hover:bg-green-600 font-semibold text-lg">
                📦 Reporte de Inventarios
              </button>
            </div>
          </div>
        )}

        {/* ── TAB: Auditoria ───────────────────────────────────────── */}
        {activeTab === 'auditoria' && isAdmin && (
          <AuditLogs />
        )}
      </div>

      {/* ── MODAL: Stock (agregar / retirar) ──── */}
      <CustomDialog
        isOpen={stockModal.open}
        title={stockModal.type === 'add' ? 'Agregar Stock' : 'Retirar Stock'}
        message={`Medicamento: ${stockModal.drugName}`}
        icon={stockModal.type === 'add' ? '📦' : '📤'}
        iconBg={stockModal.type === 'add' ? '#dcfce7' : '#fee2e2'}
        confirmLabel={stockModal.type === 'add' ? 'Agregar' : 'Retirar'}
        confirmColor={stockModal.type === 'add' ? '#16a34a' : '#dc2626'}
        onConfirm={confirmStock}
        onCancel={() => setStockModal(s => ({ ...s, open: false }))}
      >
        <div>
          <label style={{ display:'block', fontSize:13, fontWeight:600, color:'#374151', marginBottom:6 }}>
            {stockModal.type === 'add' ? 'Unidades a agregar' : 'Unidades a retirar'} <span style={{ color:'#ef4444' }}>*</span>
          </label>
          <input
            type="number"
            min={1}
            value={stockQty}
            onChange={e => setStockQty(e.target.value)}
            onFocus={() => setStockFocus(true)}
            onBlur={() => setStockFocus(false)}
            autoFocus
            style={{
              width:'100%', padding:'10px 14px', borderRadius:10,
              border: `1.5px solid ${stockFocus ? '#2563eb' : '#e2e8f0'}`,
              fontSize:15, color:'#1e293b', outline:'none',
              background: stockFocus ? '#f8fbff' : '#f9fafb',
              boxShadow: stockFocus ? '0 0 0 3px rgba(37,99,235,0.12)' : 'none',
              transition:'all 0.2s', boxSizing:'border-box' as any,
            }}
            onKeyDown={e => e.key === 'Enter' && confirmStock()}
          />
        </div>
      </CustomDialog>

      {/* ── MODAL: Confirmar eliminación inventario ── */}
      <CustomDialog
        isOpen={deleteInvModal.open}
        title="Eliminar inventario"
        message={`¿Estás seguro de eliminar el inventario de "${deleteInvModal.drugName}"? Esta acción no se puede deshacer.`}
        icon="🗑️"
        iconBg="#fee2e2"
        confirmLabel="Sí, eliminar"
        confirmColor="#dc2626"
        onConfirm={confirmDeleteInventory}
        onCancel={() => setDeleteInvModal(s => ({ ...s, open: false }))}
      />

      {/* ── MODAL: Cambiar estado turno ──── */}
      <CustomDialog
        isOpen={statusModal.open}
        title="Cambiar estado del turno"
        icon="📋"
        iconBg="#eff6ff"
        confirmLabel="Actualizar"
        confirmColor="#2563eb"
        onConfirm={confirmChangeStatus}
        onCancel={() => setStatusModal(s => ({ ...s, open: false }))}
      >
        <div style={{ display:'flex', flexDirection:'column', gap:8 }}>
          {[{v:1,l:'Recibido',c:'#3b82f6'},{v:2,l:'En Proceso',c:'#f59e0b'},{v:3,l:'Entregado',c:'#10b981'},{v:4,l:'Cancelado',c:'#ef4444'}].map(opt => (
            <button key={opt.v} onClick={() => setNewStatus(opt.v)} style={{
              padding:'10px 14px', borderRadius:10, border:'none', cursor:'pointer',
              background: newStatus === opt.v ? opt.c + '18' : '#f8fafc',
              color: newStatus === opt.v ? opt.c : '#475569',
              fontWeight: newStatus === opt.v ? 700 : 500,
              fontSize:14, textAlign:'left', display:'flex', alignItems:'center', gap:8,
              outline: newStatus === opt.v ? `2px solid ${opt.c}` : '2px solid transparent',
              transition:'all 0.15s',
            }}>
              <span style={{ width:10, height:10, borderRadius:'50%', background:opt.c, flexShrink:0 }} />
              {opt.l}
            </button>
          ))}
        </div>
      </CustomDialog>

      {/* MODALS PARA MEDICAMENTOS */}
      {showCreateDrugModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex justify-center p-4 z-50">
          <div className="bg-white p-6 rounded-xl max-w-md w-full my-auto max-h-[90vh] overflow-y-auto">
            <h3 className="font-bold text-xl mb-4">Crear Medicamento</h3>
            {/* Omito el form completo para abreviar, es similar al original */}
            <form onSubmit={handleCreateDrug} className="flex flex-col gap-3">
              <input placeholder="Nombre" value={drugForm.name} onChange={e => setDrugForm({...drugForm, name: e.target.value})} className="border p-2 rounded" required />
              <input placeholder="Genérico" value={drugForm.genericName} onChange={e => setDrugForm({...drugForm, genericName: e.target.value})} className="border p-2 rounded" required />
              <input placeholder="Laboratorio" value={drugForm.laboratory} onChange={e => setDrugForm({...drugForm, laboratory: e.target.value})} className="border p-2 rounded" required />
              <input type="number" placeholder="Precio" value={drugForm.price} onChange={e => setDrugForm({...drugForm, price: Number(e.target.value)})} className="border p-2 rounded" required />
              <input type="number" placeholder="Stock Global" value={drugForm.stock} onChange={e => setDrugForm({...drugForm, stock: Number(e.target.value)})} className="border p-2 rounded" required />
              <input placeholder="Categoría" value={drugForm.category} onChange={e => setDrugForm({...drugForm, category: e.target.value})} className="border p-2 rounded" required />
              <input type="date" value={drugForm.expirationDate} onChange={e => setDrugForm({...drugForm, expirationDate: e.target.value})} className="border p-2 rounded" required />
              <label><input type="checkbox" checked={drugForm.requiresPrescription} onChange={e => setDrugForm({...drugForm, requiresPrescription: e.target.checked})} /> Requiere receta</label>
              <div className="flex gap-2 mt-2">
                <button type="submit" className="flex-1 bg-blue-600 text-white py-2 rounded">Guardar</button>
                <button type="button" onClick={() => setShowCreateDrugModal(false)} className="flex-1 bg-gray-200 py-2 rounded">Cancelar</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {showEditDrugModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex justify-center p-4 z-50">
          <div className="bg-white p-6 rounded-xl max-w-md w-full my-auto max-h-[90vh] overflow-y-auto">
            <h3 className="font-bold text-xl mb-4">Editar Medicamento</h3>
            <form onSubmit={handleUpdateDrug} className="flex flex-col gap-3">
              <input placeholder="Nombre" value={drugForm.name} onChange={e => setDrugForm({...drugForm, name: e.target.value})} className="border p-2 rounded" required />
              <input placeholder="Genérico" value={drugForm.genericName} onChange={e => setDrugForm({...drugForm, genericName: e.target.value})} className="border p-2 rounded" required />
              <input placeholder="Laboratorio" value={drugForm.laboratory} onChange={e => setDrugForm({...drugForm, laboratory: e.target.value})} className="border p-2 rounded" required />
              <input type="number" placeholder="Precio" value={drugForm.price} onChange={e => setDrugForm({...drugForm, price: Number(e.target.value)})} className="border p-2 rounded" required />
              <input type="number" placeholder="Stock Global" value={drugForm.stock} onChange={e => setDrugForm({...drugForm, stock: Number(e.target.value)})} className="border p-2 rounded" required />
              <input placeholder="Categoría" value={drugForm.category} onChange={e => setDrugForm({...drugForm, category: e.target.value})} className="border p-2 rounded" required />
              <input type="date" value={drugForm.expirationDate} onChange={e => setDrugForm({...drugForm, expirationDate: e.target.value})} className="border p-2 rounded" required />
              <label><input type="checkbox" checked={drugForm.requiresPrescription} onChange={e => setDrugForm({...drugForm, requiresPrescription: e.target.checked})} /> Requiere receta</label>
              <div className="flex gap-2 mt-2">
                <button type="submit" className="flex-1 bg-blue-600 text-white py-2 rounded">Actualizar</button>
                <button type="button" onClick={() => setShowEditDrugModal(false)} className="flex-1 bg-gray-200 py-2 rounded">Cancelar</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL PARA CREAR SEDE */}
      {showCreateGestorModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex justify-center p-4 z-50">
          <div className="bg-white p-6 rounded-xl max-w-md w-full my-auto">
            <h3 className="font-bold text-xl mb-4">Crear Sede (Gestor)</h3>
            <form onSubmit={handleCreateGestor} className="flex flex-col gap-3">
              <input placeholder="Nombre Sede" value={gestorForm.nombreSede} onChange={e => setGestorForm({...gestorForm, nombreSede: e.target.value})} className="border p-2 rounded" required />
              <input placeholder="Dirección" value={gestorForm.direccion} onChange={e => setGestorForm({...gestorForm, direccion: e.target.value})} className="border p-2 rounded" required />
              <input placeholder="Teléfono" value={gestorForm.telefono} onChange={e => setGestorForm({...gestorForm, telefono: e.target.value})} className="border p-2 rounded" required />
              <div className="flex gap-2 mt-2">
                <button type="submit" className="flex-1 bg-blue-600 text-white py-2 rounded">Guardar</button>
                <button type="button" onClick={() => setShowCreateGestorModal(false)} className="flex-1 bg-gray-200 py-2 rounded">Cancelar</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL PARA CREAR INVENTARIO */}
      {showCreateInvModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex justify-center p-4 z-50">
          <div className="bg-white p-6 rounded-xl max-w-md w-full my-auto">
            <h3 className="font-bold text-xl mb-4">Registrar Inventario</h3>
            <form onSubmit={handleCreateInventory} className="flex flex-col gap-3">
              <select value={invForm.gestorFarmaceuticoId} onChange={e => setInvForm({...invForm, gestorFarmaceuticoId: Number(e.target.value)})} className="border p-2 rounded" required>
                <option value={0}>-- Selecciona Sede --</option>
                {gestores.map(g => <option key={g.id} value={g.id}>{g.nombreSede}</option>)}
              </select>
              <select value={invForm.drugId} onChange={e => setInvForm({...invForm, drugId: Number(e.target.value)})} className="border p-2 rounded" required>
                <option value={0}>-- Selecciona Medicamento --</option>
                {drugs.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}
              </select>
              <input type="number" placeholder="Cantidad Inicial" value={invForm.quantity} onChange={e => setInvForm({...invForm, quantity: Number(e.target.value)})} className="border p-2 rounded" required min={0} />
              <div className="flex gap-2 mt-2">
                <button type="submit" className="flex-1 bg-blue-600 text-white py-2 rounded">Guardar</button>
                <button type="button" onClick={() => setShowCreateInvModal(false)} className="flex-1 bg-gray-200 py-2 rounded">Cancelar</button>
              </div>
            </form>
          </div>
        </div>
      )}

      <ReportModal
        isOpen={showReportModal}
        onClose={() => setShowReportModal(false)}
        title={reportType === 'appointments' ? '📊 Reporte de Turnos' : '📦 Reporte de Inventario'}
        type={reportType}
        loading={reportLoading}
        onExcel={() => handleExportReport('excel')}
        onPdf={() => handleExportReport('pdf')}
        onApplyFilters={(f) => { setReportFilters(f); toast.info('Filtros aplicados'); }}
        onResetFilters={() => { setReportFilters({}); toast.info('Filtros restablecidos'); }}
        sedes={gestores}
      />
    </MainLayout>
  );
};

export default Dashboard;
