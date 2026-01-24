import React, { useState, useEffect } from 'react';
import Pagination from '../../components/Pagination';
import apiClient from '../../utils/apiClient';
import styles from './AdminGames.module.css'; // Reusing common admin styles

function AdminUsers() {
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [currentPage, setCurrentPage] = useState(1);
    const [pageSize] = useState(25);
    const [searchTerm, setSearchTerm] = useState('');

    const [paginatedData, setPaginatedData] = useState({
        items: [],
        totalCount: 0,
        pageNumber: 1,
        pageSize: 25,
        totalPages: 0,
        hasNextPage: false,
        hasPreviousPage: false
    });
    const [countries, setCountries] = useState({});

    useEffect(() => {
        setCurrentPage(1); // Reset to page 1 when search changes
    }, [searchTerm]);

    useEffect(() => {
        fetchUsers();
    }, [currentPage, searchTerm]);

    const fetchUsers = async () => {
        setLoading(true);
        try {
            const params = new URLSearchParams({
                pageNumber: currentPage,
                pageSize: pageSize,
                includeDeleted: 'true',
                sortBy: 'name'
            });

            if (searchTerm) {
                params.set('searchTerm', searchTerm);
            }

            const response = await apiClient.get(`/users/paged?${params.toString()}`);
            if (response.ok) {
                const data = await response.json();
                setPaginatedData(data);

                // Fetch country names for unique country IDs
                const uniqueCountryIds = [...new Set(data.items.map(u => u.countryId).filter(Boolean))];
                const countryMap = {};
                
                for (const countryId of uniqueCountryIds) {
                    try {
                        const countryRes = await apiClient.get(`/countries/${countryId}`);
                        if (countryRes.ok) {
                            const countryData = await countryRes.json();
                            countryMap[countryId] = countryData.countryName;
                        }
                    } catch (err) {
                        console.error(`Failed to fetch country ${countryId}:`, err);
                    }
                }
                setCountries(countryMap);
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

    const handlePageChange = (pageNumber) => {
        setCurrentPage(pageNumber);
        window.scrollTo({ top: 0, behavior: 'smooth' });
    };

    const handleSearchChange = (e) => {
        setSearchTerm(e.target.value);
    };

    const handleClearSearch = () => {
        setSearchTerm('');
    };

    if (error) return <div className={styles.container} style={{ color: 'red' }}>{error}</div>;

    return (
        <div className={styles.container}>
            <div className={styles.header}>
                <h1 className={styles.title}>User Management</h1>
            </div>

            {/* Search Bar */}
            <div style={{ marginBottom: '1.5rem' }}>
                <input
                    type="text"
                    placeholder="Search by name or email..."
                    value={searchTerm}
                    onChange={handleSearchChange}
                    style={{
                        width: '100%',
                        padding: '0.75rem',
                        borderRadius: '6px',
                        border: '2px solid #e0e0e0',
                        fontSize: '1rem',
                        boxSizing: 'border-box'
                    }}
                />
                {searchTerm && (
                    <button
                        onClick={handleClearSearch}
                        style={{
                            marginTop: '0.5rem',
                            padding: '0.5rem 1rem',
                            background: '#667eea',
                            color: 'white',
                            border: 'none',
                            borderRadius: '4px',
                            cursor: 'pointer'
                        }}
                    >
                        Clear Search
                    </button>
                )}
            </div>

            {loading && <div className={styles.loading}>Loading users...</div>}

            {!loading && (
                <>
                    <div className={styles.tableContainer}>
                        <table className={styles.table}>
                            <thead>
                                <tr>
                                    <th>User Name</th>
                                    <th>Email</th>
                                    <th>Age</th>
                                    <th>Country</th>
                                    <th>Subscription</th>
                                    <th>Status</th>
                                    <th>Joined</th>
                                    <th>Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                {paginatedData.items.map((user) => (
                                    <tr key={user.userId} style={{ opacity: user.isDeleted ? 0.5 : 1 }}>
                                        <td>{user.userName}</td>
                                        <td>{user.userEmail}</td>
                                        <td>{user.age}</td>
                                        <td>
                                            <span style={{ fontWeight: '500' }}>
                                                {countries[user.countryId] || 'N/A'}
                                            </span>
                                        </td>
                                        <td>
                                            {user.subscriptionStatus ? (
                                                <span style={{ color: '#4db8ff', fontWeight: 'bold' }}>✓ Active</span>
                                            ) : (
                                                <span style={{ color: '#999' }}>— None</span>
                                            )}
                                        </td>
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


export default AdminUsers;
