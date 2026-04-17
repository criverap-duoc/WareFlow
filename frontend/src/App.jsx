import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import { CartProvider } from './context/CartContext';
import Navbar from './components/Navbar';
import Login from './pages/Login';
import Products from './pages/Products';
import Cart from './pages/Cart';
import Orders from './pages/Orders';

function App() {
  return (
    <AuthProvider>
      <CartProvider>
        <BrowserRouter>
          <MainLayout />
        </BrowserRouter>
      </CartProvider>
    </AuthProvider>
  );
}

function MainLayout() {
  const { isAuthenticated } = useAuth();

  return (
    <>
      <Navbar />
      <AppRoutes />
    </>
  );
}

function AppRoutes() {
  const { isAuthenticated } = useAuth();

  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route
        path="/"
        element={isAuthenticated ? <Navigate to="/products" /> : <Navigate to="/login" />}
      />
      <Route
        path="/products"
        element={isAuthenticated ? <Products /> : <Navigate to="/login" />}
      />
      <Route
        path="/cart"
        element={isAuthenticated ? <Cart /> : <Navigate to="/login" />}
      />
      <Route
        path="/orders"
        element={isAuthenticated ? <Orders /> : <Navigate to="/login" />}
      />
    </Routes>
  );
}

export default App;
