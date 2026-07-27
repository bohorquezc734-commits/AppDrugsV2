import React, { useState, useEffect, useRef } from 'react';
import { authService } from '../../services/auth';
import { notificationsService, NotificationDto } from '../../services/notifications';

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

const IconCheck = () => (
  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
    <polyline points="20 6 9 17 4 12"></polyline>
  </svg>
);

const IconSun = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <circle cx="12" cy="12" r="5" />
    <line x1="12" y1="1" x2="12" y2="3" />
    <line x1="12" y1="21" x2="12" y2="23" />
    <line x1="4.22" y1="4.22" x2="5.64" y2="5.64" />
    <line x1="18.36" y1="18.36" x2="19.78" y2="19.78" />
    <line x1="1" y1="12" x2="3" y2="12" />
    <line x1="21" y1="12" x2="23" y2="12" />
    <line x1="4.22" y1="19.78" x2="5.64" y2="18.36" />
    <line x1="18.36" y1="5.64" x2="19.78" y2="4.22" />
  </svg>
);

const IconMoon = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z" />
  </svg>
);

const Topbar: React.FC<TopbarProps> = ({ section }) => {
  const user = authService.getUser();
  const [notifications, setNotifications] = useState<NotificationDto[]>([]);
  const [showDropdown, setShowDropdown] = useState(false);
  const [isDarkMode, setIsDarkMode] = useState(() => {
    return document.documentElement.classList.contains('dark');
  });
  const dropdownRef = useRef<HTMLDivElement>(null);

  const initials = user?.fullName
    ? user.fullName.split(' ').map((n: string) => n[0]).slice(0, 2).join('').toUpperCase()
    : 'U';

  const unreadCount = notifications.filter(n => !n.isRead).length;

  useEffect(() => {
    loadNotifications();
  }, []);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setShowDropdown(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const loadNotifications = async () => {
    try {
      const data = await notificationsService.getMyNotifications();
      setNotifications(data);
    } catch (error) {
      console.error('Error fetching notifications:', error);
    }
  };

  const handleMarkAsRead = async (id: number) => {
    try {
      await notificationsService.markAsRead(id);
      setNotifications(notifications.map(n => n.id === id ? { ...n, isRead: true } : n));
    } catch (error) {
      console.error('Error marking notification as read:', error);
    }
  };

  const handleToggleDropdown = () => {
    setShowDropdown(!showDropdown);
    if (!showDropdown) {
      loadNotifications();
    }
  };

  const handleToggleTheme = () => {
    const root = document.documentElement;
    if (isDarkMode) {
      root.classList.remove('dark');
      localStorage.setItem('theme', 'light');
      setIsDarkMode(false);
    } else {
      root.classList.add('dark');
      localStorage.setItem('theme', 'dark');
      setIsDarkMode(true);
    }
  };

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
        
        {/* Theme Toggle */}
        <button 
          onClick={handleToggleTheme}
          style={{
            position: 'relative',
            width: 36, height: 36,
            borderRadius: '50%',
            border: '1px solid var(--border)',
            background: 'var(--surface)',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            color: 'var(--text-secondary)',
            cursor: 'pointer',
            transition: 'all 0.15s',
          }}
          onMouseEnter={e => {
            (e.currentTarget as HTMLElement).style.background = 'var(--surface-hover)';
            (e.currentTarget as HTMLElement).style.color = 'var(--accent)';
          }}
          onMouseLeave={e => {
            (e.currentTarget as HTMLElement).style.background = 'var(--surface)';
            (e.currentTarget as HTMLElement).style.color = 'var(--text-secondary)';
          }}
          title={isDarkMode ? 'Cambiar a modo claro' : 'Cambiar a modo oscuro'}
        >
          {isDarkMode ? <IconSun /> : <IconMoon />}
        </button>

        {/* Notifications Container */}
        <div style={{ position: 'relative' }} ref={dropdownRef}>
          {/* Notification bell */}
          <button 
            onClick={handleToggleDropdown}
            style={{
              position: 'relative',
              width: 36, height: 36,
              borderRadius: '50%',
              border: '1px solid var(--border)',
              background: showDropdown ? 'var(--accent-light)' : 'var(--surface)',
              display: 'flex', alignItems: 'center', justifyContent: 'center',
              color: showDropdown ? 'var(--accent)' : 'var(--text-secondary)',
              cursor: 'pointer',
              transition: 'all 0.15s',
            }}
            onMouseEnter={e => {
              (e.currentTarget as HTMLElement).style.background = 'var(--accent-light)';
              (e.currentTarget as HTMLElement).style.color = 'var(--accent)';
            }}
            onMouseLeave={e => {
              (e.currentTarget as HTMLElement).style.background = showDropdown ? 'var(--accent-light)' : 'var(--surface)';
              (e.currentTarget as HTMLElement).style.color = showDropdown ? 'var(--accent)' : 'var(--text-secondary)';
            }}
          >
            <IconBell />
            {/* Dot indicator */}
            {unreadCount > 0 && (
              <span style={{
                position: 'absolute', top: -2, right: -2,
                width: 16, height: 16, borderRadius: '50%',
                background: '#ef4444',
                color: '#fff',
                fontSize: 10,
                fontWeight: 700,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                border: '1.5px solid #fff',
              }}>
                {unreadCount > 9 ? '9+' : unreadCount}
              </span>
            )}
          </button>

          {/* Dropdown Menu */}
          {showDropdown && (
            <div style={{
              position: 'absolute',
              top: 'calc(100% + 12px)',
              right: 0,
              width: 320,
              background: 'var(--surface)',
              borderRadius: 16,
              boxShadow: '0 10px 40px rgba(0,0,0,0.1), 0 1px 3px rgba(0,0,0,0.05)',
              border: '1px solid var(--border)',
              overflow: 'hidden',
              display: 'flex',
              flexDirection: 'column',
              animation: 'fadeIn 0.2s ease',
            }}>
              {/* Header */}
              <div style={{
                padding: '16px',
                borderBottom: '1px solid var(--border)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
              }}>
                <h3 style={{ margin: 0, fontSize: 15, fontWeight: 700, color: 'var(--text-primary)' }}>
                  Notificaciones
                </h3>
                {unreadCount > 0 && (
                  <span style={{ fontSize: 12, color: '#3b82f6', fontWeight: 600, background: '#eff6ff', padding: '2px 8px', borderRadius: 12 }}>
                    {unreadCount} nuevas
                  </span>
                )}
              </div>

              {/* List */}
              <div style={{ maxHeight: 360, overflowY: 'auto' }}>
                {notifications.length === 0 ? (
                  <div style={{ padding: '32px 16px', textAlign: 'center', color: '#94a3b8', fontSize: 14 }}>
                    No tienes notificaciones
                  </div>
                ) : (
                  notifications.map(notification => (
                    <div 
                      key={notification.id} 
                      style={{
                        padding: '14px 16px',
                        borderBottom: '1px solid var(--border-light)',
                        background: notification.isRead ? 'var(--surface)' : 'var(--surface-hover)',
                        display: 'flex',
                        gap: 12,
                        transition: 'background 0.2s',
                        position: 'relative'
                      }}
                      onMouseEnter={e => {
                        if (notification.isRead) (e.currentTarget as HTMLElement).style.background = 'var(--surface-hover)';
                      }}
                      onMouseLeave={e => {
                        if (notification.isRead) (e.currentTarget as HTMLElement).style.background = 'var(--surface)';
                      }}
                    >
                      {/* Icon depending on Type */}
                      <div style={{
                        width: 36, height: 36, borderRadius: '50%', flexShrink: 0,
                        display: 'flex', alignItems: 'center', justifyContent: 'center',
                        background: notification.type === 'Success' ? '#dcfce7' : 
                                    notification.type === 'Warning' ? '#fef3c7' : 
                                    notification.type === 'Error' ? '#fee2e2' : '#dbeafe',
                        color: notification.type === 'Success' ? '#16a34a' : 
                               notification.type === 'Warning' ? '#d97706' : 
                               notification.type === 'Error' ? '#dc2626' : '#2563eb',
                      }}>
                        <IconBell />
                      </div>

                      {/* Content */}
                      <div style={{ flex: 1, paddingRight: 24 }}>
                        <p style={{ margin: '0 0 4px 0', fontSize: 13, color: 'var(--text-primary)', lineHeight: 1.4, fontWeight: notification.isRead ? 400 : 600 }}>
                          {notification.message}
                        </p>
                        <span style={{ fontSize: 11, color: 'var(--text-muted)' }}>
                          {new Date(notification.createdAt).toLocaleDateString()} a las {new Date(notification.createdAt).toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'})}
                        </span>
                      </div>

                      {/* Action */}
                      {!notification.isRead && (
                        <button
                          onClick={() => handleMarkAsRead(notification.id)}
                          title="Marcar como leída"
                          style={{
                            position: 'absolute',
                            right: 16,
                            top: '50%',
                            transform: 'translateY(-50%)',
                            width: 26,
                            height: 26,
                            borderRadius: '50%',
                            border: 'none',
                            background: '#e0e7ff',
                            color: '#4f46e5',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            cursor: 'pointer',
                          }}
                        >
                          <IconCheck />
                        </button>
                      )}
                    </div>
                  ))
                )}
              </div>
            </div>
          )}
        </div>

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
      <style>{`
        @keyframes fadeIn {
          from { opacity: 0; transform: translateY(-8px) scale(0.98); }
          to { opacity: 1; transform: translateY(0) scale(1); }
        }
      `}</style>
    </header>
  );
};

export default Topbar;
