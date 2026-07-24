import React, { useState, useEffect, useCallback } from 'react';
import { drugsService, Drug } from '../../services/drugs';
import { toast } from 'react-toastify';
import { CartItem } from '../../pages/UserDashboard';

interface CatalogTabProps {
  cart: CartItem[];
  setCart: React.Dispatch<React.SetStateAction<CartItem[]>>;
}

const CatalogTab: React.FC<CatalogTabProps> = ({ cart, setCart }) => {
  const [drugs, setDrugs] = useState<Drug[]>([]);
  const [loadingDrugs, setLoadingDrugs] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 10;

  const loadDrugs = useCallback(async (page = 1) => {
    try {
      setLoadingDrugs(true);
      const data = await drugsService.getAll({ searchTerm: searchTerm || undefined, page, pageSize });
      setDrugs(data);
      setCurrentPage(page);
    } catch {
      toast.error('Error cargando medicamentos');
    } finally {
      setLoadingDrugs(false);
    }
  }, [searchTerm]);

  useEffect(() => { loadDrugs(1); }, [loadDrugs]);

  const handleAddToCart = (drug: Drug) => {
    if (cart.find(c => c.drugId === drug.id)) {
      toast.info('El medicamento ya está en el carrito.');
      return;
    }
    setCart(prev => [...prev, { drugId: drug.id, drugName: drug.name, quantity: 1 }]);
    toast.success('Medicamento agregado. Ve a "Crear Turno" para continuar.');
  };

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20, flexWrap: 'wrap', gap: 12 }}>
        <h2 style={{ fontSize: 24, fontWeight: 700, color: '#1e293b', margin: 0 }}>📋 Lista de Medicamentos</h2>
        <form onSubmit={e => { e.preventDefault(); loadDrugs(1); }} style={{ display: 'flex', gap: 8 }}>
          <input
            type="text"
            value={searchTerm}
            onChange={e => setSearchTerm(e.target.value)}
            placeholder="Buscar medicamento..."
            style={{ padding: '8px 14px', borderRadius: 8, border: '1px solid #cbd5e1', fontSize: 14, width: 220 }}
          />
          <button type="submit" style={{ background: '#2563eb', color: '#fff', border: 'none', borderRadius: 8, padding: '8px 18px', cursor: 'pointer', fontWeight: 600 }}>
            🔍 Buscar
          </button>
        </form>
      </div>

      <div style={{ background: '#eff6ff', border: '1px solid #bfdbfe', borderRadius: 10, padding: '12px 16px', marginBottom: 20 }}>
        <p style={{ margin: 0, fontSize: 14, color: '#1e40af' }}>
          <strong>📌 Rol: Afiliado</strong> — Puedes ver medicamentos y agregarlos a tu próximo pedido.
        </p>
      </div>

      {loadingDrugs ? (
        <div style={{ textAlign: 'center', padding: 60, color: '#64748b' }}>⏳ Cargando medicamentos...</div>
      ) : drugs.length === 0 ? (
        <div style={{ textAlign: 'center', padding: 60, background: '#fff', borderRadius: 12, color: '#64748b' }}>No hay medicamentos disponibles</div>
      ) : (
        <>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(240px, 1fr))', gap: 16 }}>
            {drugs.map(drug => (
              <div key={drug.id} style={{ display: 'flex', flexDirection: 'column', background: '#fff', borderRadius: 12, padding: 18, boxShadow: '0 2px 8px rgba(0,0,0,0.07)', transition: 'box-shadow 0.2s' }}>
                <h3 style={{ fontWeight: 700, fontSize: 16, color: '#1e293b', margin: '0 0 4px' }}>{drug.name}</h3>
                <p style={{ fontSize: 13, color: '#94a3b8', margin: '0 0 6px' }}>{drug.genericName}</p>
                <p style={{ fontSize: 13, color: '#64748b', margin: '0 0 4px' }}>🧪 {drug.laboratory}</p>
                <p style={{ fontSize: 20, fontWeight: 700, color: '#16a34a', margin: '8px 0 4px' }}>${drug.price.toFixed(2)}</p>
                <p style={{ fontSize: 13, color: '#64748b', margin: '0 0 2px' }}>📦 Stock General: {drug.stock}</p>
                <p style={{ fontSize: 13, color: '#64748b', margin: 0 }}>📂 {drug.category}</p>
                {drug.requiresPrescription && (
                  <span style={{ display: 'inline-block', marginTop: 8, background: '#fef9c3', color: '#854d0e', borderRadius: 6, padding: '2px 8px', fontSize: 12, fontWeight: 600 }}>📋 Requiere receta</span>
                )}
                
                <div style={{ marginTop: 'auto', paddingTop: 16 }}>
                  <button 
                    onClick={() => handleAddToCart(drug)}
                    style={{ width: '100%', background: '#10b981', color: '#fff', border: 'none', borderRadius: 8, padding: '8px 10px', fontWeight: 600, cursor: 'pointer', transition: 'background 0.2s' }}
                    onMouseEnter={e => (e.currentTarget as HTMLElement).style.background = '#059669'}
                    onMouseLeave={e => (e.currentTarget as HTMLElement).style.background = '#10b981'}
                  >
                    + Añadir al Turno
                  </button>
                </div>
              </div>
            ))}
          </div>

          <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', gap: 16, marginTop: 24 }}>
            <button onClick={() => loadDrugs(currentPage - 1)} disabled={currentPage === 1}
              style={{ padding: '8px 18px', borderRadius: 8, border: '1px solid #e2e8f0', background: currentPage === 1 ? '#f1f5f9' : '#fff', cursor: currentPage === 1 ? 'not-allowed' : 'pointer', color: '#475569' }}>
              ← Anterior
            </button>
            <span style={{ fontSize: 14, color: '#64748b' }}>Página {currentPage}</span>
            <button onClick={() => loadDrugs(currentPage + 1)} disabled={drugs.length < pageSize}
              style={{ padding: '8px 18px', borderRadius: 8, border: '1px solid #e2e8f0', background: drugs.length < pageSize ? '#f1f5f9' : '#fff', cursor: drugs.length < pageSize ? 'not-allowed' : 'pointer', color: '#475569' }}>
              Siguiente →
            </button>
          </div>
        </>
      )}
    </div>
  );
};

export default CatalogTab;
