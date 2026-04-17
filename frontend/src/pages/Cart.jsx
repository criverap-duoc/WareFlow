import React, { useState } from 'react';
import { useCart } from '../context/CartContext';
import { useNavigate } from 'react-router-dom';
import { orderService } from '../services/api';

function Cart() {
  const { cart, totalItems, totalAmount, removeFromCart, updateQuantity, clearCart } = useCart();
  const navigate = useNavigate();
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
      items: cart.map(item => ({
        productId: item.id,
        quantity: item.quantity
      })),
      notes: 'Orden desde el carrito - ' + new Date().toLocaleString()
    };

    try {
      const response = await orderService.create(orderData);
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
      <div style={styles.container}>
        <h2>🛒 Carrito de Compras</h2>
        <div style={styles.emptyCart}>
          <p>Tu carrito está vacío</p>
          <button onClick={() => navigate('/products')} style={styles.continueButton}>
            Continuar comprando
          </button>
        </div>
      </div>
    );
  }

  return (
    <div style={styles.container}>
      <h2>🛒 Carrito de Compras ({totalItems} productos)</h2>

      {error && <div style={styles.error}>{error}</div>}

      <div style={styles.cartContainer}>
        <div style={styles.cartItems}>
          {cart.map(item => (
            <div key={item.id} style={styles.cartItem}>
              <img
                src={item.imageUrl || 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=100'}
                alt={item.name}
                style={styles.itemImage}
              />
              <div style={styles.itemDetails}>
                <h4>{item.name}</h4>
                <p>SKU: {item.sku}</p>
                <p>Precio: ${item.price.toLocaleString()}</p>
              </div>
              <div style={styles.itemQuantity}>
                <button onClick={() => updateQuantity(item.id, item.quantity - 1)} style={styles.qtyButton}>-</button>
                <span style={styles.qtyValue}>{item.quantity}</span>
                <button onClick={() => updateQuantity(item.id, item.quantity + 1)} style={styles.qtyButton}>+</button>
              </div>
              <div style={styles.itemSubtotal}>
                ${(item.price * item.quantity).toLocaleString()}
              </div>
              <button onClick={() => removeFromCart(item.id)} style={styles.removeButton}>🗑️</button>
            </div>
          ))}
        </div>

        <div style={styles.cartSummary}>
          <h3>Resumen de Compra</h3>
          <div style={styles.summaryRow}>
            <span>Subtotal:</span>
            <span>${totalAmount.toLocaleString()}</span>
          </div>
          <div style={styles.summaryRow}>
            <span>Envío:</span>
            <span>Gratis</span>
          </div>
          <div style={styles.summaryTotal}>
            <span>Total:</span>
            <span>${totalAmount.toLocaleString()}</span>
          </div>
          <button onClick={handleCheckout} disabled={loading} style={styles.checkoutButton}>
            {loading ? 'Procesando...' : '✓ Finalizar Compra'}
          </button>
          <button onClick={() => navigate('/products')} style={styles.continueButton}>
            Seguir comprando
          </button>
        </div>
      </div>
    </div>
  );
}

const styles = {
  container: { padding: '20px', maxWidth: '1200px', margin: '0 auto' },
  title: { marginBottom: '20px', color: '#2c3e50' },
  cartContainer: { display: 'flex', gap: '30px', flexWrap: 'wrap' },
  cartItems: { flex: '2', minWidth: '300px', backgroundColor: '#fff', borderRadius: '8px', boxShadow: '0 2px 4px rgba(0,0,0,0.1)' },
  cartItem: { display: 'flex', alignItems: 'center', gap: '15px', padding: '15px', borderBottom: '1px solid #eee', flexWrap: 'wrap' },
  itemImage: { width: '80px', height: '80px', objectFit: 'cover', borderRadius: '8px' },
  itemDetails: { flex: '2' },
  itemName: { margin: '0 0 5px 0', color: '#2c3e50' },
  itemSku: { fontSize: '12px', color: '#7f8c8d', margin: '0 0 5px 0' },
  itemPrice: { fontSize: '14px', fontWeight: 'bold', color: '#27ae60', margin: 0 },
  itemQuantity: { display: 'flex', alignItems: 'center', gap: '10px' },
  qtyButton: { padding: '5px 10px', backgroundColor: '#ecf0f1', color: '#2c3e50', border: 'none', borderRadius: '4px', cursor: 'pointer', fontSize: '16px', fontWeight: 'bold' },
  qtyValue: { minWidth: '30px', textAlign: 'center', fontWeight: 'bold' },
  itemSubtotal: { minWidth: '100px', fontWeight: 'bold', color: '#2c3e50', fontSize: '16px' },
  removeButton: { backgroundColor: '#e74c3c', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer', padding: '8px 12px' },
  cartSummary: { flex: '1', minWidth: '250px', backgroundColor: '#f8f9fa', padding: '20px', borderRadius: '8px', height: 'fit-content', boxShadow: '0 2px 4px rgba(0,0,0,0.1)' },
  summaryTitle: { margin: '0 0 15px 0', color: '#2c3e50', borderBottom: '2px solid #3498db', paddingBottom: '10px' },
  summaryRow: { display: 'flex', justifyContent: 'space-between', marginBottom: '10px', color: '#7f8c8d' },
  summaryTotal: { display: 'flex', justifyContent: 'space-between', marginTop: '15px', paddingTop: '15px', borderTop: '2px solid #ddd', fontWeight: 'bold', fontSize: '18px', color: '#2c3e50' },
  checkoutButton: { width: '100%', padding: '12px', backgroundColor: '#27ae60', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer', marginBottom: '10px', fontSize: '16px', fontWeight: 'bold' },
  continueButton: { width: '100%', padding: '12px', backgroundColor: '#3498db', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' },
  emptyCart: { textAlign: 'center', padding: '50px', backgroundColor: '#f8f9fa', borderRadius: '8px' },
  error: { padding: '10px', backgroundColor: '#f8d7da', color: '#721c24', borderRadius: '4px', marginBottom: '20px' }
};

export default Cart;
