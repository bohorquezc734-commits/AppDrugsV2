import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import Login from './pages/Login';
import Register from './pages/Register';
import Dashboard from './pages/Dashboard';
import UserDashboard from './pages/UserDashboard';
import { authService } from './services/auth';
import { DrugiAssistant } from './components/Drugi/DrugiAssistant';

const PrivateRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const token = localStorage.getItem('token');
  return token ? <>{children}</> : <Navigate to="/login" />;
};

// 🔐 Componente para redirigir según rol
const RoleBasedDashboard: React.FC = () => {
  const user = authService.getUser();
  
  if (user?.role === 'User') {
    return <UserDashboard />;
  } else if (user?.role === 'Admin' || user?.role === 'Pharmacist') {
    return <Dashboard />;
  } else {
    return <Dashboard />;
  }
};

function App() {
  return (
    <>
      <ToastContainer position="top-right" autoClose={3000} />
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          
          {/* 🔐 Rutas protegidas */}
          <Route path="/dashboard" element={<PrivateRoute><RoleBasedDashboard /></PrivateRoute>} />
          <Route
            path="/user-dashboard"
            element={
              <PrivateRoute>
                <UserDashboard />
              </PrivateRoute>
            }
          />
          
          {/* Redirección por defecto */}
          <Route path="/" element={<Navigate to="/dashboard" />} />
        </Routes>
      </BrowserRouter>
      {/* Asistente Virtual Global */}
      <DrugiAssistant />
    </>
  );
}

export default App;