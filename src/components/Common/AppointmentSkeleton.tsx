import React from 'react';

export const AppointmentSkeleton: React.FC = () => {
  return (
    <div className="bg-white rounded-xl p-5 shadow-sm border border-gray-100 animate-pulse">
      <div className="flex justify-between items-start flex-wrap gap-2">
        <div>
          <div className="h-5 bg-gray-200 rounded w-24 mb-3"></div>
          <div className="h-4 bg-gray-200 rounded w-48 mb-2"></div>
          <div className="h-3 bg-gray-200 rounded w-32"></div>
        </div>
        <div className="h-6 bg-gray-200 rounded-full w-24"></div>
      </div>
      
      <div className="mt-4 border-t border-gray-100 pt-3">
        <div className="h-3 bg-gray-200 rounded w-40 mb-2"></div>
        <div className="flex gap-2">
          <div className="h-6 bg-gray-200 rounded w-20"></div>
          <div className="h-6 bg-gray-200 rounded w-24"></div>
        </div>
      </div>

      <div className="mt-4 border-t border-gray-100 pt-4 flex justify-center">
        <div className="h-32 bg-gray-200 rounded w-32"></div>
      </div>
    </div>
  );
};
