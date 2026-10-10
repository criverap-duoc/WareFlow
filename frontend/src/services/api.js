import axios from 'axios';
import { toast } from 'sonner';

// En Docker, VITE_API_URL="/api" (nginx hace proxy al backend). En local, backend de desarrollo.
const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5276/api';

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

// Interceptor de respuesta: manejo global de 401 (sesión expirada) y 429 (rate limit)
api.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error.response?.status;

    if (status === 401) {
      // Evitar el toast si ya estamos en la pantalla de login
      if (window.location.pathname !== '/login') {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        toast.error('Tu sesión expiró. Inicia sesión de nuevo.');
        window.location.href = '/login';
      }
    } else if (status === 429) {
      toast.error('Demasiadas peticiones. Espera un momento.');
    }

    return Promise.reject(error);
  }
);

// Servicio de autenticación
export const authService = {
  register: (userData) => api.post('/Auth/register', userData),
  login: (credentials) => api.post('/Auth/login', credentials),
  logout: () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  },
};

// Servicio de productos
export const productService = {
  getAll: () => api.get('/Products'),
  getById: (id) => api.get('/Products/' + id),
  create: (product) => api.post('/Products', product),
  update: (id, product) => api.put('/Products/' + id, product),
  delete: (id) => api.delete('/Products/' + id),
};

// Servicio de órdenes
export const orderService = {
  getAll: () => api.get('/Orders'),
  getById: (id) => api.get('/Orders/' + id),
  create: (order) => api.post('/Orders', order),
};

export default api;
