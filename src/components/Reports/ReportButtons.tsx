import React from 'react';
import { FaFileExcel, FaFilePdf } from 'react-icons/fa';

interface ReportButtonsProps {
  onExcel: () => void;
  onPdf: () => void;
  loading: boolean;
  disabled?: boolean;
  variant?: 'appointments' | 'inventory';
}

const ReportButtons: React.FC<ReportButtonsProps> = ({
  onExcel,
  onPdf,
  loading,
  disabled = false,
  variant = 'appointments',
}) => {
  const buttonClass = variant === 'appointments'
    ? 'bg-blue-500 hover:bg-blue-600'
    : 'bg-green-500 hover:bg-green-600';

  const ExcelIcon = FaFileExcel as React.JSXElementConstructor<any>;
  const PdfIcon = FaFilePdf as React.JSXElementConstructor<any>;

  return (
    <div className="flex gap-2 flex-wrap">
      <button
        type="button"
        onClick={onExcel}
        disabled={disabled || loading}
        className={`${buttonClass} text-white px-3 py-2 rounded-lg transition flex items-center gap-2 text-sm disabled:opacity-50`}
      >
        <ExcelIcon className="text-lg" />
        {loading ? 'Generando...' : 'Excel'}
      </button>
      <button
        type="button"
        onClick={onPdf}
        disabled={disabled || loading}
        className={`${buttonClass} text-white px-3 py-2 rounded-lg transition flex items-center gap-2 text-sm disabled:opacity-50`}
      >
        <PdfIcon className="text-lg" />
        {loading ? 'Generando...' : 'PDF'}
      </button>
    </div>
  );
};

export default ReportButtons;
