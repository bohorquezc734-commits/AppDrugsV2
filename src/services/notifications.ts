import api from './api';

export interface NotificationDto {
  id: number;
  message: string;
  type: string;
  isRead: boolean;
  createdAt: string;
}

export const notificationsService = {
  getMyNotifications: async () => {
    const response = await api.get('/Notifications/me');
    return response.data as NotificationDto[];
  },

  markAsRead: async (id: number) => {
    const response = await api.put(`/Notifications/${id}/read`);
    return response.data;
  }
};
