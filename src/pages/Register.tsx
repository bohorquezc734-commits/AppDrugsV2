import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { authService } from '../services/auth';
import { toast } from 'react-toastify';

const EyeIcon = ({ open }: { open: boolean }) => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    {open ? (
      <>
        <path d="M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7z"/>
        <circle cx="12" cy="12" r="3"/>
      </>
    ) : (
      <>
        <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94"/>
        <path d="M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19"/>
        <line x1="1" y1="1" x2="23" y2="23"/>
      </>
    )}
  </svg>
);

const Register: React.FC = () => {
  const [form, setForm] = useState({
    email: '',
    password: '',
    fullName: '',
    role: 'User'
  });
  const [loading, setLoading] = useState(false);
  const [showPwd, setShowPwd] = useState(false);
  
  // Focus states
  const [focusName, setFocusName] = useState(false);
  const [focusEmail, setFocusEmail] = useState(false);
  const [focusPwd, setFocusPwd] = useState(false);
  const [focusRole, setFocusRole] = useState(false);

  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      await authService.register(form);
      toast.success('¡Registro exitoso! Ahora inicia sesión.');
      navigate('/login');
    } catch (err: any) {
      const errorMessage = err.response?.data?.error || 'Error al registrar usuario';
      toast.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const inputStyle = (isFocused: boolean) => ({
    width: '100%',
    padding: '11px 14px',
    borderRadius: 10,
    border: `1.5px solid ${isFocused ? '#2563eb' : '#e2e8f0'}`,
    fontSize: 14,
    color: '#1e293b',
    outline: 'none',
    background: isFocused ? '#f8fbff' : '#f9fafb',
    transition: 'all 0.2s',
    boxShadow: isFocused ? '0 0 0 3px rgba(37,99,235,0.12)' : 'none',
    boxSizing: 'border-box' as any,
  });

  const labelStyle = {
    display: 'block', 
    fontSize: 13, 
    fontWeight: 600,
    color: '#374151', 
    marginBottom: 6,
  };

  return (
    <div style={{
      minHeight: '100vh',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      background: 'radial-gradient(ellipse at 60% 20%, #dbeafe 0%, #eef2fb 45%, #f0fdf4 100%)',
      fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif",
      padding: '24px',
    }}>
      {/* Card */}
      <div style={{
        width: '100%',
        maxWidth: 420,
        background: '#ffffff',
        borderRadius: 20,
        boxShadow: '0 20px 60px rgba(37,99,235,0.12), 0 4px 16px rgba(0,0,0,0.06)',
        padding: '40px 36px 32px',
        position: 'relative',
        overflow: 'hidden',
      }}>
        {/* Top accent line */}
        <div style={{
          position: 'absolute', top: 0, left: 0, right: 0, height: 4,
          background: 'linear-gradient(90deg, #2563eb 0%, #3b82f6 50%, #60a5fa 100%)',
        }} />

        {/* Logo */}
        <div style={{ textAlign: 'center', marginBottom: 28 }}>
          <div style={{
            display: 'inline-flex',
            alignItems: 'center',
            justifyContent: 'center',
            width: 64, height: 64,
            background: 'linear-gradient(135deg, #2563eb 0%, #3b82f6 100%)',
            borderRadius: 18,
            boxShadow: '0 8px 24px rgba(37,99,235,0.35)',
            marginBottom: 14,
          }}>
            <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="white" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round">
              <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2" />
              <circle cx="9" cy="7" r="4" />
              <path d="M22 21v-2a4 4 0 0 0-3-3.87" />
              <path d="M16 3.13a4 4 0 0 1 0 7.75" />
            </svg>
          </div>
          <div style={{ fontWeight: 800, fontSize: 24, color: '#1e293b', letterSpacing: '-0.5px', lineHeight: 1 }}>
            Crear cuenta
          </div>
          <div style={{ fontSize: 13, color: '#64748b', marginTop: 6, fontWeight: 400 }}>
            Únete al Sistema de Gestión Farmacéutica
          </div>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit}>
          
          {/* Full Name */}
          <div style={{ marginBottom: 16 }}>
            <label style={labelStyle}>
              Nombre Completo <span style={{ color: '#ef4444' }}>*</span>
            </label>
            <input
              type="text"
              value={form.fullName}
              onChange={e => setForm({ ...form, fullName: e.target.value })}
              onFocus={() => setFocusName(true)}
              onBlur={() => setFocusName(false)}
              placeholder="Juan Pérez"
              required
              style={inputStyle(focusName)}
            />
          </div>

          {/* Email */}
          <div style={{ marginBottom: 16 }}>
            <label style={labelStyle}>
              Correo <span style={{ color: '#ef4444' }}>*</span>
            </label>
            <input
              type="email"
              value={form.email}
              onChange={e => setForm({ ...form, email: e.target.value })}
              onFocus={() => setFocusEmail(true)}
              onBlur={() => setFocusEmail(false)}
              placeholder="nombre@ejemplo.com"
              required
              style={inputStyle(focusEmail)}
            />
          </div>

          {/* Password field */}
          <div style={{ marginBottom: 16 }}>
            <label style={labelStyle}>
              Contraseña <span style={{ color: '#ef4444' }}>*</span>
            </label>
            <div style={{ position: 'relative' }}>
              <input
                type={showPwd ? 'text' : 'password'}
                value={form.password}
                onChange={e => setForm({ ...form, password: e.target.value })}
                onFocus={() => setFocusPwd(true)}
                onBlur={() => setFocusPwd(false)}
                placeholder="••••••••"
                required
                minLength={8}
                style={{
                  ...inputStyle(focusPwd),
                  paddingRight: 42,
                }}
              />
              <button
                type="button"
                onClick={() => setShowPwd(!showPwd)}
                style={{
                  position: 'absolute', right: 12, top: '50%',
                  transform: 'translateY(-50%)',
                  background: 'none', border: 'none',
                  color: '#94a3b8', cursor: 'pointer',
                  display: 'flex', alignItems: 'center',
                  padding: 2,
                }}
              >
                <EyeIcon open={showPwd} />
              </button>
            </div>
            <p style={{ fontSize: 11.5, color: '#94a3b8', marginTop: 6, lineHeight: 1.4 }}>
              Mínimo 8 caracteres con mayúscula, minúscula y número.
            </p>
          </div>

          {/* Role */}
          <div style={{ marginBottom: 24 }}>
            <label style={labelStyle}>
              Rol <span style={{ color: '#ef4444' }}>*</span>
            </label>
            <select
              value={form.role}
              onChange={e => setForm({ ...form, role: e.target.value })}
              onFocus={() => setFocusRole(true)}
              onBlur={() => setFocusRole(false)}
              style={inputStyle(focusRole)}
            >
              <option value="User">Usuario (Afiliado)</option>
              <option value="Pharmacist">Gestor Farmacéutico</option>
              <option value="Admin">Administrador</option>
            </select>
          </div>

          {/* Submit button */}
          <button
            type="submit"
            disabled={loading}
            style={{
              width: '100%',
              padding: '13px',
              background: loading
                ? '#93c5fd'
                : 'linear-gradient(135deg, #2563eb 0%, #3b82f6 100%)',
              color: '#fff',
              border: 'none',
              borderRadius: 12,
              fontWeight: 700,
              fontSize: 15,
              letterSpacing: '0.3px',
              cursor: loading ? 'not-allowed' : 'pointer',
              boxShadow: loading ? 'none' : '0 4px 16px rgba(37,99,235,0.35)',
              transition: 'all 0.2s',
              marginBottom: 16,
            }}
            onMouseEnter={e => {
              if (!loading) (e.currentTarget as HTMLElement).style.transform = 'translateY(-1px)';
            }}
            onMouseLeave={e => {
              (e.currentTarget as HTMLElement).style.transform = 'translateY(0)';
            }}
          >
            {loading ? 'Registrando...' : 'REGISTRARSE'}
          </button>
        </form>

        {/* Divider + Login */}
        <div style={{
          borderTop: '1px solid #f1f5f9',
          marginTop: 16,
          paddingTop: 16,
          textAlign: 'center',
        }}>
          <span style={{ fontSize: 13, color: '#64748b' }}>¿Ya tienes cuenta? </span>
          <Link
            to="/login"
            style={{
              fontSize: 13, fontWeight: 700,
              color: '#2563eb',
              textDecoration: 'none',
            }}
            onMouseEnter={e => (e.currentTarget as HTMLElement).style.textDecoration = 'underline'}
            onMouseLeave={e => (e.currentTarget as HTMLElement).style.textDecoration = 'none'}
          >
            Inicia sesión aquí
          </Link>
        </div>
      </div>
    </div>
  );
};

export default Register;