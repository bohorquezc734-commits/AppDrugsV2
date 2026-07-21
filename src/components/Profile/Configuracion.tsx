import React from 'react';
import { authService } from '../../services/auth';
import { toast } from 'react-toastify';

const Configuracion: React.FC = () => {
  const user = authService.getUser();

  const handleNotImplemented = () => {
    toast.info('Función en desarrollo');
  };

  const getRoleName = (role: string) => {
    if (role === 'Admin') return 'Administrador';
    if (role === 'Pharmacist') return 'Gestor Farmacéutico';
    if (role === 'User') return 'Afiliado';
    return role;
  };

  return (
    <div style={{ maxWidth: 800, margin: '0 auto' }}>
      <h2 style={{ fontSize: 24, fontWeight: 800, color: '#1e293b', marginBottom: 24 }}>Configuración de Perfil</h2>

      <div style={{
        background: '#fff',
        borderRadius: 16,
        padding: 32,
        boxShadow: '0 4px 12px rgba(0,0,0,0.03)',
        marginBottom: 24,
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 24, marginBottom: 32 }}>
          <div style={{
            width: 80, height: 80,
            borderRadius: '50%',
            background: 'linear-gradient(135deg, #2563eb, #7c3aed)',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            color: '#fff', fontWeight: 700, fontSize: 32,
            boxShadow: '0 8px 16px rgba(37,99,235,0.2)'
          }}>
            {user?.fullName ? user.fullName[0].toUpperCase() : 'U'}
          </div>
          <div>
            <h3 style={{ fontSize: 22, fontWeight: 700, color: '#1e293b', margin: 0 }}>
              {user?.fullName || 'Nombre no disponible'}
            </h3>
            <p style={{ fontSize: 14, color: '#64748b', marginTop: 4 }}>
              {user?.email || 'Email no disponible'}
            </p>
            <span style={{
              display: 'inline-block',
              marginTop: 10,
              padding: '4px 12px',
              background: '#eff6ff',
              color: '#2563eb',
              borderRadius: 20,
              fontSize: 12,
              fontWeight: 700,
              textTransform: 'uppercase',
              letterSpacing: '0.5px'
            }}>
              {getRoleName(user?.role || '')}
            </span>
          </div>
        </div>

        <div style={{ borderTop: '1px solid #f1f5f9', paddingTop: 32 }}>
          <h4 style={{ fontSize: 16, fontWeight: 700, color: '#334155', marginBottom: 20 }}>Información de la Cuenta</h4>
          
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 24 }}>
            <div>
              <label style={{ display: 'block', fontSize: 13, fontWeight: 600, color: '#64748b', marginBottom: 8 }}>
                Nombre Completo
              </label>
              <input
                type="text"
                value={user?.fullName || ''}
                readOnly
                style={{
                  width: '100%', padding: '12px 14px', borderRadius: 10, border: '1.5px solid #e2e8f0',
                  background: '#f8fafc', color: '#475569', fontSize: 14, outline: 'none'
                }}
              />
            </div>
            <div>
              <label style={{ display: 'block', fontSize: 13, fontWeight: 600, color: '#64748b', marginBottom: 8 }}>
                Correo Electrónico
              </label>
              <input
                type="email"
                value={user?.email || ''}
                readOnly
                style={{
                  width: '100%', padding: '12px 14px', borderRadius: 10, border: '1.5px solid #e2e8f0',
                  background: '#f8fafc', color: '#475569', fontSize: 14, outline: 'none'
                }}
              />
            </div>
          </div>
          <div style={{ marginTop: 24, display: 'flex', justifyContent: 'flex-end' }}>
            <button
              onClick={handleNotImplemented}
              style={{
                padding: '10px 20px', background: '#f1f5f9', color: '#475569',
                border: 'none', borderRadius: 10, fontWeight: 600, fontSize: 14,
                cursor: 'pointer', transition: 'background 0.2s'
              }}
            >
              Guardar Cambios
            </button>
          </div>
        </div>
      </div>

      <div style={{
        background: '#fff',
        borderRadius: 16,
        padding: 32,
        boxShadow: '0 4px 12px rgba(0,0,0,0.03)',
      }}>
        <h4 style={{ fontSize: 16, fontWeight: 700, color: '#334155', marginBottom: 8 }}>Seguridad</h4>
        <p style={{ fontSize: 14, color: '#64748b', marginBottom: 20 }}>
          Si crees que tu cuenta ha sido comprometida, puedes cambiar tu contraseña.
        </p>
        <button
          onClick={handleNotImplemented}
          style={{
            padding: '12px 24px', background: 'linear-gradient(135deg, #2563eb 0%, #3b82f6 100%)',
            color: '#fff', border: 'none', borderRadius: 10, fontWeight: 700, fontSize: 14,
            cursor: 'pointer', boxShadow: '0 4px 12px rgba(37,99,235,0.3)', transition: 'transform 0.2s'
          }}
        >
          Cambiar Contraseña
        </button>
      </div>
    </div>
  );
};

export default Configuracion;
