import React, { useState, useEffect } from 'react';
import { productService } from '../services/api';
import { useAuth } from '../context/AuthContext';

function Products() {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [editingProduct, setEditingProduct] = useState(null);
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    sku: '',
    price: '',
    stock: '',
    minimumStock: '',
    category: ''
  });
  const { logout } = useAuth();

  // Cargar productos al montar el componente
  useEffect(() => {
    loadProducts();
  }, []);

  const loadProducts = async () => {
    try {
      setLoading(true);
      const response = await productService.getAll();
      setProducts(response.data);
      setError('');
    } catch (err) {
      setError('Error al cargar productos');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    // Validaciones
    if (!formData.name.trim()) {
      setError('El nombre del producto es requerido');
      return;
    }
    if (!formData.sku.trim()) {
      setError('El SKU del producto es requerido');
      return;
    }
    if (!formData.price || formData.price <= 0) {
      setError('El precio debe ser mayor a 0');
      return;
    }
    if (!formData.stock || formData.stock < 0) {
      setError('El stock no puede ser negativo');
      return;
    }
    if (!formData.minimumStock || formData.minimumStock < 0) {
      setError('El stock mínimo no puede ser negativo');
      return;
    }

    const productToSend = {
      ...formData,
      price: parseFloat(formData.price),
      stock: parseInt(formData.stock),
      minimumStock: parseInt(formData.minimumStock)
    };

    try {
      if (editingProduct) {
        await productService.update(editingProduct.id, productToSend);
      } else {
        await productService.create(productToSend);
      }
      resetForm();
      loadProducts();
      setError('');
    } catch (err) {
      setError(editingProduct ? 'Error al actualizar' : 'Error al crear');
      console.error(err);
    }
  };

  const handleDelete = async (id) => {
    if (window.confirm('¿Eliminar este producto?')) {
      try {
        await productService.delete(id);
        loadProducts();
      } catch (err) {
        setError('Error al eliminar');
      }
    }
  };

  const handleEdit = (product) => {
    setEditingProduct(product);
    setFormData({
      name: product.name,
      description: product.description || '',
      sku: product.sku,
      price: product.price,
      stock: product.stock,
      minimumStock: product.minimumStock,
      category: product.category || ''
    });
    setShowForm(true);
  };

  const resetForm = () => {
    setShowForm(false);
    setEditingProduct(null);
    setFormData({
      name: '',
      description: '',
      sku: '',
      price: '',
      stock: '',
      minimumStock: '',
      category: ''
    });
  };

  if (loading) return <div style={styles.container}>Cargando productos...</div>;

  return (
    <div style={styles.container}>
      <div style={styles.header}>
        <h1>Gestión de Productos</h1>
        <div>
          <button onClick={() => setShowForm(!showForm)} style={styles.addButton}>
            {showForm ? 'Cancelar' : '+ Nuevo Producto'}
          </button>
          <button onClick={logout} style={styles.logoutButton}>
            Cerrar Sesión
          </button>
        </div>
      </div>

      {error && <div style={styles.error}>{error}</div>}

      {showForm && (
        <div style={styles.formContainer}>
          <h2>{editingProduct ? '✏️ Editar Producto' : '➕ Nuevo Producto'}</h2>
          <form onSubmit={handleSubmit} style={styles.form}>
            <div style={styles.formGroup}>
              <label style={styles.label}>Nombre *</label>
              <input
                type="text"
                placeholder="Ej: Laptop HP Pavilion"
                value={formData.name}
                onChange={(e) => setFormData({...formData, name: e.target.value})}
                style={styles.input}
                required
              />
            </div>

            <div style={styles.formGroup}>
              <label style={styles.label}>SKU *</label>
              <input
                type="text"
                placeholder="Ej: HP-LAP-001 (código único)"
                value={formData.sku}
                onChange={(e) => setFormData({...formData, sku: e.target.value.toUpperCase()})}
                style={styles.input}
                required
              />
              <small style={styles.helperText}>Código único del producto, se guarda en mayúsculas</small>
            </div>

            <div style={styles.formRow}>
              <div style={styles.formGroup}>
                <label style={styles.label}>Precio ($) *</label>
                <input
                  type="number"
                  placeholder="Ej: 499990"
                  value={formData.price}
                  onChange={(e) => setFormData({...formData, price: e.target.value})}
                  style={styles.input}
                  required
                  min="0"
                  step="1"
                />
                <small style={styles.helperText}>Precio en pesos chilenos</small>
              </div>

              <div style={styles.formGroup}>
                <label style={styles.label}>Stock *</label>
                <input
                  type="number"
                  placeholder="Ej: 10"
                  value={formData.stock}
                  onChange={(e) => setFormData({...formData, stock: e.target.value})}
                  style={styles.input}
                  required
                  min="0"
                  step="1"
                />
                <small style={styles.helperText}>Cantidad disponible en inventario</small>
              </div>

              <div style={styles.formGroup}>
                <label style={styles.label}>Stock Mínimo *</label>
                <input
                  type="number"
                  placeholder="Ej: 2"
                  value={formData.minimumStock}
                  onChange={(e) => setFormData({...formData, minimumStock: e.target.value})}
                  style={styles.input}
                  required
                  min="0"
                  step="1"
                />
                <small style={styles.helperText}>Alerta cuando el stock baja de este nivel</small>
              </div>
            </div>

            <div style={styles.formGroup}>
              <label style={styles.label}>Categoría</label>
              <input
                type="text"
                placeholder="Ej: Electrónica, Componentes, Accesorios"
                value={formData.category}
                onChange={(e) => setFormData({...formData, category: e.target.value})}
                style={styles.input}
              />
            </div>

            <div style={styles.formGroup}>
              <label style={styles.label}>Descripción</label>
              <textarea
                placeholder="Descripción detallada del producto..."
                value={formData.description}
                onChange={(e) => setFormData({...formData, description: e.target.value})}
                style={styles.textarea}
                rows="3"
              />
            </div>

            <div style={styles.formActions}>
              <button type="submit" style={styles.submitButton}>
                {editingProduct ? '✓ Actualizar Producto' : '✓ Crear Producto'}
              </button>
              <button type="button" onClick={resetForm} style={styles.cancelButton}>
                ✗ Cancelar
              </button>
            </div>
          </form>
        </div>
      )}

      <div style={styles.productGrid}>
        {products.map(product => (
          <div key={product.id} style={styles.productCard}>
            <h3>{product.name}</h3>
            <p><strong>SKU:</strong> {product.sku}</p>
            <p><strong>Precio:</strong> </p>
            <p><strong>Stock:</strong> {product.stock}</p>
            <p><strong>Stock Mínimo:</strong> {product.minimumStock}</p>
            <p><strong>Categoría:</strong> {product.category || 'Sin categoría'}</p>
            <p><strong>Descripción:</strong> {product.description || 'Sin descripción'}</p>
            <div style={styles.cardActions}>
              <button onClick={() => handleEdit(product)} style={styles.editButton}>✏️ Editar</button>
              <button onClick={() => handleDelete(product.id)} style={styles.deleteButton}>🗑️ Eliminar</button>
            </div>
          </div>
        ))}
      </div>

      {products.length === 0 && !loading && (
        <p style={styles.empty}>No hay productos. ¡Crea tu primer producto!</p>
      )}
    </div>
  );
}

