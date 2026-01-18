import { useEffect } from 'react';
import styles from './Toast.module.css';

/**
 * Toast Notification Component
 * @param {Object} props
 * @param {string} props.message - The message to display
 * @param {'success' | 'error'} props.type - The type of toast
 * @param {Function} props.onClose - Callback when toast closes
 * @param {number} props.duration - Duration in ms (default 3000)
 */
function Toast({ message, type = 'success', onClose, duration = 3000 }) {
    useEffect(() => {
        if (duration > 0) {
            const timer = setTimeout(() => {
                onClose();
            }, duration);
            return () => clearTimeout(timer);
        }
    }, [duration, onClose]);

    const icon = type === 'success' ? '✓' : '✕';

    return (
        <div className={`${styles.toast} ${styles[type]}`}>
            <span className={styles.icon}>{icon}</span>
            <div className={styles.content}>
                <p className={styles.message}>{message}</p>
            </div>
            <button className={styles.closeButton} onClick={onClose}>
                ×
            </button>
        </div>
    );
}

export default Toast;
