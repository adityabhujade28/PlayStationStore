import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useCart } from '../context/CartContext';
import { formatPrice } from '../utils/currency';
import Navbar from '../components/Navbar';
import styles from './GameDetails.module.css';

function GameDetails() {
  const { gameId } = useParams();
  const navigate = useNavigate();
  const { token, getDecodedToken } = useAuth();
  const { addToCart, loading: cartLoading } = useCart();
  const [game, setGame] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [userCurrency, setUserCurrency] = useState('INR');

  useEffect(() => {
    fetchGameDetails();
    fetchUserCurrency();
  }, [gameId]);

  const fetchUserCurrency = async () => {
    try {
      const decoded = getDecodedToken();
      const userId = decoded?.userId;

      const response = await fetch(`http://localhost:5160/api/users/${userId}`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.ok) {
        const userData = await response.json();
        if (userData.countryId) {
          const countryResponse = await fetch(`http://localhost:5160/api/countries/${userData.countryId}`, {
            headers: {
              'Authorization': `Bearer ${token}`
            }
          });
          
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
    setError('');

    try {
      const decoded = getDecodedToken();
      const userId = decoded?.userId;

      // Fetch game with access information - userId as route parameter
      const response = await fetch(
        `http://localhost:5160/api/games/${gameId}/access/${userId}`,
        {
          headers: {
            'Authorization': `Bearer ${token}`
          }
        }
      );

      if (!response.ok) {
        throw new Error('Failed to fetch game details');
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
    const result = await addToCart(gameId);
    if (result.success) {
      alert('Game added to cart!');
    } else {
      alert(result.error || 'Failed to add to cart');
    }
  };

  const handleBuyNow = async () => {
    const result = await addToCart(gameId);
    if (result.success) {
      navigate('/cart');
    } else {
      alert(result.error || 'Failed to add to cart');
    }
  };

  if (loading) {
    return (
      <>
        <Navbar />
        <div className={styles.loading}>Loading game details...</div>
      </>
    );
  }

  if (error) {
    return (
      <>
        <Navbar />
        <div className={styles.error}>{error}</div>
      </>
    );
  }

  if (!game) {
    return (
      <>
        <Navbar />
        <div className={styles.error}>Game not found</div>
      </>
    );
  }

  return (
    <>
      <Navbar />
      <div className={styles.details}>
        <div className={styles.container}>
          <button className={styles.backButton} onClick={() => navigate(-1)}>
            ← Back
          </button>

          <div className={styles.gameHeader}>
            <div className={styles.imageSection}>
              <div className={styles.mainImage}>
                <div className={styles.imagePlaceholder}>🎮</div>
                {game.freeToPlay && <span className={styles.freeBadge}>FREE</span>}
              </div>
            </div>

            <div className={styles.infoSection}>
              <h1 className={styles.gameTitle}>{game.gameName}</h1>
              <p className={styles.publisher}>{game.publishedBy}</p>

              <div className={styles.badges}>
                {game.isMultiplayer && (
                  <span className={styles.badge}>Multiplayer</span>
                )}
                {game.freeToPlay && (
                  <span className={styles.badge}>Free to Play</span>
                )}
              </div>

              <div className={styles.priceSection}>
                {game.freeToPlay ? (
                  <span className={styles.freePrice}>Free to Play</span>
                ) : (
                  <>
                    <span className={styles.price}>
                      {game.price ? formatPrice(game.price, userCurrency) : 'Price TBA'}
                    </span>
                  </>
                )}
              </div>

              {game.releaseDate && (
                <p className={styles.releaseDate}>
                  Released: {new Date(game.releaseDate).toLocaleDateString()}
                </p>
              )}

              <div className={styles.actions}>
                {game.canAccess ? (
                  <button className={`${styles.button} ${styles.playButton}`}>
                    ▶ Play Game
                  </button>
                ) : game.freeToPlay ? (
                  <button className={`${styles.button} ${styles.playButton}`}>
                    ▶ Play Now
                  </button>
                ) : (
                  <>
                    <button
                      className={`${styles.button} ${styles.buyButton}`}
                      onClick={handleBuyNow}
                      disabled={cartLoading}
                    >
                      Buy Now
                    </button>
                    <button
                      className={`${styles.button} ${styles.cartButton}`}
                      onClick={handleAddToCart}
                      disabled={cartLoading}
                    >
                      {cartLoading ? 'Adding...' : 'Add to Cart'}
                    </button>
                  </>
                )}
              </div>

              {game.availableThrough && game.availableThrough.length > 0 && (
                <div className={styles.subscriptionInfo}>
                  <p className={styles.subscriptionText}>
                    Also available with:
                  </p>
                  <div className={styles.subscriptionBadges}>
                    {game.availableThrough.map((plan, index) => (
                      <span key={index} className={styles.subscriptionBadge}>
                        {plan}
                      </span>
                    ))}
                  </div>
                </div>
              )}
            </div>
          </div>

          <div className={styles.description}>
            <h2 className={styles.sectionTitle}>About This Game</h2>
            <p className={styles.descriptionText}>
              {game.gameName} is an exciting game published by {game.publishedBy}.
              {game.isMultiplayer && ' Experience thrilling multiplayer action with friends!'}
              {game.freeToPlay && ' Best of all, it\'s completely free to play!'}
            </p>
          </div>
        </div>
      </div>
    </>
  );
}

export default GameDetails;
