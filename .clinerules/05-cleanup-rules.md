# Cleanup Rules — WareFlow

## Principio: reportar, no arreglar sin aprobación

Antes de commitear, REPORTAR (no eliminar):
1. `using` no usados en archivos `.cs`
2. `import` no usados en archivos `.jsx`/`.js`
3. Código huérfano (clases/funciones reemplazadas pero no eliminadas)
4. Archivos muertos (ej: controladores duplicados, `.backup`, `.old`)
5. Debug leftovers: `console.log` en frontend, `Console.WriteLine` en backend

## Detección rápida

### Backend
```bash
# TODO / FIXME sin contexto
grep -rn "TODO\|FIXME" backend/ --include="*.cs"

# Warnings de build (incluye usings no usados)
dotnet build --nologo -v q 2>&1 | grep -i "warning"
```

### Frontend
```bash
# console.log
grep -rn "console\.log" frontend/src/

# Archivos de backup
find frontend/src -name "*.backup" -o -name "*.old" -o -name "*~"
```

## Código basura típico (eliminar antes de commit)

- `if (false) { ... }` — placeholder muerto
- `// TODO` sin autor ni fecha
- Código después de un `return`
- Variables declaradas y nunca usadas
- Comentarios tipo "provisional" o "temporal"
- `DebugController.cs` (era temporal, eliminar cuando ya no se use)
- `Program.cs.backup` (era backup, eliminar)

## Verificación mínima antes de commit (Nivel 3)

### Backend
```bash
dotnet build --nologo -v q
```
→ Debe terminar con 0 errores. Warnings se reportan pero no bloquean.

### Frontend
- Verificar manualmente en navegador que el flujo funciona:
  1. Login
  2. Listado de productos
  3. Agregar al carrito
  4. Checkout
  5. Ver orden en /orders
- No hay tests automatizados de frontend (etapa actual)

### Git
```bash
git status --short
git diff --cached --stat
```

## "cleanup pass" — cuando el usuario lo pida

```bash
git diff --name-only
```

Para cada archivo:
1. Backend: `dotnet build --nologo -v q | grep warning`
2. Frontend: `grep -n "console.log" <archivo>`
3. Buscar archivos con `backup`, `old`, `copy` en el nombre

Reportar como tabla: `Archivo | Problema | Acción sugerida`.
No aplicar cambios sin aprobación.

## Reglas anti-indentación rota

WareFlow es menos sensible que CRM (Django), pero igual hay riesgo en:
- `Program.cs` (bloques anidados de configuración)
- `ApplicationDbContext.cs` (configuración fluent)
- `Products.jsx` / `Cart.jsx` (JSX anidado)

Señal de alarma: `dotnet build` falla con error de sintaxis tras un edit.
Acción: revertir con `git checkout <archivo>` y rehacer el edit con el
bloque completo (no una línea suelta).

## Reglas anti-desperdicio de tokens

1. **No ejecutar `dotnet test` tras cada edit.** Solo Nivel 2/3.
2. **No rebuildear la solución completa tras un edit menor.**
3. **No pegar output completo de build.** Solo errores/warnings.
4. **No ejecutar seed SQL salvo que se pida.**
5. **Máximo 2 edits por archivo por turno.**

## Package manager
- Frontend: pnpm exclusivamente. Reportar si aparece `package-lock.json` o `yarn.lock`.
- Si un comando sugiere `npm install`, reemplazar por `pnpm install`.