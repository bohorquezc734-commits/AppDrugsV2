import React, { useState, useEffect, useCallback } from 'react';
import { toast } from 'react-toastify';
import Select from 'react-select';
import { useDropzone } from 'react-dropzone';
import { gestoresService, GestorDto } from '../../services/gestores';
import { inventoriesService, InventoryDto } from '../../services/inventories';
import { appointmentsService, CreateAppointmentDetailRequest } from '../../services/appointments';
import { CartItem } from '../../pages/UserDashboard';
import Lottie from 'lottie-react';

interface CreateAppointmentTabProps {
  cart: CartItem[];
  setCart: React.Dispatch<React.SetStateAction<CartItem[]>>;
  selectedGestor: number;
  setSelectedGestor: React.Dispatch<React.SetStateAction<number>>;
  onSuccess: () => void;
}

const CreateAppointmentTab: React.FC<CreateAppointmentTabProps> = ({ 
  cart, setCart, selectedGestor, setSelectedGestor, onSuccess 
}) => {
  const [gestores, setGestores] = useState<GestorDto[]>([]);
  const [sedeInventories, setSedeInventories] = useState<InventoryDto[]>([]);
  const [selectedInventory, setSelectedInventory] = useState<number>(0);
  const [qty, setQty] = useState<number>(1);
  const [archivo, setArchivo] = useState<File | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [loadingInventory, setLoadingInventory] = useState(false);
  const [emptyCartAnim, setEmptyCartAnim] = useState<any>(null);

  useEffect(() => {
    fetch('https://assets9.lottiefiles.com/packages/lf20_t9gkjjz4.json')
      .then(res => res.json())
      .then(data => setEmptyCartAnim(data))
      .catch(() => {});
  }, []);

  useEffect(() => {
    gestoresService.getAll()
      .then(data => setGestores(data))
      .catch(() => toast.error('Error cargando sedes farmacéuticas'));
  }, []);

  useEffect(() => {
    if (selectedGestor > 0) {
      setLoadingInventory(true);
      inventoriesService.getAll({ gestorFarmaceuticoId: selectedGestor, onlyActive: true })
        .then(data => {
          setSedeInventories(data);
          if (data.length === 0) {
            toast.warn('Esta sede no tiene medicamentos disponibles en inventario');
          }
        })
        .catch((err) => {
          console.error('Error cargando inventario:', err);
          toast.error('Error cargando inventario de la sede');
        })
        .finally(() => setLoadingInventory(false));
    } else {
      setSedeInventories([]);
    }
  }, [selectedGestor]);

  // Options para react-select de Gestores
  const gestorOptions = gestores.map(g => ({
    value: g.id,
    label: `${g.nombreSede} — ${g.direccion}`
  }));

  const handleGestorSelectChange = (selectedOption: any) => {
    const newGestor = selectedOption ? selectedOption.value : 0;
    
    // Si hay items en el carrito y se intenta cambiar de sede (desde otra sede ya seleccionada)
    if (cart.length > 0 && selectedGestor !== 0 && selectedGestor !== newGestor) {
      if (window.confirm("Cambiar de sede vaciará tu carrito actual porque el inventario es diferente. ¿Estás seguro?")) {
        setCart([]);
        setSelectedGestor(newGestor);
        setSelectedInventory(0);
      }
    } else {
      setSelectedGestor(newGestor);
      setSelectedInventory(0);
    }
  };

  // Dropzone Setup
  const onDrop = useCallback((acceptedFiles: File[]) => {
    if (acceptedFiles.length > 0) {
      setArchivo(acceptedFiles[0]);
    }
  }, []);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: {
      'application/pdf': ['.pdf'],
      'image/*': ['.jpg', '.jpeg', '.png']
    },
    maxFiles: 1
  });

  // Options para react-select de Inventario (Medicamentos)
  const inventoryOptions = sedeInventories.map(inv => ({
    value: inv.id,
    label: `${inv.drugName} (Stock: ${inv.quantity})`
  }));

  const addToCart = () => {
    const inv = sedeInventories.find(i => i.id === selectedInventory);
    if (!inv) { toast.warn('Selecciona un medicamento'); return; }
    if (qty < 1)  { toast.warn('La cantidad debe ser al menos 1'); return; }
    if (qty > inv.quantity) { toast.warn(`Solo hay ${inv.quantity} en stock para esta sede`); return; }
    if (cart.find(c => c.drugId === inv.drugId)) {
      toast.warn('Ese medicamento ya está en el pedido');
      return;
    }
    setCart(prev => [...prev, { drugId: inv.drugId, drugName: inv.drugName, quantity: qty }]);
    setSelectedInventory(0);
    setQty(1);
  };

  const removeFromCart = (drugId: number) =>
    setCart(prev => prev.filter(c => c.drugId !== drugId));

  const handleCreateAppointment = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedGestor) { toast.warn('Selecciona una sede'); return; }
    if (cart.length === 0) { toast.warn('Agrega al menos un medicamento al pedido'); return; }

    setSubmitting(true);
    try {
      const details: CreateAppointmentDetailRequest[] = [];
      for (const c of cart) {
        const inv = sedeInventories.find(i => i.drugId === c.drugId);
        if (!inv) {
          toast.error(`El medicamento ${c.drugName} no está disponible en la sede seleccionada.`);
          setSubmitting(false);
          return;
        }
        if (c.quantity > inv.quantity) {
          toast.error(`Solo hay ${inv.quantity} de ${c.drugName} en esta sede.`);
          setSubmitting(false);
          return;
        }
        details.push({
          inventoryId: inv.id,
          quantity: c.quantity,
        });
      }

      await appointmentsService.create(selectedGestor, details, archivo || undefined);
      toast.success('¡Turno creado exitosamente!');
      setCart([]);
      setSelectedGestor(0);
      setArchivo(null);
      onSuccess();
    } catch (err: any) {
      const msg = err.response?.data?.error || 'Error al crear el turno';
      toast.error(msg);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div>
      <h2 style={{ fontSize: 24, fontWeight: 700, color: '#1e293b', marginBottom: 24 }}>📋 Crear Nuevo Turno</h2>

      <form onSubmit={handleCreateAppointment}>
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 24 }}>
          {/* Columna izquierda */}
          <div>
            {/* Seleccionar sede */}
            <div style={{ background: '#fff', borderRadius: 12, padding: 20, boxShadow: '0 2px 8px rgba(0,0,0,0.07)', marginBottom: 20 }}>
              <h3 style={{ fontSize: 16, fontWeight: 700, color: '#1e293b', marginBottom: 14 }}>🏪 1. Selecciona la Sede Farmacéutica</h3>
              {gestores.length === 0 ? (
                <p style={{ color: '#94a3b8', fontSize: 14 }}>⏳ Cargando sedes...</p>
              ) : (
                <Select
                  value={gestorOptions.find(o => o.value === selectedGestor) || null}
                  onChange={handleGestorSelectChange}
                  options={gestorOptions}
                  placeholder="-- Selecciona una sede --"
                  isClearable
                  styles={{
                    control: (base, state) => ({
                      ...base,
                      borderRadius: '8px',
                      borderColor: state.isFocused ? '#2563eb' : '#cbd5e1',
                      boxShadow: state.isFocused ? '0 0 0 1px #2563eb' : 'none',
                      padding: '2px',
                      fontSize: '14px',
                      '&:hover': {
                        borderColor: '#94a3b8'
                      }
                    }),
                    option: (base, state) => ({
                      ...base,
                      fontSize: '14px',
                      backgroundColor: state.isSelected ? '#2563eb' : state.isFocused ? '#eff6ff' : 'white',
                      color: state.isSelected ? 'white' : '#1e293b',
                      cursor: 'pointer'
                    })
                  }}
                />
              )}
            </div>

            {/* Archivo de autorización */}
            <div style={{ background: '#fff', borderRadius: 12, padding: 20, boxShadow: '0 2px 8px rgba(0,0,0,0.07)' }}>
              <h3 style={{ fontSize: 16, fontWeight: 700, color: '#1e293b', marginBottom: 6 }}>📎 2. Archivo de Autorización <span style={{ fontWeight: 400, color: '#94a3b8', fontSize: 13 }}>(opcional)</span></h3>
              <p style={{ fontSize: 13, color: '#64748b', marginBottom: 12 }}>Adjunta la autorización médica si algún medicamento la requiere.</p>
              
              <div 
                {...getRootProps()} 
                style={{
                  border: `2px dashed ${isDragActive ? '#2563eb' : '#cbd5e1'}`,
                  borderRadius: '12px',
                  padding: '24px',
                  textAlign: 'center',
                  backgroundColor: isDragActive ? '#eff6ff' : '#f8fafc',
                  cursor: 'pointer',
                  transition: 'all 0.2s ease',
                  display: 'flex',
                  flexDirection: 'column',
                  alignItems: 'center',
                  justifyContent: 'center'
                }}
              >
                <input {...getInputProps()} />
                <div style={{ marginBottom: '8px' }}>
                  <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke={isDragActive ? '#2563eb' : '#94a3b8'} strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
                    <polyline points="17 8 12 3 7 8"></polyline>
                    <line x1="12" y1="3" x2="12" y2="15"></line>
                  </svg>
                </div>
                <p style={{ margin: 0, fontSize: 14, color: isDragActive ? '#1e40af' : '#64748b', fontWeight: 500 }}>
                  {isDragActive 
                    ? "Suelta el archivo aquí..." 
                    : "Arrastra y suelta tu autorización aquí, o haz clic para explorar"}
                </p>
                <p style={{ margin: '4px 0 0', fontSize: 12, color: '#94a3b8' }}>PDF, JPG o PNG (Max. 1 archivo)</p>
              </div>

              {archivo && (
                <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginTop: 12, padding: '10px 14px', backgroundColor: '#ecfdf5', borderRadius: '8px', border: '1px solid #a7f3d0' }}>
                  <p style={{ margin: 0, fontSize: 13, color: '#065f46', display: 'flex', alignItems: 'center', gap: 6 }}>
                    ✅ <span style={{ fontWeight: 600 }}>{archivo.name}</span>
                  </p>
                  <button 
                    type="button" 
                    onClick={(e) => { e.stopPropagation(); setArchivo(null); }}
                    style={{ background: 'transparent', border: 'none', color: '#047857', cursor: 'pointer', fontWeight: 'bold', fontSize: 14 }}
                  >
                    ✕
                  </button>
                </div>
              )}
            </div>
          </div>

          {/* Columna derecha: agregar medicamentos */}
          <div>
            <div style={{ background: '#fff', borderRadius: 12, padding: 20, boxShadow: '0 2px 8px rgba(0,0,0,0.07)', marginBottom: 20 }}>
              <h3 style={{ fontSize: 16, fontWeight: 700, color: '#1e293b', marginBottom: 14 }}>💊 3. Medicamentos del Pedido</h3>

              {loadingInventory && (
                <p style={{ fontSize: 13, color: '#3b82f6', marginBottom: 8 }}>⏳ Cargando medicamentos de la sede...</p>
              )}
              {!loadingInventory && selectedGestor > 0 && sedeInventories.length === 0 && (
                <div style={{ background: '#fef9c3', border: '1px solid #fde047', borderRadius: 8, padding: '10px 14px', marginBottom: 8 }}>
                  <p style={{ margin: 0, fontSize: 13, color: '#854d0e' }}>
                    ⚠️ Esta sede no tiene medicamentos en inventario. Contacta al administrador.
                  </p>
                </div>
              )}
              <div style={{ display: 'flex', gap: 8, marginBottom: 12 }}>
                <Select
                  value={inventoryOptions.find(o => o.value === selectedInventory) || null}
                  onChange={(opt: any) => setSelectedInventory(opt ? opt.value : 0)}
                  options={inventoryOptions}
                  isDisabled={selectedGestor === 0 || loadingInventory || sedeInventories.length === 0}
                  placeholder="-- Selecciona medicamento --"
                  styles={{
                    container: (base) => ({ ...base, flex: 1 }),
                    control: (base, state) => ({
                      ...base,
                      borderRadius: '8px',
                      borderColor: state.isFocused ? '#2563eb' : '#cbd5e1',
                      boxShadow: state.isFocused ? '0 0 0 1px #2563eb' : 'none',
                      fontSize: '13px',
                      backgroundColor: state.isDisabled ? '#f1f5f9' : 'white',
                    }),
                    option: (base, state) => ({
                      ...base,
                      fontSize: '13px',
                      backgroundColor: state.isSelected ? '#2563eb' : state.isFocused ? '#eff6ff' : 'white',
                      color: state.isSelected ? 'white' : '#1e293b',
                    })
                  }}
                />
                <input
                  type="number"
                  min={1}
                  value={qty}
                  onChange={e => setQty(Number(e.target.value))}
                  style={{ width: 70, padding: '8px 10px', borderRadius: 8, border: '1px solid #cbd5e1', fontSize: 13 }}
                />
                <button type="button" onClick={addToCart}
                  style={{ background: '#2563eb', color: '#fff', border: 'none', borderRadius: 8, padding: '8px 14px', cursor: 'pointer', fontWeight: 600, fontSize: 13 }}>
                  + Agregar
                </button>
              </div>

              {cart.length === 0 ? (
                <div style={{ textAlign: 'center', padding: '40px 20px', color: '#94a3b8', fontSize: 14, border: '2px dashed #e2e8f0', borderRadius: 12, display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
                  {emptyCartAnim ? (
                    <Lottie animationData={emptyCartAnim} style={{ width: 120, height: 120, marginBottom: 10 }} />
                  ) : (
                    <div style={{ fontSize: 40, marginBottom: 10 }}>🛒</div>
                  )}
                  <p style={{ margin: 0, fontWeight: 600, color: '#475569' }}>El carrito está vacío</p>
                  <p style={{ margin: '4px 0 0', fontSize: 13 }}>Ve al Catálogo para añadir medicamentos.</p>
                </div>
              ) : (
                <div>
                  {cart.map(item => (
                    <div key={item.drugId} style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', padding: '10px 12px', background: '#f8fafc', borderRadius: 8, marginBottom: 6 }}>
                      <div>
                        <p style={{ margin: 0, fontWeight: 600, fontSize: 14, color: '#1e293b' }}>{item.drugName}</p>
                        <p style={{ margin: 0, fontSize: 13, color: '#64748b' }}>Cantidad: {item.quantity}</p>
                      </div>
                      <button type="button" onClick={() => removeFromCart(item.drugId)}
                        style={{ background: '#fee2e2', color: '#ef4444', border: 'none', borderRadius: 6, padding: '4px 10px', cursor: 'pointer', fontWeight: 600 }}>
                        ✕
                      </button>
                    </div>
                  ))}
                </div>
              )}
            </div>

            <button
              type="submit"
              disabled={submitting}
              style={{ width: '100%', padding: '14px', background: submitting ? '#93c5fd' : '#2563eb', color: '#fff', border: 'none', borderRadius: 10, fontSize: 16, fontWeight: 700, cursor: submitting ? 'not-allowed' : 'pointer', boxShadow: '0 4px 12px rgba(37,99,235,0.3)', transition: 'background 0.2s' }}
            >
              {submitting ? '⏳ Enviando...' : '📋 Crear Turno'}
            </button>
          </div>
        </div>
      </form>
    </div>
  );
};

export default CreateAppointmentTab;
