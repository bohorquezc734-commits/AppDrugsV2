import React, { useState, useEffect } from 'react';
import { toast } from 'react-toastify';
import { reportsService, downloadFile } from '../../services/reports';
import ReportModal from '../Reports/ReportModal';
import { gestoresService, GestorDto } from '../../services/gestores';

const PharmacistReportsTab: React.FC = () => {
  const [reportLoading, setReportLoading] = useState(false);
  const [showReportModal, setShowReportModal] = useState(false);
  const [reportType, setReportType] = useState<'appointments' | 'inventory'>('appointments');
  const [reportFilters, setReportFilters] = useState<any>({});
  const [gestores, setGestores] = useState<GestorDto[]>([]);

  useEffect(() => {
    gestoresService.getAll().then(data => setGestores(data)).catch(() => {});
  }, []);

  const handleOpenReportModal = (type: 'appointments' | 'inventory') => {
    setReportType(type);
    setShowReportModal(true);
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
      <div style={{ background: '#fff', padding: 32, borderRadius: 12, boxShadow: '0 2px 8px rgba(0,0,0,0.05)' }}>
        <h2 style={{ fontSize: 24, fontWeight: 700, color: '#1e293b', marginBottom: 24 }}>Generación de Reportes</h2>
        <div style={{ display: 'flex', gap: 16 }}>
          <button 
            onClick={() => handleOpenReportModal('appointments')} 
            style={{ background: '#3b82f6', color: '#fff', padding: '16px 24px', borderRadius: 12, border: 'none', cursor: 'pointer', fontWeight: 600, fontSize: 16, display: 'flex', alignItems: 'center', gap: 10, boxShadow: '0 4px 12px rgba(59,130,246,0.3)' }}
          >
            📊 Reporte de Turnos
          </button>
          <button 
            onClick={() => handleOpenReportModal('inventory')} 
            style={{ background: '#10b981', color: '#fff', padding: '16px 24px', borderRadius: 12, border: 'none', cursor: 'pointer', fontWeight: 600, fontSize: 16, display: 'flex', alignItems: 'center', gap: 10, boxShadow: '0 4px 12px rgba(16,185,129,0.3)' }}
          >
            📦 Reporte de Inventarios
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

export default PharmacistReportsTab;
