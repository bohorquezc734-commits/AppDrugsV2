import api from './api';

export interface AuditLog {
  id: number;
  userId: number | null;
  userName?: string;
  action: string;
  entityName: string;
  primaryKey: string;
  oldValues: string | null;
  newValues: string | null;
  timestamp: string;
}

export const auditService = {
  getAuditLogs: async (): Promise<AuditLog[]> => {
    const response = await api.get<AuditLog[]>('/AuditLogs');
    return response.data;
  },
};
