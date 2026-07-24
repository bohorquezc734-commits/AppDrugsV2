import React from 'react';

interface DialogProps {
  isOpen: boolean;
  title: string;
  message?: string;
  icon?: React.ReactNode;
  iconBg?: string;
  children?: React.ReactNode;
  confirmLabel?: string;
  confirmColor?: string;
  cancelLabel?: string;
  onConfirm: () => void;
  onCancel: () => void;
}

export const CustomDialog: React.FC<DialogProps> = ({
  isOpen, title, message, icon, iconBg, children,
  confirmLabel = 'Confirmar', confirmColor = '#2563eb',
  cancelLabel = 'Cancelar', onConfirm, onCancel,
}) => {
  if (!isOpen) return null;
  return (
    <div style={{
      position: 'fixed', inset: 0, zIndex: 9999,
      background: 'rgba(15,23,42,0.5)',
      display: 'flex', alignItems: 'center', justifyContent: 'center',
      padding: 20, backdropFilter: 'blur(4px)',
    }} onClick={onCancel}>
      <div style={{
        background: '#fff', borderRadius: 18, padding: '28px 28px 24px',
        maxWidth: 400, width: '100%',
        boxShadow: '0 24px 60px rgba(0,0,0,0.18)',
        animation: 'dialogIn 0.2s ease',
      }} onClick={e => e.stopPropagation()}>
        {icon && (
          <div style={{
            width: 52, height: 52, borderRadius: 14,
            background: iconBg || '#eff6ff',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            margin: '0 auto 16px', fontSize: 24,
          }}>{icon}</div>
        )}
        <h3 style={{ fontWeight: 700, fontSize: 17, color: '#1e293b', textAlign: 'center', marginBottom: message ? 8 : 16 }}>
          {title}
        </h3>
        {message && (
          <p style={{ fontSize: 14, color: '#64748b', textAlign: 'center', marginBottom: 20, lineHeight: 1.5 }}>
            {message}
          </p>
        )}
        {children && <div style={{ marginBottom: 20 }}>{children}</div>}
        <div style={{ display: 'flex', gap: 10 }}>
          <button onClick={onCancel} style={{
            flex: 1, padding: '10px', borderRadius: 10, border: '1.5px solid #e2e8f0',
            background: '#f8fafc', color: '#475569', fontWeight: 600, fontSize: 14, cursor: 'pointer',
          }}>{cancelLabel}</button>
          <button onClick={onConfirm} style={{
            flex: 1, padding: '10px', borderRadius: 10, border: 'none',
            background: confirmColor, color: '#fff', fontWeight: 700, fontSize: 14, cursor: 'pointer',
            boxShadow: `0 4px 12px ${confirmColor}55`,
          }}>{confirmLabel}</button>
        </div>
      </div>
    </div>
  );
};
