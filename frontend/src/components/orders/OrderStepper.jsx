import { Fragment } from 'react';
import { cn } from 'cn';

const STEPS = ['Pending', 'Processing', 'Shipped', 'Delivered'];
const STEP_LABELS = {
  Pending: 'Pendiente',
  Processing: 'En proceso',
  Shipped: 'Enviada',
  Delivered: 'Entregada',
};

export function OrderStepper({ statusKey }) {
  const isTerminal = statusKey === 'Cancelled' || statusKey === 'Returned';
  const currentIndex = STEPS.indexOf(statusKey);

  return (
    <div className="flex items-center gap-3">
      <div className="flex-1 space-y-1">
        <div className="flex items-center gap-2">
          {STEPS.map((step, i) => {
            const reached = !isTerminal && i <= currentIndex;
            return (
              <Fragment key={step}>
                <span
                  className={cn(
                    'size-2.5 shrink-0 rounded-full border-2',
                    reached ? 'border-primary bg-primary' : 'border-border bg-transparent'
                  )}
                />
                {i < STEPS.length - 1 && (
                  <span
                    className={cn(
                      'h-px flex-1',
                      !isTerminal && i < currentIndex ? 'bg-primary' : 'bg-border'
                    )}
                  />
                )}
              </Fragment>
            );
          })}
        </div>
        <div className="flex items-center justify-between">
          {STEPS.map((step) => (
            <span key={step} className="text-[10px] text-muted-foreground">
              {STEP_LABELS[step]}
            </span>
          ))}
        </div>
      </div>
      {isTerminal && (
        <span className="shrink-0 text-xs text-muted-foreground">
          {statusKey === 'Cancelled' ? 'Orden cancelada' : 'Orden devuelta'}
        </span>
      )}
    </div>
  );
}
