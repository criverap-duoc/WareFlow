# ADR 004 — Tailwind CSS 4 + shadcn/ui para el frontend

- **Fecha:** 2026-10-10
- **Estado:** Aceptada
- **Decisores:** Cristóbal Vera (desarrollo completo)

## Contexto

El frontend (React 19 + Vite 8, sin TypeScript) necesitaba una capa visual
consistente para un dashboard con tablas, KPIs, formularios en diálogos,
dropdowns y notificaciones, sin un equipo de diseño que mantenga un sistema de
componentes propio.

Los criterios fueron: velocidad de iteración, control del resultado (nada de
look genérico), accesibilidad razonable y tamaño del bundle.

## Decisión

Combinar **Tailwind CSS 4** (utilidades + tokens CSS con `@theme inline` en
`frontend/src/index.css`) con **shadcn/ui** (componentes copiados al repositorio
en `components/ui/`, no una dependencia de runtime).

Lineamientos derivados, documentados en las reglas del proyecto:

- Paleta teal + neutrales expuesta como tokens semánticos (`--primary`,
  `--success`, `--warning`, `--danger`, `--muted`) y uso con modificadores
  (`bg-success/10 text-success border-success/20`), nunca colores hardcodeados.
- Iconos con **lucide-react** (nunca emojis) y avisos con **sonner**.
- Tipografía Geist Sans / Geist Mono, con `tabular-nums` (clase `.num`) para
  precios, SKU y cifras.
- Radios `rounded-lg`, `max-w-7xl` como contenedor y espaciado en múltiplos de 4.

## Consecuencias

**Positivas**
- El markup de las páginas es legible y el diseño es consistente sin CSS propio
  (se eliminó la necesidad de archivos `.css` por componente).
- Los componentes de shadcn/ui viven en el repo: se editan libremente, sin
  esperar releases ni pelear con el theming de una librería externa.
- Accesibilidad más fácil: Radix (base de shadcn/ui) resuelve focus trap y
  roles ARIA en dialogs, dropdowns y sheets.
- El modo oscuro sale casi gratis: sólo se redefinen los tokens.

**Negativas / costos**
- Muchas clases utilitarias en el JSX (mitigado con componentes y constantes
  compartidas como `TONE_CLASSES`).
- No hay `node_modules` de UI que actualizar: el mantenimiento es manual.
- Tailwind 4 cambió la configuración respecto a v3 (CSS-first, `@theme inline`),
  lo que invalida parte de la documentación y ejemplos encontrados en internet.
- Curva de aprendizaje de las clases utilitarias para quien lea el código.

## Alternativas consideradas

- **CSS Modules / CSS plano:** control total, pero cada componente exige su
  archivo de estilos y no hay sistema de diseño compartido: inconsistencia y
  más código que mantener.
- **styled-components (CSS-in-JS):** ergonomía similar a Tailwind, pero añade
  costo en runtime, complica el SSR y su ecosistema está en declive frente a
  las utilidades atómicas.
- **Material UI (MUI):** componentes completos y accesibles, pero impone su
  lenguaje visual (look "Material" genérico) y es difícil de personalizar sin
  overrides agresivos.
