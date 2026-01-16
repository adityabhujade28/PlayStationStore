import { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import Navbar from './Navbar';
import styles from './Dashboard.module.css';

function Dashboard() {
  const { token } = useAuth();
  const [games, setGames] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const fetchGames = async () => {
    setLoading(true);
    setError('');
    
    try {
      const response = await fetch('http://localhost:5160/api/games', {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });
      
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      
      const data = await response.json();
      setGames(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };


  return (
    <>
      <Navbar />
      <div className={styles.dashboard}>
        <div className={styles.container}>
          <div className={styles.header}>
            <h1 className={styles.title}>Dashboard</h1>
            <p className={styles.subtitle}>Browse games and manage your profile</p>
          </div>

          {error && <div className={styles.error}>{error}</div>}

          <div className={styles.section}>
            <h2 className={styles.sectionTitle}>Games Catalog</h2>
            <div className={styles.buttonGroup}>
              <button 
                className={`${styles.fetchButton} ${styles.games}`}
                onClick={fetchGames}
                disabled={loading}
              >
                {loading ? 'Loading...' : 'Fetch Games'}
              </button>
            </div>

            {loading && <div className={styles.loading}>Loading games...</div>}
            
            {games.length === 0 && !loading && (
              <div className={styles.emptyState}>
                No games loaded. Click "Fetch Games" to load the catalog.
              </div>
            )}

            <div className={styles.grid}>
              {games.map(game => (
                <div key={game.gameId} className={styles.card}>
                  <h3 className={styles.cardTitle}>{game.gameName}</h3>
                  <p className={styles.cardDetail}>
                    <strong>Publisher:</strong> {game.publishedBy || 'N/A'}
                  </p>
                  <p className={styles.cardDetail}>
                    <strong>Release:</strong> {new Date(game.releaseDate).toLocaleDateString()}
                  </p>
                  <p className={styles.cardDetail}>
                    <strong>Multiplayer:</strong> {game.isMultiplayer ? 'Yes' : 'No'}
                  </p>
                  <span className={`${styles.priceTag} ${game.freeToPlay ? styles.free : ''}`}>
                    {game.freeToPlay ? 'FREE' : `$${game.price}`}
                  </span>
                </div>
              ))}
            </div>
          </div>

        </div>
      </div>
    </>
  );
}

export default Dashboard;
