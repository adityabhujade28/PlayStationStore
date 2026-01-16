import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { formatPrice } from '../utils/currency';
import styles from './Library.module.css';

function Library() {
  const { token, getDecodedToken } = useAuth();
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState('all');
  const [games, setGames] = useState([]);
  const [subscriptions, setSubscriptions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [userCurrency, setUserCurrency] = useState('INR');

  useEffect(() => {
    fetchLibrary();
    fetchSubscriptions();
    fetchUserCurrency();
  }, []);

  const fetchUserCurrency = async () => {
    try {
      const decoded = getDecodedToken();
      const userId = decoded?.userId;

      if (!userId) return;

      const userResponse = await fetch(`http://localhost:5160/api/users/${userId}`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (userResponse.ok) {
        const userData = await userResponse.json();
        
        if (userData.countryId) {
          const countryResponse = await fetch(
            `http://localhost:5160/api/countries/${userData.countryId}`,
            {
              headers: {
                'Authorization': `Bearer ${token}`
              }
            }
          );

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

  const fetchLibrary = async () => {
    setLoading(true);
    setError(null);
    try {
      const decoded = getDecodedToken();
      const userId = decoded?.userId;

      if (!userId) {
        setError('User not authenticated');
        return;
      }

      const response = await fetch(
        `http://localhost:5160/api/users/${userId}/library`,
        {
          headers: {
            'Authorization': `Bearer ${token}`
          }
        }
      );

      if (response.ok) {
        const data = await response.json();
        console.log('Library data:', data); // Debug log
        // Backend returns AccessibleGames array - keep all non-subscription games
        const accessibleGames = (data.accessibleGames || []).filter(
          game => game.accessType !== 'SUBSCRIPTION'
        );
        
        // Fetch full game details for each accessible game
        const gamesWithDetails = await Promise.all(
          accessibleGames.map(async (game) => {
            try {
              const gameResponse = await fetch(
                `http://localhost:5160/api/games/${game.gameId}/access/${userId}`,
                {
                  headers: {
                    'Authorization': `Bearer ${token}`
                  }
                }
              );
              
              if (gameResponse.ok) {
                const gameDetails = await gameResponse.json();
                return {
                  gameId: game.gameId,
                  name: game.gameName,
                  publisher: gameDetails.publishedBy || 'Unknown',
                  releaseDate: gameDetails.releaseDate,
                  isMultiplayer: gameDetails.isMultiplayer,
                  accessType: game.accessType,
                  message: game.message,
                  purchasedOn: game.purchasedOn
                };
              }
              
              // If fetch fails, return basic info
              return {
                gameId: game.gameId,
                name: game.gameName,
                accessType: game.accessType,
                message: game.message
              };
            } catch (err) {
              console.error(`Failed to fetch details for game ${game.gameId}:`, err);
              return {
                gameId: game.gameId,
                name: game.gameName,
                accessType: game.accessType,
                message: game.message
              };
            }
          })
        );
        
        setGames(gamesWithDetails);
      } else {
        setError('Failed to load library');
      }
    } catch (err) {
      setError('Failed to load library: ' + err.message);
    } finally {
      setLoading(false);
    }
  };

  const fetchSubscriptions = async () => {
    try {
      const decoded = getDecodedToken();
      const userId = decoded?.userId;

      if (!userId) return;

      const response = await fetch(
        `http://localhost:5160/api/Subscriptions/user/${userId}/active`,
        {
          headers: {
            'Authorization': `Bearer ${token}`
          }
        }
      );

      if (response.ok) {
        const activeSubscription = await response.json();

        // Try to enrich subscription with plan details (plan name, included games)
        try {
          const plansRes = await fetch(`http://localhost:5160/api/Subscriptions/plans`, {
            headers: { 'Authorization': `Bearer ${token}` }
          });

          if (plansRes.ok) {
            const plans = await plansRes.json();
            const matchingPlan = plans.find(p =>
              p.subscriptionId === activeSubscription.subscriptionId ||
              p.subscriptionName === activeSubscription.subscriptionName
            );

            if (matchingPlan) {
              activeSubscription.subscriptionName = activeSubscription.subscriptionName || matchingPlan.subscriptionName;
              activeSubscription.includedGamesCount = matchingPlan.includedGames?.length || 0;
            }
          }
        } catch (err) {
          console.warn('Failed to enrich subscription with plan details:', err);
        }

        setSubscriptions([activeSubscription]);
      }
    } catch (err) {
      console.error('Failed to fetch subscriptions:', err);
    }
  };

  const filteredGames = games.filter(game => {
    if (activeTab === 'all') return true;
    if (activeTab === 'purchased') return true; // Show all owned games
    if (activeTab === 'subscription') return false; // Don't show games in subscription tab
    return true;
  });

  const showSubscriptions = activeTab === 'all' || activeTab === 'subscription';

  if (loading) {
    return (
      <div className={styles.container}>
        <div className={styles.loading}>Loading your library...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className={styles.container}>
        <div className={styles.error}>{error}</div>
      </div>
    );
  }

  return (
    
    <div className={styles.container}>
      <button className={styles.backButton} onClick={() => navigate('/')}>
                  ← Back
      </button>
      <div className={styles.header}>
        <h1 className={styles.title}>My Library</h1>
        <div className={styles.stats}>
          <div className={styles.statCard}>
            <div className={styles.statNumber}>{games.length + subscriptions.length}</div>
            <div className={styles.statLabel}>Total Items</div>
          </div>
          <div className={styles.statCard}>
            <div className={styles.statNumber}>
              {games.length}
            </div>
            <div className={styles.statLabel}>Owned Games</div>
          </div>
          <div className={styles.statCard}>
            <div className={styles.statNumber}>
              {subscriptions.length}
            </div>
            <div className={styles.statLabel}>Subscriptions</div>
          </div>
        </div>
      </div>

      <div className={styles.tabs}>
        <button
          className={`${styles.tab} ${activeTab === 'all' ? styles.activeTab : ''}`}
          onClick={() => setActiveTab('all')}
        >
          All ({games.length + subscriptions.length})
        </button>
        <button
          className={`${styles.tab} ${activeTab === 'purchased' ? styles.activeTab : ''}`}
          onClick={() => setActiveTab('purchased')}
        >
          Games ({games.length})
        </button>
        <button
          className={`${styles.tab} ${activeTab === 'subscription' ? styles.activeTab : ''}`}
          onClick={() => setActiveTab('subscription')}
        >
          Subscriptions ({subscriptions.length})
        </button>
      </div>

      {filteredGames.length === 0 && subscriptions.length === 0 ? (
        <div className={styles.emptyState}>
          <div className={styles.emptyIcon}>📚</div>
          <h2>No {activeTab === 'all' ? 'items' : activeTab === 'subscription' ? 'subscriptions' : 'games'} found</h2>
          <p>
            {activeTab === 'all' 
              ? "You don't have any games or subscriptions yet. Browse the store to get started!"
              : activeTab === 'subscription'
              ? "You don't have an active subscription. Check out our subscription plans!"
              : `You don't have any ${activeTab} games yet.`
            }
          </p>
          <button className={styles.browseButton} onClick={() => navigate(activeTab === 'subscription' ? '/subscriptions' : '/')}>
            {activeTab === 'subscription' ? 'View Plans' : 'Browse Store'}
          </button>
        </div>
      ) : (
        <>
          {showSubscriptions && subscriptions.length > 0 && (
            <div className={styles.subscriptionsSection}>
              <h2 className={styles.sectionTitle}>Active Subscriptions</h2>
              <div className={styles.subscriptionsGrid}>
                {subscriptions.map((sub) => (
                  <div key={sub.userSubscriptionId} className={styles.subscriptionCard}>
                    <div className={styles.subscriptionHeader}>
                      <div className={styles.subscriptionIcon}>⭐</div>
                      <div className={styles.subscriptionInfo}>
                        <h3 className={styles.subscriptionTitle}>{sub.subscriptionName || 'Active Subscription'}</h3>
                        <p className={styles.subscriptionStatus}>
                          {sub.isActive ? '✓ Active' : 'Expired'}
                        </p>
                        {typeof sub.includedGamesCount !== 'undefined' && (
                          <p style={{ marginTop: 6, opacity: 0.9 }}>
                            {sub.includedGamesCount} games included
                          </p>
                        )}
                      </div>
                    </div>
                    <div className={styles.subscriptionDetails}>
                      <div className={styles.subscriptionDate}>
                        <span className={styles.dateLabel}>Started:</span>
                        <span>{new Date(sub.planStartDate).toLocaleDateString()}</span>
                      </div>
                      <div className={styles.subscriptionDate}>
                        <span className={styles.dateLabel}>Expires:</span>
                        <span>{new Date(sub.planEndDate).toLocaleDateString()}</span>
                      </div>
                    </div>
                    <button
                      className={styles.manageSubscriptionButton}
                      onClick={() => navigate('/subscriptions')}
                    >
                      Manage Subscription
                    </button>
                  </div>
                ))}
              </div>
            </div>
          )}

          {filteredGames.length > 0 && (
            <>
              {showSubscriptions && subscriptions.length > 0 && (
                <h2 className={styles.sectionTitle}>Your Games</h2>
              )}
              <div className={styles.gamesGrid}>
                {filteredGames.map((game) => (
                  <div key={game.gameId} className={styles.gameCard}>
                    <div className={styles.gameImage}>
                      <div className={styles.placeholder}>🎮</div>
                      <div className={styles.accessBadge}>
                        {game.accessType === 'PURCHASED' && '✓ Owned'}
                        {game.accessType === 'FREE' && '🎮 Free'}
                      </div>
                    </div>
                    <div className={styles.gameInfo}>
                      <h3 className={styles.gameName}>{game.name}</h3>
                      <p className={styles.gamePublisher}>{game.publisher}</p>
                      <div className={styles.gameDetails}>
                        <span className={styles.releaseDate}>
                          {game.releaseDate ? new Date(game.releaseDate).getFullYear() : 'N/A'}
                        </span>
                        {game.isMultiplayer && (
                          <span className={styles.multiplayerBadge}>Multiplayer</span>
                        )}
                      </div>
                    </div>
                    <button
                      className={styles.playButton}
                      onClick={() => navigate(`/game/${game.gameId}`)}
                    >
                      Play Now
                    </button>
                  </div>
                ))}
              </div>
            </>
          )}
        </>
      )}
    </div>
  );
}

export default Library;
