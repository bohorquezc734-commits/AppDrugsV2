import React, { useState, useEffect, useCallback } from 'react';
import { toast } from 'react-toastify';
import { inventoriesService, InventoryDto } from '../../services/inventories';
import { gestoresService, GestorDto } from '../../services/gestores';
import { drugsService, Drug } from '../../services/drugs';
import { CustomDialog } from '../Common/CustomDialog';

const AdminInventoryTab: React.FC = () => {
  const [inventories, setInventories] = useState<InventoryDto[]>([]);
  const [gestores, setGestores] = useState<GestorDto[]>([]);
  const [drugs, setDrugs] = useState<Drug[]>([]);
  const [loadingInventories, setLoadingInventories] = useState(false);
  const [invFilterSede, setInvFilterSede] = useState<number>(0);
  const [showCreateInvModal, setShowCreateInvModal] = useState(false);
  const [invForm, setInvForm] = useState({ drugId: 0, gestorFarmaceuticoId: 0, quantity: 0 });

  const [stockModal, setStockModal] = useState<{ open: boolean; type: 'add' | 'remove'; invId: number; drugName: string }>({
    open: false, type: 'add', invId: 0, drugName: '',
  });
  const [stockQty, setStockQty] = useState<string>('1');
  const [stockFocus, setStockFocus] = useState(false);

  const [deleteInvModal, setDeleteInvModal] = useState<{ open: boolean; invId: number; drugName: string }>({
    open: false, invId: 0, drugName: '',
  });

  const loadDependencies = useCallback(async () => {
    try {
      const [gest, drg] = await Promise.all([gestoresService.getAll(), drugsService.getAll({ pageSize: 1000 })]);
      setGestores(gest);
      setDrugs(drg);
    } catch { toast.error('Error cargando dependencias'); }
  }, []);

  const loadInventories = useCallback(async () => {
    try {
      setLoadingInventories(true);
      const params = invFilterSede > 0 ? { gestorFarmaceuticoId: invFilterSede } : undefined;
      const data = await inventoriesService.getAll(params);
      setInventories(data);
    } catch { toast.error('Error cargando inventario'); }
    finally { setLoadingInventories(false); }
  }, [invFilterSede]);

  useEffect(() => {
    loadDependencies();
  }, [loadDependencies]);

  useEffect(() => {
    loadInventories();
  }, [loadInventories]);

  const handleCreateInventory = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await inventoriesService.create(invForm);
      toast.success('Inventario creado exitosamente');
      setShowCreateInvModal(false);
      loadInventories();
    } catch { toast.error('Error al crear inventario'); }
  };

  const openStockModal = (type: 'add' | 'remove', inv: InventoryDto) => {
    setStockQty('1');
    setStockModal({ open: true, type, invId: inv.id, drugName: inv.drugName });
  };

  const confirmStock = async () => {
    const q = Number(stockQty);
    if (!stockQty || isNaN(q) || q <= 0) { toast.warn('Ingresa una cantidad válida'); return; }
    try {
      if (stockModal.type === 'add') {
        await inventoriesService.addStock(stockModal.invId, q);
        toast.success('Stock agregado exitosamente');
      } else {
        await inventoriesService.removeStock(stockModal.invId, q);
        toast.success('Stock retirado exitosamente');
      }
      setStockModal(s => ({ ...s, open: false }));
      loadInventories();
    } catch { toast.error(stockModal.type === 'add' ? 'Error al agregar stock' : 'Error al retirar stock'); }
  };

  const confirmDeleteInventory = async () => {
    try {
      await inventoriesService.delete(deleteInvModal.invId);
      toast.success('Inventario eliminado');
      setDeleteInvModal(s => ({ ...s, open: false }));
      loadInventories();
    } catch { toast.error('Error al eliminar inventario'); }
  };

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
        <h2 className="text-2xl font-bold text-gray-800">Inventario por Sede</h2>
        <div className="flex gap-2">
          <select value={invFilterSede} onChange={e => setInvFilterSede(Number(e.target.value))} className="border rounded-lg px-4 py-2 bg-white">
            <option value={0}>Todas las sedes</option>
            {gestores.map(g => <option key={g.id} value={g.id}>{g.nombreSede}</option>)}
          </select>
          <button onClick={() => setShowCreateInvModal(true)} className="bg-green-500 text-white px-4 py-2 rounded-lg font-bold">+ Nuevo Inventario</button>
        </div>
      </div>

      {loadingInventories ? <p>Cargando...</p> : (
        <div className="bg-white rounded-lg shadow overflow-hidden">
          <table className="w-full text-left">
            <thead className="bg-gray-50 border-b">
              <tr><th className="p-4 text-gray-600">Sede</th><th className="p-4 text-gray-600">Medicamento</th><th className="p-4 text-gray-600">Cantidad</th><th className="p-4 text-gray-600">Acciones</th></tr>
            </thead>
            <tbody>
              {inventories.map(inv => (
                <tr key={inv.id} className="border-b hover:bg-gray-50">
                  <td className="p-4 text-gray-700">{inv.sedeName}</td>
                  <td className="p-4 font-semibold text-gray-800">{inv.drugName}</td>
                  <td className="p-4 text-blue-600 font-bold">{inv.quantity}</td>
                  <td className="p-4 flex gap-2">
                     <button onClick={() => openStockModal('add', inv)} className="bg-green-100 text-green-700 hover:bg-green-200 border-none rounded px-3 py-1 text-sm font-bold cursor-pointer transition">+ Stock</button>
                     <button onClick={() => openStockModal('remove', inv)} className="bg-orange-100 text-orange-700 hover:bg-orange-200 border-none rounded px-3 py-1 text-sm font-bold cursor-pointer transition">- Stock</button>
                     <button onClick={() => setDeleteInvModal({ open: true, invId: inv.id, drugName: inv.drugName })} className="bg-red-100 text-red-700 hover:bg-red-200 border-none rounded px-3 py-1 text-sm font-bold cursor-pointer transition">Eliminar</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {showCreateInvModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex justify-center p-4 z-50">
          <div className="bg-white p-6 rounded-xl max-w-md w-full my-auto shadow-2xl">
            <h3 className="font-bold text-xl mb-4 text-gray-800">Registrar Inventario</h3>
            <form onSubmit={handleCreateInventory} className="flex flex-col gap-3">
              <select value={invForm.gestorFarmaceuticoId} onChange={e => setInvForm({...invForm, gestorFarmaceuticoId: Number(e.target.value)})} className="border p-3 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500" required>
                <option value={0}>-- Selecciona Sede --</option>
                {gestores.map(g => <option key={g.id} value={g.id}>{g.nombreSede}</option>)}
              </select>
              <select value={invForm.drugId} onChange={e => setInvForm({...invForm, drugId: Number(e.target.value)})} className="border p-3 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500" required>
                <option value={0}>-- Selecciona Medicamento --</option>
                {drugs.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}
              </select>
              <input type="number" placeholder="Cantidad Inicial" value={invForm.quantity} onChange={e => setInvForm({...invForm, quantity: Number(e.target.value)})} className="border p-3 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500" required min={0} />
              <div className="flex gap-3 mt-4">
                <button type="submit" className="flex-1 bg-blue-600 text-white py-2 rounded-lg font-bold hover:bg-blue-700">Guardar</button>
                <button type="button" onClick={() => setShowCreateInvModal(false)} className="flex-1 bg-gray-200 text-gray-700 py-2 rounded-lg font-bold hover:bg-gray-300">Cancelar</button>
              </div>
            </form>
          </div>
        </div>
      )}

      <CustomDialog
        isOpen={stockModal.open}
        title={stockModal.type === 'add' ? 'Agregar Stock' : 'Retirar Stock'}
        message={`Medicamento: ${stockModal.drugName}`}
        icon={stockModal.type === 'add' ? '📦' : '📤'}
        iconBg={stockModal.type === 'add' ? '#dcfce7' : '#fee2e2'}
        confirmLabel={stockModal.type === 'add' ? 'Agregar' : 'Retirar'}
        confirmColor={stockModal.type === 'add' ? '#16a34a' : '#dc2626'}
        onConfirm={confirmStock}
        onCancel={() => setStockModal(s => ({ ...s, open: false }))}
      >
        <div>
          <label style={{ display:'block', fontSize:13, fontWeight:600, color:'#374151', marginBottom:6 }}>
            {stockModal.type === 'add' ? 'Unidades a agregar' : 'Unidades a retirar'} <span style={{ color:'#ef4444' }}>*</span>
          </label>
          <input
            type="number"
            min={1}
            value={stockQty}
            onChange={e => setStockQty(e.target.value)}
            onFocus={() => setStockFocus(true)}
            onBlur={() => setStockFocus(false)}
            autoFocus
            style={{
              width:'100%', padding:'10px 14px', borderRadius:10,
              border: `1.5px solid ${stockFocus ? '#2563eb' : '#e2e8f0'}`,
              fontSize:15, color:'#1e293b', outline:'none',
              background: stockFocus ? '#f8fbff' : '#f9fafb',
              boxShadow: stockFocus ? '0 0 0 3px rgba(37,99,235,0.12)' : 'none',
              transition:'all 0.2s', boxSizing:'border-box' as any,
            }}
            onKeyDown={e => e.key === 'Enter' && confirmStock()}
          />
        </div>
      </CustomDialog>

      <CustomDialog
        isOpen={deleteInvModal.open}
        title="Eliminar inventario"
        message={`¿Estás seguro de eliminar el inventario de "${deleteInvModal.drugName}"? Esta acción no se puede deshacer.`}
        icon="🗑️"
        iconBg="#fee2e2"
        confirmLabel="Sí, eliminar"
        confirmColor="#dc2626"
        onConfirm={confirmDeleteInventory}
        onCancel={() => setDeleteInvModal(s => ({ ...s, open: false }))}
      />
    </div>
  );
};

export default AdminInventoryTab;
