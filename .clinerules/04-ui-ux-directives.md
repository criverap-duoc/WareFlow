# UI/UX Directives — WareFlow

Aplican a cualquier componente React del frontend.

## Principios generales
- Estilos inline con objeto `styles` (etapa actual — migrar a CSS modules
  cuando el proyecto crezca)
- Paleta: azul (#007bff) como primario, gris (#6c757d) secundario,
  verde (#28a745) éxito, rojo (#dc3545) peligro
- Tipografía: `system-ui, -apple-system, sans-serif`
- Sin dependencias de UI externas (no Tailwind, no shadcn) por ahora —
  el objetivo es demostrar CSS puro + React

## Componentes clave

### Navbar (componente `Navbar.jsx`)
- Barra superior sticky, fondo oscuro `#2c3e50`
- Links: Productos, Carrito (con badge), Mis Órdenes, Salir
- Logo clickeable → `/products`
- Badge del carrito: círculo rojo con número

### Tarjetas de producto
- Grid responsivo: `repeat(auto-fill, minmax(320px, 1fr))`
- Imagen 200px alto, `object-fit: cover`
- Título con `WebkitLineClamp: 2` (máx 2 líneas)
- Stock bajo (< stock mínimo) en rojo negrita
- Botones: Editar (amarillo), Eliminar (rojo), Agregar al carrito (azul)

### Formulario de producto
- Grid de 2-3 columnas con `repeat(auto-fit, minmax(200px, 1fr))`
- Labels descriptivos + placeholders con ejemplos
- Texto de ayuda debajo de campos complejos (SKU, stock mínimo)
- Validación client-side antes de submit

### Dashboard KPIs
- Grid `repeat(auto-fit, minmax(150px, 1fr))`
- Card con valor grande (24px, azul) + label pequeño (12px, gris)
- 6 KPIs: Total, Valor Inventario, Stock Bajo, Sin Stock, Categorías,
  Precio Promedio

### Carrito
- Layout 2 columnas: items (flex 2) + resumen (flex 1)
- Item: imagen 80px + detalles + cantidad + subtotal + eliminar
- Resumen sticky con subtotal, envío, total, botón checkout

### Órdenes
- Lista vertical de cards
- Status badge con color según estado
- Detalle expandible con tabla de items
- Total destacado en verde

## Imágenes por defecto (getDefaultImage)
- Detección por nombre/categoría con orden de prioridad:
  1. GPU (gpu, rtx, gtx, nvidia, amd, radeon, tarjeta)
  2. CPU (cpu, procesador, intel, ryzen, core i)
  3. SSD (ssd, disco, almacenamiento, wd black)
  4. RAM (ram, memoria, corsair, kingston)
  5. Laptop, Monitor, Teclado, Mouse, Audífonos, Smartphone, Tablet
  6. Fallback: imagen genérica
- Preferir Pexels sobre Unsplash para CPU/GPU (más confiable)
- Fallback con `onError` a imagen genérica

## Accesibilidad mínima
- Todo botón con texto visible o `title`/`aria-label`
- Imágenes con `alt` descriptivo
- `loading="lazy"` en imágenes de listados

## Anti-patrones (evitar)
- Gradientes morados genéricos
- Fuentes Roboto/Arial/Open Sans
- Animaciones aleatorias sin propósito
- Emojis en exceso en UI (ok en títulos de sección, no en datos)
- Estilos con `!important`