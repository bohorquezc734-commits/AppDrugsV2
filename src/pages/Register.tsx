import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { authService } from '../services/auth';
import { toast } from 'react-toastify';

const Register: React.FC = () => {
  const [form, setForm] = useState({
    email: '',
    password: '',
    fullName: '',
    role: 'User'
  });
  const [loading, setLoading] = useState(false);
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
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-green-50 via-blue-50 to-cyan-50">
      <div className="max-w-md w-full bg-white p-10 rounded-xl shadow-lg">
        {/* Logo y Titulo */}
        <div className="text-center mb-10">
          <div className="text-5xl mb-4">📝</div>
          <h1 className="text-3xl font-bold text-gray-800">Registro</h1>
          <p className="text-gray-500 mt-2 text-sm">Crea tu cuenta nueva</p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          {/* Nombre Completo */}
          <div>
            <label className="block text-gray-700 text-sm font-semibold mb-2">
              Nombre Completo <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              value={form.fullName}
              onChange={(e) => setForm({ ...form, fullName: e.target.value })}
              className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition placeholder-gray-400"
              placeholder="Juan Pérez"
              required
            />
          </div>

          {/* Email */}
          <div>
            <label className="block text-gray-700 text-sm font-semibold mb-2">
              Email <span className="text-red-500">*</span>
            </label>
            <input
              type="email"
              value={form.email}
              onChange={(e) => setForm({ ...form, email: e.target.value })}
              className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition placeholder-gray-400"
              placeholder="correo@ejemplo.com"
              required
            />
          </div>

          {/* Contraseña */}
          <div>
            <label className="block text-gray-700 text-sm font-semibold mb-2">
              Contraseña <span className="text-red-500">*</span>
            </label>
            <input
              type="password"
              value={form.password}
              onChange={(e) => setForm({ ...form, password: e.target.value })}
              className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition placeholder-gray-400"
              placeholder="••••••"
              required
              minLength={8}
            />
            <p className="text-xs text-gray-500 mt-1">Mínimo 8 caracteres con mayúscula, minúscula y número</p>
          </div>

          {/* Rol */}
          <div>
            <label className="block text-gray-700 text-sm font-semibold mb-2">
              Rol <span className="text-red-500">*</span>
            </label>
            <select
              value={form.role}
              onChange={(e) => setForm({ ...form, role: e.target.value })}
              className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition text-gray-700"
            >
              <option value="User">Usuario (Afiliado)</option>
              <option value="Pharmacist">Gestor Farmacéutico</option>
              <option value="Admin">Administrador</option>
            </select>
          </div>

          {/* Botón Registro */}
          <button
            type="submit"
            disabled={loading}
            className="w-full bg-gradient-to-r from-green-500 to-green-600 text-white py-3 rounded-full font-bold text-lg hover:from-green-600 hover:to-green-700 transition disabled:opacity-50 disabled:cursor-not-allowed mt-6"
          >
            {loading ? 'Registrando...' : 'Registrarse'}
          </button>
        </form>

        {/* Link de login */}
        <p className="text-center text-sm text-gray-600 mt-6 border-t pt-6">
          ¿Ya tienes cuenta?{' '}
          <Link to="/login" className="text-blue-600 hover:underline font-semibold">
            Inicia sesión aquí
          </Link>
        </p>
      </div>
    </div>
  );
};

export default Register;