import api from './api';

export interface GestorDto {
  id: number;
  nombreSede: string;
  direccion: string;
  telefono: string;
  idEps: number;
  isActive: boolean;
  createdAt: string;
}

export interface CreateGestorRequest {
  nombreSede: string;
  direccion: string;
  telefono: string;
  idEps: number;
}

export const gestoresService = {
  getAll: async (params?: { searchTerm?: string; isActive?: boolean; page?: number; pageSize?: number }) => {
    const response = await api.get<GestorDto[]>('/Gestores', { params });
    return response.data;
  },

  create: async (data: CreateGestorRequest) => {
    const response = await api.post('/Gestores', data);
    return response.data;
  },
};
