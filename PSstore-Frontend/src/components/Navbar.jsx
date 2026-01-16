import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useCart } from '../context/CartContext';
import styles from './Navbar.module.css';

function Navbar() {
  const { token, getDecodedToken, logout } = useAuth();
  const { cartItemCount } = useCart();
  const navigate = useNavigate();
  const [userName, setUserName] = useState('User');

  useEffect(() => {
    // Decode token to get userId and fetch user details
    const decoded = getDecodedToken();
    if (decoded) {
      fetchUserName(decoded.userId);
    }
  }, [token]);

  const fetchUserName = async (userId) => {
    try {
      const response = await fetch(`http://localhost:5160/api/users/${userId}`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });
      
      if (response.ok) {
        const data = await response.json();
        setUserName(data.userName);
      }
    } catch (err) {
      console.error('Failed to fetch user name:', err);
    }
  };

  return (
    <nav className={styles.navbar}>
      <div className={styles.logo} onClick={() => navigate('/')}>
        🎮 PSstore
      </div>
      <div className={styles.navLinks}>
        <button className={styles.navLink} onClick={() => navigate('/')}>
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
