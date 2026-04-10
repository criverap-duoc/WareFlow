import axios from 'axios';

const API_URL = 'http://localhost:5276/api';

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
