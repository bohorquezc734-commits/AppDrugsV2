import React, { useState } from 'react';
import { authService } from '../../services/auth';
import { toast } from 'react-toastify';
import { APP_CONSTANTS } from '../../constants/appConstants';

const Configuracion: React.FC = () => {
  const [user, setUser] = useState(authService.getUser());
  
  // Profile state
  const [fullName, setFullName] = useState(user?.fullName || '');
  const [updatingProfile, setUpdatingProfile] = useState(false);
  
  // Password state
  const [showPasswordModal, setShowPasswordModal] = useState(false);
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [updatingPassword, setUpdatingPassword] = useState(false);

  const getRoleName = (role: string) => {
    if (role === APP_CONSTANTS.ROLES.ADMIN) return APP_CONSTANTS.ROLE_NAMES.ADMIN;
    if (role === APP_CONSTANTS.ROLES.PHARMACIST) return APP_CONSTANTS.ROLE_NAMES.PHARMACIST;
    if (role === APP_CONSTANTS.ROLES.USER) return APP_CONSTANTS.ROLE_NAMES.USER;
    return role;
  };

  const handleUpdateProfile = async () => {
    if (!fullName.trim()) {
      toast.warn(APP_CONSTANTS.MESSAGES.PROFILE_NAME_EMPTY);
      return;
    }
    
    setUpdatingProfile(true);
    try {
      await authService.updateProfile(fullName);
      
      // Update local storage
      const updatedUser = { ...user, fullName };
      localStorage.setItem(APP_CONSTANTS.STORAGE_KEYS.USER, JSON.stringify(updatedUser));
      setUser(updatedUser);
      
      toast.success(APP_CONSTANTS.MESSAGES.PROFILE_UPDATE_SUCCESS);
    } catch (err: any) {
      toast.error(err.response?.data?.error || APP_CONSTANTS.MESSAGES.PROFILE_UPDATE_ERROR);
    } finally {
      setUpdatingProfile(false);
    }
  };

  const handleChangePassword = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!currentPassword || !newPassword || !confirmPassword) {
      toast.warn(APP_CONSTANTS.MESSAGES.PASSWORD_FIELDS_EMPTY);
      return;
    }
    if (newPassword !== confirmPassword) {
      toast.warn(APP_CONSTANTS.MESSAGES.PASSWORD_MISMATCH);
      return;
    }
    if (newPassword.length < 6) {
      toast.warn(APP_CONSTANTS.MESSAGES.PASSWORD_MIN_LENGTH);
      return;
    }

    setUpdatingPassword(true);
    try {
      await authService.changePassword(currentPassword, newPassword);
      toast.success(APP_CONSTANTS.MESSAGES.PASSWORD_UPDATE_SUCCESS);
      setShowPasswordModal(false);
      setCurrentPassword('');
      setNewPassword('');
      setConfirmPassword('');
    } catch (err: any) {
      toast.error(err.response?.data?.error || APP_CONSTANTS.MESSAGES.PASSWORD_UPDATE_ERROR);
    } finally {
      setUpdatingPassword(false);
    }
  };

  return (
    <div style={{ maxWidth: 800, margin: '0 auto' }}>
      <h2 style={{ fontSize: 24, fontWeight: 800, color: '#1e293b', marginBottom: 24 }}>{APP_CONSTANTS.UI.PROFILE_SETTINGS_TITLE}</h2>

      <div style={{
        background: '#fff',
        borderRadius: 16,
        padding: 32,
        boxShadow: '0 4px 12px rgba(0,0,0,0.03)',
        marginBottom: 24,
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 24, marginBottom: 32 }}>
          <div style={{
            width: 80, height: 80,
            borderRadius: '50%',
            background: 'linear-gradient(135deg, #2563eb, #7c3aed)',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            color: '#fff', fontWeight: 700, fontSize: 32,
            boxShadow: '0 8px 16px rgba(37,99,235,0.2)'
          }}>
            {user?.fullName ? user.fullName[0].toUpperCase() : 'U'}
          </div>
          <div>
            <h3 style={{ fontSize: 22, fontWeight: 700, color: '#1e293b', margin: 0 }}>
              {user?.fullName || APP_CONSTANTS.UI.NAME_NOT_AVAILABLE}
            </h3>
            <p style={{ fontSize: 14, color: '#64748b', marginTop: 4 }}>
              {user?.email || APP_CONSTANTS.UI.EMAIL_NOT_AVAILABLE}
            </p>
            <span style={{
              display: 'inline-block',
              marginTop: 10,
              padding: '4px 12px',
              background: '#eff6ff',
              color: '#2563eb',
              borderRadius: 20,
              fontSize: 12,
              fontWeight: 700,
              textTransform: 'uppercase',
              letterSpacing: '0.5px'
            }}>
              {getRoleName(user?.role || '')}
            </span>
          </div>
        </div>

        <div style={{ borderTop: '1px solid #f1f5f9', paddingTop: 32 }}>
          <h4 style={{ fontSize: 16, fontWeight: 700, color: '#334155', marginBottom: 20 }}>{APP_CONSTANTS.UI.ACCOUNT_INFO}</h4>
          
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 24 }}>
            <div>
              <label style={{ display: 'block', fontSize: 13, fontWeight: 600, color: '#64748b', marginBottom: 8 }}>
                {APP_CONSTANTS.UI.FULL_NAME}
              </label>
              <input
                type="text"
                value={fullName}
                onChange={(e) => setFullName(e.target.value)}
                style={{
                  width: '100%', padding: '12px 14px', borderRadius: 10, border: '1.5px solid #cbd5e1',
                  background: '#fff', color: '#1e293b', fontSize: 14, outline: 'none', transition: 'border-color 0.2s'
                }}
                onFocus={(e) => e.target.style.borderColor = '#2563eb'}
                onBlur={(e) => e.target.style.borderColor = '#cbd5e1'}
              />
            </div>
            <div>
              <label style={{ display: 'block', fontSize: 13, fontWeight: 600, color: '#64748b', marginBottom: 8 }}>
                {APP_CONSTANTS.UI.EMAIL_READONLY}
              </label>
              <input
                type="email"
                value={user?.email || ''}
                readOnly
                style={{
                  width: '100%', padding: '12px 14px', borderRadius: 10, border: '1.5px solid #e2e8f0',
                  background: '#f8fafc', color: '#94a3b8', fontSize: 14, outline: 'none', cursor: 'not-allowed'
                }}
              />
            </div>
          </div>
          <div style={{ marginTop: 24, display: 'flex', justifyContent: 'flex-end' }}>
            <button
              onClick={handleUpdateProfile}
              disabled={updatingProfile || fullName === user?.fullName}
              style={{
                padding: '10px 20px', 
                background: (updatingProfile || fullName === user?.fullName) ? '#cbd5e1' : '#2563eb', 
                color: '#fff',
                border: 'none', borderRadius: 10, fontWeight: 600, fontSize: 14,
                cursor: (updatingProfile || fullName === user?.fullName) ? 'not-allowed' : 'pointer', 
                transition: 'background 0.2s'
              }}
            >
              {updatingProfile ? APP_CONSTANTS.UI.SAVING : APP_CONSTANTS.UI.SAVE_CHANGES}
            </button>
          </div>
        </div>
      </div>

      <div style={{
        background: '#fff',
        borderRadius: 16,
        padding: 32,
        boxShadow: '0 4px 12px rgba(0,0,0,0.03)',
      }}>
        <h4 style={{ fontSize: 16, fontWeight: 700, color: '#334155', marginBottom: 8 }}>{APP_CONSTANTS.UI.SECURITY}</h4>
        <p style={{ fontSize: 14, color: '#64748b', marginBottom: 20 }}>
          {APP_CONSTANTS.UI.SECURITY_DESC}
        </p>
        <button
          onClick={() => setShowPasswordModal(true)}
          style={{
            padding: '12px 24px', background: 'linear-gradient(135deg, #2563eb 0%, #3b82f6 100%)',
            color: '#fff', border: 'none', borderRadius: 10, fontWeight: 700, fontSize: 14,
            cursor: 'pointer', boxShadow: '0 4px 12px rgba(37,99,235,0.3)', transition: 'transform 0.2s'
          }}
        >
          {APP_CONSTANTS.UI.CHANGE_PASSWORD_BTN}
        </button>
      </div>

      {/* Password Modal */}
      {showPasswordModal && (
        <div style={{
          position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
          background: 'rgba(15, 23, 42, 0.4)', backdropFilter: 'blur(4px)',
          display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 9999
        }}>
          <div style={{
            background: '#fff', borderRadius: 16, padding: 32, width: '100%', maxWidth: 400,
            boxShadow: '0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04)'
          }}>
            <h3 style={{ fontSize: 20, fontWeight: 700, color: '#1e293b', margin: '0 0 8px 0' }}>{APP_CONSTANTS.UI.CHANGE_PASSWORD_TITLE}</h3>
            <p style={{ fontSize: 14, color: '#64748b', margin: '0 0 24px 0' }}>{APP_CONSTANTS.UI.CHANGE_PASSWORD_DESC}</p>
            
            <form onSubmit={handleChangePassword}>
              <div style={{ marginBottom: 16 }}>
                <label style={{ display: 'block', fontSize: 13, fontWeight: 600, color: '#475569', marginBottom: 8 }}>
                  {APP_CONSTANTS.UI.CURRENT_PASSWORD}
                </label>
                <input
                  type="password"
                  value={currentPassword}
                  onChange={e => setCurrentPassword(e.target.value)}
                  style={{
                    width: '100%', padding: '10px 14px', borderRadius: 8, border: '1px solid #cbd5e1',
                    fontSize: 14, outline: 'none'
                  }}
                  required
                />
              </div>
              <div style={{ marginBottom: 16 }}>
                <label style={{ display: 'block', fontSize: 13, fontWeight: 600, color: '#475569', marginBottom: 8 }}>
                  {APP_CONSTANTS.UI.NEW_PASSWORD}
                </label>
                <input
                  type="password"
                  value={newPassword}
                  onChange={e => setNewPassword(e.target.value)}
                  style={{
                    width: '100%', padding: '10px 14px', borderRadius: 8, border: '1px solid #cbd5e1',
                    fontSize: 14, outline: 'none'
                  }}
                  required
                />
              </div>
              <div style={{ marginBottom: 24 }}>
                <label style={{ display: 'block', fontSize: 13, fontWeight: 600, color: '#475569', marginBottom: 8 }}>
                  {APP_CONSTANTS.UI.CONFIRM_NEW_PASSWORD}
                </label>
                <input
                  type="password"
                  value={confirmPassword}
                  onChange={e => setConfirmPassword(e.target.value)}
                  style={{
                    width: '100%', padding: '10px 14px', borderRadius: 8, border: '1px solid #cbd5e1',
                    fontSize: 14, outline: 'none'
                  }}
                  required
                />
              </div>

              <div style={{ display: 'flex', gap: 12, justifyContent: 'flex-end' }}>
                <button
                  type="button"
                  onClick={() => setShowPasswordModal(false)}
                  style={{
                    padding: '10px 16px', background: '#f1f5f9', color: '#475569',
                    border: 'none', borderRadius: 8, fontWeight: 600, fontSize: 14, cursor: 'pointer'
                  }}
                >
                  {APP_CONSTANTS.UI.CANCEL}
                </button>
                <button
                  type="submit"
                  disabled={updatingPassword}
                  style={{
                    padding: '10px 16px', background: updatingPassword ? '#93c5fd' : '#2563eb', color: '#fff',
                    border: 'none', borderRadius: 8, fontWeight: 600, fontSize: 14, cursor: updatingPassword ? 'not-allowed' : 'pointer'
                  }}
                >
                  {updatingPassword ? APP_CONSTANTS.UI.SAVING : APP_CONSTANTS.UI.UPDATE}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default Configuracion;
