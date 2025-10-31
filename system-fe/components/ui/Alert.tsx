import { ReactNode } from 'react';

interface AlertProps {
  type: 'success' | 'error' | 'warning' | 'info';
  title?: string;
  children: ReactNode;
  onClose?: () => void;
  className?: string;
}

export function Alert({ type, title, children, onClose, className = '' }: AlertProps) {
  const typeStyles = {
    success: {
      container: 'bg-green-50 border-green-200 text-green-800',
      icon: '✓',
      iconBg: 'bg-green-100'
    },
    error: {
      container: 'bg-red-50 border-red-200 text-red-800',
      icon: '✕',
      iconBg: 'bg-red-100'
    },
    warning: {
      container: 'bg-yellow-50 border-yellow-200 text-yellow-800',
      icon: '⚠',
      iconBg: 'bg-yellow-100'
    },
    info: {
      container: 'bg-blue-50 border-blue-200 text-blue-800',
      icon: 'ℹ',
      iconBg: 'bg-blue-100'
    }
  };

  const styles = typeStyles[type];

  return (
    <div className={`rounded-lg border p-4 ${styles.container} ${className}`} role="alert">
      <div className="flex items-start">
        <div className={`flex-shrink-0 w-6 h-6 rounded-full ${styles.iconBg} flex items-center justify-center font-bold`}>
          {styles.icon}
        </div>
        <div className="ml-3 flex-1">
          {title && <h3 className="text-sm font-medium mb-1">{title}</h3>}
          <div className="text-sm">{children}</div>
        </div>
        {onClose && (
          <button
            onClick={onClose}
            className="flex-shrink-0 ml-3 -mr-1 -mt-1 text-current opacity-70 hover:opacity-100"
            aria-label="Close"
          >
            <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
              <path fillRule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clipRule="evenodd" />
            </svg>
          </button>
        )}
      </div>
    </div>
  );
}
