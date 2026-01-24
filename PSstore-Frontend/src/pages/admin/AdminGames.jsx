import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import Pagination from '../../components/Pagination';
import apiClient from '../../utils/apiClient';
import styles from './AdminGames.module.css';

function AdminGames() {
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [currentPage, setCurrentPage] = useState(1);
    const [pageSize] = useState(25);
    
    const [paginatedData, setPaginatedData] = useState({
        items: [],
        totalCount: 0,
        pageNumber: 1,
        pageSize: 25,
        totalPages: 0,
        hasNextPage: false,
        hasPreviousPage: false
    });

    useEffect(() => {
        fetchGames();
    }, [currentPage]);

    const fetchGames = async () => {
        setLoading(true);
        try {
            const params = new URLSearchParams({
                pageNumber: currentPage,
                pageSize: pageSize,
                includeDeleted: 'true',
                sortBy: 'name'
            });

            const response = await apiClient.get(`/games/paged?${params.toString()}`);
            if (response.ok) {
                const data = await response.json();
                setPaginatedData(data);
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

    const handlePageChange = (pageNumber) => {
        setCurrentPage(pageNumber);
        window.scrollTo({ top: 0, behavior: 'smooth' });
    };

    if (error) return <div className={styles.container} style={{ color: 'red' }}>{error}</div>;

    return (
        <div className={styles.container}>
            <div className={styles.header}>
                <h1 className={styles.title}>Manage Games</h1>
                <Link to="/admin/games/new" className={styles.addButton}>+ Add New Game</Link>
            </div>

            {loading && <div className={styles.loading}>Loading games...</div>}

            {!loading && (
                <>
                    <div className={styles.tableContainer}>
                        <table className={styles.table}>
                            <thead>
                                <tr>
                                    <th>Image</th>
                                    <th>Name</th>
                                    <th>Publisher</th>
                                    <th>Price</th>
                                    <th>Status</th>
                                    <th>Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                {paginatedData.items.map((game) => (
                                    <tr key={game.gameId}>
                                        <td>
                                            <div className={styles.thumbnailContainer}>
                                                {game.imageUrl ? (
                                                    <img src={game.imageUrl} alt={game.gameName} className={styles.thumbnail} />
                                                ) : (
                                                    <div className={styles.placeholderImage}>No Image</div>
                                                )}
                                            </div>
                                        </td>
                                        <td>{game.gameName}</td>
                                        <td>{game.publishedBy}</td>
                                        <td className={styles.price}>
                                            {game.freeToPlay ? 'Free' : `₹${game.price}`}
                                        </td>
                                        <td>
                                            {game.isDeleted ? (
                                                <span style={{ color: '#ff6b6b' }}>Deleted</span>
                                            ) : (
                                                <span style={{ color: '#4db8ff' }}>Active</span>
                                            )}
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

                    {/* Pagination Controls */}
                    <Pagination
                        currentPage={paginatedData.pageNumber}
                        totalPages={paginatedData.totalPages}
                        hasNextPage={paginatedData.hasNextPage}
                        hasPreviousPage={paginatedData.hasPreviousPage}
                        onPageChange={handlePageChange}
                        isLoading={loading}
                    />
                </>
            )}
        </div>
    );
}

export default AdminGames;
