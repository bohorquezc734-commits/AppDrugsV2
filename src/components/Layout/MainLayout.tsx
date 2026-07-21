import React from 'react';
import Sidebar, { AnyTab } from './Sidebar';
import Topbar from './Topbar';

interface MainLayoutProps {
  children: React.ReactNode;
  activeTab: AnyTab;
  onTabChange: (tab: AnyTab) => void;
  role: 'admin' | 'pharmacist' | 'user';
  sectionLabel: string;
}

const MainLayout: React.FC<MainLayoutProps> = ({
  children,
  activeTab,
  onTabChange,
  role,
  sectionLabel,
}) => {
  return (
    <div style={{
      display: 'flex',
      minHeight: '100vh',
      background: 'var(--bg-main)',
    }}>
      {/* Fixed Sidebar */}
      <Sidebar activeTab={activeTab} onTabChange={onTabChange} role={role} />

      {/* Main content shifted right */}
      <div style={{
        flex: 1,
        marginLeft: 'var(--sidebar-width)',
        display: 'flex',
        flexDirection: 'column',
        minHeight: '100vh',
      }}>
        <Topbar section={sectionLabel} />

        {/* Page content */}
        <main style={{
          flex: 1,
          padding: '28px 32px',
          overflowY: 'auto',
        }}>
          {children}
        </main>
      </div>
    </div>
  );
};

export default MainLayout;
