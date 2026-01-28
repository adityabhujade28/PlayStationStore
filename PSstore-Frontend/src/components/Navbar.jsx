import { useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useCart } from '../context/CartContext';
import { useQuery } from '@tanstack/react-query';
import styles from './Navbar.module.css';
import apiClient from '../utils/apiClient';

function Navbar() {
  const { token, getDecodedToken, logout } = useAuth();
  const { cartItemCount } = useCart();
  const navigate = useNavigate();

  const decoded = getDecodedToken();
  const userId = decoded?.userId;

  const { data: user } = useQuery({
    queryKey: ['user', userId],
    queryFn: async () => {
      const response = await apiClient.get(`/users/${userId}`);
      if (!response.ok) throw new Error('Failed to fetch user');
      return response.json();
    },
    enabled: !!userId,
    staleTime: 5 * 60 * 1000,
    cacheTime: 10 * 60 * 1000,
  });

  const userName = useMemo(() => user?.userName || 'User', [user]);

  return (
    <nav className={styles.navbar}>
      <div className={styles.logo} onClick={() => navigate('/store')}>
        🎮 PSstore
      </div>
      <div className={styles.navLinks}>
        <button className={styles.navLink} onClick={() => navigate('/store')}>
          Store
        </button>
        <button className={styles.navLink} onClick={() => navigate('/library')}>
          Library
        </button>
        <button className={styles.navLink} onClick={() => navigate('/subscriptions')}>
          Subscriptions
        </button>
      </div>
      <div className={styles.userSection}>
        <button className={styles.cartButton} onClick={() => navigate('/cart')}>
          🛒
          {cartItemCount > 0 && (
            <span className={styles.cartBadge}>{cartItemCount}</span>
          )}
        </button>
        <span className={styles.userName} onClick={() => navigate('/profile')}>
          Welcome, {userName}
        </span>
        <button className={styles.logoutButton} onClick={logout}>
          Logout
        </button>
      </div>
    </nav>
  );
}

export default Navbar;
