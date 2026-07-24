import api from './api';

export interface UserDto {
  id: number;
  email: string;
  fullName: string;
  role: string;
  isActive: boolean;
  createdAt: string;
  lastLoginAt?: string;
}

export const usersService = {
  getAll: async (): Promise<UserDto[]> => {
    const response = await api.get<UserDto[]>('/Auth/users');
    return response.data;
  },
};
