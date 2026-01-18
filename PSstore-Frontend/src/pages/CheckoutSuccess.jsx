import { useEffect, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { formatPrice } from '../utils/currency';
import styles from './CheckoutSuccess.module.css';
import apiClient from '../utils/apiClient';

function CheckoutSuccess() {
  const location = useLocation();
  const navigate = useNavigate();
  const { getDecodedToken } = useAuth();
  const purchase = location.state?.purchase;
  const [userCurrency, setUserCurrency] = useState('INR');

  useEffect(() => {
    if (!purchase) {
      navigate('/');
    } else {
      fetchUserCurrency();
    }
  }, [purchase, navigate]);

  const fetchUserCurrency = async () => {
    try {
      const decoded = getDecodedToken();
      const userId = decoded?.userId;

      if (!userId) return;

      const userResponse = await apiClient.get(`/users/${userId}`);

      if (userResponse.ok) {
        const userData = await userResponse.json();

        if (userData.countryId) {
          const countryResponse = await apiClient.get(`/countries/${userData.countryId}`);

          if (countryResponse.ok) {
            const countryData = await countryResponse.json();
            setUserCurrency(countryData.currency);
          }
        }
      }
    } catch (err) {
      console.error('Failed to fetch user currency:', err);
    }
  };

  if (!purchase) return null;

  const currentDate = new Date();
  const formattedDate = currentDate.toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });

  return (
    <div className={styles.container}>
      <div className={styles.successCard}>
        <div className={styles.successIcon}>✓</div>
        <h1 className={styles.title}>Purchase Successful!</h1>
        <p className={styles.subtitle}>Thank you for your purchase</p>

        <div className={styles.orderDetails}>
          <h2 className={styles.sectionTitle}>Order Details</h2>
          <div className={styles.detailRow}>
            <span className={styles.label}>Order ID:</span>
            <span className={styles.value}>#{Date.now().toString(36).toUpperCase()}</span>
          </div>
          <div className={styles.detailRow}>
            <span className={styles.label}>Date:</span>
            <span className={styles.value}>{formattedDate}</span>
          </div>
          <div className={styles.detailRow}>
            <span className={styles.label}>Total Amount:</span>
            <span className={styles.value}>
              {formatPrice(purchase.totalAmount, userCurrency)}
            </span>
          </div>
        </div>

        {purchase.purchasedGames && purchase.purchasedGames.length > 0 && (
          <div className={styles.gamesSection}>
            <h2 className={styles.sectionTitle}>Purchased Games</h2>
            <div className={styles.gamesList}>
              {purchase.purchasedGames.map((gameName, index) => (
                <div key={index} className={styles.gameItem}>
                  <div className={styles.gamePlaceholder}>🎮</div>
                  <span className={styles.gameName}>{gameName}</span>
                </div>
              ))}
            </div>
          </div>
        )}

        {purchase.failedGames && purchase.failedGames.length > 0 && (
          <div className={styles.failedSection}>
            <h3>Failed Purchases:</h3>
            <ul>
              {purchase.failedGames.map((error, index) => (
                <li key={index}>{error}</li>
              ))}
            </ul>
          </div>
        )}

        <div className={styles.actions}>
          <button
            className={styles.libraryButton}
            onClick={() => navigate('/library')}
          >
            Go to Library
          </button>
          <button
            className={styles.storeButton}
            onClick={() => navigate('/')}
          >
            Continue Shopping
          </button>
        </div>
      </div>
    </div>
  );
}

export default CheckoutSuccess;
