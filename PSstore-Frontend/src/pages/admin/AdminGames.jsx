import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import apiClient from '../../utils/apiClient';
import styles from './AdminGames.module.css';

function AdminGames() {
    const [games, setGames] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        fetchGames();
    }, []);

    const fetchGames = async () => {
        try {
            const response = await apiClient.get('/games?includeDeleted=true');
            if (response.ok) {
                const data = await response.json();
                setGames(data);
            } else {
                setError('Failed to fetch games');
            }
        } catch (err) {
            setError('Error loading games');
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    const handleDelete = async (gameId) => {
        if (!window.confirm('Are you sure you want to delete this game?')) return;

        try {
            const response = await apiClient.delete(`/games/${gameId}`);
            if (response.ok) {
                // Refresh list
                fetchGames();
            } else {
                alert('Failed to delete game');
            }
        } catch (error) {
            console.error('Error deleting game:', error);
            alert('Error deleting game');
        }
    };

    const handleRestore = async (gameId) => {
        try {
            const response = await apiClient.post(`/games/${gameId}/restore`);
            if (response.ok) {
                fetchGames();
            }
        } catch (error) {
            console.error("Error restoring game:", error);
        }
    };

    if (loading) return <div className={styles.container}>Loading games...</div>;
    if (error) return <div className={styles.container} style={{ color: 'red' }}>{error}</div>;

    return (
        <div className={styles.container}>
            <div className={styles.header}>
                <h1 className={styles.title}>Manage Games</h1>
                <Link to="/admin/games/new" className={styles.addButton}>+ Add New Game</Link>
            </div>

            <div className={styles.tableContainer}>
                <table className={styles.table}>
                    <thead>
                        <tr>
                            <th>Name</th>
                            <th>Publisher</th>
                            <th>Price</th>
                            <th>Status</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {games.map((game) => (
                            <tr key={game.gameId} style={{ opacity: game.isDeleted ? 0.5 : 1 }}>
                                <td>{game.gameName}</td>
                                <td>{game.publishedBy}</td>
                                <td className={styles.price}>
                                    {game.freeToPlay ? 'Free' : `₹${game.price}`}
                                </td>
                                <td>
                                    {game.isDeleted ? <span style={{ color: 'orange' }}>Deleted</span> : <span style={{ color: '#4db8ff' }}>Active</span>}
                                </td>
                                <td>
                                    <Link to={`/admin/games/edit/${game.gameId}`} className={`${styles.actionButton} ${styles.editBtn}`} style={{ display: 'inline-block', textDecoration: 'none' }}>Edit</Link>

                                    {!game.isDeleted ? (
                                        <button
                                            onClick={() => handleDelete(game.gameId)}
                                            className={`${styles.actionButton} ${styles.deleteBtn}`}
                                        >
                                            Delete
                                        </button>
                                    ) : (
                                        <button
                                            onClick={() => handleRestore(game.gameId)}
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

export default AdminGames;
