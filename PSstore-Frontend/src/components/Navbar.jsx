import { useAuth } from '../context/AuthContext';
import styles from './Navbar.module.css';

function Navbar() {
  const { user, logout } = useAuth();

  return (
    <nav className={styles.navbar}>
      <div className={styles.logo}>🎮 PSstore</div>
      <div className={styles.userSection}>
        <span className={styles.userName}>Welcome, {user?.userName}</span>
        <button className={styles.logoutButton} onClick={logout}>
          Logout
        </button>
      </div>
    </nav>
  );
}

export default Navbar;
