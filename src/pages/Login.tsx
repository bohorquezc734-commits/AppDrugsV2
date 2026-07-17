import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { authService } from '../services/auth';
import { toast } from 'react-toastify';

const Login: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);

    try {
      const response = await authService.login({ email, password });
      localStorage.setItem('token', response.token);
      localStorage.setItem('user', JSON.stringify(response));
      toast.success('¡Inicio de sesión exitoso!');
      navigate('/dashboard');
    } catch (err: any) {
      const errorMessage = err.response?.data?.error || 'Error al iniciar sesión';
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
          <div className="text-5xl mb-4">💊</div>
          <h1 className="text-3xl font-bold text-gray-800">AppDrugsV2</h1>
          <p className="text-gray-500 mt-2 text-sm">Inicia sesión en tu cuenta</p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-5">
          {/* Email */}
          <div>
            <label className="block text-gray-700 text-sm font-semibold mb-2">
              Email <span className="text-red-500">*</span>
            </label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition placeholder-gray-400"
              placeholder="nombre@ejemplo.com"
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
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition placeholder-gray-400"
              placeholder="••••••"
              required
            />
          </div>

          {/* Botón Login */}
          <button
            type="submit"
            disabled={loading}
            className="w-full bg-gradient-to-r from-green-500 to-green-600 text-white py-3 rounded-full font-bold text-lg hover:from-green-600 hover:to-green-700 transition disabled:opacity-50 disabled:cursor-not-allowed mt-6"
          >
            {loading ? 'Cargando...' : 'Iniciar Sesión'}
          </button>
        </form>

        {/* Link de recuperación */}
        <div className="text-center mt-6">
          <button type="button" className="text-gray-600 text-sm hover:text-gray-800">
            ¿Olvidaste tu contraseña?
          </button>
        </div>

        {/* Link de registro */}
        <p className="text-center text-sm text-gray-600 mt-6 border-t pt-6">
          ¿No tienes cuenta?{' '}
          <Link to="/register" className="text-blue-600 hover:underline font-semibold">
            Regístrate aquí
          </Link>
        </p>
      </div>
    </div>
  );
};

export default Login;