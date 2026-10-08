# UI/UX Directives — WareFlow

Aplican a cualquier componente React del frontend.

## Stack visual (obligatorio)

- **Tailwind CSS 4** con `@theme inline` para tokens
- **shadcn/ui** para componentes base (Button, Card, Input, Dialog, Sheet, Badge, Table, etc.)
- **lucide-react** para iconos (NUNCA emojis en UI)
- **sonner** para toasts
- **Geist Sans** para body y headings; **Geist Mono** para números, SKU, fechas, precios tabulares

## Sistema de diseño

### Paleta (teal + neutrales)
Definida en `frontend/src/index.css` con tokens CSS:
- `--primary` teal `#0F766E` (light) / `#2DD4BF` (dark)
- Neutrales: `--background`, `--foreground`, `--card`, `--muted`, `--border`, `--input`
- Semánticos: `--success` (verde), `--warning` (ámbar), `--danger` (rojo), `--info` (azul)

**Regla:** usar tokens semánticos con modificadores (`bg-success/10 text-success border-success/20`), no colores hardcodeados.

### Tipografía
- Título de página: `text-2xl font-semibold tracking-tight`
- Título de card/sección: `text-base font-semibold`
- Body: `text-sm`
- Meta/SKU/labels: `text-xs text-muted-foreground`
- Cifras y precios: `num font-semibold` (usa `.num` para `tabular-nums`); SKU y números de orden en `font-mono`

### Espaciado
- Solo múltiplos de 4 (`gap-2/3/4/6`, `p-4/6`)
- Contenedor: `max-w-7xl mx-auto px-4 sm:px-6`

### Radios y sombras
- Radios: `rounded-lg` (0.5rem) por defecto
- Cards: `rounded-lg border bg-card shadow-sm`
- Botones: variantes de shadcn (`default`, `secondary`, `ghost`, `destructive`, `outline`)

## Navbar — patrón único

- Barra superior sticky (`h-14 sticky top-0 border-b bg-background/80 backdrop-blur`)
- Logo (icono `Boxes` de lucide + "WareFlow") a la izquierda
- Links planos al centro-izquierda con subrayado de 2px en `primary` para el activo
- Carrito con contador `tabular-nums` a la derecha
- **Avatar con DropdownMenu** para usuario (nombre, rol, Cerrar sesión). NO botón rojo suelto
- Mobile: hamburguesa que abre `Sheet`
- Links filtrados por rol desde `NAV_ITEMS` (definido en `lib/constants.js`)

## Cards de producto

Orden visual (de mayor a menor peso):
1. Imagen `aspect-[4/3] object-cover` con `loading="lazy"` y fallback (icono `Package`)
2. Categoría (`text-xs` muted) + nombre (`font-medium line-clamp-2`)
3. SKU en `font-mono text-xs`
4. Precio (`text-lg font-semibold num`) + badge de stock
5. **Botón primario "Agregar" ancho completo**. Editar y Eliminar van en `DropdownMenu` `⋯` (solo Admin); Eliminar pasa por `AlertDialog`

NO usar: `SKU:`, `Precio:`, `Stock:` como labels en negrita apilados. La info se lee por jerarquía, no por etiquetas.

## Badge de stock

Usar `stockState(stock, min)` de `lib/constants.js`:
- `out` → "Sin stock" (danger)
- `low` → "Stock bajo (n)" (warning)
- `ok` → "n en stock" (success)

Siempre con punto de color + texto. NUNCA solo color.

## KPIs

- Card grande para Valor del inventario, cards chicas para el resto
- **Cada KPI es clickeable y aplica el filtro correspondiente**
- Si hay 0 alertas, mostrar "Todo en orden" con check, no un cero rojo

## Formularios

- `Dialog` en desktop; en mobile `max-sm:h-dvh max-sm:max-w-none max-sm:rounded-none`
- Labels descriptivos; errores bajo el campo con `aria-invalid` y `aria-describedby`
- Feedback con `sonner` al guardar

## Imágenes por defecto

- Fuente única: `frontend/src/lib/images.js` → `getDefaultImage(name, category)`
- Detección por prioridad: GPU → CPU → SSD → RAM → Laptop → Monitor → Teclado → Mouse → Audífonos → Smartphone → Tablet → fallback genérico
- Preferir Pexels sobre Unsplash para CPU/GPU (más confiable)

## Accesibilidad mínima

- Todo botón con texto visible o `aria-label`/`title`
- Imágenes con `alt` descriptivo
- `loading="lazy"` en imágenes de listados
- Focus visible en interactivos

## Anti-patrones (evitar)

- Emojis como iconos (usar lucide-react)
- Colores hardcodeados (usar tokens)
- Estilos inline nuevos (migrar a Tailwind/shadcn)
- Gradientes genéricos morados
- Animaciones sin propósito
- `!important`