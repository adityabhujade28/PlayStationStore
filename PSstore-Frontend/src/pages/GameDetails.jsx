import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { formatPrice } from '../utils/currency';
import { useAuth } from '../context/AuthContext';
import { useCart } from '../context/CartContext';
import Navbar from '../components/Navbar';
import Toast from '../components/Toast'; // Import Toast
import styles from './GameDetails.module.css';
import apiClient from '../utils/apiClient';

function GameDetails() {
  const { gameId } = useParams();
  const navigate = useNavigate();
  const { getDecodedToken } = useAuth();
  const { addToCart } = useCart();

  const [game, setGame] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [userCurrency, setUserCurrency] = useState('INR');
  const [addingToCart, setAddingToCart] = useState(false);
  const [toast, setToast] = useState(null); // { message, type }

  const showToast = (message, type = 'success') => {
    setToast({ message, type });
  };

  useEffect(() => {
    fetchGameDetails();
    fetchUserCurrency();
  }, [gameId]);

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

  const fetchGameDetails = async () => {
    setLoading(true);
    try {
      const decoded = getDecodedToken();
      const userId = decoded?.userId;

      const response = await apiClient.get(`/games/${gameId}/access/${userId}`);

      if (!response.ok) {
        throw new Error('Game not found');
      }

      const data = await response.json();
      setGame(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleAddToCart = async () => {
    setAddingToCart(true);
    const result = await addToCart(game.gameId);
    setAddingToCart(false);

    if (result.success) {
      showToast('Added to cart!', 'success');
    } else {
      showToast(result.message, 'error');
    }
  };

  const handleBuyNow = async () => {
    setAddingToCart(true);
    const result = await addToCart(game.gameId);
    setAddingToCart(false);

    if (result.success) {
      navigate('/cart');
    } else {
      showToast(result.message, 'error');
    }
  };

  if (loading) {
    return (
      <>
        <Navbar />
        <div className={styles.container}>
          <div className={styles.loading}>Loading game details...</div>
        </div>
      </>
    );
  }

  if (error || !game) {
    return (
      <>
        <Navbar />
        <div className={styles.container}>
          <div className={styles.error}>{error || 'Game not found'}</div>
          <button onClick={() => navigate('/')} className={styles.backButton}>
            Back to Store
          </button>
        </div>
      </>
    );
  }

  return (
    <>
      <Navbar />
      {toast && (
        <Toast
          message={toast.message}
          type={toast.type}
          onClose={() => setToast(null)}
        />
      )}
      <div className={styles.details}>
        <div className={styles.container}>
          <button onClick={() => navigate('/')} className={styles.backButton}>
            &larr; Back to Store
          </button>

          <div className={styles.gameHeader}>
            <div className={styles.imageSection}>
              <div className={styles.mainImage}>
                {game.imageUrl ? (
                  <img src={game.imageUrl} alt={game.gameName} className={styles.gameImage} />
                ) : (
                  <div className={styles.imagePlaceholder}>🎮</div>
                )}
              </div>
              {game.freeToPlay && <div className={styles.freeBadge}>Free</div>}
            </div>

            <div className={styles.infoSection}>
              <h1 className={styles.gameTitle}>{game.gameName}</h1>
              <p className={styles.publisher}>Published by: {game.publishedBy || 'Unknown'}</p>

              <div className={styles.badges}>
                {game.isMultiplayer && <span className={styles.badge}>Multiplayer</span>}
                {game.canAccess && (
                  <span className={styles.badge}>
                    {game.accessType === 'SUBSCRIPTION' ? 'Included with Subscription' : 'Owned'}
                  </span>
                )}
              </div>



              <div className={styles.priceSection}>
                {game.freeToPlay ? (
                  <div className={styles.freePrice}>Free</div>
                ) : !game.canAccess && (
                  <div className={styles.price}>
                    {game.price ? formatPrice(game.price, userCurrency) : 'Price TBA'}
                  </div>
                )}
              </div>

              <div className={styles.actions}>
                {game.canAccess ? (
                  <button className={styles.playButton} disabled>
                    {game.accessType === 'SUBSCRIPTION' ? 'Play with Subscription' : 'Play Now'}
                  </button>
                ) : (
                  <>
                    {!game.freeToPlay && (
                      <>
                        <button
                          className={styles.cartButton}
                          onClick={handleAddToCart}
                          disabled={addingToCart}
                        >
                          {addingToCart ? 'Adding...' : 'Add to Cart'}
                        </button>
                        <button
                          className={styles.buyButton}
                          onClick={handleBuyNow}
                          disabled={addingToCart}
                        >
                          Buy Now
                        </button>
                      </>
                    )}
                    {game.freeToPlay && (
                      <button className={styles.playButton} onClick={handleBuyNow}>Get Free</button>
                    )}
                  </>
                )}
              </div>

              <div className={styles.description}>
                <h2 className={styles.sectionTitle}>About this game</h2>
                <p className={styles.descriptionText}>
                  Experience the thrill of {game.gameName}.
                  {game.isMultiplayer ? ' Join your friends in this exciting multiplayer adventure.' : ' Immerse yourself in this single-player journey.'}
                  <br /><br />
                  <strong>Release Date:</strong> {game.releaseDate ? new Date(game.releaseDate).toLocaleDateString() : 'TBA'}
                  <br />
                  <strong>Categories:</strong> {game.categories && game.categories.length > 0 ? game.categories.join(', ') : 'None'}
                </p>

                {/* Subscription info if available */}
                {game.availableInPlans && game.availableInPlans.length > 0 && (
                  <div className={styles.subscriptionInfo}>
                    <p className={styles.subscriptionText}>Included in subscriptions:</p>
                    <div className={styles.subscriptionBadges}>
                      {game.availableInPlans.map((plan, index) => (
                        <span key={index} className={styles.subscriptionBadge}>{plan}</span>
                      ))}
                    </div>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>
    </>
  );
}

export default GameDetails;
