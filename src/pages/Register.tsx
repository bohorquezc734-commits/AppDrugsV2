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

  return (
    <div className="relative min-h-screen bg-slate-900 flex items-center justify-end px-8 md:px-24 overflow-hidden font-sans">
      
      {/* Background Animated Blobs */}
      <div className="absolute inset-0 z-0 flex items-center justify-center pointer-events-none">
        <div className="relative w-full max-w-4xl mx-auto h-96">
          <div className="absolute top-0 -left-4 w-72 h-72 bg-emerald-500 rounded-full mix-blend-multiply filter blur-3xl opacity-30 animate-blob"></div>
          <div className="absolute top-0 -right-4 w-72 h-72 bg-teal-400 rounded-full mix-blend-multiply filter blur-3xl opacity-30 animate-blob animation-delay-2000"></div>
          <div className="absolute -bottom-8 left-20 w-72 h-72 bg-blue-500 rounded-full mix-blend-multiply filter blur-3xl opacity-30 animate-blob animation-delay-4000"></div>
        </div>
      </div>

      {/* Glassmorphism Form Container */}
      <div className="relative z-10 w-full max-w-md p-10 bg-white/10 backdrop-blur-xl border border-white/20 rounded-3xl shadow-2xl">
        
        <div className="mb-8 text-center">
          <div className="w-16 h-16 bg-gradient-to-br from-emerald-500 to-teal-600 rounded-2xl flex items-center justify-center shadow-lg shadow-emerald-500/30 mx-auto mb-4">
            <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="white" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2" />
              <circle cx="9" cy="7" r="4" />
              <path d="M22 21v-2a4 4 0 0 0-3-3.87" />
              <path d="M16 3.13a4 4 0 0 1 0 7.75" />
            </svg>
          </div>
          <h1 className="text-3xl font-bold text-white tracking-tight">Crear cuenta</h1>
          <p className="text-slate-300 mt-2 text-sm font-medium">Únete al Sistema de Gestión Farmacéutica</p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-5">
          
          <div>
            <label className="block text-sm font-semibold text-slate-300 mb-1.5">
              Nombre Completo <span className="text-emerald-400">*</span>
            </label>
            <input 
              type="text" 
              value={form.fullName}
              onChange={(e) => setForm({ ...form, fullName: e.target.value })}
              className="w-full px-5 py-3 bg-white/10 border border-white/20 text-white rounded-2xl shadow-inner focus:outline-none focus:ring-2 focus:ring-emerald-500/50 focus:border-emerald-500 transition-all duration-200 placeholder:text-slate-400"
              placeholder="Juan Pérez"
              required
            />
          </div>

          <div>
            <label className="block text-sm font-semibold text-slate-300 mb-1.5">
              Correo Electrónico <span className="text-emerald-400">*</span>
            </label>
            <input 
              type="email" 
              value={form.email}
              onChange={(e) => setForm({ ...form, email: e.target.value })}
              className="w-full px-5 py-3 bg-white/10 border border-white/20 text-white rounded-2xl shadow-inner focus:outline-none focus:ring-2 focus:ring-emerald-500/50 focus:border-emerald-500 transition-all duration-200 placeholder:text-slate-400"
              placeholder="nombre@ejemplo.com"
              required
            />
          </div>
          
          <div>
            <label className="block text-sm font-semibold text-slate-300 mb-1.5">
              Contraseña <span className="text-emerald-400">*</span>
            </label>
            <div className="relative">
              <input 
                type={showPwd ? 'text' : 'password'}
                value={form.password}
                onChange={(e) => setForm({ ...form, password: e.target.value })}
                className="w-full px-5 py-3 bg-white/10 border border-white/20 text-white rounded-2xl shadow-inner focus:outline-none focus:ring-2 focus:ring-emerald-500/50 focus:border-emerald-500 transition-all duration-200 placeholder:text-slate-400 pr-12"
                placeholder="••••••••"
                required
                minLength={8}
              />
              <button
                type="button"
                onClick={() => setShowPwd(!showPwd)}
                className="absolute right-4 top-1/2 -translate-y-1/2 text-slate-400 hover:text-white transition-colors"
              >
                <EyeIcon open={showPwd} />
              </button>
            </div>
            <p className="text-xs text-slate-400 mt-1.5 ml-1">
              Mínimo 8 caracteres con mayúscula, minúscula y número.
            </p>
          </div>

          <div>
            <label className="block text-sm font-semibold text-slate-300 mb-1.5">
              Rol <span className="text-emerald-400">*</span>
            </label>
            <select
              value={form.role}
              onChange={(e) => setForm({ ...form, role: e.target.value })}
              className="w-full px-5 py-3 bg-white/10 border border-white/20 text-white rounded-2xl shadow-inner focus:outline-none focus:ring-2 focus:ring-emerald-500/50 focus:border-emerald-500 transition-all duration-200 appearance-none cursor-pointer"
              style={{
                /* El fondo en las opciones no hereda el glassmorphism tan fácil en HTML puro */
              }}
            >
              <option value="User" className="text-slate-900">Usuario (Afiliado)</option>
              <option value="Pharmacist" className="text-slate-900">Gestor Farmacéutico</option>
              <option value="Admin" className="text-slate-900">Administrador</option>
            </select>
          </div>

          <button 
            type="submit"
            disabled={loading}
            className="w-full py-4 mt-2 bg-emerald-500 text-white rounded-full font-bold text-lg shadow-lg shadow-emerald-500/30 hover:bg-emerald-600 hover:-translate-y-0.5 hover:shadow-xl hover:shadow-emerald-500/40 transition-all duration-200 disabled:opacity-70 disabled:cursor-not-allowed"
          >
            {loading ? 'Registrando...' : 'CREAR CUENTA'}
          </button>
        </form>

        {/* Divider + Login */}
        <div className="mt-6 pt-6 border-t border-white/10 text-center">
          <span className="text-sm text-slate-300">¿Ya tienes cuenta? </span>
          <Link
            to="/login"
            className="text-sm font-bold text-emerald-400 hover:text-emerald-300 hover:underline transition-colors"
          >
            Inicia sesión aquí
          </Link>
        </div>

      </div>
    </div>
  );
};

export default Register;