import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { Suspense, lazy } from 'react';
import { AuthProvider, useAuth } from './context/AuthContext';
import { CartProvider } from './context/CartContext';
import Login from './components/Login';
import Signup from './components/Signup';
import GamesStore from './pages/GamesStore';
import GameDetails from './pages/GameDetails';
import Cart from './pages/Cart';
import CheckoutSuccess from './pages/CheckoutSuccess';
import Library from './pages/Library';
import Subscriptions from './pages/Subscriptions';
import Profile from './pages/Profile';
import AdminRoute from './components/AdminRoute';
import './App.css';

// Lazy load admin components
const AdminLogin = lazy(() => import('./pages/admin/AdminLogin'));
const AdminLayout = lazy(() => import('./layouts/AdminLayout'));
const AdminDashboard = lazy(() => import('./pages/admin/AdminDashboard'));
const AdminGames = lazy(() => import('./pages/admin/AdminGames'));
const AdminGameForm = lazy(() => import('./pages/admin/AdminGameForm'));
const AdminUsers = lazy(() => import('./pages/admin/AdminUsers'));
const AdminSubscriptions = lazy(() => import('./pages/admin/AdminSubscriptions'));
const AdminSubscriptionForm = lazy(() => import('./pages/admin/AdminSubscriptionForm'));
const AdminSubscriptionDetail = lazy(() => import('./pages/admin/AdminSubscriptionDetail'));

// Loading fallback component
function AdminLoadingFallback() {
  return (
    <div style={{
      display: 'flex',
      justifyContent: 'center',
      alignItems: 'center',
      height: '100vh',
      fontSize: '18px',
      color: '#667eea'
    }}>
      Loading Admin Panel...
    </div>
  );
}

function ProtectedRoute({ children }) {
  const { isAuthenticated, loading } = useAuth();

  if (loading) {
    return (
      <div style={{
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        height: '100vh',
        fontSize: '18px',
        color: '#667eea'
      }}>
        Loading...
      </div>
    );
  }

  return isAuthenticated() ? children : <Navigate to="/login" />;
}

function AppContent() {
  const { isAuthenticated, loading } = useAuth();

  if (loading) {
    return (
      <div style={{
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        height: '100vh',
        fontSize: '18px',
        color: '#667eea'
      }}>
        Loading...
      </div>
    );
  }

  return (
    <Routes>
      <Route
        path="/login"
        element={isAuthenticated() ? <Navigate to="/store" /> : <Login />}
      />
      <Route
        path="/signup"
        element={isAuthenticated() ? <Navigate to="/store" /> : <Signup />}
      />
      <Route
        path="/"
        element={<Navigate to={isAuthenticated() ? "/store" : "/login"} />}
      />
      <Route
        path="/store"
        element={
          <ProtectedRoute>
            <GamesStore />
          </ProtectedRoute>
        }
      />
      <Route
        path="/game/:gameId"
        element={
          <ProtectedRoute>
            <GameDetails />
          </ProtectedRoute>
        }
      />
      <Route
        path="/cart"
        element={
          <ProtectedRoute>
            <Cart />
          </ProtectedRoute>
        }
      />
      <Route
        path="/checkout-success"
        element={
          <ProtectedRoute>
            <CheckoutSuccess />
          </ProtectedRoute>
        }
      />
      <Route
        path="/library"
        element={
          <ProtectedRoute>
            <Library />
          </ProtectedRoute>
        }
      />
      <Route
        path="/subscriptions"
        element={
          <ProtectedRoute>
            <Subscriptions />
          </ProtectedRoute>
        }
      />
      <Route
        path="/profile"
        element={
          <ProtectedRoute>
            <Profile />
          </ProtectedRoute>
        }
      />
      <Route path="/admin/login" element={
        <Suspense fallback={<AdminLoadingFallback />}>
          <AdminLogin />
        </Suspense>
      } />
      <Route path="/admin" element={
        <AdminRoute>
          <Suspense fallback={<AdminLoadingFallback />}>
            <AdminLayout />
          </Suspense>
        </AdminRoute>
      }>
        <Route path="dashboard" element={
          <Suspense fallback={<AdminLoadingFallback />}>
            <AdminDashboard />
          </Suspense>
        } />
        <Route path="games" element={
          <Suspense fallback={<AdminLoadingFallback />}>
            <AdminGames />
          </Suspense>
        } />
        <Route path="games/new" element={
          <Suspense fallback={<AdminLoadingFallback />}>
            <AdminGameForm />
          </Suspense>
        } />
        <Route path="games/edit/:id" element={
          <Suspense fallback={<AdminLoadingFallback />}>
            <AdminGameForm />
          </Suspense>
        } />
        <Route path="users" element={
          <Suspense fallback={<AdminLoadingFallback />}>
            <AdminUsers />
          </Suspense>
        } />
        <Route path="subscriptions" element={
          <Suspense fallback={<AdminLoadingFallback />}>
            <AdminSubscriptions />
          </Suspense>
        } />
        <Route path="subscriptions/new" element={
          <Suspense fallback={<AdminLoadingFallback />}>
            <AdminSubscriptionForm />
          </Suspense>
        } />
        <Route path="subscriptions/edit/:id" element={
          <Suspense fallback={<AdminLoadingFallback />}>
            <AdminSubscriptionForm />
          </Suspense>
        } />
        <Route path="subscriptions/:id" element={
          <Suspense fallback={<AdminLoadingFallback />}>
            <AdminSubscriptionDetail />
          </Suspense>
        } />
      </Route>

      <Route path="*" element={<Navigate to="/" />} />
    </Routes>
  );
}

function App() {
  return (
    <AuthProvider>
      <CartProvider>
        <BrowserRouter>
          <AppContent />
        </BrowserRouter>
      </CartProvider>
    </AuthProvider>
  );
}

export default App;
