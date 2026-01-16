import { useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { formatPrice } from '../utils/currency';
import styles from './CheckoutSuccess.module.css';

function CheckoutSuccess() {
  const location = useLocation();
  const navigate = useNavigate();
  const purchase = location.state?.purchase;

  useEffect(() => {
    if (!purchase) {
      navigate('/');
    }
  }, [purchase, navigate]);

  if (!purchase) return null;

  const date = new Date(purchase.purchaseDate);
  const formattedDate = date.toLocaleDateString('en-US', {
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
            <span className={styles.value}>{purchase.purchaseId}</span>
          </div>
          <div className={styles.detailRow}>
            <span className={styles.label}>Date:</span>
            <span className={styles.value}>{formattedDate}</span>
          </div>
          <div className={styles.detailRow}>
            <span className={styles.label}>Total Amount:</span>
            <span className={styles.totalValue}>
              {formatPrice(purchase.totalAmount, purchase.currency || 'INR')}
            </span>
          </div>
        </div>

        {purchase.games && purchase.games.length > 0 && (
          <div className={styles.gamesSection}>
            <h2 className={styles.sectionTitle}>Purchased Games</h2>
            <div className={styles.gamesList}>
              {purchase.games.map((game, index) => (
                <div key={index} className={styles.gameItem}>
                  <div className={styles.gamePlaceholder}>🎮</div>
                  <span className={styles.gameName}>{game.name || game.gameName}</span>
                </div>
              ))}
            </div>
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
