import React from 'react';

const SkeletonLoader: React.FC = () => {
  return (
    <div className="w-full h-full p-8 flex flex-col gap-6 animate-pulse" style={{ background: 'var(--bg-main)' }}>
      {/* Header Skeleton */}
      <div className="flex gap-4 mb-4">
        <div className="h-8 bg-slate-200 dark:bg-slate-700 rounded-md w-1/4"></div>
        <div className="h-8 bg-slate-200 dark:bg-slate-700 rounded-md w-1/3"></div>
      </div>

      {/* Stats Cards Skeleton */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
        {[...Array(4)].map((_, i) => (
          <div key={i} className="h-32 bg-slate-200 dark:bg-slate-700 rounded-2xl"></div>
        ))}
      </div>

      {/* Big Chart Skeleton */}
      <div className="h-96 bg-slate-200 dark:bg-slate-700 rounded-2xl w-full mt-4"></div>
    </div>
  );
};

export default SkeletonLoader;
