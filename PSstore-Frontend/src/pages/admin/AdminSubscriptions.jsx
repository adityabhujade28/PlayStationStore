import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import apiClient from '../../utils/apiClient';
import styles from './AdminGames.module.css'; // Reusing games styles for consistency

function AdminSubscriptions() {
    const [plans, setPlans] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        fetchPlans();
    }, []);

    const fetchPlans = async () => {
        try {
            const response = await apiClient.get('/subscriptions/plans');
            if (response.ok) {
                const data = await response.json();
                setPlans(data);
            } else {
                setError('Failed to fetch subscription plans');
            }
        } catch (err) {
            setError('Error loading plans');
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    const handleDelete = async (planId) => {
        if (!window.confirm('Are you sure you want to delete this plan? This action cannot be undone if users are subscribed.')) return;

        try {
            const response = await apiClient.delete(`/subscriptions/plans/${planId}`);
            if (response.ok) {
                fetchPlans();
            } else {
                const err = await response.json();
                alert(err.message || 'Failed to delete plan');
            }
        } catch (error) {
            console.error('Error deleting plan:', error);
            alert('Error deleting plan');
        }
    };

    if (loading) return <div className={styles.container}>Loading plans...</div>;
    if (error) return <div className={styles.container} style={{ color: 'red' }}>{error}</div>;

    return (
        <div className={styles.container}>
            <div className={styles.header}>
                <h1 className={styles.title}>Manage Subscriptions</h1>
                <Link to="/admin/subscriptions/new" className={styles.addButton}>+ Add New Plan</Link>
            </div>

            <div className={styles.tableContainer}>
                <table className={styles.table}>
                    <thead>
                        <tr>
                            <th>Plan Name</th>
                            <th>Included Games</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {plans.map((plan) => (
                            <tr key={plan.subscriptionId}>
                                <td>{plan.subscriptionName}</td>
                                <td>{plan.includedGames?.length || 0} Games</td>
                                <td>
                                    <Link
                                        to={`/admin/subscriptions/edit/${plan.subscriptionId}`}
                                        className={`${styles.actionButton} ${styles.editBtn}`}
                                        style={{ display: 'inline-block', textDecoration: 'none' }}
                                    >
                                        Edit / Manage
                                    </Link>
                                    <button
                                        onClick={() => handleDelete(plan.subscriptionId)}
                                        className={`${styles.actionButton} ${styles.deleteBtn}`}
                                    >
                                        Delete
                                    </button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
}

export default AdminSubscriptions;
