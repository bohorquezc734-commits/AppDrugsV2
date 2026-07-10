import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { drugsService, Drug } from '../services/drugs';
import { authService } from '../services/auth';
import { toast } from 'react-toastify';
import api from '../services/api';
import { reportsService, downloadFile } from '../services/reports';
import ReportModal from '../components/Reports/ReportModal';

const Dashboard: React.FC = () => {
  const [drugs, setDrugs] = useState<Drug[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [selectedDrug, setSelectedDrug] = useState<Drug | null>(null);
  const [reportLoading, setReportLoading] = useState(false);
  const [showReportModal, setShowReportModal] = useState(false);
  const [reportType, setReportType] = useState<'appointments' | 'inventory'>('appointments');
  const [sedes, setSedes] = useState<Array<{ id: number; nombreSede: string }>>([]);
  const [reportFilters, setReportFilters] = useState<any>({});
  const navigate = useNavigate();
  const user = authService.getUser();

  // 🔐 Verificar permisos
  const isAdmin = authService.isAdmin();
  const isPharmacist = authService.isPharmacist();
  const isRegularUser = authService.isUser();

  // Redirigir afiliados al dashboard simple
  useEffect(() => {
    if (isRegularUser) {
      navigate('/user-dashboard');
    }
  }, [isRegularUser, navigate]);

  const [form, setForm] = useState({
    name: '',
    genericName: '',
    laboratory: '',
    price: 0,
    stock: 0,
    category: '',
    requiresPrescription: false,
    expirationDate: '',
  });
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 10;

  useEffect(() => {
    loadDrugs(1);
  }, []);

  const loadDrugs = async (page = 1) => {
    try {
      setLoading(true);
      const data = await drugsService.getAll({
        searchTerm: searchTerm || undefined,
        page,
        pageSize,
      });
      setDrugs(data);
      setCurrentPage(page);
    } catch (error) {
      console.error('❌ Error cargando medicamentos:', error);
      toast.error('Error cargando medicamentos');
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    loadDrugs(1);
  };

  const handleLogout = () => {
    authService.logout();
    toast.info('Sesión cerrada');
  };

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await drugsService.create(form);
      toast.success('Medicamento creado exitosamente');
      setShowCreateModal(false);
      setForm({
        name: '',
        genericName: '',
        laboratory: '',
        price: 0,
        stock: 0,
        category: '',
        requiresPrescription: false,
        expirationDate: '',
      });
      loadDrugs();
    } catch (error) {
      toast.error('Error al crear medicamento');
    }
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm('¿Estás seguro de eliminar este medicamento?')) return;
    try {
      await drugsService.delete(id);
      toast.success('Medicamento eliminado');
      loadDrugs();
    } catch (error) {
      toast.error('Error al eliminar medicamento');
    }
  };

  const openEditModal = (drug: Drug) => {
    setSelectedDrug(drug);
    setForm({
      name: drug.name,
      genericName: drug.genericName,
      laboratory: drug.laboratory,
      price: drug.price,
      stock: drug.stock,
      category: drug.category,
      requiresPrescription: drug.requiresPrescription,
      expirationDate: drug.expirationDate.split('T')[0],
    });
    setShowEditModal(true);
  };

  const handleUpdate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedDrug) return;
    try {
      await drugsService.update({ id: selectedDrug.id, ...form });
      toast.success('Medicamento actualizado');
      setShowEditModal(false);
      setSelectedDrug(null);
      loadDrugs();
    } catch (error) {
      toast.error('Error al actualizar medicamento');
    }
  };

  const handleUpdateStock = async (drugId: number) => {
    const quantity = prompt('¿Cuántas unidades agregar? (usa negativo para quitar)', '0');
    if (quantity === null) return;
    try {
      await drugsService.updateStock(drugId, parseInt(quantity));
      toast.success('Stock actualizado');
      loadDrugs();
    } catch (error) {
      toast.error('Error al actualizar stock');
    }
  };

  const cleanReportFilters = (filters: any) => {
    const params: any = {};
    if (filters.dateFrom) params.dateFrom = filters.dateFrom;
    if (filters.dateTo) params.dateTo = filters.dateTo;
    if (filters.status) params.status = filters.status;
    if (filters.gestorId !== undefined && filters.gestorId !== null) params.gestorId = filters.gestorId;
    if (filters.onlyActive !== undefined) params.onlyActive = filters.onlyActive;
    return params;
  };

  const loadSedes = async () => {
    try {
      const response = await api.get('/Gestores');
      setSedes(response.data);
    } catch (error) {
      console.error('Error cargando sedes:', error);
      toast.error('No se pudieron cargar las sedes');
    }
  };

  const handleOpenReportModal = async (type: 'appointments' | 'inventory') => {
    setReportType(type);
    setShowReportModal(true);
    if (type === 'inventory') {
      await loadSedes();
    }
  };

  const handleExportReport = async (format: 'excel' | 'pdf') => {
    setReportLoading(true);
    try {
      const params = cleanReportFilters(reportFilters);
      let data: Blob;
      let fileName = '';

      if (reportType === 'appointments') {
        if (format === 'excel') {
          data = await reportsService.exportAppointmentsExcel(params);
          fileName = `Turnos_${new Date().toISOString().slice(0, 10)}.xlsx`;
        } else {
          data = await reportsService.exportAppointmentsPDF(params);
          fileName = `Turnos_${new Date().toISOString().slice(0, 10)}.pdf`;
        }
      } else {
        if (format === 'excel') {
          data = await reportsService.exportInventoryExcel(params);
          fileName = `Inventario_${new Date().toISOString().slice(0, 10)}.xlsx`;
        } else {
          data = await reportsService.exportInventoryPDF(params);
          fileName = `Inventario_${new Date().toISOString().slice(0, 10)}.pdf`;
        }
      }

      downloadFile(data, fileName);
      toast.success('Reporte descargado exitosamente');
    } catch (error) {
      console.error('Error exportando reporte:', error);
      toast.error('Error al descargar el reporte');
    } finally {
      setReportLoading(false);
    }
  };

  const handleApplyFilters = (filters: any) => {
    setReportFilters(filters);
    toast.info('Filtros aplicados');
  };

  const handleResetFilters = () => {
    setReportFilters({});
    toast.info('Filtros restablecidos');
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <div className="text-4xl mb-4">⏳</div>
          <p className="text-gray-600">Cargando medicamentos...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100">
      {/* Navbar */}
      <nav className="bg-white shadow-md p-4 sticky top-0 z-10">
        <div className="container mx-auto flex flex-wrap justify-between items-center">
          <div className="flex items-center space-x-2">
            <span className="text-2xl">💊</span>
            <h1 className="text-xl font-bold text-gray-800">AppDrugsV2</h1>
          </div>
          
          <div className="flex items-center space-x-4">
            <span className="text-sm text-gray-600 hidden sm:inline">
              👤 {user?.fullName || 'Usuario'} ({user?.role || 'User'})
            </span>
            <button
              onClick={handleLogout}
              className="bg-red-500 text-white px-4 py-2 rounded-lg hover:bg-red-600 transition text-sm"
            >
              Cerrar Sesión
            </button>
          </div>
        </div>
      </nav>

      <div className="container mx-auto p-4">
        {/* Header */}
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
          <h2 className="text-2xl font-bold text-gray-800">📋 Lista de Medicamentos</h2>
          <div className="flex flex-col sm:flex-row gap-3 w-full sm:w-auto">
            <form onSubmit={handleSearch} className="flex gap-2">
              <input
                type="text"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                placeholder="Buscar medicamento..."
                className="px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 w-full sm:w-48"
              />
              <button
                type="submit"
                className="bg-blue-500 text-white px-4 py-2 rounded-lg hover:bg-blue-600 transition"
              >
                🔍 Buscar
              </button>
            </form>
            {/* 🔐 Botón "+ Nuevo" solo para Admin */}
            {isAdmin && (
              <button
                onClick={() => setShowCreateModal(true)}
                className="bg-green-500 text-white px-4 py-2 rounded-lg hover:bg-green-600 transition"
              >
                + Nuevo
              </button>
            )}
          </div>
        </div>

        <div className="mb-6 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <div className="text-sm text-gray-600">
            {isAdmin && 'Administrador: Exporta reportes de turnos o inventario desde aquí.'}
            {isPharmacist && 'Gestor Farmacéutico: Exporta reportes de tu sede.'}
          </div>
          {/* 🔐 Botones de reportes solo para Admin y Pharmacist */}
          {(isAdmin || isPharmacist) && (
            <div className="flex flex-col sm:flex-row gap-2">
              <button
                type="button"
                onClick={() => handleOpenReportModal('appointments')}
                className="bg-blue-500 text-white px-4 py-2 rounded-lg hover:bg-blue-600 transition"
              >
                📊 Reporte Turnos
              </button>
              <button
                type="button"
                onClick={() => handleOpenReportModal('inventory')}
                className="bg-green-500 text-white px-4 py-2 rounded-lg hover:bg-green-600 transition"
              >
                📦 Reporte Inventario
              </button>
            </div>
          )}
        </div>

        {/* Lista de medicamentos */}
        {drugs.length === 0 ? (
          <div className="bg-white rounded-lg shadow p-8 text-center">
            <p className="text-gray-500">No hay medicamentos disponibles</p>
          </div>
        ) : (
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
            {drugs.map((drug) => (
              <div key={drug.id} className="bg-white p-4 rounded-lg shadow-md hover:shadow-lg transition">
                <h3 className="font-bold text-lg text-gray-800">{drug.name}</h3>
                <p className="text-sm text-gray-500">{drug.genericName}</p>
                <p className="text-sm text-gray-600 mt-1">🧪 {drug.laboratory}</p>
                <p className="text-green-600 font-bold text-lg mt-2">${drug.price.toFixed(2)}</p>
                <p className="text-sm text-gray-600">📦 Stock: {drug.stock}</p>
                <p className="text-sm text-gray-600">📂 {drug.category}</p>
                {drug.requiresPrescription && (
                  <span className="inline-block mt-2 bg-yellow-200 text-yellow-800 px-2 py-1 rounded text-xs">
                    📋 Requiere receta
                  </span>
                )}
                {drug.isExpired && (
                  <span className="inline-block mt-2 bg-red-200 text-red-800 px-2 py-1 rounded text-xs ml-1">
                    ⚠️ Vencido
                  </span>
                )}
                
                {/* 🔐 Botones de acción - Condicionales por rol */}
                <div className="mt-4 flex gap-2 flex-wrap">
                  {/* ✏️ EDITAR - Solo Admin */}
                  {isAdmin && (
                    <button
                      onClick={() => openEditModal(drug)}
                      className="bg-blue-500 text-white px-3 py-1 rounded text-sm hover:bg-blue-600 transition"
                    >
                      ✏️ Editar
                    </button>
                  )}
                  
                  {/* 🗑️ ELIMINAR - Solo Admin */}
                  {isAdmin && (
                    <button
                      onClick={() => handleDelete(drug.id)}
                      className="bg-red-500 text-white px-3 py-1 rounded text-sm hover:bg-red-600 transition"
                    >
                      🗑️ Eliminar
                    </button>
                  )}
                  
                  {/* 📦 STOCK - Admin y Pharmacist */}
                  {(isAdmin || isPharmacist) && (
                    <button
                      onClick={() => handleUpdateStock(drug.id)}
                      className="bg-purple-500 text-white px-3 py-1 rounded text-sm hover:bg-purple-600 transition"
                    >
                      📦 Stock
                    </button>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}

        <div className="flex flex-col sm:flex-row items-center justify-center gap-3 mt-4">
          <button
            type="button"
            onClick={() => loadDrugs(currentPage - 1)}
            disabled={currentPage === 1}
            className="bg-gray-200 text-gray-700 px-4 py-2 rounded-lg hover:bg-gray-300 transition disabled:cursor-not-allowed disabled:opacity-50"
          >
            ← Anterior
          </button>
          <span className="text-sm text-gray-700">Página {currentPage}</span>
          <button
            type="button"
            onClick={() => loadDrugs(currentPage + 1)}
            disabled={drugs.length < pageSize}
            className="bg-gray-200 text-gray-700 px-4 py-2 rounded-lg hover:bg-gray-300 transition disabled:cursor-not-allowed disabled:opacity-50"
          >
            Siguiente →
          </button>
        </div>

        <p className="text-sm text-gray-500 mt-4 text-center">
          Mostrando {drugs.length} medicamento(s)
        </p>
      </div>

      {/* Modal de creación - Ya lo tienes */}
      {showCreateModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl p-6 max-w-md w-full max-h-[90vh] overflow-y-auto">
            <h3 className="text-xl font-bold mb-4">➕ Crear Medicamento</h3>
            <form onSubmit={handleCreate}>
              <div className="mb-3">
                <label className="block text-sm font-semibold">Nombre *</label>
                <input
                  type="text"
                  value={form.name}
                  onChange={(e) => setForm({ ...form, name: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg"
                  required
                />
              </div>
              <div className="mb-3">
                <label className="block text-sm font-semibold">Nombre Genérico *</label>
                <input
                  type="text"
                  value={form.genericName}
                  onChange={(e) => setForm({ ...form, genericName: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg"
                  required
                />
              </div>
              <div className="mb-3">
                <label className="block text-sm font-semibold">Laboratorio *</label>
                <input
                  type="text"
                  value={form.laboratory}
                  onChange={(e) => setForm({ ...form, laboratory: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg"
                  required
                />
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div className="mb-3">
                  <label className="block text-sm font-semibold">Precio *</label>
                  <input
                    type="number"
                    step="0.01"
                    value={form.price}
                    onChange={(e) => setForm({ ...form, price: parseFloat(e.target.value) })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg"
                    required
                  />
                </div>
                <div className="mb-3">
                  <label className="block text-sm font-semibold">Stock *</label>
                  <input
                    type="number"
                    value={form.stock}
                    onChange={(e) => setForm({ ...form, stock: parseInt(e.target.value) })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg"
                    required
                  />
                </div>
              </div>
              <div className="mb-3">
                <label className="block text-sm font-semibold">Categoría *</label>
                <input
                  type="text"
                  value={form.category}
                  onChange={(e) => setForm({ ...form, category: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg"
                  required
                />
              </div>
              <div className="mb-3">
                <label className="block text-sm font-semibold">Fecha de Expiración *</label>
                <input
                  type="date"
                  value={form.expirationDate}
                  onChange={(e) => setForm({ ...form, expirationDate: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg"
                  required
                />
              </div>
              <div className="mb-4">
                <label className="flex items-center">
                  <input
                    type="checkbox"
                    checked={form.requiresPrescription}
                    onChange={(e) => setForm({ ...form, requiresPrescription: e.target.checked })}
                    className="mr-2"
                  />
                  <span className="text-sm">Requiere receta médica</span>
                </label>
              </div>
              <div className="flex gap-2">
                <button
                  type="submit"
                  className="flex-1 bg-green-500 text-white py-2 rounded-lg hover:bg-green-600 transition"
                >
                  Crear
                </button>
                <button
                  type="button"
                  onClick={() => setShowCreateModal(false)}
                  className="flex-1 bg-gray-300 text-gray-700 py-2 rounded-lg hover:bg-gray-400 transition"
                >
                  Cancelar
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal de edición */}
      {showEditModal && selectedDrug && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl p-6 max-w-md w-full max-h-[90vh] overflow-y-auto">
            <h3 className="text-xl font-bold mb-4">✏️ Editar Medicamento</h3>
            <form onSubmit={handleUpdate}>
              <div className="mb-3">
                <label className="block text-sm font-semibold">Nombre *</label>
                <input
                  type="text"
                  value={form.name}
                  onChange={(e) => setForm({ ...form, name: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg"
                  required
                />
              </div>
              <div className="mb-3">
                <label className="block text-sm font-semibold">Nombre Genérico *</label>
                <input
                  type="text"
                  value={form.genericName}
                  onChange={(e) => setForm({ ...form, genericName: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg"
                  required
                />
              </div>
              <div className="mb-3">
                <label className="block text-sm font-semibold">Laboratorio *</label>
                <input
                  type="text"
                  value={form.laboratory}
                  onChange={(e) => setForm({ ...form, laboratory: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg"
                  required
                />
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div className="mb-3">
                  <label className="block text-sm font-semibold">Precio *</label>
                  <input
                    type="number"
                    step="0.01"
                    value={form.price}
                    onChange={(e) => setForm({ ...form, price: parseFloat(e.target.value) })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg"
                    required
                  />
                </div>
                <div className="mb-3">
                  <label className="block text-sm font-semibold">Stock *</label>
                  <input
                    type="number"
                    value={form.stock}
                    onChange={(e) => setForm({ ...form, stock: parseInt(e.target.value) })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg"
                    required
                  />
                </div>
              </div>
              <div className="mb-3">
                <label className="block text-sm font-semibold">Categoría *</label>
                <input
                  type="text"
                  value={form.category}
                  onChange={(e) => setForm({ ...form, category: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg"
                  required
                />
              </div>
              <div className="mb-3">
                <label className="block text-sm font-semibold">Fecha de Expiración *</label>
                <input
                  type="date"
                  value={form.expirationDate}
                  onChange={(e) => setForm({ ...form, expirationDate: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg"
                  required
                />
              </div>
              <div className="mb-4">
                <label className="flex items-center">
                  <input
                    type="checkbox"
                    checked={form.requiresPrescription}
                    onChange={(e) => setForm({ ...form, requiresPrescription: e.target.checked })}
                    className="mr-2"
                  />
                  <span className="text-sm">Requiere receta médica</span>
                </label>
              </div>
              <div className="flex gap-2">
                <button
                  type="submit"
                  className="flex-1 bg-blue-500 text-white py-2 rounded-lg hover:bg-blue-600 transition"
                >
                  Actualizar
                </button>
                <button
                  type="button"
                  onClick={() => {
                    setShowEditModal(false);
                    setSelectedDrug(null);
                  }}
                  className="flex-1 bg-gray-300 text-gray-700 py-2 rounded-lg hover:bg-gray-400 transition"
                >
                  Cancelar
                </button>
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
        onApplyFilters={handleApplyFilters}
        onResetFilters={handleResetFilters}
        sedes={sedes}
      />
    </div>
  );
};

export default Dashboard;
