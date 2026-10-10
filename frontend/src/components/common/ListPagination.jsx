import { ChevronLeft, ChevronRight } from 'lucide-react';
import { cn } from 'cn';
import {
  Pagination,
  PaginationContent,
  PaginationEllipsis,
  PaginationItem,
  PaginationLink,
} from '../ui/pagination';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '../ui/select';

const PAGE_SIZE_OPTIONS = [10, 20, 50];

/**
 * Páginas visibles alrededor de la actual. Usa 'ellipsis' como marcador cuando
 * el rango es más amplio que el máximo de botones.
 */
const buildVisiblePages = (page, totalPages) => {
  const maxButtons = 5;

  if (totalPages <= maxButtons) {
    return Array.from({ length: totalPages }, (_, i) => i + 1);
  }

  let start = Math.max(1, page - Math.floor(maxButtons / 2));
  const end = Math.min(totalPages, start + maxButtons - 1);
  start = Math.max(1, end - maxButtons + 1);

  const pages = Array.from({ length: end - start + 1 }, (_, i) => start + i);

  if (start > 1) pages.unshift('ellipsis');
  if (end < totalPages) pages.push('ellipsis');

  return pages;
};

/**
 * Paginador de listados con paginación server-side. Muestra el rango visible,
 * el selector de tamaño de página y los controles del componente Pagination
 * de shadcn/ui.
 */
export function ListPagination({
  page,
  pageSize,
  totalItems,
  totalPages,
  onPageChange,
  onPageSizeChange,
  pageSizeOptions = PAGE_SIZE_OPTIONS,
  itemLabel = 'resultados',
}) {
  if (totalItems === 0) return null;

  const from = (page - 1) * pageSize + 1;
  const to = Math.min(page * pageSize, totalItems);
  const visiblePages = buildVisiblePages(page, totalPages);

  const handleNavigate = (event, target) => {
    event.preventDefault();
    if (target < 1 || target > totalPages || target === page) return;
    onPageChange(target);
  };

  const linkProps = (target, disabled) => ({
    href: '#',
    'aria-disabled': disabled,
    className: cn(disabled && 'pointer-events-none opacity-50'),
    onClick: (event) => handleNavigate(event, target),
  });

  return (
    <div className="flex flex-col gap-3">
      <div className="flex flex-col items-center justify-between gap-3 sm:flex-row">
        <p className="num text-sm text-muted-foreground">
          Mostrando {from}–{to} de {totalItems} {itemLabel}
        </p>

        {onPageSizeChange && (
          <div className="flex items-center gap-2">
            <span className="text-sm text-muted-foreground">Por página</span>
            <Select
              value={String(pageSize)}
              onValueChange={(value) => onPageSizeChange(Number(value))}
            >
              <SelectTrigger className="w-20" aria-label="Resultados por página">
                <SelectValue placeholder={String(pageSize)} />
              </SelectTrigger>
              <SelectContent>
                {pageSizeOptions.map((size) => (
                  <SelectItem key={size} value={String(size)}>
                    {size}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        )}
      </div>

      {totalPages > 1 && (
        <Pagination>
          <PaginationContent>
            <PaginationItem>
              <PaginationLink
                size="default"
                aria-label="Página anterior"
                {...linkProps(page - 1, page <= 1)}
              >
                <ChevronLeft />
                <span className="hidden sm:block">Anterior</span>
              </PaginationLink>
            </PaginationItem>

            {visiblePages.map((entry, index) =>
              entry === 'ellipsis' ? (
                <PaginationItem key={`ellipsis-${index}`}>
                  <PaginationEllipsis />
                </PaginationItem>
              ) : (
                <PaginationItem key={entry}>
                  <PaginationLink
                    isActive={entry === page}
                    aria-label={`Ir a la página ${entry}`}
                    {...linkProps(entry, false)}
                  >
                    {entry}
                  </PaginationLink>
                </PaginationItem>
              )
            )}

            <PaginationItem>
              <PaginationLink
                size="default"
                aria-label="Página siguiente"
                {...linkProps(page + 1, page >= totalPages)}
              >
                <span className="hidden sm:block">Siguiente</span>
                <ChevronRight />
              </PaginationLink>
            </PaginationItem>
          </PaginationContent>
        </Pagination>
      )}
    </div>
  );
}

export default ListPagination;
