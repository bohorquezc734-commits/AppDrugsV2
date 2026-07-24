import React, { useState, useEffect } from 'react';
import { usersService, UserDto } from '../../services/users';

const AdminUsersTab: React.FC = () => {
  const [users, setUsers] = useState<UserDto[]>([]);
  const [loading, setLoading] = useState(true);

  const fetchUsers = async () => {
    try {
      setLoading(true);
      const data = await usersService.getAll();
      setUsers(data);
    } catch (err) {
      console.error('Error fetching users', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchUsers();
  }, []);

  return (
    <div>
      <div style={{ marginBottom: 24 }}>
        <h2 style={{ fontSize: 24, fontWeight: 800, color: '#1e293b', margin: '0 0 8px' }}>Usuarios del Sistema</h2>
        <p style={{ color: '#64748b', margin: 0 }}>Listado global de todos los usuarios registrados en la plataforma.</p>
      </div>

      <div style={{ background: '#fff', borderRadius: 12, border: '1px solid #e2e8f0', overflow: 'hidden' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 14 }}>
          <thead>
            <tr style={{ background: '#f8fafc', borderBottom: '1px solid #e2e8f0', textAlign: 'left' }}>
              <th style={{ padding: '12px 16px', color: '#64748b', fontWeight: 600 }}>ID</th>
              <th style={{ padding: '12px 16px', color: '#64748b', fontWeight: 600 }}>Nombre</th>
              <th style={{ padding: '12px 16px', color: '#64748b', fontWeight: 600 }}>Email</th>
              <th style={{ padding: '12px 16px', color: '#64748b', fontWeight: 600 }}>Rol</th>
              <th style={{ padding: '12px 16px', color: '#64748b', fontWeight: 600 }}>Estado</th>
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr><td colSpan={5} style={{ padding: 24, textAlign: 'center', color: '#64748b' }}>Cargando usuarios...</td></tr>
            ) : users.length === 0 ? (
              <tr><td colSpan={5} style={{ padding: 24, textAlign: 'center', color: '#64748b' }}>No hay usuarios.</td></tr>
            ) : (
              users.map(u => (
                <tr key={u.id} style={{ borderBottom: '1px solid #e2e8f0' }}>
                  <td style={{ padding: '12px 16px', color: '#1e293b' }}>#{u.id}</td>
                  <td style={{ padding: '12px 16px', color: '#1e293b', fontWeight: 500 }}>{u.fullName}</td>
                  <td style={{ padding: '12px 16px', color: '#64748b' }}>{u.email}</td>
                  <td style={{ padding: '12px 16px', color: '#1e293b' }}>
                    <span style={{ 
                      background: u.role === 'Admin' ? '#fef2f2' : u.role === 'Gestor' ? '#f0fdf4' : '#eff6ff',
                      color: u.role === 'Admin' ? '#dc2626' : u.role === 'Gestor' ? '#16a34a' : '#2563eb',
                      padding: '2px 8px', borderRadius: 12, fontSize: 12, fontWeight: 600 
                    }}>
                      {u.role}
                    </span>
                  </td>
                  <td style={{ padding: '12px 16px' }}>
                    <span style={{ color: u.isActive ? '#10b981' : '#ef4444', fontWeight: 600 }}>
                      {u.isActive ? 'Activo' : 'Inactivo'}
                    </span>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default AdminUsersTab;
