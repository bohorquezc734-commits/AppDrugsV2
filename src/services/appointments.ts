import api from './api';

export interface AppointmentDetailDto {
  id: number;
  appointmentId: number;
  inventoryId: number;
  drugName: string;
  quantity: number;
  createdAt: string;
}

export interface AppointmentDto {
  id: number;
  userId: number;
  userName: string;
  gestorFarmaceuticoId: number;
  sedeName: string;
  status: number;
  statusName: string;
  archivoNombre?: string;
  createdAt: string;
  fechaEntrega?: string;
  observaciones?: string;
  isActive: boolean;
  details: AppointmentDetailDto[];
}

export interface CreateAppointmentDetailRequest {
  inventoryId: number;
  quantity: number;
}

export const appointmentsService = {
  // Obtener mis turnos (usuario autenticado)
  getMyAppointments: async (): Promise<AppointmentDto[]> => {
    const response = await api.get<AppointmentDto[]>('/Appointments/mis-turnos');
    return response.data;
  },

  // Obtener todos los turnos (Admin/Pharmacist)
  getAll: async (): Promise<AppointmentDto[]> => {
    const response = await api.get<AppointmentDto[]>('/Appointments');
    return response.data;
  },

  // Crear turno con FormData (soporta archivo adjunto)
  create: async (
    gestorFarmaceuticoId: number,
    details: CreateAppointmentDetailRequest[],
    archivo?: File
  ): Promise<{ appointmentId: number }> => {
    const formData = new FormData();
    formData.append('GestorFarmaceuticoId', gestorFarmaceuticoId.toString());

    details.forEach((d, i) => {
      formData.append(`Details[${i}].InventoryId`, d.inventoryId.toString());
      formData.append(`Details[${i}].Quantity`, d.quantity.toString());
    });

    if (archivo) {
      formData.append('file', archivo);
    }

    const response = await api.post<{ appointmentId: number }>('/Appointments', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return response.data;
  },

  // Actualizar estado del turno (Admin/Pharmacist)
  // El backend espera: { AppointmentId, NewStatus } (UpdateAppointmentStatusCommand)
  updateStatus: async (id: number, status: number) => {
    const response = await api.patch(`/Appointments/${id}/status`, { appointmentId: id, newStatus: status });
    return response.data;
  },
};
