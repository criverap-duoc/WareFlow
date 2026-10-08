import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Minus, Plus, ShoppingCart, Trash2 } from 'lucide-react';
import { useCart } from '../context/CartContext';
import { orderService } from '../services/api';
import { formatCLP, formatDate, calcIVA } from '../lib/formatters';
import { getDefaultImage } from '../lib/images';
import { PageHeader } from '../components/layout/PageHeader';
import { Card } from '../components/ui/card';
import { Button } from '../components/ui/button';
import { Separator } from '../components/ui/separator';
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from '../components/ui/sheet';

function Cart() {
  const navigate = useNavigate();
  const { cart, totalItems, totalAmount, removeFromCart, updateQuantity, clearCart } = useCart();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleCheckout = async () => {
    if (cart.length === 0) {
      setError('El carrito está vacío');
      return;
    }

    setLoading(true);
    setError('');

    const orderData = {
      items: cart.map((item) => ({
        productId: item.id,
        quantity: item.quantity,
      })),
      notes: 'Orden desde el carrito - ' + formatDate(new Date().toISOString(), true),
    };

    try {
      await orderService.create(orderData);
      clearCart();
      navigate('/orders');
    } catch (err) {
      setError(err.response?.data?.message || 'Error al crear la orden');
    } finally {
      setLoading(false);
    }
  };

  if (cart.length === 0) {
    return (
      <div className="space-y-6">
        <PageHeader title="Carrito" className="pb-0" />
        <div className="flex flex-col items-center justify-center gap-3 rounded-lg border bg-card py-16 text-center">
          <ShoppingCart className="h-12 w-12 text-muted-foreground" />
          <p className="text-sm text-muted-foreground">Tu carrito está vacío</p>
          <Button onClick={() => navigate('/products')}>Continuar comprando</Button>
        </div>
      </div>
    );
  }

  const iva = calcIVA(totalAmount);

  const summary = (
    <>
      <div className="flex justify-between text-sm">
        <span className="text-muted-foreground">Neto</span>
        <span className="num">{formatCLP(iva.neto)}</span>
      </div>
      <div className="flex justify-between text-sm">
        <span className="text-muted-foreground">IVA (19%)</span>
        <span className="num">{formatCLP(iva.iva)}</span>
      </div>
      <div className="flex justify-between text-sm">
        <span className="text-muted-foreground">Envío</span>
        <span className="num">Gratis</span>
      </div>
      <Separator />
      <div className="flex justify-between text-base font-semibold">
        <span>Total</span>
        <span className="num">{formatCLP(iva.total)}</span>
      </div>
      <p className="text-xs text-muted-foreground">Precios incluyen IVA</p>
      <Button className="w-full" onClick={handleCheckout} disabled={loading}>
        {loading ? 'Procesando…' : 'Finalizar compra'}
      </Button>
      <Button
        variant="outline"
        className="w-full"
        onClick={() => navigate('/products')}
        disabled={loading}
      >
        Seguir comprando
      </Button>
    </>
  );

  return (
    <div className="space-y-6 pb-24 lg:pb-0">
      <PageHeader
        title="Carrito"
        description={`${totalItems} ${totalItems === 1 ? 'producto' : 'productos'} en el carrito`}
        className="pb-0"
      />

      {error && (
        <div className="rounded-md border border-danger/20 bg-danger/10 px-3 py-2 text-sm text-danger">
          {error}
        </div>
      )}

      <div className="grid gap-6 lg:grid-cols-[1fr_340px]">
        <div className="space-y-3">
          {cart.map((item) => (
            <Card key={item.id} className="flex-row items-center gap-4 p-4">
              <img
                src={item.imageUrl || getDefaultImage(item.name, item.category)}
                alt={item.name}
                loading="lazy"
                className="size-20 shrink-0 rounded-md object-cover"
              />
              <div className="min-w-0 flex-1">
                <p className="truncate font-medium">{item.name}</p>
                <p className="font-mono text-xs text-muted-foreground">{item.sku}</p>
                <p className="num mt-1 text-sm text-muted-foreground">{formatCLP(item.price)}</p>
              </div>
              <div className="flex items-center gap-1">
                <Button
                  variant="outline"
                  size="icon"
                  className="h-8 w-8"
                  onClick={() => updateQuantity(item.id, item.quantity - 1)}
                  disabled={item.quantity <= 1}
                  aria-label="Disminuir cantidad"
                >
                  <Minus className="size-4" />
                </Button>
                <span className="num min-w-[2ch] text-center text-sm font-medium">
                  {item.quantity}
                </span>
                <Button
                  variant="outline"
                  size="icon"
                  className="h-8 w-8"
                  onClick={() => updateQuantity(item.id, item.quantity + 1)}
                  aria-label="Aumentar cantidad"
                >
                  <Plus className="size-4" />
                </Button>
              </div>
              <span className="num w-24 text-right text-sm font-semibold">
                {formatCLP(item.price * item.quantity)}
              </span>
              <Button
                variant="ghost"
                size="icon-sm"
                onClick={() => removeFromCart(item.id)}
                aria-label="Eliminar del carrito"
                title="Eliminar"
              >
                <Trash2 className="size-4 text-danger" />
              </Button>
            </Card>
          ))}
        </div>

        <Card className="hidden h-fit flex-col gap-3 p-4 lg:sticky lg:top-20 lg:flex">
          <h3 className="text-base font-semibold">Resumen de compra</h3>
          {summary}
        </Card>
      </div>

      <div className="fixed inset-x-0 bottom-0 z-40 border-t bg-background/95 p-4 backdrop-blur lg:hidden">
        <div className="flex items-center justify-between gap-4">
          <div>
            <p className="text-xs text-muted-foreground">Total</p>
            <p className="num font-semibold">{formatCLP(iva.total)}</p>
          </div>
          <Sheet>
            <SheetTrigger asChild>
              <Button>Ver resumen</Button>
            </SheetTrigger>
            <SheetContent side="bottom" className="gap-3">
              <SheetHeader>
                <SheetTitle>Resumen de compra</SheetTitle>
              </SheetHeader>
              <div className="space-y-3 px-4 pb-4">{summary}</div>
            </SheetContent>
          </Sheet>
        </div>
      </div>
    </div>
  );
}

export default Cart;
