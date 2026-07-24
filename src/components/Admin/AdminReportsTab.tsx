import React, { useState, useEffect } from 'react';
import { toast } from 'react-toastify';
import { reportsService, downloadFile } from '../../services/reports';
import { gestoresService, GestorDto } from '../../services/gestores';
import ReportModal from '../Reports/ReportModal';

const AdminReportsTab: React.FC = () => {
  const [gestores, setGestores] = useState<GestorDto[]>([]);
  const [reportLoading, setReportLoading] = useState(false);
  const [showReportModal, setShowReportModal] = useState(false);
  const [reportType, setReportType] = useState<'appointments' | 'inventory'>('appointments');
  const [reportFilters, setReportFilters] = useState<any>({});

  const handleOpenReportModal = async (type: 'appointments' | 'inventory') => {
    setReportType(type);
    setShowReportModal(true);
    if (gestores.length === 0) {
      try {
        const data = await gestoresService.getAll();
        setGestores(data);
      } catch {
        toast.error('Error cargando sedes para los filtros');
      }
    }
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
      toast.error('Error al descargar reporte');
    }
    finally { setReportLoading(false); }
  };

  return (
    <div>
      <div className="bg-white p-8 rounded-xl shadow-sm border border-gray-100">
        <h2 className="text-2xl font-bold text-gray-800 mb-2">Generación de Reportes Globales</h2>
        <p className="text-gray-500 mb-8">Exporta la información consolidada de todas las sedes en formatos Excel o PDF.</p>
        
        <div className="flex gap-4">
          <button onClick={() => handleOpenReportModal('appointments')} className="flex-1 bg-blue-50 border border-blue-200 text-blue-700 px-6 py-8 rounded-xl hover:bg-blue-100 font-bold text-lg transition flex flex-col items-center justify-center gap-3">
            <span className="text-4xl">📊</span>
            Reporte de Turnos
          </button>
          <button onClick={() => handleOpenReportModal('inventory')} className="flex-1 bg-green-50 border border-green-200 text-green-700 px-6 py-8 rounded-xl hover:bg-green-100 font-bold text-lg transition flex flex-col items-center justify-center gap-3">
            <span className="text-4xl">📦</span>
            Reporte de Inventarios
          </button>
        </div>
      </div>

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

export default AdminReportsTab;
