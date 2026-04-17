import React, { useState, useEffect } from 'react';
import { productService } from '../services/api';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import { useCart } from '../context/CartContext';

// Función para obtener imagen por defecto según el producto
const getDefaultImage = (productName, category) => {
  const name = (productName || '').toLowerCase();
  const cat = (category || '').toLowerCase();

  // URLs confiables que funcionan siempre
  const reliableImages = {
    gpu: 'https://images.pexels.com/photos/2582937/pexels-photo-2582937.jpeg?w=300',
    gpu2: 'https://images.pexels.com/photos/2582934/pexels-photo-2582934.jpeg?w=300',
    cpu: 'https://images.pexels.com/photos/2582936/pexels-photo-2582936.jpeg?w=300',
    default: 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=300'
  };

  // GPU / Tarjeta gráfica (usando Pexels que es más confiable)
  if (name.includes('gpu') || name.includes('rtx') || name.includes('gtx') ||
      name.includes('nvidia') || name.includes('amd') || name.includes('radeon') ||
      name.includes('tarjeta') || name.includes('grafica') || cat.includes('gpu') ||
      cat.includes('componentes') && (name.includes('rtx') || name.includes('gtx'))) {
    return reliableImages.gpu;
  }

  // CPU / Procesador
  if (name.includes('cpu') || name.includes('procesador') || name.includes('intel') ||
      name.includes('ryzen') || name.includes('core i') || cat.includes('cpu')) {
    return reliableImages.cpu;
  }

  // SSD / Almacenamiento
  if (name.includes('ssd') || name.includes('disco') || name.includes('almacenamiento') ||
      name.includes('wd black') || cat.includes('ssd')) {
    return 'https://images.unsplash.com/photo-1597878400966-7c9d0e8a7b8a?w=300';
  }

  // RAM / Memoria
  if (name.includes('ram') || name.includes('memoria') || name.includes('corsair') ||
      name.includes('kingston') || cat.includes('ram')) {
    return 'https://images.unsplash.com/photo-1562976540-1502c2145186?w=300';
  }

  // Laptops
  if (name.includes('laptop') || name.includes('notebook') || name.includes('dell') ||
      name.includes('hp') || name.includes('lenovo') || name.includes('macbook') ||
      name.includes('asus') || name.includes('acer') || cat.includes('laptop')) {
    return 'https://images.unsplash.com/photo-1541807084-5c52b6b3adef?w=300';
  }

  // Monitores
  if (name.includes('monitor') || name.includes('pantalla') || name.includes('samsung') ||
      name.includes('lg') || name.includes('dell monitor') || name.includes('asus monitor') ||
      name.includes('ultrawide') || cat.includes('monitor')) {
    return 'https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?w=300';
  }

  // Teclados
  if (name.includes('teclado') || name.includes('keyboard') || name.includes('redragon') ||
      name.includes('corsair') || name.includes('razer') || name.includes('logitech') ||
      cat.includes('teclado')) {
    return 'https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=300';
  }

  // Mouses
  if (name.includes('mouse') || name.includes('logitech') || name.includes('raton') ||
      name.includes('razer') || name.includes('corsair') || name.includes('redragon') ||
      cat.includes('mouse')) {
    return 'https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?w=300';
  }

  // Audífonos
  if (name.includes('headset') || name.includes('audifonos') || name.includes('hyperx') ||
      name.includes('corsair head') || name.includes('razer') || name.includes('sony') ||
      name.includes('jbl') || name.includes('airpods') || cat.includes('audio')) {
    return 'https://images.unsplash.com/photo-1583394838336-acd977736f90?w=300';
  }

  // Smartphones
  if (name.includes('iphone') || name.includes('samsung galaxy') || name.includes('xiaomi') ||
      name.includes('celular') || name.includes('telefono') || name.includes('pixel') ||
      cat.includes('smartphone')) {
    return 'https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=300';
  }

  // Tablets
  if (name.includes('tablet') || name.includes('ipad') || name.includes('tab') ||
      cat.includes('tablet')) {
    return 'https://images.unsplash.com/photo-1561154464-82e9adf32764?w=300';
  }

  // Default
  return reliableImages.default;
};

