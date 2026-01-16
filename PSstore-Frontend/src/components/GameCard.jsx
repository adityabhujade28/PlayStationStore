import { useNavigate } from 'react-router-dom';
import { formatPrice } from '../utils/currency';
import styles from './GameCard.module.css';

function GameCard({ game, currency = 'INR' }) {
  const navigate = useNavigate();

  const handleClick = () => {
    navigate(`/game/${game.gameId}`);
  };

  return (
    <div className={styles.card} onClick={handleClick}>
      <div className={styles.imageContainer}>
        {game.freeToPlay && <span className={styles.freeBadge}>FREE</span>}
        <div className={styles.placeholder}>🎮</div>
      </div>
      <div className={styles.content}>
        <h3 className={styles.title}>{game.gameName}</h3>
        <p className={styles.publisher}>{game.publishedBy}</p>
        <div className={styles.footer}>
          {game.freeToPlay ? (
            <span className={styles.freeText}>Free to Play</span>
          ) : (
            <span className={styles.price}>
              {game.price ? formatPrice(game.price, currency) : 'Price TBA'}
            </span>
          )}
          {game.isMultiplayer && (
            <span className={styles.multiplayerBadge}>Multiplayer</span>
          )}
        </div>
      </div>
    </div>
  );
}

export default GameCard;
