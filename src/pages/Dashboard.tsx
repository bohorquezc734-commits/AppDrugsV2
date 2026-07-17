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
import api from '../services/api';

type TabType = 'medicamentos' | 'sedes' | 'inventarios' | 'turnos' | 'reportes';

const STATUS_LABELS: Record<number, { label: string; color: string }> = {
  1: { label: 'Recibido', color: '#3b82f6' },
  2: { label: 'En Proceso', color: '#f59e0b' },
  3: { label: 'Entregado', color: '#10b981' },
  4: { label: 'Cancelado', color: '#ef4444' },
};

const Dashboard: React.FC = () => {
  const navigate = useNavigate();
  const user = authService.getUser();
  const [activeTab, setActiveTab] = useState<TabType>('medicamentos');

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

  const handleAddStock = async (id: number) => {
    const q = prompt('¿Cuántas unidades agregar?');
    if (!q || isNaN(Number(q))) return;
    try {
      await inventoriesService.addStock(id, Number(q));
      toast.success('Stock agregado');
      loadInventories();
    } catch { toast.error('Error al agregar stock'); }
  };

  const handleRemoveStock = async (id: number) => {
    const q = prompt('¿Cuántas unidades retirar?');
    if (!q || isNaN(Number(q))) return;
    try {
      await inventoriesService.removeStock(id, Number(q));
      toast.success('Stock retirado');
      loadInventories();
    } catch { toast.error('Error al retirar stock'); }
  };

  const handleDeleteInventory = async (id: number) => {
    if (!window.confirm('¿Estás seguro de eliminar este registro de inventario?')) return;
    try {
      await inventoriesService.delete(id);
      toast.success('Inventario eliminado');
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

  const handleChangeStatus = async (id: number, currentStatus: number) => {
    const newStatusStr = prompt('Nuevo estado (1=Recibido, 2=En Proceso, 3=Entregado, 4=Cancelado):', currentStatus.toString());
    if (!newStatusStr) return;
    const newStatus = Number(newStatusStr);
    if (![1,2,3,4].includes(newStatus)) {
      toast.warn('Estado inválido'); return;
    }
    try {
      await appointmentsService.updateStatus(id, newStatus);
      toast.success('Estado actualizado');
      loadAppointments();
    } catch { toast.error('Error al actualizar estado'); }
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
    } catch { toast.error('Error al descargar reporte'); }
    finally { setReportLoading(false); }
  };

  // ==========================================
  // UI HELPERS
  // ==========================================
  const tabClass = (tab: TabType) =>
    `px-5 py-2 rounded-full text-sm font-semibold transition-all ${
      activeTab === tab
        ? 'bg-blue-600 text-white shadow'
        : 'bg-white text-gray-600 hover:bg-blue-50 border border-gray-200'
    }`;

  const handleLogout = () => { authService.logout(); toast.info('Sesión cerrada'); };

  return (
    <div style={{ minHeight: '100vh', background: '#f1f5f9' }}>
      <nav style={{ background: '#fff', boxShadow: '0 2px 8px rgba(0,0,0,0.08)', padding: '0 24px', height: 64, display: 'flex', alignItems: 'center', justifyContent: 'space-between', position: 'sticky', top: 0, zIndex: 100 }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
          <span style={{ fontSize: 28 }}>💊</span>
          <span style={{ fontWeight: 700, fontSize: 20, color: '#1e293b' }}>AppDrugsV2 (Panel Admin)</span>
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
          <span style={{ fontSize: 14, color: '#64748b' }}>👤 {user?.fullName} ({user?.role})</span>
          <button onClick={handleLogout} style={{ background: '#ef4444', color: '#fff', border: 'none', borderRadius: 8, padding: '8px 18px', cursor: 'pointer', fontWeight: 600, fontSize: 14 }}>
            Cerrar Sesión
          </button>
        </div>
      </nav>

      <div style={{ maxWidth: 1200, margin: '0 auto', padding: '28px 20px' }}>
        <div style={{ display: 'flex', gap: 10, marginBottom: 28, flexWrap: 'wrap' }}>
          <button className={tabClass('medicamentos')} onClick={() => setActiveTab('medicamentos')}>💊 Medicamentos</button>
          {isAdmin && <button className={tabClass('sedes')} onClick={() => setActiveTab('sedes')}>🏪 Sedes (Gestores)</button>}
          <button className={tabClass('inventarios')} onClick={() => setActiveTab('inventarios')}>📦 Inventarios</button>
          <button className={tabClass('turnos')} onClick={() => setActiveTab('turnos')}>📋 Turnos</button>
          <button className={tabClass('reportes')} onClick={() => setActiveTab('reportes')}>📊 Reportes</button>
        </div>

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
                          <button onClick={() => handleAddStock(inv.id)} className="bg-green-100 text-green-700 px-2 py-1 rounded">+ Stock</button>
                          <button onClick={() => handleRemoveStock(inv.id)} className="bg-red-100 text-red-700 px-2 py-1 rounded">- Stock</button>
                          {isAdmin && <button onClick={() => handleDeleteInventory(inv.id)} className="bg-gray-200 text-gray-700 px-2 py-1 rounded">Eliminar</button>}
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
                          <button onClick={() => handleChangeStatus(apt.id, apt.status)} className="block mt-2 text-sm text-blue-600 underline">
                            Cambiar Estado
                          </button>
                        </div>
                      </div>
                      <div className="mt-3 text-sm text-gray-700">
                        <strong>Medicamentos:</strong> {apt.details.map(d => `${d.drugName} (x${d.quantity})`).join(', ')}
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
      </div>

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
    </div>
  );
};

export default Dashboard;
