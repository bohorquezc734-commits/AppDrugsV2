import api from './api';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  fullName: string;
  role?: string;
}

export interface LoginResponse {
  userId: number;
  fullName: string;
  role: string;
  token: string;
  expiresAt: string;
}

export const authService = {
  login: async (data: LoginRequest): Promise<LoginResponse> => {
    const response = await api.post<LoginResponse>('/Auth/login', data);
    return response.data;
  },

  register: async (data: RegisterRequest) => {
    const response = await api.post('/Auth/register', data);
    return response.data;
  },

  logout: () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    window.location.href = '/login';
  },

  getToken: () => localStorage.getItem('token'),
  
  getUser: () => {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
  },

  isAuthenticated: () => !!localStorage.getItem('token'),

  // 🔐 MÉTODOS DE ROLES
  getUserRole: () => {
    const user = localStorage.getItem('user');
    if (user) {
      try {
        const parsed = JSON.parse(user);
        return parsed.role || null;
      } catch {
        return null;
      }
    }
    return null;
  },

  isAdmin: () => {
    return authService.getUserRole() === 'Admin';
  },

  isPharmacist: () => {
    return authService.getUserRole() === 'Pharmacist';
  },

  isUser: () => {
    return authService.getUserRole() === 'User';
  },

  hasRole: (role: string) => {
    return authService.getUserRole() === role;
  },

  canEditDrugs: () => {
    const userRole = authService.getUserRole();
    return userRole === 'Admin';
  },

  canManageSedes: () => {
    const userRole = authService.getUserRole();
    return userRole === 'Admin';
  },

  canViewReports: () => {
    const userRole = authService.getUserRole();
    return userRole === 'Admin' || userRole === 'Pharmacist';
  },

  canUpdateStock: () => {
    const userRole = authService.getUserRole();
    return userRole === 'Admin' || userRole === 'Pharmacist';
  },
};