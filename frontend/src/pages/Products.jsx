import { useState, useEffect, useMemo, useRef } from 'react';
import { cn } from 'cn';
import { toast } from 'sonner';
import {
  AlertTriangle,
  CheckCircle2,
  MoreVertical,
  Package,
  Pencil,
  Plus,
  Search,
  ShoppingCart,
  SlidersHorizontal,
  Tags,
  Trash2,
  Wallet,
  X,
} from 'lucide-react';
import { productService } from '../services/api';
import { useCart } from '../context/CartContext';
import { useAuth } from '../context/AuthContext';
import { getDefaultImage } from '../lib/images';
import { formatCLP } from '../lib/formatters';
import { PageHeader } from '../components/layout/PageHeader';
import { StockBadge } from '../components/products/StockBadge';
import { Button } from '../components/ui/button';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Textarea } from '../components/ui/textarea';
import { Card } from '../components/ui/card';
import { Skeleton } from '../components/ui/skeleton';
import { Badge } from '../components/ui/badge';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '../components/ui/select';
import { Popover, PopoverContent, PopoverTrigger } from '../components/ui/popover';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '../components/ui/dropdown-menu';
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '../components/ui/dialog';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '../components/ui/alert-dialog';

const EMPTY_FORM = {
  name: '',
  description: '',
  sku: '',
  price: '',
  stock: '',
  minimumStock: '',
  category: '',
  imageUrl: '',
};

const PRICE_RANGES = [
  { value: 'all', label: 'Todos los precios' },
  { value: '0-100000', label: 'Hasta $100.000' },
  { value: '100000-500000', label: '$100.000 – $500.000' },
  { value: '500000-1000000', label: '$500.000 – $1.000.000' },
  { value: '1000000-99999999', label: '$1.000.000 o más' },
];

const STOCK_OPTIONS = [
  { value: 'all', label: 'Todo el stock' },
  { value: 'low', label: 'Stock bajo' },
  { value: 'out', label: 'Sin stock' },
  { value: 'in', label: 'Con stock' },
  { value: 'alert', label: 'Stock bajo o sin stock' },
];

const SORT_OPTIONS = [
  { value: 'name-asc', label: 'Nombre (A-Z)' },
  { value: 'name-desc', label: 'Nombre (Z-A)' },
  { value: 'price-asc', label: 'Precio (menor a mayor)' },
  { value: 'price-desc', label: 'Precio (mayor a menor)' },
  { value: 'stock-asc', label: 'Stock (menor a mayor)' },
  { value: 'stock-desc', label: 'Stock (mayor a menor)' },
];

const priceRangeLabel = (value) => PRICE_RANGES.find((r) => r.value === value)?.label ?? value;
const stockLabel = (value) => STOCK_OPTIONS.find((s) => s.value === value)?.label ?? value;

const FALLBACK_IMAGE = 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=300';

