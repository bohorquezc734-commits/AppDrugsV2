import React, { useState, useEffect, useCallback } from 'react';
import { toast } from 'react-toastify';
import { inventoriesService, InventoryDto } from '../../services/inventories';
import { gestoresService, GestorDto } from '../../services/gestores';
import { CustomDialog } from '../Common/CustomDialog';
import Select from 'react-select';

const PharmacistInventoryTab: React.FC = () => {
  const [inventories, setInventories] = useState<InventoryDto[]>([]);
  const [gestores, setGestores] = useState<GestorDto[]>([]);
  const [loadingInventories, setLoadingInventories] = useState(false);
  const [invFilterSede, setInvFilterSede] = useState<number>(0);
  
  // Modal: stock
  const [stockModal, setStockModal] = useState<{ open: boolean; type: 'add' | 'remove'; invId: number; drugName: string }>({
    open: false, type: 'add', invId: 0, drugName: '',
  });
  const [stockQty, setStockQty] = useState<string>('1');
  const [stockFocus, setStockFocus] = useState(false);

  const loadGestores = useCallback(async () => {
    try {
      const data = await gestoresService.getAll();
      setGestores(data);
    } catch { toast.error('Error cargando sedes'); }
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
    loadGestores();
  }, [loadGestores]);

  useEffect(() => {
    loadInventories();
  }, [loadInventories]);

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

  const gestorOptions = [
    { value: 0, label: 'Todas las sedes' },
    ...gestores.map(g => ({ value: g.id, label: g.nombreSede }))
  ];

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
        <h2 style={{ fontSize: 24, fontWeight: 700, color: '#1e293b', margin: 0 }}>Gestión de Inventario</h2>
        <div style={{ width: 250 }}>
          <Select
            value={gestorOptions.find(o => o.value === invFilterSede) || gestorOptions[0]}
            onChange={(opt: any) => setInvFilterSede(opt ? opt.value : 0)}
            options={gestorOptions}
            placeholder="Filtrar por sede..."
            styles={{
              control: (base, state) => ({
                ...base,
                borderRadius: '8px',
                borderColor: state.isFocused ? '#2563eb' : '#cbd5e1',
                boxShadow: state.isFocused ? '0 0 0 1px #2563eb' : 'none',
                fontSize: '14px',
              }),
              option: (base, state) => ({
                ...base,
                fontSize: '14px',
                backgroundColor: state.isSelected ? '#2563eb' : state.isFocused ? '#eff6ff' : 'white',
                color: state.isSelected ? 'white' : '#1e293b',
              })
            }}
          />
        </div>
      </div>

      {loadingInventories ? (
        <p style={{ color: '#64748b' }}>Cargando inventario...</p>
      ) : inventories.length === 0 ? (
        <div style={{ textAlign: 'center', padding: '40px', background: '#fff', borderRadius: 12, color: '#64748b', border: '1px dashed #cbd5e1' }}>
          <p style={{ fontSize: 16 }}>No hay medicamentos en el inventario para esta sede.</p>
        </div>
      ) : (
        <div style={{ background: '#fff', borderRadius: 12, boxShadow: '0 2px 8px rgba(0,0,0,0.05)', overflow: 'hidden' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left' }}>
            <thead style={{ background: '#f8fafc', borderBottom: '1px solid #e2e8f0' }}>
              <tr>
                <th style={{ padding: '16px', fontWeight: 600, color: '#475569', fontSize: 14 }}>Sede</th>
                <th style={{ padding: '16px', fontWeight: 600, color: '#475569', fontSize: 14 }}>Medicamento</th>
                <th style={{ padding: '16px', fontWeight: 600, color: '#475569', fontSize: 14 }}>Cantidad</th>
                <th style={{ padding: '16px', fontWeight: 600, color: '#475569', fontSize: 14 }}>Acciones de Stock</th>
              </tr>
            </thead>
            <tbody>
              {inventories.map(inv => (
                <tr key={inv.id} style={{ borderBottom: '1px solid #f1f5f9' }}>
                  <td style={{ padding: '16px', fontSize: 14, color: '#1e293b' }}>{inv.sedeName}</td>
                  <td style={{ padding: '16px', fontSize: 14, fontWeight: 600, color: '#1e293b' }}>{inv.drugName}</td>
                  <td style={{ padding: '16px', fontSize: 15, fontWeight: 700, color: '#2563eb' }}>{inv.quantity}</td>
                  <td style={{ padding: '16px', display: 'flex', gap: 8 }}>
                    <button onClick={() => openStockModal('add', inv)} style={{ background:'#dcfce7', color:'#15803d', border:'none', borderRadius:6, padding:'6px 12px', fontSize:13, fontWeight:600, cursor:'pointer' }}>+ Agregar</button>
                    <button onClick={() => openStockModal('remove', inv)} style={{ background:'#fee2e2', color:'#dc2626', border:'none', borderRadius:6, padding:'6px 12px', fontSize:13, fontWeight:600, cursor:'pointer' }}>- Retirar</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* MODAL: Stock (agregar / retirar) */}
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
    </div>
  );
};

export default PharmacistInventoryTab;
