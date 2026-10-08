import { cn } from 'cn';
import { Badge } from '../ui/badge';
import { ORDER_STATUS, TONE_CLASSES } from '../../lib/constants';

export function OrderStatusBadge({ status }) {
  const key = typeof status === 'number' ? Object.keys(ORDER_STATUS)[status - 1] : status;
  const config = ORDER_STATUS[key] ?? { label: 'Desconocido', tone: 'muted' };
  return (
    <Badge variant="outline" className={cn('font-normal', TONE_CLASSES[config.tone])}>
      {config.label}
    </Badge>
  );
}