function Products() {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [editingProduct, setEditingProduct] = useState(null);
  const [formError, setFormError] = useState('');
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [saving, setSaving] = useState(false);

  const [searchTerm, setSearchTerm] = useState('');
  const [selectedCategory, setSelectedCategory] = useState('all');
  const [priceRange, setPriceRange] = useState('all');
  const [stockFilter, setStockFilter] = useState('all');
  const [sortBy, setSortBy] = useState('name-asc');
  const [formData, setFormData] = useState(EMPTY_FORM);

  const filtersRef = useRef(null);
  const { addToCart } = useCart();
  const { user } = useAuth();
  // Sin roles implementados (fase 1) se permite gestionar a todos.
  const canManage = !user?.role || user.role === 'Admin';

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

  const categories = useMemo(
    () => [...new Set(products.map((p) => p.category).filter(Boolean))].sort(),
    [products]
  );

  const stats = useMemo(() => {
    const inventoryValue = products.reduce((sum, p) => {
      return sum + (Number(p.price) || 0) * (Number(p.stock) || 0);
    }, 0);
    const alerts = products.filter((p) => {
      const stock = Number(p.stock) || 0;
      const min = Number(p.minimumStock) || 0;
      return stock === 0 || stock <= min;
    }).length;
    return {
      totalProducts: products.length,
      inventoryValue,
      categories: categories.length,
      alerts,
    };
  }, [products, categories]);

  const filteredProducts = useMemo(() => {
    let filtered = [...products];
    const term = searchTerm.trim().toLowerCase();

    if (term) {
      filtered = filtered.filter(
        (p) =>
          p.name?.toLowerCase().includes(term) ||
          p.sku?.toLowerCase().includes(term) ||
          p.category?.toLowerCase().includes(term)
      );
    }

    if (selectedCategory && selectedCategory !== 'all') {
      filtered = filtered.filter((p) => p.category === selectedCategory);
    }

    if (priceRange !== 'all') {
      const [min, max] = priceRange.split('-').map(Number);
      filtered = filtered.filter((p) => {
        const price = Number(p.price) || 0;
        return price >= min && (max ? price <= max : true);
      });
    }

    if (stockFilter !== 'all') {
      filtered = filtered.filter((p) => {
        const stock = Number(p.stock) || 0;
        const min = Number(p.minimumStock) || 0;
        if (stockFilter === 'alert') return stock === 0 || stock <= min;
        if (stockFilter === 'low') return stock <= min;
        if (stockFilter === 'out') return stock === 0;
        if (stockFilter === 'in') return stock > 0;
        return true;
      });
    }

    const [field, direction] = sortBy.split('-');
    const sign = direction === 'desc' ? -1 : 1;
    filtered.sort((a, b) => {
      let comparison = 0;
      if (field === 'name') comparison = (a.name || '').localeCompare(b.name || '');
      if (field === 'price') comparison = (Number(a.price) || 0) - (Number(b.price) || 0);
      if (field === 'stock') comparison = (Number(a.stock) || 0) - (Number(b.stock) || 0);
      return comparison * sign;
    });

    return filtered;
  }, [products, searchTerm, selectedCategory, priceRange, stockFilter, sortBy]);

  const activeFilters = [
    searchTerm.trim() && {
      key: 'search',
      label: `Búsqueda: "${searchTerm.trim()}"`,
      clear: () => setSearchTerm(''),
    },
    selectedCategory !== 'all' && {
      key: 'category',
      label: selectedCategory,
      clear: () => setSelectedCategory('all'),
    },
    priceRange !== 'all' && {
      key: 'price',
      label: priceRangeLabel(priceRange),
      clear: () => setPriceRange('all'),
    },
    stockFilter !== 'all' && {
      key: 'stock',
      label: stockLabel(stockFilter),
      clear: () => setStockFilter('all'),
    },
  ].filter(Boolean);

  const resetFilters = () => {
    setSearchTerm('');
    setSelectedCategory('all');
    setPriceRange('all');
    setStockFilter('all');
    setSortBy('name-asc');
  };

  const openCreate = () => {
    setEditingProduct(null);
    setFormData(EMPTY_FORM);
    setFormError('');
    setShowForm(true);
  };

  const closeForm = () => {
    setShowForm(false);
    setEditingProduct(null);
    setFormData(EMPTY_FORM);
    setFormError('');
  };

  const handleEdit = (product) => {
    setEditingProduct(product);
    setFormData({
      name: product.name || '',
      description: product.description || '',
      sku: product.sku || '',
      price: product.price ?? '',
      stock: product.stock ?? '',
      minimumStock: product.minimumStock ?? '',
      category: product.category || '',
      imageUrl: product.imageUrl || '',
    });
    setFormError('');
    setShowForm(true);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setFormError('');

    if (!formData.name.trim()) {
      setFormError('El nombre del producto es requerido');
      return;
    }
    if (!formData.sku.trim()) {
      setFormError('El SKU del producto es requerido');
      return;
    }
    if (!formData.price || Number(formData.price) <= 0) {
      setFormError('El precio debe ser mayor a 0');
      return;
    }

    const productToSend = {
      ...formData,
      price: parseFloat(formData.price),
      stock: parseInt(formData.stock) || 0,
      minimumStock: parseInt(formData.minimumStock) || 0,
      imageUrl: formData.imageUrl || getDefaultImage(formData.name, formData.category),
    };

    try {
      setSaving(true);
      if (editingProduct) {
        await productService.update(editingProduct.id, productToSend);
        toast.success('Producto actualizado');
      } else {
        await productService.create(productToSend);
        toast.success('Producto creado');
      }
      closeForm();
      loadProducts();
    } catch (err) {
      console.error(err);
      const message = editingProduct
        ? 'Error al actualizar el producto'
        : 'Error al crear el producto';
      setFormError(message);
      toast.error(message);
    } finally {
      setSaving(false);
    }
  };

  const confirmDelete = async () => {
    if (!deleteTarget) return;
    try {
      await productService.delete(deleteTarget.id);
      toast.success('Producto eliminado');
      loadProducts();
    } catch (err) {
      console.error(err);
      toast.error('Error al eliminar el producto');
    } finally {
      setDeleteTarget(null);
    }
  };

  const renderFilterSelects = () => (
    <>
      <Select value={selectedCategory} onValueChange={setSelectedCategory}>
        <SelectTrigger className="w-full sm:w-44" aria-label="Categoría">
          <SelectValue placeholder="Categoría" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">Todas las categorías</SelectItem>
          {categories.map((cat) => (
            <SelectItem key={cat} value={cat}>
              {cat}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      <Select value={priceRange} onValueChange={setPriceRange}>
        <SelectTrigger className="w-full sm:w-48" aria-label="Precio">
          <SelectValue placeholder="Precio" />
        </SelectTrigger>
        <SelectContent>
          {PRICE_RANGES.map((r) => (
            <SelectItem key={r.value} value={r.value}>
              {r.label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      <Select value={stockFilter} onValueChange={setStockFilter}>
        <SelectTrigger className="w-full sm:w-40" aria-label="Stock">
          <SelectValue placeholder="Stock" />
        </SelectTrigger>
        <SelectContent>
          {STOCK_OPTIONS.map((s) => (
            <SelectItem key={s.value} value={s.value}>
              {s.label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      <Select value={sortBy} onValueChange={setSortBy}>
        <SelectTrigger className="w-full sm:w-52" aria-label="Ordenar por">
          <SelectValue placeholder="Ordenar" />
        </SelectTrigger>
        <SelectContent>
          {SORT_OPTIONS.map((o) => (
            <SelectItem key={o.value} value={o.value}>
              {o.label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </>
  );

  const hasAlerts = stats.alerts > 0;

  return (
    <div className="space-y-6">
      <PageHeader
        title="Productos"
        description={`${stats.totalProducts} productos en el catálogo`}
        className="pb-0"
        actions={
          <Button onClick={openCreate}>
            <Plus className="size-4" />
            Nuevo producto
          </Button>
        }
      />

      <div className="space-y-6">
        {error && (
          <div className="rounded-md border border-danger/20 bg-danger/10 px-3 py-2 text-sm text-danger">
            {error}
          </div>
        )}

        {/* KPIs */}
        <div className="grid grid-cols-2 gap-3 lg:grid-cols-4">
          <div className="col-span-2 flex items-center justify-between gap-4 rounded-lg border border-primary/20 bg-primary/5 p-4 lg:col-span-1">
            <div className="space-y-1">
              <p className="text-xs font-medium text-muted-foreground">Valor del inventario</p>
              <p className="num text-2xl font-semibold text-primary">
                {formatCLP(stats.inventoryValue)}
              </p>
            </div>
            <Wallet className="size-5 shrink-0 text-primary/70" />
          </div>

          <button
            type="button"
            onClick={resetFilters}
            className="col-span-1 flex items-center justify-between gap-4 rounded-lg border bg-card p-4 text-left shadow-sm transition-colors hover:bg-accent focus-visible:ring-2 focus-visible:ring-ring focus-visible:outline-none"
          >
            <div className="space-y-1">
              <p className="text-xs font-medium text-muted-foreground">Productos</p>
              <p className="num text-2xl font-semibold">{stats.totalProducts}</p>
            </div>
            <Package className="size-5 shrink-0 text-muted-foreground" />
          </button>

          <button
            type="button"
            onClick={() =>
              filtersRef.current?.scrollIntoView({ behavior: 'smooth', block: 'center' })
            }
            className="col-span-1 flex items-center justify-between gap-4 rounded-lg border bg-card p-4 text-left shadow-sm transition-colors hover:bg-accent focus-visible:ring-2 focus-visible:ring-ring focus-visible:outline-none"
          >
            <div className="space-y-1">
              <p className="text-xs font-medium text-muted-foreground">Categorías</p>
              <p className="num text-2xl font-semibold">{stats.categories}</p>
            </div>
            <Tags className="size-5 shrink-0 text-muted-foreground" />
          </button>

          <button
            type="button"
            onClick={() => setStockFilter('alert')}
            className={cn(
              'col-span-2 flex items-center justify-between gap-4 rounded-lg border p-4 text-left shadow-sm transition-colors focus-visible:ring-2 focus-visible:ring-ring focus-visible:outline-none lg:col-span-1',
              hasAlerts
                ? 'border-warning/20 bg-warning/5 hover:bg-warning/10'
                : 'border-success/20 bg-success/5 hover:bg-success/10'
            )}
          >
            <div className="space-y-1">
              <p className="text-xs font-medium text-muted-foreground">Alertas</p>
              <p
                className={cn(
                  'num font-semibold',
                  hasAlerts ? 'text-2xl text-warning' : 'text-lg text-success'
                )}
              >
                {hasAlerts ? stats.alerts : 'Todo en orden'}
              </p>
            </div>
            {hasAlerts ? (
              <AlertTriangle className="size-5 shrink-0 text-warning" />
            ) : (
              <CheckCircle2 className="size-5 shrink-0 text-success" />
            )}
          </button>
        </div>

        {/* Filtros */}
        <div ref={filtersRef} className="rounded-lg border bg-card p-3 shadow-sm sm:p-4">
          <div className="flex flex-col gap-3 sm:flex-row">
            <div className="relative flex-1">
              <Search className="pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                placeholder="Buscar por nombre, SKU o categoría"
                className="pl-9"
                aria-label="Buscar productos"
              />
            </div>

            <div className="hidden items-center gap-3 sm:flex">{renderFilterSelects()}</div>

            <Popover>
              <PopoverTrigger asChild>
                <Button variant="outline" className="sm:hidden">
                  <SlidersHorizontal className="size-4" />
                  Filtros
                </Button>
              </PopoverTrigger>
              <PopoverContent align="end" className="w-[calc(100vw-2rem)] space-y-3">
                <div className="flex flex-col gap-3">{renderFilterSelects()}</div>
              </PopoverContent>
            </Popover>
          </div>

          <div className="mt-3 flex flex-wrap items-center gap-2 border-t pt-3">
            <span className="text-sm text-muted-foreground">
              {filteredProducts.length} {filteredProducts.length === 1 ? 'producto' : 'productos'}
            </span>
            {activeFilters.map((f) => (
              <Badge key={f.key} variant="outline" className="gap-1 bg-muted/50 font-normal">
                {f.label}
                <button
                  type="button"
                  onClick={f.clear}
                  aria-label={`Quitar filtro ${f.label}`}
                  className="rounded-full hover:text-foreground"
                >
                  <X className="size-3" />
                </button>
              </Badge>
            ))}
            {activeFilters.length > 0 && (
              <Button
                variant="ghost"
                size="sm"
                onClick={resetFilters}
                className="ml-auto text-muted-foreground"
              >
                Limpiar
              </Button>
            )}
          </div>
        </div>

        {/* Lista de productos */}
        {loading ? (
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
            {Array.from({ length: 8 }).map((_, i) => (
              <Card key={i} className="gap-0 overflow-hidden p-0">
                <Skeleton className="aspect-[4/3] max-h-[200px] w-full rounded-none" />
                <div className="space-y-3 p-4">
                  <Skeleton className="h-3 w-1/3" />
                  <Skeleton className="h-4 w-3/4" />
                  <Skeleton className="h-3 w-1/2" />
                </div>
              </Card>
            ))}
          </div>
        ) : filteredProducts.length === 0 ? (
          <div className="flex flex-col items-center justify-center gap-3 rounded-lg border bg-card py-16 text-center">
            <Package className="h-12 w-12 text-muted-foreground" />
            <div className="space-y-1">
              <h3 className="text-base font-semibold">No hay productos que coincidan</h3>
              <p className="text-sm text-muted-foreground">
                Ajusta los filtros o crea un nuevo producto
              </p>
            </div>
            <div className="flex flex-wrap items-center justify-center gap-2">
              <Button variant="outline" onClick={resetFilters}>
                Limpiar filtros
              </Button>
              <Button onClick={openCreate}>
                <Plus className="size-4" /> Nuevo producto
              </Button>
            </div>
          </div>
        ) : (
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
            {filteredProducts.map((product) => {
              const safePrice = Number(product?.price) || 0;
              const safeStock = Number(product?.stock) || 0;
              const safeMinStock = Number(product?.minimumStock) || 0;
              const productImage =
                product?.imageUrl || getDefaultImage(product?.name, product?.category);
              return (
                <Card
                  key={product.id}
                  className="group gap-0 overflow-hidden p-0 transition-shadow hover:shadow-md"
                >
                  <div className="relative aspect-[4/3] max-h-[200px] w-full overflow-hidden bg-muted">
                    <img
                      src={productImage}
                      alt={product.name || 'Producto'}
                      loading="lazy"
                      className="h-full max-h-[200px] w-full object-cover transition-transform duration-300 group-hover:scale-105"
                      onError={(e) => {
                        if (e.target.src !== FALLBACK_IMAGE) {
                          e.target.src = FALLBACK_IMAGE;
                        } else {
                          e.target.onerror = null;
                        }
                      }}
                    />
                    {canManage && (
                      <div className="absolute top-2 right-2">
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button
                              variant="secondary"
                              size="icon-sm"
                              className="bg-background/80 backdrop-blur"
                              aria-label="Acciones del producto"
                              title="Acciones del producto"
                            >
                              <MoreVertical className="size-4" />
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end">
                            <DropdownMenuItem onSelect={() => handleEdit(product)}>
                              <Pencil className="size-4" /> Editar
                            </DropdownMenuItem>
                            <DropdownMenuItem
                              variant="destructive"
                              onSelect={() => setDeleteTarget(product)}
                            >
                              <Trash2 className="size-4" /> Eliminar
                            </DropdownMenuItem>
                          </DropdownMenuContent>
                        </DropdownMenu>
                      </div>
                    )}
                  </div>

                  <div className="space-y-3 p-4">
                    <div className="space-y-1">
                      <p className="text-xs tracking-wide text-muted-foreground uppercase">
                        {product.category || 'Sin categoría'}
                      </p>
                      <h3 className="line-clamp-2 leading-snug font-medium">
                        {product.name || 'Sin nombre'}
                      </h3>
                    </div>
                    <p className="font-mono text-xs text-muted-foreground">
                      {product.sku || 'N/A'}
                    </p>
                    <div className="flex items-center justify-between gap-2">
                      <span className="num text-lg font-semibold">{formatCLP(safePrice)}</span>
                      <StockBadge stock={safeStock} minimumStock={safeMinStock} />
                    </div>
                  </div>

                  <div className="p-4 pt-0">
                    <Button size="sm" className="w-full" onClick={() => addToCart(product, 1)}>
                      <ShoppingCart className="size-4" /> Agregar
                    </Button>
                  </div>
                </Card>
              );
            })}
          </div>
        )}

        {/* Formulario */}
        <Dialog
          open={showForm}
          onOpenChange={(open) => {
            if (!open) closeForm();
          }}
        >
          <DialogContent className="max-w-2xl max-sm:h-dvh max-sm:max-w-none max-sm:rounded-none">
            <DialogHeader>
              <DialogTitle>{editingProduct ? 'Editar producto' : 'Nuevo producto'}</DialogTitle>
            </DialogHeader>

            <form onSubmit={handleSubmit} className="grid gap-4">
              <div className="grid gap-4 sm:grid-cols-2">
                <div className="grid gap-2">
                  <Label htmlFor="p-name">
                    Nombre <span className="text-destructive">*</span>
                  </Label>
                  <Input
                    id="p-name"
                    value={formData.name}
                    onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                    aria-invalid={Boolean(formError)}
                    aria-describedby="product-form-error"
                  />
                </div>

                <div className="grid gap-2">
                  <Label htmlFor="p-sku">
                    SKU <span className="text-destructive">*</span>
                  </Label>
                  <Input
                    id="p-sku"
                    value={formData.sku}
                    onChange={(e) =>
                      setFormData({ ...formData, sku: e.target.value.toUpperCase() })
                    }
                    aria-invalid={Boolean(formError)}
                    aria-describedby="product-form-error"
                  />
                </div>

                <div className="grid gap-2">
                  <Label htmlFor="p-price">
                    Precio (CLP) <span className="text-destructive">*</span>
                  </Label>
                  <Input
                    id="p-price"
                    type="number"
                    min="0"
                    value={formData.price}
                    onChange={(e) => setFormData({ ...formData, price: e.target.value })}
                    aria-invalid={Boolean(formError)}
                    aria-describedby="product-form-error"
                  />
                </div>

                <div className="grid gap-2">
                  <Label htmlFor="p-stock">Stock</Label>
                  <Input
                    id="p-stock"
                    type="number"
                    min="0"
                    value={formData.stock}
                    onChange={(e) => setFormData({ ...formData, stock: e.target.value })}
                  />
                </div>

                <div className="grid gap-2">
                  <Label htmlFor="p-min">Stock mínimo</Label>
                  <Input
                    id="p-min"
                    type="number"
                    min="0"
                    value={formData.minimumStock}
                    onChange={(e) => setFormData({ ...formData, minimumStock: e.target.value })}
                  />
                </div>

                <div className="grid gap-2">
                  <Label htmlFor="p-category">Categoría</Label>
                  <Input
                    id="p-category"
                    value={formData.category}
                    onChange={(e) => setFormData({ ...formData, category: e.target.value })}
                  />
                </div>

                <div className="grid gap-2 sm:col-span-2">
                  <Label htmlFor="p-image">URL de imagen (opcional)</Label>
                  <Input
                    id="p-image"
                    value={formData.imageUrl}
                    onChange={(e) => setFormData({ ...formData, imageUrl: e.target.value })}
                    placeholder="Se genera automáticamente si se deja vacío"
                  />
                </div>
              </div>

              <div className="grid gap-2">
                <Label htmlFor="p-desc">Descripción</Label>
                <Textarea
                  id="p-desc"
                  rows={3}
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                />
              </div>

              {formError && (
                <p id="product-form-error" role="alert" className="text-sm text-destructive">
                  {formError}
                </p>
              )}

              <DialogFooter>
                <Button type="button" variant="outline" onClick={closeForm}>
                  Cancelar
                </Button>
                <Button type="submit" disabled={saving}>
                  {editingProduct ? 'Guardar cambios' : 'Crear producto'}
                </Button>
              </DialogFooter>
            </form>
          </DialogContent>
        </Dialog>

        {/* Confirmación de borrado */}
        <AlertDialog
          open={Boolean(deleteTarget)}
          onOpenChange={(open) => {
            if (!open) setDeleteTarget(null);
          }}
        >
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>Eliminar producto</AlertDialogTitle>
              <AlertDialogDescription>
                ¿Seguro que quieres eliminar &quot;{deleteTarget?.name}&quot;? Esta acción no se
                puede deshacer.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel>Cancelar</AlertDialogCancel>
              <AlertDialogAction variant="destructive" onClick={confirmDelete}>
                Eliminar
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>
      </div>
    </div>
  );
}

export default Products;

