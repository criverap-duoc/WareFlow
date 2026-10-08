import { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { Boxes, LogOut, Menu, ShoppingCart } from 'lucide-react';
import { cn } from 'cn';
import { useAuth } from '../context/AuthContext';
import { useCart } from '../context/CartContext';
import { NAV_ITEMS } from '../lib/constants';
import { Container } from './layout/Container';
import { Button } from './ui/button';
import { Avatar, AvatarFallback } from './ui/avatar';
import {
  DropdownMenu,
  DropdownMenuTrigger,
  DropdownMenuContent,
  DropdownMenuLabel,
  DropdownMenuItem,
} from './ui/dropdown-menu';
import { Separator } from './ui/separator';
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetTrigger } from './ui/sheet';
import { ThemeToggle } from './ThemeToggle';

function Navbar() {
  const location = useLocation();
  const { logout, isAuthenticated, user } = useAuth();
  const { totalItems } = useCart();
  const [mobileOpen, setMobileOpen] = useState(false);

  if (!isAuthenticated) return null;

  // Si el usuario no tiene rol (fase 1), se muestran todos los links.
  const items = user?.role
    ? NAV_ITEMS.filter((item) => item.roles.includes(user.role))
    : NAV_ITEMS;

  const fullName =
    [user?.firstName, user?.lastName].filter(Boolean).join(' ') || user?.email || 'Usuario';
  const initials = (
    `${user?.firstName?.[0] ?? ''}${user?.lastName?.[0] ?? ''}` ||
    user?.email?.[0] ||
    'U'
  ).toUpperCase();

  return (
    <header className="sticky top-0 z-50 h-14 border-b bg-background/80 backdrop-blur">
      <Container className="flex h-full items-center justify-between gap-4">
        {/* Logo */}
        <Link to="/products" className="flex items-center gap-2 font-semibold tracking-tight">
          <Boxes className="size-5 text-primary" />
          <span className="hidden text-base sm:inline">WareFlow</span>
        </Link>

        {/* Links (desktop) */}
        <nav className="hidden flex-1 items-center gap-1 sm:flex">
          {items.map((item) => {
            const Icon = item.icon;
            const active = location.pathname === item.to;
            return (
              <Link
                key={item.to}
                to={item.to}
                className={cn(
                  'relative flex h-14 items-center gap-2 px-3 text-sm font-medium transition-colors hover:text-foreground',
                  active ? 'text-foreground' : 'text-muted-foreground'
                )}
              >
                <Icon className="size-4" />
                {item.label}
                {active && (
                  <span className="absolute inset-x-3 bottom-0 h-0.5 rounded-full bg-primary" />
                )}
              </Link>
            );
          })}
        </nav>

        {/* Acciones */}
        <div className="flex items-center gap-1">
          <Button variant="ghost" size="icon" asChild className="relative">
            <Link to="/cart" aria-label="Carrito" title="Carrito">
              <ShoppingCart className="size-5" />
              {totalItems > 0 && (
                <span className="num absolute -top-1 -right-1 flex h-4 min-w-4 items-center justify-center rounded-full bg-primary px-1 text-[10px] font-medium text-primary-foreground">
                  {totalItems}
                </span>
              )}
            </Link>
          </Button>

          <ThemeToggle />

          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <button
                type="button"
                aria-label="Menú de usuario"
                title="Menú de usuario"
                className="rounded-full outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background"
              >
                <Avatar className="size-8">
                  <AvatarFallback className="bg-primary/10 text-xs font-medium text-primary">
                    {initials}
                  </AvatarFallback>
                </Avatar>
              </button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="w-56">
              <DropdownMenuLabel className="flex flex-col gap-0.5">
                <span className="truncate text-sm font-medium">{fullName}</span>
                {user?.email && (
                  <span className="truncate text-xs font-normal text-muted-foreground">
                    {user.email}
                  </span>
                )}
                {user?.role && (
                  <span className="mt-1 w-fit rounded bg-muted px-1.5 py-0.5 text-[10px] font-medium text-muted-foreground">
                    {user.role}
                  </span>
                )}
              </DropdownMenuLabel>
              <Separator className="my-1" />
              <DropdownMenuItem onSelect={() => logout()}>
                <LogOut className="size-4" />
                Cerrar sesión
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>

          <Sheet open={mobileOpen} onOpenChange={setMobileOpen}>
            <SheetTrigger asChild>
              <Button
                variant="ghost"
                size="icon"
                className="sm:hidden"
                aria-label="Abrir menú"
                title="Abrir menú"
              >
                <Menu className="size-5" />
              </Button>
            </SheetTrigger>
            <SheetContent side="right" className="w-64">
              <SheetHeader>
                <SheetTitle>Menú</SheetTitle>
              </SheetHeader>
              <nav className="flex flex-col gap-1 px-4">
                {items.map((item) => {
                  const Icon = item.icon;
                  const active = location.pathname === item.to;
                  return (
                    <Link
                      key={item.to}
                      to={item.to}
                      onClick={() => setMobileOpen(false)}
                      className={cn(
                        'flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors',
                        active
                          ? 'bg-primary/10 text-primary'
                          : 'text-muted-foreground hover:bg-muted hover:text-foreground'
                      )}
                    >
                      <Icon className="size-4" />
                      {item.label}
                    </Link>
                  );
                })}
              </nav>
            </SheetContent>
          </Sheet>
        </div>
      </Container>
    </header>
  );
}

export default Navbar;
