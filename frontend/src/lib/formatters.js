const clp = new Intl.NumberFormat("es-CL", {
  style: "currency",
  currency: "CLP",
  maximumFractionDigits: 0,
});

export const formatCLP = (n) => clp.format(n ?? 0);

export const formatDate = (iso, withTime = false) => {
  if (!iso) return "";
  // Si el ISO no trae Z ni offset, asumirlo UTC
  const hasTimezone = /Z$|[+-]\d{2}:?\d{2}$/.test(iso);
  const safeIso = hasTimezone ? iso : `${iso}Z`;
  return new Intl.DateTimeFormat("es-CL", {
    timeZone: "America/Santiago",
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    ...(withTime && { hour: "2-digit", minute: "2-digit" }),
  }).format(new Date(safeIso));
};

// Los precios de la tienda se manejan con IVA incluido (19%).
export const calcIVA = (totalConIVA) => {
  const neto = Math.round(totalConIVA / 1.19);
  return { neto, iva: totalConIVA - neto, total: totalConIVA };
};
