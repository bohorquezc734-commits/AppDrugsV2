import React, { useState, useEffect, useCallback } from 'react';
import { drugsService, Drug } from '../../services/drugs';
import { toast } from 'react-toastify';

const PharmacistCatalogTab: React.FC = () => {
  const [drugs, setDrugs] = useState<Drug[]>([]);
  const [loadingDrugs, setLoadingDrugs] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [drugCurrentPage, setDrugCurrentPage] = useState(1);
  const pageSize = 10;

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

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
        <h2 style={{ fontSize: 24, fontWeight: 700, color: '#1e293b', margin: 0 }}>Catálogo (Solo Lectura)</h2>
        <div style={{ display: 'flex', gap: 8 }}>
          <input 
            type="text" 
            value={searchTerm} 
            onChange={e => setSearchTerm(e.target.value)} 
            placeholder="Buscar medicamento..." 
            style={{ padding: '8px 12px', border: '1px solid #cbd5e1', borderRadius: 8, outline: 'none', fontSize: 14 }}
          />
          <button 
            onClick={() => loadDrugs(1)} 
            style={{ background: '#2563eb', color: 'white', padding: '8px 16px', borderRadius: 8, border: 'none', cursor: 'pointer', fontWeight: 600, fontSize: 14 }}
          >
            🔍 Buscar
          </button>
        </div>
      </div>

      {loadingDrugs ? (
        <p style={{ color: '#64748b' }}>Cargando catálogo...</p>
      ) : (
        <>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(250px, 1fr))', gap: 16 }}>
            {drugs.map(drug => (
              <div key={drug.id} style={{ background: '#fff', padding: 20, borderRadius: 12, boxShadow: '0 2px 8px rgba(0,0,0,0.05)', border: '1px solid #f1f5f9' }}>
                <h3 style={{ margin: '0 0 4px', fontSize: 18, color: '#1e293b' }}>{drug.name}</h3>
                <p style={{ margin: '0 0 12px', fontSize: 13, color: '#64748b' }}>{drug.genericName} - {drug.laboratory}</p>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: 12, borderTop: '1px solid #f1f5f9', paddingTop: 12 }}>
                  <div>
                    <p style={{ margin: 0, fontSize: 12, color: '#94a3b8' }}>Precio</p>
                    <p style={{ margin: 0, fontSize: 16, fontWeight: 700, color: '#10b981' }}>${drug.price.toFixed(2)}</p>
                  </div>
                  <div style={{ textAlign: 'right' }}>
                    <p style={{ margin: 0, fontSize: 12, color: '#94a3b8' }}>Stock Global</p>
                    <p style={{ margin: 0, fontSize: 16, fontWeight: 700, color: '#3b82f6' }}>{drug.stock}</p>
                  </div>
                </div>
              </div>
            ))}
          </div>
          
          <div style={{ display: 'flex', justifyContent: 'center', gap: 12, marginTop: 24 }}>
            <button 
              onClick={() => loadDrugs(drugCurrentPage - 1)} 
              disabled={drugCurrentPage === 1} 
              style={{ padding: '8px 16px', background: drugCurrentPage === 1 ? '#f1f5f9' : '#fff', border: '1px solid #cbd5e1', borderRadius: 8, cursor: drugCurrentPage === 1 ? 'not-allowed' : 'pointer' }}
            >
              Anterior
            </button>
            <button 
              onClick={() => loadDrugs(drugCurrentPage + 1)} 
              disabled={drugs.length < pageSize} 
              style={{ padding: '8px 16px', background: drugs.length < pageSize ? '#f1f5f9' : '#fff', border: '1px solid #cbd5e1', borderRadius: 8, cursor: drugs.length < pageSize ? 'not-allowed' : 'pointer' }}
            >
              Siguiente
            </button>
          </div>
        </>
      )}
    </div>
  );
};

export default PharmacistCatalogTab;
