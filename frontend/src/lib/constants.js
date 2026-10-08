export const ORDER_STATUS = {
  Pending:    { label: "Pendiente",  tone: "warning" },
  Processing: { label: "En proceso", tone: "info" },
  Shipped:    { label: "Enviada",    tone: "info" },
  Delivered:  { label: "Entregada",  tone: "success" },
  Cancelled:  { label: "Cancelada",  tone: "danger" },
  Returned:   { label: "Devuelta",   tone: "muted" },
};

export const TONE_CLASSES = {
  success: "bg-success/10 text-success border-success/20",
  warning: "bg-warning/10 text-warning border-warning/20",
  danger:  "bg-danger/10  text-danger  border-danger/20",
  info:    "bg-info/10    text-info    border-info/20",
  muted:   "bg-muted text-muted-foreground border-border",
};

export const stockState = (stock, min) =>
  stock === 0 ? "out" : stock <= min ? "low" : "ok";

import { Package, ShoppingCart, ClipboardList } from "lucide-react";

export const NAV_ITEMS = [
  { to: "/products", label: "Productos", icon: Package, roles: ["Admin", "Vendedor", "Bodeguero"] },
  { to: "/cart", label: "Carrito", icon: ShoppingCart, roles: ["Admin", "Vendedor"] },
  { to: "/orders", label: "Mis órdenes", icon: ClipboardList, roles: ["Admin", "Vendedor", "Bodeguero"] },
];