const styles = {
  container: {
    padding: '20px',
    maxWidth: '1200px',
    margin: '0 auto',
    fontFamily: 'system-ui, -apple-system, sans-serif',
  },
  header: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: '20px',
    flexWrap: 'wrap',
    gap: '10px',
  },
  addButton: {
    padding: '10px 20px',
    backgroundColor: '#28a745',
    color: 'white',
    border: 'none',
    borderRadius: '4px',
    cursor: 'pointer',
    marginRight: '10px',
    fontSize: '14px',
  },
  logoutButton: {
    padding: '10px 20px',
    backgroundColor: '#dc3545',
    color: 'white',
    border: 'none',
    borderRadius: '4px',
    cursor: 'pointer',
    fontSize: '14px',
  },
  formContainer: {
    backgroundColor: '#f8f9fa',
    padding: '25px',
    borderRadius: '8px',
    marginBottom: '20px',
    border: '1px solid #dee2e6',
  },
  form: {
    display: 'flex',
    flexDirection: 'column',
    gap: '15px',
  },
  formRow: {
    display: 'grid',
    gridTemplateColumns: '1fr 1fr 1fr',
    gap: '15px',
  },
  formGroup: {
    display: 'flex',
    flexDirection: 'column',
    gap: '5px',
  },
  label: {
    fontWeight: '600',
    fontSize: '14px',
    color: '#333',
  },
  input: {
    padding: '10px',
    fontSize: '14px',
    border: '1px solid #ddd',
    borderRadius: '4px',
    outline: 'none',
    transition: 'border-color 0.2s',
  },
  textarea: {
    padding: '10px',
    fontSize: '14px',
    border: '1px solid #ddd',
    borderRadius: '4px',
    fontFamily: 'system-ui, -apple-system, sans-serif',
    outline: 'none',
  },
  helperText: {
    fontSize: '12px',
    color: '#666',
    marginTop: '2px',
  },
  formActions: {
    display: 'flex',
    gap: '10px',
    marginTop: '10px',
  },
  submitButton: {
    padding: '10px 20px',
    backgroundColor: '#007bff',
    color: 'white',
    border: 'none',
    borderRadius: '4px',
    cursor: 'pointer',
    fontSize: '14px',
  },
  cancelButton: {
    padding: '10px 20px',
    backgroundColor: '#6c757d',
    color: 'white',
    border: 'none',
    borderRadius: '4px',
    cursor: 'pointer',
    fontSize: '14px',
  },
  productGrid: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fill, minmax(350px, 1fr))',
    gap: '20px',
  },
  productCard: {
    border: '1px solid #ddd',
    borderRadius: '8px',
    padding: '15px',
    backgroundColor: 'white',
    boxShadow: '0 2px 4px rgba(0,0,0,0.1)',
  },
  cardActions: {
    display: 'flex',
    gap: '10px',
    marginTop: '10px',
  },
  editButton: {
    padding: '5px 10px',
    backgroundColor: '#ffc107',
    color: '#333',
    border: 'none',
    borderRadius: '4px',
    cursor: 'pointer',
  },
  deleteButton: {
    padding: '5px 10px',
    backgroundColor: '#dc3545',
    color: 'white',
    border: 'none',
    borderRadius: '4px',
    cursor: 'pointer',
  },
  error: {
    padding: '10px',
    backgroundColor: '#f8d7da',
    color: '#721c24',
    borderRadius: '4px',
    marginBottom: '20px',
    border: '1px solid #f5c6cb',
  },
  empty: {
    textAlign: 'center',
    color: '#666',
    marginTop: '50px',
    padding: '40px',
    backgroundColor: '#f8f9fa',
    borderRadius: '8px',
  },
};

export default Products;
