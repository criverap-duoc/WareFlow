import React from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useCart } from '../context/CartContext';

function Navbar() {
  const navigate = useNavigate();
  const location = useLocation();
  const { logout, isAuthenticated } = useAuth();
  const { totalItems } = useCart();

  if (!isAuthenticated) return null;

  const isActive = (path) => location.pathname === path;

  return (
    <nav style={styles.navbar}>
      <div style={styles.logo} onClick={() => navigate('/products')}>
        📦 WareFlow
      </div>
      
      <div style={styles.navLinks}>
        <button 
          onClick={() => navigate('/products')} 
          style={{ ...styles.navButton, ...(isActive('/products') && styles.active) }}
        >
          📋 Productos
        </button>
        
        <button 
          onClick={() => navigate('/cart')} 
          style={{ ...styles.navButton, ...(isActive('/cart') && styles.active) }}
        >
          🛒 Carrito
          {totalItems > 0 && <span style={styles.cartBadge}>{totalItems}</span>}
        </button>
        
        <button 
          onClick={() => navigate('/orders')} 
          style={{ ...styles.navButton, ...(isActive('/orders') && styles.active) }}
        >
          📦 Mis Órdenes
        </button>
        
        <button onClick={logout} style={styles.logoutButton}>
          🚪 Salir
        </button>
      </div>
    </nav>
  );
}

const styles = {
  navbar: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    backgroundColor: '#2c3e50',
    padding: '15px 30px',
    color: 'white',
    boxShadow: '0 2px 4px rgba(0,0,0,0.1)',
    marginBottom: '20px'
  },
  logo: {
    fontSize: '24px',
    fontWeight: 'bold',
    cursor: 'pointer',
    transition: 'opacity 0.2s'
  },
  navLinks: {
    display: 'flex',
    gap: '15px',
    alignItems: 'center'
  },
  navButton: {
    backgroundColor: 'transparent',
    color: 'white',
    border: 'none',
    padding: '8px 16px',
    borderRadius: '4px',
    cursor: 'pointer',
    fontSize: '14px',
    transition: 'background-color 0.2s',
    position: 'relative'
  },
  active: {
    backgroundColor: '#34495e',
    borderBottom: '2px solid #3498db'
  },
  logoutButton: {
    backgroundColor: '#e74c3c',
    color: 'white',
    border: 'none',
    padding: '8px 16px',
    borderRadius: '4px',
    cursor: 'pointer',
    fontSize: '14px',
    marginLeft: '10px'
  },
  cartBadge: {
    position: 'absolute',
    top: '-5px',
    right: '-5px',
    backgroundColor: '#e74c3c',
    color: 'white',
    borderRadius: '50%',
    padding: '2px 6px',
    fontSize: '10px',
    fontWeight: 'bold'
  }
};

export default Navbar;
