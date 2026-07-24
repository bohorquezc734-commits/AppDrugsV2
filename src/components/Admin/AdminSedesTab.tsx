import React, { useState, useEffect, useCallback } from 'react';
import { toast } from 'react-toastify';
import { useForm } from 'react-hook-form';
import { gestoresService, GestorDto } from '../../services/gestores';

type GestorFormData = {
  nombreSede: string;
  direccion: string;
  telefono: string;
  idEps: number;
};

const AdminSedesTab: React.FC = () => {
  const [gestores, setGestores] = useState<GestorDto[]>([]);
  const [loadingGestores, setLoadingGestores] = useState(false);
  const [showCreateGestorModal, setShowCreateGestorModal] = useState(false);

  const { register, handleSubmit, reset, formState: { errors } } = useForm<GestorFormData>({
    defaultValues: { nombreSede: '', direccion: '', telefono: '', idEps: 1 }
  });

  const loadGestores = useCallback(async () => {
    try {
      setLoadingGestores(true);
      const data = await gestoresService.getAll();
      setGestores(data);
    } catch { toast.error('Error cargando sedes'); }
    finally { setLoadingGestores(false); }
  }, []);

  useEffect(() => {
    loadGestores();
  }, [loadGestores]);

  const onSubmit = async (data: GestorFormData) => {
    try {
      await gestoresService.create(data);
      toast.success('Sede creada exitosamente');
      setShowCreateGestorModal(false);
      reset();
      loadGestores();
    } catch { toast.error('Error al crear sede'); }
  };

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
        <h2 className="text-2xl font-bold text-gray-800">Sedes Farmacéuticas (Gestores)</h2>
        <button onClick={() => setShowCreateGestorModal(true)} className="bg-gradient-to-r from-blue-500 to-indigo-600 hover:from-blue-600 hover:to-indigo-700 text-white px-5 py-2.5 rounded-xl font-bold shadow-lg shadow-blue-500/30 transition-all">
          + Nueva Sede
        </button>
      </div>

      {loadingGestores ? <p>Cargando...</p> : (
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
          <table className="w-full text-left">
            <thead className="bg-gray-50 border-b border-gray-100">
              <tr>
                <th className="p-4 text-gray-500 font-semibold text-sm">ID</th>
                <th className="p-4 text-gray-500 font-semibold text-sm">Nombre Sede</th>
                <th className="p-4 text-gray-500 font-semibold text-sm">Dirección</th>
                <th className="p-4 text-gray-500 font-semibold text-sm">Teléfono</th>
                <th className="p-4 text-gray-500 font-semibold text-sm">Estado</th>
              </tr>
            </thead>
            <tbody>
              {gestores.map(g => (
                <tr key={g.id} className="border-b border-gray-50 hover:bg-gray-50/50 transition">
                  <td className="p-4 text-gray-500 text-sm">{g.id}</td>
                  <td className="p-4 font-bold text-gray-800">{g.nombreSede}</td>
                  <td className="p-4 text-gray-600">{g.direccion}</td>
                  <td className="p-4 text-gray-600 font-mono text-sm">{g.telefono}</td>
                  <td className="p-4">
                    {g.isActive ? 
                      <span className="bg-emerald-50 border border-emerald-200 text-emerald-700 px-3 py-1 rounded-full text-xs font-bold">Activo</span> : 
                      <span className="bg-rose-50 border border-rose-200 text-rose-700 px-3 py-1 rounded-full text-xs font-bold">Inactivo</span>
                    }
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* MODAL PREMIUM (Glassmorphism) PARA CREAR SEDE */}
      {showCreateGestorModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <div className="absolute inset-0 bg-slate-900/40 backdrop-blur-sm transition-opacity" onClick={() => setShowCreateGestorModal(false)}></div>
          
          <div className="relative bg-white/95 backdrop-blur-md p-8 rounded-2xl w-full max-w-md shadow-2xl border border-white/20 transform transition-all">
            <div className="flex justify-between items-center mb-6">
              <h3 className="font-extrabold text-2xl text-gray-900 tracking-tight">Añadir Sede</h3>
              <button onClick={() => setShowCreateGestorModal(false)} className="text-gray-400 hover:text-gray-600 bg-gray-100 hover:bg-gray-200 rounded-full p-2 transition">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round"><path d="M18 6 6 18"/><path d="m6 6 12 12"/></svg>
              </button>
            </div>
            
            <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-5">
              <div>
                <label className="block text-sm font-bold text-gray-700 mb-1.5">Nombre de la Sede</label>
                <input 
                  {...register('nombreSede', { required: 'El nombre es obligatorio', minLength: { value: 3, message: 'Mínimo 3 caracteres' } })} 
                  className={`w-full bg-gray-50 border ${errors.nombreSede ? 'border-red-400 focus:ring-red-500' : 'border-gray-200 focus:border-blue-500 focus:ring-blue-500'} text-gray-900 text-sm rounded-xl focus:ring-2 focus:outline-none block p-3.5 transition-all`} 
                  placeholder="Ej: Farmacia Central" 
                />
                {errors.nombreSede && <p className="mt-1 text-sm text-red-500 font-medium">{errors.nombreSede.message}</p>}
              </div>
              
              <div>
                <label className="block text-sm font-bold text-gray-700 mb-1.5">Dirección Completa</label>
                <input 
                  {...register('direccion', { required: 'La dirección es obligatoria' })} 
                  className={`w-full bg-gray-50 border ${errors.direccion ? 'border-red-400 focus:ring-red-500' : 'border-gray-200 focus:border-blue-500 focus:ring-blue-500'} text-gray-900 text-sm rounded-xl focus:ring-2 focus:outline-none block p-3.5 transition-all`} 
                  placeholder="Ej: Av. Siempreviva 742" 
                />
                {errors.direccion && <p className="mt-1 text-sm text-red-500 font-medium">{errors.direccion.message}</p>}
              </div>
              
              <div>
                <label className="block text-sm font-bold text-gray-700 mb-1.5">Teléfono de Contacto</label>
                <input 
                  {...register('telefono', { required: 'El teléfono es obligatorio', pattern: { value: /^[0-9+ ]+$/, message: 'Solo números' } })} 
                  className={`w-full bg-gray-50 border ${errors.telefono ? 'border-red-400 focus:ring-red-500' : 'border-gray-200 focus:border-blue-500 focus:ring-blue-500'} text-gray-900 text-sm rounded-xl focus:ring-2 focus:outline-none block p-3.5 transition-all`} 
                  placeholder="Ej: +57 300 123 4567" 
                />
                {errors.telefono && <p className="mt-1 text-sm text-red-500 font-medium">{errors.telefono.message}</p>}
              </div>

              <div className="pt-4 flex gap-3">
                <button type="button" onClick={() => setShowCreateGestorModal(false)} className="flex-1 bg-gray-100 hover:bg-gray-200 text-gray-800 font-bold py-3.5 rounded-xl transition-colors">
                  Cancelar
                </button>
                <button type="submit" className="flex-1 bg-blue-600 hover:bg-blue-700 text-white font-bold py-3.5 rounded-xl shadow-lg shadow-blue-500/30 transition-all">
                  Guardar Sede
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default AdminSedesTab;
