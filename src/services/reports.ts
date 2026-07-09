import api from './api';

export interface AppointmentReportFilters {
  dateFrom?: string;
  dateTo?: string;
  status?: string;
  gestorId?: number;
}

export interface InventoryReportFilters {
  gestorId?: number;
  onlyActive?: boolean;
}

export const reportsService = {
  exportAppointmentsExcel: async (filters?: AppointmentReportFilters) => {
    const response = await api.get('/Reports/appointments/excel', {
      params: filters,
      responseType: 'blob',
    });
    return response.data;
  },

  exportAppointmentsPDF: async (filters?: AppointmentReportFilters) => {
    const response = await api.get('/Reports/appointments/pdf', {
      params: filters,
      responseType: 'blob',
    });
    return response.data;
  },

  exportInventoryExcel: async (filters?: InventoryReportFilters) => {
    const response = await api.get('/Reports/inventory/excel', {
      params: filters,
      responseType: 'blob',
    });
    return response.data;
  },

  exportInventoryPDF: async (filters?: InventoryReportFilters) => {
    const response = await api.get('/Reports/inventory/pdf', {
      params: filters,
      responseType: 'blob',
    });
    return response.data;
  },
};

export const downloadFile = (data: Blob, fileName: string) => {
  const url = window.URL.createObjectURL(data);
  const link = document.createElement('a');
  link.href = url;
  link.setAttribute('download', fileName);
  document.body.appendChild(link);
  link.click();
  link.remove();
  window.URL.revokeObjectURL(url);
};