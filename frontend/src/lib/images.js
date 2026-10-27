// Imagen por defecto según el nombre/categoría del producto.
// Única fuente de verdad, compartida por catálogo, carrito y contexto.
//
// La CATEGORÍA manda: detectar por marcas o palabras del nombre genera falsos
// positivos ("Corsair HS60" → RAM, "Samsung Galaxy S24" → monitor, "Teclado
// Logitech" → mouse). El nombre solo se usa dentro de "Componentes" (para
// separar GPU/CPU/SSD/RAM/fuente) y como respaldo si el producto no trae
// categoría.

const IMAGES = {
  laptop: 'https://images.unsplash.com/photo-1541807084-5c52b6b3adef?w=300',
  monitor: 'https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?w=300',
  keyboard: 'https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=300',
  mouse: 'https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?w=300',
  headset: 'https://images.unsplash.com/photo-1583394838336-acd977736f90?w=300',
  // Pexels para CPU/GPU: las fotos de Unsplash fallan más seguido.
  gpu: 'https://images.pexels.com/photos/2582937/pexels-photo-2582937.jpeg?w=300',
  cpu: 'https://images.pexels.com/photos/2582936/pexels-photo-2582936.jpeg?w=300',
  ssd: 'https://images.unsplash.com/photo-1597878400966-7c9d0e8a7b8a?w=300',
  ram: 'https://images.unsplash.com/photo-1562976540-1502c2145186?w=300',
  psu: 'https://images.unsplash.com/photo-1587202372775-229f172a6d8b?w=300',
  smartphone: 'https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=300',
  tablet: 'https://images.unsplash.com/photo-1561154464-82e9adf32764?w=300',
  // Circuito/placa neutra: se usa para componentes no identificados y como
  // fallback genérico (antes era una zapatilla, que aparecía en SSDs y demás).
  component: 'https://images.unsplash.com/photo-1591488322449-0a4a5f1c7b5a?w=300'
};

// Minúsculas y sin tildes: "Audífonos" → "audifonos", "Teléfonos" → "telefonos".
const normalize = (value) =>
  (value || '')
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .trim();

const hasAny = (text, terms) => terms.some((term) => text.includes(term));

// Solo se invoca cuando la categoría es de componentes: aquí sí manda el nombre.
const detectComponentImage = (name) => {
  // El texto se acota con espacios para poder buscar palabras cortas (" rx ").
  const text = ` ${name} `;

  if (hasAny(text, ['gpu', 'rtx', 'gtx', 'nvidia', 'radeon', ' rx ', 'tarjeta', 'grafica']))
    return IMAGES.gpu;
  if (hasAny(text, ['cpu', 'procesador', 'intel', 'ryzen', 'core i']))
    return IMAGES.cpu;
  if (hasAny(text, ['ssd', 'disco', 'almacenamiento', 'nvme']))
    return IMAGES.ssd;
  if (hasAny(text, ['ram', 'memoria']))
    return IMAGES.ram;
  if (hasAny(text, ['fuente', 'psu', 'power supply']))
    return IMAGES.psu;

  return IMAGES.component;
};

export const getDefaultImage = (productName, category) => {
  const name = normalize(productName);
  const cat = normalize(category);

  // PRIORIDAD 1: categoría (más confiable que el nombre).
  if (hasAny(cat, ['laptop', 'notebook'])) return IMAGES.laptop;
  if (hasAny(cat, ['monitor'])) return IMAGES.monitor;
  if (hasAny(cat, ['teclado'])) return IMAGES.keyboard;
  if (hasAny(cat, ['mouse', 'raton'])) return IMAGES.mouse;
  if (hasAny(cat, ['audifono', 'auricular', 'audio'])) return IMAGES.headset;
  if (hasAny(cat, ['smartphone', 'telefono', 'celular'])) return IMAGES.smartphone;
  if (hasAny(cat, ['tablet'])) return IMAGES.tablet;

  // Componentes: la categoría solo dice "es un componente", el nombre decide.
  if (hasAny(cat, ['ssd', 'almacenamiento', 'disco'])) return IMAGES.ssd;
  if (hasAny(cat, ['ram', 'memoria'])) return IMAGES.ram;
  if (hasAny(cat, ['cpu', 'procesador'])) return IMAGES.cpu;
  if (hasAny(cat, ['gpu', 'tarjeta grafica'])) return IMAGES.gpu;
  if (hasAny(cat, ['componente', 'hardware'])) return detectComponentImage(name);

  // PRIORIDAD 2: sin categoría reconocida, se intenta por nombre.
  if (hasAny(name, ['laptop', 'notebook', 'macbook'])) return IMAGES.laptop;
  if (hasAny(name, ['monitor', 'pantalla'])) return IMAGES.monitor;
  if (hasAny(name, ['teclado', 'keyboard'])) return IMAGES.keyboard;
  if (hasAny(name, ['mouse', 'raton'])) return IMAGES.mouse;
  if (hasAny(name, ['audifono', 'auricular', 'headset', 'airpods'])) return IMAGES.headset;
  if (hasAny(name, ['smartphone', 'telefono', 'celular', 'iphone', 'galaxy', 'pixel', 'xiaomi']))
    return IMAGES.smartphone;
  if (hasAny(name, ['tablet', 'ipad'])) return IMAGES.tablet;
  if (hasAny(name, ['gpu', 'cpu', 'ssd', 'ram', 'fuente', 'psu', 'rtx', 'ryzen', 'nvidia', 'procesador']))
    return detectComponentImage(name);

  return IMAGES.component;
};

