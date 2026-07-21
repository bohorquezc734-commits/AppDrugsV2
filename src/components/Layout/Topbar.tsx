import React from 'react';
import { authService } from '../../services/auth';

interface TopbarProps {
  section: string;
}

const IconBell = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M6 8a6 6 0 0 1 12 0c0 7 3 9 3 9H3s3-2 3-9"/>
    <path d="M10.3 21a1.94 1.94 0 0 0 3.4 0"/>
  </svg>
);

const IconChevronRight = () => (
  <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
    <path d="m9 18 6-6-6-6"/>
  </svg>
);

const Topbar: React.FC<TopbarProps> = ({ section }) => {
  const user = authService.getUser();

  const initials = user?.fullName
    ? user.fullName.split(' ').map((n: string) => n[0]).slice(0, 2).join('').toUpperCase()
    : 'U';

  return (
    <header style={{
      height: 'var(--topbar-height)',
      background: 'var(--topbar-bg)',
      borderBottom: '1px solid var(--border)',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
      padding: '0 28px',
      position: 'sticky',
      top: 0,
      zIndex: 100,
      boxShadow: '0 1px 4px rgba(0,0,0,0.04)',
    }}>
      {/* Breadcrumb */}
      <div style={{ display: 'flex', alignItems: 'center', gap: 6, fontSize: 13 }}>
        <span style={{ color: 'var(--text-muted)', fontWeight: 500 }}>Panel principal</span>
        <IconChevronRight />
        <span style={{ color: 'var(--accent)', fontWeight: 600 }}>{section}</span>
      </div>

      {/* Right side */}
      <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
        {/* Notification bell */}
        <button style={{
          position: 'relative',
          width: 36, height: 36,
          borderRadius: '50%',
          border: '1px solid var(--border)',
          background: '#fff',
          display: 'flex', alignItems: 'center', justifyContent: 'center',
          color: 'var(--text-secondary)',
          cursor: 'pointer',
          transition: 'all 0.15s',
        }}
          onMouseEnter={e => {
            (e.currentTarget as HTMLElement).style.background = 'var(--accent-light)';
            (e.currentTarget as HTMLElement).style.color = 'var(--accent)';
          }}
          onMouseLeave={e => {
            (e.currentTarget as HTMLElement).style.background = '#fff';
            (e.currentTarget as HTMLElement).style.color = 'var(--text-secondary)';
          }}
        >
          <IconBell />
          {/* Dot indicator */}
          <span style={{
            position: 'absolute', top: 7, right: 7,
            width: 7, height: 7, borderRadius: '50%',
            background: '#2563eb',
            border: '1.5px solid #fff',
          }} />
        </button>

        {/* Avatar */}
        <div style={{
          width: 34, height: 34,
          borderRadius: '50%',
          background: 'linear-gradient(135deg, #2563eb, #7c3aed)',
          display: 'flex', alignItems: 'center', justifyContent: 'center',
          color: '#fff',
          fontWeight: 700,
          fontSize: 13,
          cursor: 'default',
          boxShadow: '0 2px 8px rgba(37,99,235,0.25)',
        }}>
          {initials}
        </div>
      </div>
    </header>
  );
};

export default Topbar;
