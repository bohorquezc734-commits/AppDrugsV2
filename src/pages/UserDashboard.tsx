import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { drugsService, Drug } from '../services/drugs';
import { authService } from '../services/auth';
import { toast } from 'react-toastify';

const UserDashboard: React.FC = () => {
  const [drugs, setDrugs] = useState<Drug[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const navigate = useNavigate();
  const user = authService.getUser();

  // Verificar que es un User (Afiliado)
  useEffect(() => {
    if (!authService.isUser()) {
      navigate('/dashboard');
    }
  }, [navigate]);

  useEffect(() => {
    loadDrugs();
  }, []);

  const loadDrugs = async () => {
    try {
      setLoading(true);
      const data = await drugsService.getAll({ searchTerm: searchTerm || undefined });
      setDrugs(data);
    } catch (error) {
      console.error('Error cargando medicamentos:', error);
      toast.error('Error cargando medicamentos');
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    loadDrugs();
  };

  const handleLogout = () => {
    authService.logout();
    toast.info('Sesión cerrada');
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <div className="text-4xl mb-4">⏳</div>
          <p className="text-gray-600">Cargando medicamentos...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100">
      {/* Navbar */}
      <nav className="bg-white shadow-md p-4 sticky top-0 z-10">
        <div className="container mx-auto flex flex-wrap justify-between items-center">
          <div className="flex items-center space-x-2">
            <span className="text-2xl">💊</span>
            <h1 className="text-xl font-bold text-gray-800">AppDrugsV2</h1>
          </div>

          <div className="flex items-center space-x-4">
            <span className="text-sm text-gray-600 hidden sm:inline">
              👤 {user?.fullName || 'Usuario'} ({user?.role || 'User'})
            </span>
            <button
              onClick={handleLogout}
              className="bg-red-500 text-white px-4 py-2 rounded-lg hover:bg-red-600 transition text-sm"
            >
              Cerrar Sesión
            </button>
          </div>
        </div>
      </nav>

      <div className="container mx-auto p-4">
        {/* Header */}
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
          <h2 className="text-2xl font-bold text-gray-800">📋 Lista de Medicamentos</h2>
          <form onSubmit={handleSearch} className="flex gap-2 w-full sm:w-auto">
            <input
              type="text"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              placeholder="Buscar medicamento..."
              className="px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 w-full sm:w-48"
            />
            <button
              type="submit"
              className="bg-blue-500 text-white px-4 py-2 rounded-lg hover:bg-blue-600 transition"
            >
              🔍 Buscar
            </button>
          </form>
        </div>

        {/* Información de perfil */}
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6">
          <p className="text-sm text-gray-700">
            <strong>📌 Rol:</strong> Afiliado (Usuario) - Tienes acceso de solo lectura a la lista de medicamentos.
          </p>
        </div>

        {/* Lista de medicamentos - SOLO LECTURA */}
        {drugs.length === 0 ? (
          <div className="bg-white rounded-lg shadow p-8 text-center">
            <p className="text-gray-500">No hay medicamentos disponibles</p>
          </div>
        ) : (
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
            {drugs.map((drug) => (
              <div
                key={drug.id}
                className="bg-white p-4 rounded-lg shadow-md hover:shadow-lg transition"
              >
                <h3 className="font-bold text-lg text-gray-800">{drug.name}</h3>
                <p className="text-sm text-gray-500">{drug.genericName}</p>
                <p className="text-sm text-gray-600 mt-1">🧪 {drug.laboratory}</p>
                <p className="text-green-600 font-bold text-lg mt-2">
                  ${drug.price.toFixed(2)}
                </p>
                <p className="text-sm text-gray-600">📦 Stock: {drug.stock}</p>
                <p className="text-sm text-gray-600">📂 {drug.category}</p>

                {drug.requiresPrescription && (
                  <span className="inline-block mt-2 bg-yellow-200 text-yellow-800 px-2 py-1 rounded text-xs">
                    📋 Requiere receta
                  </span>
                )}
                {drug.isExpired && (
                  <span className="inline-block mt-2 bg-red-200 text-red-800 px-2 py-1 rounded text-xs ml-1">
                    ⚠️ Vencido
                  </span>
                )}

                {/* Sin botones de acción para afiliados */}
                <div className="mt-4 p-3 bg-gray-50 rounded text-center">
                  <p className="text-xs text-gray-500">
                    📖 Solo visualización - Sin permisos para editar
                  </p>
                </div>
              </div>
            ))}
          </div>
        )}

        <p className="text-sm text-gray-500 mt-4 text-center">
          Mostrando {drugs.length} medicamento(s)
        </p>
      </div>
    </div>
  );
};

export default UserDashboard;
