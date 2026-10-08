import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { ChevronDown, ChevronUp, Package } from 'lucide-react';
import { orderService } from '../services/api';
import { formatCLP, formatDate } from '../lib/formatters';
import { ORDER_STATUS } from '../lib/constants';
import { PageHeader } from '../components/layout/PageHeader';
import { OrderStatusBadge } from '../components/orders/OrderStatusBadge';
import { OrderStepper } from '../components/orders/OrderStepper';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardHeader } from '../components/ui/card';
import { Skeleton } from '../components/ui/skeleton';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '../components/ui/table';

const ORDER_STATUS_KEYS = Object.keys(ORDER_STATUS);

const statusKeyOf = (status) =>
  typeof status === 'number' ? ORDER_STATUS_KEYS[status - 1] : status;

function Orders() {
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [expandedOrder, setExpandedOrder] = useState(null);
  const navigate = useNavigate();

  useEffect(() => {
    loadOrders();
  }, []);

  const loadOrders = async () => {
    try {
      setLoading(true);
      const response = await orderService.getAll();
      setOrders(response.data.value || response.data || []);
      setError('');
    } catch (err) {
      setError('Error al cargar órdenes');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="space-y-4">
      <PageHeader
        title="Mis órdenes"
        description="Historial de tus compras"
        className="pb-0"
        actions={
          <Button variant="outline" onClick={() => navigate('/products')}>
            Seguir comprando
          </Button>
        }
      />

      {error && (
        <div className="rounded-md border border-danger/20 bg-danger/10 px-3 py-2 text-sm text-danger">
          {error}
        </div>
      )}

      {loading ? (
        <div className="space-y-4">
          {Array.from({ length: 3 }).map((_, i) => (
            <Card key={i} className="gap-4 p-4 sm:p-6">
              <div className="space-y-2">
                <Skeleton className="h-5 w-40" />
                <Skeleton className="h-3 w-28" />
              </div>
              <Skeleton className="h-3 w-24" />
              <Skeleton className="h-8 w-full sm:w-32" />
            </Card>
          ))}
        </div>
      ) : orders.length === 0 ? (
        <div className="flex flex-col items-center justify-center gap-3 rounded-lg border bg-card py-16 text-center">
          <Package className="h-12 w-12 text-muted-foreground" />
          <div className="space-y-1">
            <h3 className="text-lg font-semibold">Aún no tienes órdenes</h3>
            <p className="text-sm text-muted-foreground">
              Cuando compres, tus órdenes aparecerán aquí
            </p>
          </div>
          <Button onClick={() => navigate('/products')}>Ver productos</Button>
        </div>
      ) : (
        <div className="space-y-4">
          {orders.map((order) => {
            const statusKey = statusKeyOf(order.status);
            const isExpanded = expandedOrder === order.id;

            return (
              <Card key={order.id} className="gap-0 p-0">
                <CardHeader className="flex flex-col gap-3 px-4 py-4 sm:px-6">
                  <div className="flex items-start justify-between gap-4">
                    <div className="space-y-1">
                      <p className="font-mono text-base font-semibold">{order.orderNumber}</p>
                      <p className="text-xs text-muted-foreground">
                        {formatDate(order.orderDate, true)}
                      </p>
                    </div>
                    <OrderStatusBadge status={order.status} />
                  </div>
                  <OrderStepper statusKey={statusKey} />
                </CardHeader>

                <CardContent className="px-4 pb-4 sm:px-6">
                  <div className="flex items-center justify-between text-sm">
                    <span className="text-muted-foreground">Total</span>
                    <span className="num text-base font-semibold">
                      {formatCLP(order.totalAmount)}
                    </span>
                  </div>
                  <div className="mt-1 flex items-center justify-between text-sm">
                    <span className="text-muted-foreground">Productos</span>
                    <span className="num">{order.items?.length || 0}</span>
                  </div>

                  <Button
                    variant="ghost"
                    size="sm"
                    className="mt-2 w-full justify-start"
                    onClick={() => setExpandedOrder(isExpanded ? null : order.id)}
                    aria-expanded={isExpanded}
                  >
                    {isExpanded ? (
                      <ChevronUp className="size-4" />
                    ) : (
                      <ChevronDown className="size-4" />
                    )}
                    {isExpanded ? 'Ocultar detalles' : 'Ver detalles'}
                  </Button>

                  {isExpanded && (
                    <div className="mt-4 space-y-4 border-t pt-4">
                      <div className="hidden sm:block">
                        <Table>
                          <TableHeader>
                            <TableRow>
                              <TableHead>Producto</TableHead>
                              <TableHead className="text-center">Cantidad</TableHead>
                              <TableHead className="text-right">Precio unitario</TableHead>
                              <TableHead className="text-right">Subtotal</TableHead>
                            </TableRow>
                          </TableHeader>
                          <TableBody>
                            {order.items?.map((item) => (
                              <TableRow key={item.id}>
                                <TableCell className="whitespace-normal font-medium">
                                  {item.productName}
                                </TableCell>
                                <TableCell className="num text-center">{item.quantity}</TableCell>
                                <TableCell className="num text-right">
                                  {formatCLP(item.unitPrice)}
                                </TableCell>
                                <TableCell className="num text-right font-medium">
                                  {formatCLP(item.subtotal)}
                                </TableCell>
                              </TableRow>
                            ))}
                          </TableBody>
                        </Table>
                      </div>

                      <div className="space-y-3 sm:hidden">
                        {order.items?.map((item) => (
                          <div
                            key={item.id}
                            className="flex items-start justify-between gap-3 text-sm"
                          >
                            <div className="min-w-0">
                              <p className="font-medium">{item.productName}</p>
                              <p className="num text-xs text-muted-foreground">
                                {item.quantity} × {formatCLP(item.unitPrice)}
                              </p>
                            </div>
                            <span className="num shrink-0 font-medium">
                              {formatCLP(item.subtotal)}
                            </span>
                          </div>
                        ))}
                      </div>

                      <div className="flex items-center justify-between border-t pt-3">
                        <span className="text-sm text-muted-foreground">Total</span>
                        <span className="num text-lg font-semibold">
                          {formatCLP(order.totalAmount)}
                        </span>
                      </div>

                      {order.notes && (
                        <div className="rounded-md bg-muted/50 p-3 text-sm">
                          <span className="text-muted-foreground">Notas: </span>
                          {order.notes}
                        </div>
                      )}
                    </div>
                  )}
                </CardContent>
              </Card>
            );
          })}
        </div>
      )}
    </div>
  );
}

export default Orders;
