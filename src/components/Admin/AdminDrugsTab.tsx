import React, { useState, useEffect, useCallback } from 'react';
import { toast } from 'react-toastify';
import { useForm } from 'react-hook-form';
import { drugsService, Drug } from '../../services/drugs';
import { useDrugiStore } from '../../store/useDrugiStore';

type DrugFormData = {
  name: string;
  genericName: string;
  laboratory: string;
  price: number;
  stock: number;
  category: string;
  requiresPrescription: boolean;
  expirationDate: string;
};

const AdminDrugsTab: React.FC = () => {
  const [drugs, setDrugs] = useState<Drug[]>([]);
  const [loadingDrugs, setLoadingDrugs] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [showDrugModal, setShowDrugModal] = useState(false);
  const [editingDrugId, setEditingDrugId] = useState<number | null>(null);
  const [drugCurrentPage, setDrugCurrentPage] = useState(1);
  const pageSize = 10;

  const { showMessage } = useDrugiStore();

  const { register, handleSubmit, reset, setValue, formState: { errors } } = useForm<DrugFormData>({
    defaultValues: {
      name: '', genericName: '', laboratory: '', price: 0, stock: 0,
      category: '', requiresPrescription: false, expirationDate: ''
    }
  });

  const loadDrugs = useCallback(async (page = 1) => {
    try {
      setLoadingDrugs(true);
      const data = await drugsService.getAll({ searchTerm: searchTerm || undefined, page, pageSize });
      setDrugs(data);
      setDrugCurrentPage(page);
    } catch {
      toast.error('Error cargando medicamentos');
    } finally {
      setLoadingDrugs(false);
    }
  }, [searchTerm]);

  useEffect(() => {
    loadDrugs(1);
  }, [loadDrugs]);

  const openCreateModal = () => {
    setEditingDrugId(null);
    reset({ name: '', genericName: '', laboratory: '', price: 0, stock: 0, category: '', requiresPrescription: false, expirationDate: '' });
    setShowDrugModal(true);
  };

  const openEditModal = (drug: Drug) => {
    setEditingDrugId(drug.id);
    reset({
      name: drug.name,
      genericName: drug.genericName,
      laboratory: drug.laboratory,
      price: drug.price,
      stock: drug.stock,
      category: drug.category,
      requiresPrescription: drug.requiresPrescription,
      expirationDate: drug.expirationDate.split('T')[0]
    });
    setShowDrugModal(true);
  };

  const onSubmit = async (data: DrugFormData) => {
    try {
      if (editingDrugId) {
        await drugsService.update({ id: editingDrugId, ...data });
        toast.success('Medicamento actualizado');
      } else {
        await drugsService.create(data);
        toast.success('Medicamento creado exitosamente');
        showMessage('¡Excelente! El nuevo medicamento ya está en el catálogo. 💊', 'feliz');
      }
      setShowDrugModal(false);
      loadDrugs(drugCurrentPage);
    } catch { 
      toast.error(editingDrugId ? 'Error al actualizar medicamento' : 'Error al crear medicamento'); 
    }
  };

  const handleDeleteDrug = async (id: number) => {
    if (!window.confirm('¿Eliminar este medicamento?')) return;
    try {
      await drugsService.delete(id);
      toast.success('Medicamento eliminado');
      loadDrugs(drugCurrentPage);
    } catch { toast.error('Error al eliminar'); }
  };

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
        <h2 className="text-2xl font-bold text-gray-800">Catálogo de Medicamentos</h2>
        <div className="flex gap-3">
          <div className="relative">
            <span className="absolute inset-y-0 left-0 flex items-center pl-3 text-gray-400">🔍</span>
            <input 
              type="text" 
              value={searchTerm} 
              onChange={e => setSearchTerm(e.target.value)} 
              placeholder="Buscar medicamento..." 
              className="pl-10 pr-4 py-2.5 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500 w-64 transition-all shadow-sm" 
            />
          </div>
          <button onClick={() => loadDrugs(1)} className="bg-gray-800 hover:bg-gray-900 text-white px-5 py-2.5 rounded-xl font-bold transition-all shadow-sm">
            Buscar
          </button>
          <button onClick={openCreateModal} className="bg-gradient-to-r from-emerald-500 to-green-600 hover:from-emerald-600 hover:to-green-700 text-white px-5 py-2.5 rounded-xl font-bold shadow-lg shadow-green-500/30 transition-all">
            + Nuevo Medicamento
          </button>
        </div>
      </div>

      {loadingDrugs ? <p>Cargando...</p> : (
        <div className="grid gap-5 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {drugs.map(drug => (
            <div key={drug.id} className="bg-white p-6 rounded-2xl shadow-sm border border-gray-100 transition-all hover:shadow-xl hover:-translate-y-1 relative overflow-hidden group">
              <div className="absolute top-0 right-0 w-20 h-20 bg-blue-50 rounded-bl-full -z-10 transition-transform group-hover:scale-110"></div>
              
              <h3 className="font-extrabold text-xl text-gray-900 truncate" title={drug.name}>{drug.name}</h3>
              <p className="text-sm text-gray-500 mb-4 truncate" title={`${drug.genericName} - ${drug.laboratory}`}>{drug.genericName} - {drug.laboratory}</p>
              
              <div className="flex justify-between items-end mt-4 pt-4 border-t border-gray-100/60">
                <div>
                   <p className="text-[11px] uppercase tracking-wider font-bold text-gray-400 mb-1">Precio</p>
                   <p className="text-green-600 font-black text-xl">${drug.price.toFixed(2)}</p>
                </div>
                <div className="text-right">
                   <p className="text-[11px] uppercase tracking-wider font-bold text-gray-400 mb-1">Stock</p>
                   <p className="font-black text-blue-600 text-xl">{drug.stock}</p>
                </div>
              </div>
              
              <div className="mt-5 flex gap-2">
                <button onClick={() => openEditModal(drug)} className="flex-1 bg-gray-50 hover:bg-blue-50 text-gray-700 hover:text-blue-700 border border-gray-200 hover:border-blue-200 font-bold py-2.5 rounded-xl transition-all">
                  Editar
                </button>
                <button onClick={() => handleDeleteDrug(drug.id)} className="flex-1 bg-gray-50 hover:bg-red-50 text-gray-700 hover:text-red-700 border border-gray-200 hover:border-red-200 font-bold py-2.5 rounded-xl transition-all">
                  Eliminar
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
      
      <div className="flex justify-center gap-3 mt-10">
        <button onClick={() => loadDrugs(drugCurrentPage - 1)} disabled={drugCurrentPage === 1} className={`px-5 py-2.5 rounded-xl font-bold transition-all ${drugCurrentPage === 1 ? 'bg-gray-100 text-gray-400 cursor-not-allowed' : 'bg-white border hover:shadow-md text-gray-700'}`}>Anterior</button>
        <button onClick={() => loadDrugs(drugCurrentPage + 1)} disabled={drugs.length < pageSize} className={`px-5 py-2.5 rounded-xl font-bold transition-all ${drugs.length < pageSize ? 'bg-gray-100 text-gray-400 cursor-not-allowed' : 'bg-white border hover:shadow-md text-gray-700'}`}>Siguiente</button>
      </div>

      {/* MODAL PREMIUM PARA MEDICAMENTOS */}
      {showDrugModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <div className="absolute inset-0 bg-slate-900/50 backdrop-blur-sm transition-opacity" onClick={() => setShowDrugModal(false)}></div>
          
          <div className="relative bg-white/95 backdrop-blur-md p-8 rounded-3xl w-full max-w-2xl shadow-2xl border border-white/20 transform transition-all max-h-[95vh] overflow-y-auto custom-scrollbar">
            <div className="flex justify-between items-center mb-6">
              <h3 className="font-extrabold text-2xl text-gray-900">{editingDrugId ? 'Editar Medicamento' : 'Nuevo Medicamento'}</h3>
              <button onClick={() => setShowDrugModal(false)} className="text-gray-400 hover:text-gray-900 bg-gray-100 hover:bg-gray-200 rounded-full p-2 transition">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round"><path d="M18 6 6 18"/><path d="m6 6 12 12"/></svg>
              </button>
            </div>
            
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-bold text-gray-700 mb-1.5">Nombre Comercial</label>
                  <input {...register('name', { required: 'Requerido' })} className={`w-full bg-gray-50 border ${errors.name ? 'border-red-400' : 'border-gray-200 focus:border-blue-500'} rounded-xl focus:ring-2 focus:ring-blue-500/20 text-sm p-3 transition-all outline-none`} />
                  {errors.name && <p className="text-xs text-red-500 mt-1 font-bold">{errors.name.message}</p>}
                </div>
                <div>
                  <label className="block text-sm font-bold text-gray-700 mb-1.5">Nombre Genérico</label>
                  <input {...register('genericName', { required: 'Requerido' })} className={`w-full bg-gray-50 border ${errors.genericName ? 'border-red-400' : 'border-gray-200 focus:border-blue-500'} rounded-xl focus:ring-2 focus:ring-blue-500/20 text-sm p-3 transition-all outline-none`} />
                  {errors.genericName && <p className="text-xs text-red-500 mt-1 font-bold">{errors.genericName.message}</p>}
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-bold text-gray-700 mb-1.5">Laboratorio</label>
                  <input {...register('laboratory', { required: 'Requerido' })} className={`w-full bg-gray-50 border ${errors.laboratory ? 'border-red-400' : 'border-gray-200 focus:border-blue-500'} rounded-xl focus:ring-2 focus:ring-blue-500/20 text-sm p-3 transition-all outline-none`} />
                  {errors.laboratory && <p className="text-xs text-red-500 mt-1 font-bold">{errors.laboratory.message}</p>}
                </div>
                <div>
                  <label className="block text-sm font-bold text-gray-700 mb-1.5">Categoría</label>
                  <input {...register('category', { required: 'Requerido' })} className={`w-full bg-gray-50 border ${errors.category ? 'border-red-400' : 'border-gray-200 focus:border-blue-500'} rounded-xl focus:ring-2 focus:ring-blue-500/20 text-sm p-3 transition-all outline-none`} />
                  {errors.category && <p className="text-xs text-red-500 mt-1 font-bold">{errors.category.message}</p>}
                </div>
              </div>

              <div className="grid grid-cols-3 gap-4">
                <div>
                  <label className="block text-sm font-bold text-gray-700 mb-1.5">Precio ($)</label>
                  <input type="number" step="0.01" {...register('price', { required: 'Requerido', min: {value: 0.1, message: '> 0'} })} className={`w-full bg-gray-50 border ${errors.price ? 'border-red-400' : 'border-gray-200 focus:border-blue-500'} rounded-xl focus:ring-2 focus:ring-blue-500/20 text-sm p-3 transition-all outline-none font-bold text-green-700`} />
                  {errors.price && <p className="text-xs text-red-500 mt-1 font-bold">{errors.price.message}</p>}
                </div>
                <div>
                  <label className="block text-sm font-bold text-gray-700 mb-1.5">Stock Inicial</label>
                  <input type="number" {...register('stock', { required: 'Requerido', min: {value: 0, message: '>= 0'} })} className={`w-full bg-gray-50 border ${errors.stock ? 'border-red-400' : 'border-gray-200 focus:border-blue-500'} rounded-xl focus:ring-2 focus:ring-blue-500/20 text-sm p-3 transition-all outline-none font-bold text-blue-700`} />
                  {errors.stock && <p className="text-xs text-red-500 mt-1 font-bold">{errors.stock.message}</p>}
                </div>
                <div>
                  <label className="block text-sm font-bold text-gray-700 mb-1.5">Vencimiento</label>
                  <input type="date" {...register('expirationDate', { required: 'Requerido' })} className={`w-full bg-gray-50 border ${errors.expirationDate ? 'border-red-400' : 'border-gray-200 focus:border-blue-500'} rounded-xl focus:ring-2 focus:ring-blue-500/20 text-sm p-3 transition-all outline-none`} />
                  {errors.expirationDate && <p className="text-xs text-red-500 mt-1 font-bold">{errors.expirationDate.message}</p>}
                </div>
              </div>

              <div className="pt-2">
                <label className="flex items-center gap-3 p-4 border border-gray-200 rounded-xl bg-gray-50 cursor-pointer hover:bg-gray-100 transition-colors">
                  <input type="checkbox" {...register('requiresPrescription')} className="w-5 h-5 text-blue-600 rounded focus:ring-blue-500" />
                  <span className="font-bold text-gray-700">Requiere receta médica obligatoria</span>
                </label>
              </div>

              <div className="pt-6 flex gap-3">
                <button type="button" onClick={() => setShowDrugModal(false)} className="flex-1 bg-gray-100 hover:bg-gray-200 text-gray-800 font-bold py-3.5 rounded-xl transition-colors">
                  Cancelar
                </button>
                <button type="submit" className="flex-1 bg-blue-600 hover:bg-blue-700 text-white font-bold py-3.5 rounded-xl shadow-lg shadow-blue-500/30 transition-all">
                  {editingDrugId ? 'Guardar Cambios' : 'Crear Medicamento'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default AdminDrugsTab;
