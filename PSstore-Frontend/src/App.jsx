import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
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
import AdminLogin from './pages/admin/AdminLogin';
import AdminLayout from './layouts/AdminLayout';
import AdminDashboard from './pages/admin/AdminDashboard';
import AdminGames from './pages/admin/AdminGames';
import AdminGameForm from './pages/admin/AdminGameForm';
import AdminUsers from './pages/admin/AdminUsers';
import AdminSubscriptions from './pages/admin/AdminSubscriptions';
import AdminSubscriptionForm from './pages/admin/AdminSubscriptionForm';
// import AdminSubscriptionDetail from './pages/admin/AdminSubscriptionDetail';
import AdminSubscriptionDetail from './pages/admin/AdminSubscriptionDetail';
import AdminRoute from './components/AdminRoute';
import './App.css';

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
      <Route path="/admin/login" element={<AdminLogin />} />
      <Route path="/admin" element={
        <AdminRoute>
          <AdminLayout />
        </AdminRoute>
      }>
        <Route path="dashboard" element={<AdminDashboard />} />
        <Route path="games" element={<AdminGames />} />
        <Route path="games/new" element={<AdminGameForm />} />
        <Route path="games/edit/:id" element={<AdminGameForm />} />
        <Route path="users" element={<AdminUsers />} />
        <Route path="subscriptions" element={<AdminSubscriptions />} />
        <Route path="subscriptions/new" element={<AdminSubscriptionForm />} />
        <Route path="subscriptions/edit/:id" element={<AdminSubscriptionForm />} />
        <Route path="subscriptions/:id" element={<AdminSubscriptionDetail />} />
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
