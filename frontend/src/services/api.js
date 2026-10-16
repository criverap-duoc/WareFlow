import axios from 'axios';
import { toast } from 'sonner';

// En Docker, VITE_API_URL="/api/v1" (nginx hace proxy al backend). En local, backend de desarrollo.
const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5276/api/v1';

const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor para agregar el token a todas las peticiones
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = 'Bearer ' + token;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Interceptor de respuesta: 401 → intenta refrescar el access token una sola vez;
// 429 → aviso de rate limit.
let isRefreshing = false;
let failedQueue = [];

const processQueue = (error, token = null) => {
  failedQueue.forEach((prom) => {
    if (error) prom.reject(error);
    else prom.resolve(token);
  });
  failedQueue = [];
};

// Limpia sólo las claves de sesión (el carrito se conserva).
const clearSession = () => {
  localStorage.removeItem('token');
  localStorage.removeItem('refreshToken');
  localStorage.removeItem('user');
};

const forceLogin = () => {
  clearSession();
  if (window.location.pathname !== '/login') {
    window.location.href = '/login';
  }
};

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    const status = error.response?.status;

    // Si el access token expiró y no es un endpoint de auth, intentar refrescar una vez.
    if (
      status === 401 &&
      originalRequest &&
      !originalRequest._retry &&
      !originalRequest.url?.includes('/Auth/')
    ) {
      if (isRefreshing) {
        // Ya hay un refresh en curso: encolar y reintentar cuando termine.
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        })
          .then((token) => {
            originalRequest.headers.Authorization = `Bearer ${token}`;
            return api(originalRequest);
          })
          .catch((err) => Promise.reject(err));
      }

      originalRequest._retry = true;
      isRefreshing = true;

      const refreshToken = localStorage.getItem('refreshToken');
      if (!refreshToken) {
        isRefreshing = false;
        forceLogin();
        return Promise.reject(error);
      }

      try {
        // axios "crudo" para no reingresar al interceptor.
        const response = await axios.post(
          `${API_URL}/Auth/refresh`,
          { refreshToken },
          { headers: { 'Content-Type': 'application/json' } }
        );

        const { accessToken: newAccessToken, refreshToken: newRefreshToken } = response.data;
        localStorage.setItem('token', newAccessToken);
        localStorage.setItem('refreshToken', newRefreshToken);

        api.defaults.headers.common.Authorization = `Bearer ${newAccessToken}`;
        originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
        processQueue(null, newAccessToken);

        return api(originalRequest);
      } catch (refreshError) {
        processQueue(refreshError, null);
        forceLogin();
        toast.error('Tu sesión expiró. Inicia sesión de nuevo.');
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    if (status === 429) {
      toast.error('Demasiadas peticiones. Espera un momento.');
    }

    return Promise.reject(error);
  }
);

// Servicio de autenticación
export const authService = {
  register: (userData) => api.post('/Auth/register', userData),
  login: (credentials) => api.post('/Auth/login', credentials),
  // Rota el refresh token y devuelve un access token nuevo
  refresh: (refreshToken) => api.post('/Auth/refresh', { refreshToken }),
  // Revoca los refresh tokens del usuario en el backend
  logout: () => api.post('/Auth/logout'),
  // Limpia la sesión local (sin llamar al backend)
  clearSession,
};

// Servicio de productos
// getAll acepta params opcionales de paginación y filtros:
// { page, pageSize, search, category, stockFilter, sortBy, sortOrder, minPrice, maxPrice }
export const productService = {
  getAll: (params) => api.get('/Products', { params }),
  getById: (id) => api.get('/Products/' + id),
  create: (product) => api.post('/Products', product),
  update: (id, product) => api.put('/Products/' + id, product),
  delete: (id) => api.delete('/Products/' + id),
  // Predicción de demanda (Fase IA.1): ventas diarias promedio de los últimos
  // 30 días, días hasta el stock mínimo y cantidad recomendada de reabastecimiento.
  getDemandForecast: (id) => api.get(`/Products/${id}/demand-forecast`),
  getTopDemandForecasts: (limit = 5) =>
    api.get('/Products/demand-forecast/top', { params: { limit } }),
};

// Servicio de órdenes
// getAll acepta params opcionales de paginación: { page, pageSize }
export const orderService = {
  getAll: (params) => api.get('/Orders', { params }),
  getById: (id) => api.get('/Orders/' + id),
  create: (order) => api.post('/Orders', order),
};

export default api;
