import React from 'react';
import styles from './UserDetailsModal.module.css';

function UserDetailsModal({ user, subscription, subscriptionPlan, country, onClose }) {
    if (!user) return null;

    console.log('Modal props:', { user, subscription, subscriptionPlan, country });

    const formatDate = (dateString) => {
        if (!dateString) return 'N/A';
        return new Date(dateString).toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
    };

    const calculateDuration = (startDate, endDate) => {
        if (!startDate || !endDate) return 'N/A';
        const start = new Date(startDate);
        const end = new Date(endDate);
        const diffTime = Math.abs(end - start);
        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

        if (diffDays < 30) {
            return `${diffDays} days`;
        } else if (diffDays < 365) {
            const months = Math.floor(diffDays / 30);
            return `${months} month${months > 1 ? 's' : ''}`;
        } else {
            const years = Math.floor(diffDays / 365);
            return `${years} year${years > 1 ? 's' : ''}`;
        }
    };

    const isSubscriptionActive = subscription && new Date(subscription.planEndDate) > new Date();
    console.log('Is subscription active?', isSubscriptionActive, subscription);

    return (
        <div className={styles.modalOverlay} onClick={onClose}>
            <div className={styles.modalContent} onClick={(e) => e.stopPropagation()}>
                <div className={styles.modalHeader}>
                    <h2>User Details</h2>
                    <button className={styles.closeButton} onClick={onClose}>×</button>
                </div>

                <div className={styles.modalBody}>
                    {/* Basic Information */}
                    <section className={styles.section}>
                        <h3 className={styles.sectionTitle}>Basic Information</h3>
                        <div className={styles.infoGrid}>
                            <div className={styles.infoItem}>
                                <span className={styles.label}>Name:</span>
                                <span className={styles.value}>{user.userName}</span>
                            </div>
                            <div className={styles.infoItem}>
                                <span className={styles.label}>Email:</span>
                                <span className={styles.value}>{user.userEmail}</span>
                            </div>
                            <div className={styles.infoItem}>
                                <span className={styles.label}>Age:</span>
                                <span className={styles.value}>{user.age}</span>
                            </div>
                            <div className={styles.infoItem}>
                                <span className={styles.label}>Country:</span>
                                <span className={styles.value}>{country || 'N/A'}</span>
                            </div>
                            <div className={styles.infoItem}>
                                <span className={styles.label}>Joined:</span>
                                <span className={styles.value}>{formatDate(user.createdAt)}</span>
                            </div>
                            <div className={styles.infoItem}>
                                <span className={styles.label}>Status:</span>
                                <span className={`${styles.value} ${user.isDeleted ? styles.blocked : styles.active}`}>
                                    {user.isDeleted ? 'Blocked' : 'Active'}
                                </span>
                            </div>
                        </div>
                    </section>

                    {/* Subscription Information */}
                    <section className={styles.section}>
                        <h3 className={styles.sectionTitle}>Subscription Details</h3>
                        {isSubscriptionActive ? (
                            <div className={styles.subscriptionActive}>
                                <div className={styles.subscriptionBadge}>
                                    <span className={styles.badgeIcon}>✓</span>
                                    <span className={styles.badgeText}>Active Subscription</span>
                                </div>
                                <div className={styles.infoGrid}>
                                    <div className={styles.infoItem}>
                                        <span className={styles.label}>Plan Name:</span>
                                        <span className={`${styles.value} ${styles.planName}`}>
                                            {subscription?.subscriptionName || 'N/A'}
                                        </span>
                                    </div>
                                    <div className={styles.infoItem}>
                                        <span className={styles.label}>Start Date:</span>
                                        <span className={styles.value}>{formatDate(subscription.planStartDate)}</span>
                                    </div>
                                    <div className={styles.infoItem}>
                                        <span className={styles.label}>End Date:</span>
                                        <span className={styles.value}>{formatDate(subscription.planEndDate)}</span>
                                    </div>
                                    <div className={styles.infoItem}>
                                        <span className={styles.label}>Duration:</span>
                                        <span className={styles.value}>
                                            {calculateDuration(subscription.planStartDate, subscription.planEndDate)}
                                        </span>
                                    </div>
                                    <div className={styles.infoItem}>
                                        <span className={styles.label}>Games Included:</span>
                                        <span className={styles.value}>
                                            {subscriptionPlan?.includedGames?.length || 0} games
                                        </span>
                                    </div>
                                </div>
                            </div>
                        ) : (
                            <div className={styles.noSubscription}>
                                <span className={styles.noSubIcon}>—</span>
                                <p>No active subscription</p>
                            </div>
                        )}
                    </section>
                </div>

                <div className={styles.modalFooter}>
                    <button className={styles.closeBtn} onClick={onClose}>Close</button>
                </div>
            </div>
        </div>
    );
}

export default UserDetailsModal;
