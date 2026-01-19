import React, { useState, useEffect } from 'react';
import apiClient from '../../utils/apiClient';
import styles from './AdminGames.module.css'; // Reusing common admin styles

function AdminUsers() {
    const [users, setUsers] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        fetchUsers();
    }, []);

    const fetchUsers = async () => {
        try {
            const response = await apiClient.get('/users');
            if (response.ok) {
                const data = await response.json();
                setUsers(data);
            } else {
                setError('Failed to fetch users');
            }
        } catch (err) {
            setError('Error loading users');
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    const handleBlock = async (userId) => {
        if (!window.confirm('Are you sure you want to block this user?')) return;

        try {
            const response = await apiClient.delete(`/users/${userId}`);
            if (response.ok) {
                fetchUsers();
            } else {
                alert('Failed to block user');
            }
        } catch (error) {
            console.error('Error blocking user:', error);
        }
    };

    const handleRestore = async (userId) => {
        if (!window.confirm('Are you sure you want to restore this user?')) return;

        try {
            const response = await apiClient.post(`/users/${userId}/restore`);
            if (response.ok) {
                fetchUsers();
            } else {
                alert('Failed to restore user');
            }
        } catch (error) {
            console.error('Error restoring user:', error);
        }
    };

    if (loading) return <div className={styles.container}>Loading users...</div>;
    if (error) return <div className={styles.container} style={{ color: 'red' }}>{error}</div>;

    return (
        <div className={styles.container}>
            <div className={styles.header}>
                <h1 className={styles.title}>User Management</h1>
            </div>

            <div className={styles.tableContainer}>
                <table className={styles.table}>
                    <thead>
                        <tr>
                            <th>User Name</th>
                            <th>Email</th>
                            <th>Age</th>
                            <th>Status</th>
                            <th>Joined</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {users.map((user) => (
                            <tr key={user.userId}>
                                <td>{user.userName}</td>
                                <td>{user.userEmail}</td>
                                <td>{user.age}</td>
                                <td>
                                    {user.isDeleted ? (
                                        <span style={{ color: 'red' }}>Blocked</span>
                                    ) : (
                                        <span style={{ color: 'green' }}>Active</span>
                                    )}
                                </td>
                                <td>{new Date(user.createdAt).toLocaleDateString()}</td>
                                <td>
                                    {!user.isDeleted ? (
                                        <button
                                            onClick={() => handleBlock(user.userId)}
                                            className={`${styles.actionButton} ${styles.deleteBtn}`}
                                        >
                                            Block
                                        </button>
                                    ) : (
                                        <button
                                            onClick={() => handleRestore(user.userId)}
                                            className={`${styles.actionButton}`}
                                            style={{ backgroundColor: '#0070d1', color: 'white' }}
                                        >
                                            Restore
                                        </button>
                                    )}
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
}

export default AdminUsers;
