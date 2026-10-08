// Imagen por defecto según el nombre/categoría del producto.
// Única fuente de verdad, compartida por catálogo, carrito y contexto.
export const getDefaultImage = (productName, category) => {
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