function Products() {
  const [products, setProducts] = useState([]);
  const [filteredProducts, setFilteredProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [editingProduct, setEditingProduct] = useState(null);

  // Estados para filtros
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedCategory, setSelectedCategory] = useState('');
  const [priceRange, setPriceRange] = useState('all');
  const [stockFilter, setStockFilter] = useState('all');
  const [sortBy, setSortBy] = useState('name');
  const [sortOrder, setSortOrder] = useState('asc');
  const [categories, setCategories] = useState([]);

  const [formData, setFormData] = useState({
    name: '',
    description: '',
    sku: '',
    price: '',
    stock: '',
    minimumStock: '',
    category: '',
    imageUrl: ''
  });
  const { logout } = useAuth();

  useEffect(() => {
    loadProducts();
  }, []);

  useEffect(() => {
    applyFilters();
  }, [products, searchTerm, selectedCategory, priceRange, stockFilter, sortBy, sortOrder]);

  const loadProducts = async () => {
    try {
      setLoading(true);
      const response = await productService.getAll();
      setProducts(response.data);
      const uniqueCategories = [...new Set(response.data.map(p => p.category).filter(c => c))];
      setCategories(uniqueCategories);
      setError('');
    } catch (err) {
      setError('Error al cargar productos');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const applyFilters = () => {
    let filtered = [...products];

    if (searchTerm) {
      filtered = filtered.filter(p =>
        p.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        p.sku.toLowerCase().includes(searchTerm.toLowerCase()) ||
        (p.category && p.category.toLowerCase().includes(searchTerm.toLowerCase()))
      );
    }

    if (selectedCategory) {
      filtered = filtered.filter(p => p.category === selectedCategory);
    }

    if (priceRange !== 'all') {
      const [min, max] = priceRange.split('-').map(Number);
      filtered = filtered.filter(p => p.price >= min && (max ? p.price <= max : true));
    }

    if (stockFilter === 'low') {
      filtered = filtered.filter(p => p.stock <= p.minimumStock);
    } else if (stockFilter === 'out') {
      filtered = filtered.filter(p => p.stock === 0);
    } else if (stockFilter === 'in') {
      filtered = filtered.filter(p => p.stock > 0);
    }

    filtered.sort((a, b) => {
      let comparison = 0;
      if (sortBy === 'name') comparison = a.name.localeCompare(b.name);
      if (sortBy === 'price') comparison = a.price - b.price;
      if (sortBy === 'stock') comparison = a.stock - b.stock;
      if (sortBy === 'category') comparison = (a.category || '').localeCompare(b.category || '');
      return sortOrder === 'asc' ? comparison : -comparison;
    });

    setFilteredProducts(filtered);
  };

  const navigate = useNavigate();
  const { totalItems, addToCart } = useCart();

  const handleSubmit = async (e) => {
    e.preventDefault();
    
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

    const productToSend = {
      ...formData,
      price: parseFloat(formData.price),
      stock: parseInt(formData.stock) || 0,
      minimumStock: parseInt(formData.minimumStock) || 0,
      imageUrl: formData.imageUrl || getDefaultImage(formData.name, formData.category)
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
      category: product.category || '',
      imageUrl: product.imageUrl || ''
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
      category: '',
      imageUrl: ''
    });
  };

  const resetFilters = () => {
    setSearchTerm('');
    setSelectedCategory('');
    setPriceRange('all');
    setStockFilter('all');
    setSortBy('name');
    setSortOrder('asc');
  };

  // Calcular estadísticas para el dashboard con valores seguros
  const stats = {
    totalProducts: products.length,
    totalValue: products.reduce((sum, p) => {
      const price = typeof p.price === 'number' ? p.price : parseFloat(p.price) || 0;
      const stock = typeof p.stock === 'number' ? p.stock : parseInt(p.stock) || 0;
      return sum + (price * stock);
    }, 0),
    lowStock: products.filter(p => {
      const stock = typeof p.stock === 'number' ? p.stock : parseInt(p.stock) || 0;
      const minStock = typeof p.minimumStock === 'number' ? p.minimumStock : parseInt(p.minimumStock) || 0;
      return stock <= minStock;
    }).length,
    outOfStock: products.filter(p => {
      const stock = typeof p.stock === 'number' ? p.stock : parseInt(p.stock) || 0;
      return stock === 0;
    }).length,
    categories: categories.length,
    avgPrice: products.reduce((sum, p) => {
      const price = typeof p.price === 'number' ? p.price : parseFloat(p.price) || 0;
      return sum + price;
    }, 0) / (products.length || 1)
  };

  if (loading) return <div style={styles.container}>Cargando productos...</div>;

  return (
    <div style={styles.container}>
      <div style={styles.header}>
        <h1>📦 Gestión de Productos</h1>
        <button onClick={() => setShowForm(!showForm)} style={styles.addButton}>
          {showForm ? '✗ Cancelar' : '+ Nuevo Producto'}
        </button>
      </div>

      {/* Dashboard KPIs */}
      <div style={styles.dashboard}>
        <div style={styles.kpiCard}>
          <div style={styles.kpiValue}>{stats.totalProducts || 0}</div>
          <div style={styles.kpiLabel}>Total Productos</div>
        </div>
        <div style={styles.kpiCard}>
          <div style={styles.kpiValue}>
            ${(stats.totalValue || 0).toLocaleString()}
          </div>
          <div style={styles.kpiLabel}>Valor Inventario</div>
        </div>
        <div style={styles.kpiCard}>
          <div style={styles.kpiValue}>{stats.lowStock || 0}</div>
          <div style={styles.kpiLabel}>Stock Bajo ⚠️</div>
        </div>
        <div style={styles.kpiCard}>
          <div style={styles.kpiValue}>{stats.outOfStock || 0}</div>
          <div style={styles.kpiLabel}>Sin Stock ❌</div>
        </div>
        <div style={styles.kpiCard}>
          <div style={styles.kpiValue}>{stats.categories || 0}</div>
          <div style={styles.kpiLabel}>Categorías</div>
        </div>
        <div style={styles.kpiCard}>
          <div style={styles.kpiValue}>
            ${Math.round(stats.avgPrice || 0).toLocaleString()}
          </div>
          <div style={styles.kpiLabel}>Precio Promedio</div>
        </div>
      </div>

      {/* Filtros */}
      <div style={styles.filtersContainer}>
        <div style={styles.filterRow}>
          <input type="text" placeholder="🔍 Buscar por nombre, SKU o categoría..." value={searchTerm} onChange={(e) => setSearchTerm(e.target.value)} style={styles.searchInput} />
          <button onClick={resetFilters} style={styles.resetButton}>🔄 Limpiar Filtros</button>
        </div>
        <div style={styles.filterRow}>
          <select value={selectedCategory} onChange={(e) => setSelectedCategory(e.target.value)} style={styles.filterSelect}>
            <option value="">📂 Todas las categorías</option>
            {categories.map(cat => <option key={cat} value={cat}>{cat}</option>)}
          </select>
          <select value={priceRange} onChange={(e) => setPriceRange(e.target.value)} style={styles.filterSelect}>
            <option value="all">💰 Todos los precios</option>
            <option value="0-100000"> - .000</option>
            <option value="100000-500000">.000 - .000</option>
            <option value="500000-1000000">.000 - .000.000</option>
            <option value="1000000-99999999">.000.000+</option>
          </select>
          <select value={stockFilter} onChange={(e) => setStockFilter(e.target.value)} style={styles.filterSelect}>
            <option value="all">📊 Todo el stock</option>
            <option value="low">⚠️ Stock bajo</option>
            <option value="out">❌ Sin stock</option>
            <option value="in">✅ Con stock</option>
          </select>
          <select value={sortBy} onChange={(e) => setSortBy(e.target.value)} style={styles.filterSelect}>
            <option value="name">📝 Ordenar por nombre</option>
            <option value="price">💰 Ordenar por precio</option>
            <option value="stock">📦 Ordenar por stock</option>
            <option value="category">🏷️ Ordenar por categoría</option>
          </select>
          <button onClick={() => setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc')} style={styles.sortButton}>
            {sortOrder === 'asc' ? '↑ Ascendente' : '↓ Descendente'}
          </button>
        </div>
      </div>

      {error && <div style={styles.error}>{error}</div>}

      {/* Formulario */}
      {showForm && (
        <div style={styles.formContainer}>
          <h2>{editingProduct ? '✏️ Editar Producto' : '➕ Nuevo Producto'}</h2>
          <form onSubmit={handleSubmit} style={styles.form}>
            <div style={styles.formRow}>
              <div style={styles.formGroup}><label>Nombre *</label><input type="text" value={formData.name} onChange={(e) => setFormData({...formData, name: e.target.value})} style={styles.input} required /></div>
              <div style={styles.formGroup}><label>SKU *</label><input type="text" value={formData.sku} onChange={(e) => setFormData({...formData, sku: e.target.value.toUpperCase()})} style={styles.input} required /></div>
            </div>
            <div style={styles.formRow}>
              <div style={styles.formGroup}><label>Precio ($) *</label><input type="number" value={formData.price} onChange={(e) => setFormData({...formData, price: e.target.value})} style={styles.input} min="0" /></div>
              <div style={styles.formGroup}><label>Stock</label><input type="number" value={formData.stock} onChange={(e) => setFormData({...formData, stock: e.target.value})} style={styles.input} min="0" /></div>
              <div style={styles.formGroup}><label>Stock Mínimo</label><input type="number" value={formData.minimumStock} onChange={(e) => setFormData({...formData, minimumStock: e.target.value})} style={styles.input} min="0" /></div>
            </div>
            <div style={styles.formRow}>
              <div style={styles.formGroup}><label>Categoría</label><input type="text" value={formData.category} onChange={(e) => setFormData({...formData, category: e.target.value})} style={styles.input} /></div>
              <div style={styles.formGroup}><label>URL Imagen (opcional)</label><input type="text" value={formData.imageUrl} onChange={(e) => setFormData({...formData, imageUrl: e.target.value})} style={styles.input} placeholder="Dejar vacío para imagen automática" /></div>
            </div>
            <div style={styles.formGroup}><label>Descripción</label><textarea value={formData.description} onChange={(e) => setFormData({...formData, description: e.target.value})} style={styles.textarea} rows="3" /></div>
            <div style={styles.formActions}>
              <button type="submit" style={styles.submitButton}>{editingProduct ? '✓ Actualizar' : '✓ Crear'}</button>
              <button type="button" onClick={resetForm} style={styles.cancelButton}>✗ Cancelar</button>
            </div>
          </form>
        </div>
      )}

      {/* Lista de Productos */}
      <div style={styles.productGrid}>
        {filteredProducts.map(product => {
          // Manejo seguro de precios y stocks
          const safePrice = product?.price || 0;
          const safeStock = product?.stock || 0;
          const safeMinStock = product?.minimumStock || 0;
          const productImage = product?.imageUrl || getDefaultImage(product?.name, product?.category);

          return (
            <div key={product.id} style={styles.productCard}>
              <div style={styles.imageContainer}>
                <img
                  src={productImage}
                  alt={product.name || 'Producto'}
                  style={styles.productImage}
                  loading="lazy"
                  onError={(e) => {
                    // Si falla la imagen, usar una URL de respaldo absoluta
                    const fallbackUrl = 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=300';
                    if (e.target.src !== fallbackUrl) {
                      e.target.src = fallbackUrl;
                    } else {
                      e.target.onerror = null;
                    }
                  }}
                />
              </div>
              <h3 style={styles.productTitle}>{product.name || 'Sin nombre'}</h3>
              <div style={styles.productDetails}>
                <p><strong>SKU:</strong> {product.sku || 'N/A'}</p>
                <p><strong>Precio:</strong> ${safePrice.toLocaleString()}</p>
                <p><strong>Stock:</strong> <span style={safeStock <= safeMinStock ? styles.lowStockText : {}}>{safeStock}</span></p>
                <p><strong>Stock Mínimo:</strong> {safeMinStock}</p>
                <p><strong>Categoría:</strong> {product.category || 'Sin categoría'}</p>
                {product.description && <p><strong>Descripción:</strong> {product.description.substring(0, 80)}...</p>}
              </div>
              <div style={styles.cardActions}>
                <button onClick={() => handleEdit(product)} style={styles.editButton}>✏️ Editar</button>
                <button onClick={() => handleDelete(product.id)} style={styles.deleteButton}>🗑️ Eliminar</button>
                <button
                  onClick={() => addToCart(product, 1)}
                  style={styles.cartButton}
                >
                  🛒 Agregar
                </button>
              </div>
            </div>
          );
        })}
      </div>

      {filteredProducts.length === 0 && !loading && <p style={styles.empty}>No hay productos que coincidan con los filtros.</p>}
    </div>
  );
}

const styles = {
  container: { padding: '20px', maxWidth: '1400px', margin: '0 auto', fontFamily: 'system-ui' },
  header: { display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px', flexWrap: 'wrap', gap: '10px' },
  dashboard: { display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(150px, 1fr))', gap: '15px', marginBottom: '25px' },
  kpiCard: { backgroundColor: '#f8f9fa', padding: '15px', borderRadius: '8px', textAlign: 'center', border: '1px solid #dee2e6' },
  kpiValue: { fontSize: '24px', fontWeight: 'bold', color: '#007bff' },
  kpiLabel: { fontSize: '12px', color: '#666', marginTop: '5px' },
  filtersContainer: { backgroundColor: '#f8f9fa', padding: '15px', borderRadius: '8px', marginBottom: '20px' },
  filterRow: { display: 'flex', gap: '10px', marginBottom: '10px', flexWrap: 'wrap' },
  searchInput: { flex: '1', padding: '10px', fontSize: '14px', border: '1px solid #ddd', borderRadius: '4px' },
  filterSelect: { padding: '10px', fontSize: '14px', border: '1px solid #ddd', borderRadius: '4px', backgroundColor: 'white' },
  resetButton: { padding: '10px 20px', backgroundColor: '#6c757d', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' },
  sortButton: { padding: '10px 20px', backgroundColor: '#17a2b8', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' },
  addButton: { padding: '10px 20px', backgroundColor: '#28a745', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer', marginRight: '10px' },
  logoutButton: { padding: '10px 20px', backgroundColor: '#dc3545', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' },
  formContainer: { backgroundColor: '#f8f9fa', padding: '25px', borderRadius: '8px', marginBottom: '20px', border: '1px solid #dee2e6' },
  form: { display: 'flex', flexDirection: 'column', gap: '15px' },
  formRow: { display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '15px' },
  formGroup: { display: 'flex', flexDirection: 'column', gap: '5px' },
  input: { padding: '10px', fontSize: '14px', border: '1px solid #ddd', borderRadius: '4px' },
  textarea: { padding: '10px', fontSize: '14px', border: '1px solid #ddd', borderRadius: '4px', fontFamily: 'system-ui' },
  formActions: { display: 'flex', gap: '10px', marginTop: '10px' },
  submitButton: { padding: '10px 20px', backgroundColor: '#007bff', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' },
  cancelButton: { padding: '10px 20px', backgroundColor: '#6c757d', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' },
  productGrid: { display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(320px, 1fr))', gap: '20px' },
  imageContainer: { width: '100%', height: '200px', overflow: 'hidden', borderRadius: '4px', marginBottom: '10px', backgroundColor: '#f5f5f5', display: 'flex', alignItems: 'center', justifyContent: 'center' },
  productImage: { width: '100%', height: '100%', objectFit: 'cover', transition: 'transform 0.3s ease' },
  productTitle: { fontSize: '18px', fontWeight: '600', marginBottom: '10px', color: '#333', height: '50px', overflow: 'hidden', display: '-webkit-box', WebkitLineClamp: 2, WebkitBoxOrient: 'vertical' },
  productDetails: { fontSize: '14px', lineHeight: '1.5', marginBottom: '10px' },
  productCard: { border: '1px solid #ddd', borderRadius: '8px', padding: '15px', backgroundColor: 'white', boxShadow: '0 2px 4px rgba(0,0,0,0.1)', transition: 'box-shadow 0.2s ease', animation: 'fadeIn 0.3s ease-in-out' },
  lowStockText: { color: '#dc3545', fontWeight: 'bold' },
  cardActions: { display: 'flex', gap: '10px', marginTop: '10px' },
  editButton: { padding: '5px 10px', backgroundColor: '#ffc107', color: '#333', border: 'none', borderRadius: '4px', cursor: 'pointer' },
  deleteButton: { padding: '5px 10px', backgroundColor: '#dc3545', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' },
  error: { padding: '10px', backgroundColor: '#f8d7da', color: '#721c24', borderRadius: '4px', marginBottom: '20px' },
  empty: { textAlign: 'center', color: '#666', marginTop: '50px', padding: '40px', backgroundColor: '#f8f9fa', borderRadius: '8px' }
};

const styleSheet = document.createElement("style");
styleSheet.textContent = `
  @keyframes fadeIn {
    from {
      opacity: 0;
      transform: translateY(10px);
    }
    to {
      opacity: 1;
      transform: translateY(0);
    }
  }
`;
document.head.appendChild(styleSheet);

export default Products;
