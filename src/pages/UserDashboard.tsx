import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { authService } from '../services/auth';
import MainLayout from '../components/Layout/MainLayout';
import type { AnyTab } from '../components/Layout/Sidebar';
import Configuracion from '../components/Profile/Configuracion';
import CatalogTab from '../components/User/CatalogTab';
import CreateAppointmentTab from '../components/User/CreateAppointmentTab';
import MyAppointmentsTab from '../components/User/MyAppointmentsTab';

export interface CartItem {
  drugId: number;
  drugName: string;
  quantity: number;
}

type TabType = 'medicamentos' | 'nuevo-turno' | 'mis-turnos' | 'configuracion';

const SECTION_LABELS: Record<TabType, string> = {
  medicamentos: 'Catálogo',
  'nuevo-turno': 'Agendar Turno',
  'mis-turnos': 'Mis Turnos',
  configuracion: 'Configuración de Perfil'
};

const UserDashboard: React.FC = () => {
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState<TabType>('medicamentos');

  // Lifting State Up
  const [cart, setCart] = useState<CartItem[]>([]);
  const [selectedGestor, setSelectedGestor] = useState<number>(0);

  // Verificar rol
  useEffect(() => {
    if (!authService.isUser()) {
      navigate('/dashboard');
    }
  }, [navigate]);

  return (
    <MainLayout
      activeTab={activeTab as AnyTab}
      onTabChange={(tab) => setActiveTab(tab as TabType)}
      role="user"
      sectionLabel={SECTION_LABELS[activeTab]}
      cartCount={cart.length}
    >
      <div style={{ maxWidth: 1000, margin: '0 auto' }}>
        {activeTab === 'configuracion' && <Configuracion />}
        {activeTab === 'medicamentos' && (
          <CatalogTab 
            cart={cart}
            setCart={setCart}
          />
        )}
        {activeTab === 'nuevo-turno' && (
          <CreateAppointmentTab 
            cart={cart}
            setCart={setCart}
            selectedGestor={selectedGestor}
            setSelectedGestor={setSelectedGestor}
            onSuccess={() => setActiveTab('mis-turnos')} 
          />
        )}
        {activeTab === 'mis-turnos' && (
          <MyAppointmentsTab
            isActive={activeTab === 'mis-turnos'}
            onCreateNewClick={() => setActiveTab('nuevo-turno')}
          />
        )}
      </div>
    </MainLayout>
  );
};

export default UserDashboard;
