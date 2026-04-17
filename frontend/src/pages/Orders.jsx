import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { orderService } from '../services/api';

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

  const getStatusLabel = (status) => {
    const statusMap = {
      1: { label: 'Pendiente', color: '#ffc107' },
      2: { label: 'Procesando', color: '#17a2b8' },
      3: { label: 'Enviado', color: '#007bff' },
      4: { label: 'Entregado', color: '#28a745' },
      5: { label: 'Cancelado', color: '#dc3545' },
      6: { label: 'Devuelto', color: '#6c757d' }
    };
    return statusMap[status] || { label: 'Desconocido', color: '#6c757d' };
  };

  if (loading) {
    return <div style={styles.container}>Cargando órdenes...</div>;
  }

  return (
    <div style={styles.container}>
      <div style={styles.header}>
        <h2>📋 Mis Órdenes</h2>
        <button onClick={() => navigate('/products')} style={styles.shopButton}>
          🛒 Seguir comprando
        </button>
      </div>

      {error && <div style={styles.error}>{error}</div>}

      {orders.length === 0 ? (
        <div style={styles.empty}>
          <p>No tienes órdenes aún</p>
          <button onClick={() => navigate('/products')} style={styles.emptyButton}>
            Comenzar a comprar
          </button>
        </div>
      ) : (
        <div style={styles.ordersList}>
          {orders.map((order) => {
            const statusInfo = getStatusLabel(order.status);
            const isExpanded = expandedOrder === order.id;

            return (
              <div key={order.id} style={styles.orderCard}>
                <div style={styles.orderHeader}>
                  <div>
                    <strong style={styles.orderNumber}>Orden #{order.orderNumber}</strong>
                    <div style={styles.orderDate}>
                      {new Date(order.orderDate).toLocaleString()}
                    </div>
                  </div>
                  <div>
                    <span style={{ ...styles.statusBadge, backgroundColor: statusInfo.color }}>
                      {statusInfo.label}
                    </span>
                  </div>
                </div>

                <div style={styles.orderSummary}>
                  <span>Total: <strong>${order.totalAmount.toLocaleString()}</strong></span>
                  <span>Productos: {order.items?.length || 0}</span>
                </div>

                <button
                  onClick={() => setExpandedOrder(isExpanded ? null : order.id)}
                  style={styles.detailButton}
                >
                  {isExpanded ? '▲ Ocultar detalles' : '▼ Ver detalles'}
                </button>

                {isExpanded && (
                  <div style={styles.orderDetails}>
                    <h4 style={styles.detailsTitle}>📦 Productos</h4>
                    <div style={styles.itemsHeader}>
                      <span>Producto</span>
                      <span>Cantidad</span>
                      <span>Precio Unitario</span>
                      <span>Subtotal</span>
                    </div>
                    {order.items?.map((item) => (
                      <div key={item.id} style={styles.orderItem}>
                        <span style={styles.itemName}>{item.productName}</span>
                        <span style={styles.itemQty}>{item.quantity}</span>
                        <span style={styles.itemPrice}>
                          ${item.unitPrice.toLocaleString()}
                        </span>
                        <span style={styles.itemSubtotal}>
                          ${item.subtotal.toLocaleString()}
                        </span>
                      </div>
                    ))}
                    <div style={styles.orderTotal}>
                      <span>Total: </span>
                      <span style={styles.totalAmount}>
                        ${order.totalAmount.toLocaleString()}
                      </span>
                    </div>
                    {order.notes && (
                      <div style={styles.notes}>
                        <strong>📝 Notas:</strong> {order.notes}
                      </div>
                    )}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}

const styles = {
  container: {
    padding: '20px',
    maxWidth: '1000px',
    margin: '0 auto'
  },
  header: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: '20px'
  },
  shopButton: {
    padding: '10px 20px',
    backgroundColor: '#3498db',
    color: 'white',
    border: 'none',
    borderRadius: '4px',
    cursor: 'pointer'
  },
  ordersList: {
    display: 'flex',
    flexDirection: 'column',
    gap: '20px'
  },
  orderCard: {
    border: '1px solid #ddd',
    borderRadius: '8px',
    padding: '15px',
    backgroundColor: 'white',
    boxShadow: '0 2px 4px rgba(0,0,0,0.1)'
  },
  orderHeader: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
    marginBottom: '10px'
  },
  orderNumber: {
    fontSize: '16px'
  },
  orderDate: {
    fontSize: '12px',
    color: '#666',
    marginTop: '4px'
  },
  statusBadge: {
    padding: '4px 8px',
    borderRadius: '4px',
    color: 'white',
    fontSize: '12px'
  },
  orderSummary: {
    display: 'flex',
    justifyContent: 'space-between',
    padding: '10px 0',
    borderTop: '1px solid #eee',
    borderBottom: '1px solid #eee'
  },
  detailButton: {
    marginTop: '10px',
    backgroundColor: '#f8f9fa',
    border: 'none',
    padding: '8px',
    borderRadius: '4px',
    cursor: 'pointer',
    width: '100%'
  },
  orderDetails: {
    marginTop: '15px',
    paddingTop: '15px',
    borderTop: '1px solid #eee'
  },
  detailsTitle: {
    margin: '0 0 10px 0',
    color: '#2c3e50'
  },
  itemsHeader: {
    display: 'grid',
    gridTemplateColumns: '2fr 1fr 1fr 1fr',
    gap: '10px',
    padding: '10px',
    backgroundColor: '#f8f9fa',
    borderRadius: '4px',
    fontWeight: 'bold',
    fontSize: '12px',
    marginBottom: '5px'
  },
  orderItem: {
    display: 'grid',
    gridTemplateColumns: '2fr 1fr 1fr 1fr',
    gap: '10px',
    padding: '8px 10px',
    borderBottom: '1px solid #f0f0f0'
  },
  itemName: {
    fontWeight: '500'
  },
  itemQty: {
    textAlign: 'center'
  },
  itemPrice: {
    textAlign: 'right'
  },
  itemSubtotal: {
    textAlign: 'right',
    fontWeight: 'bold'
  },
  orderTotal: {
    display: 'flex',
    justifyContent: 'flex-end',
    gap: '10px',
    marginTop: '15px',
    paddingTop: '10px',
    borderTop: '2px solid #ddd',
    fontSize: '18px',
    fontWeight: 'bold'
  },
  totalAmount: {
    color: '#27ae60'
  },
  notes: {
    marginTop: '10px',
    padding: '10px',
    backgroundColor: '#f8f9fa',
    borderRadius: '4px',
    fontSize: '14px'
  },
  error: {
    padding: '10px',
    backgroundColor: '#f8d7da',
    color: '#721c24',
    borderRadius: '4px',
    marginBottom: '20px'
  },
  empty: {
    textAlign: 'center',
    padding: '50px',
    backgroundColor: '#f8f9fa',
    borderRadius: '8px'
  },
  emptyButton: {
    marginTop: '15px',
    padding: '10px 20px',
    backgroundColor: '#3498db',
    color: 'white',
    border: 'none',
    borderRadius: '4px',
    cursor: 'pointer'
  }
};

export default Orders;
