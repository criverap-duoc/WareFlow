import { cn } from 'cn';
import { Badge } from '../ui/badge';
import { stockState } from '../../lib/constants';

export function StockBadge({ stock, minimumStock }) {
  const state = stockState(stock, minimumStock);
  const config = {
    ok: {
      label: `${stock} en stock`,
      classes: 'bg-success/10 text-success border-success/20',
      dot: 'bg-success',
    },
    low: {
      label: `Stock bajo (${stock})`,
      classes: 'bg-warning/10 text-warning border-warning/20',
      dot: 'bg-warning',
    },
    out: {
      label: 'Sin stock',
      classes: 'bg-danger/10 text-danger border-danger/20',
      dot: 'bg-danger',
    },
  }[state];

  return (
    <Badge variant="outline" className={cn('gap-1.5 font-normal', config.classes)}>
      <span className={cn('h-1.5 w-1.5 rounded-full', config.dot)} />
      {config.label}
    </Badge>
  );
}
