import React, { useState } from 'react';
import Pagination from '../../components/Pagination';
import apiClient from '../../utils/apiClient';
import styles from './AdminGames.module.css';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import UserDetailsModal from './UserDetailsModal';

function AdminUsers() {
    const [currentPage, setCurrentPage] = useState(1);
    const pageSize = 25;
    const [searchTerm, setSearchTerm] = useState('');
    const [selectedCountry, setSelectedCountry] = useState('');
    const queryClient = useQueryClient();

    // Modal state
    const [selectedUser, setSelectedUser] = useState(null);
    const [showModal, setShowModal] = useState(false);
    const [modalSubscription, setModalSubscription] = useState(null);
    const [modalSubscriptionPlan, setModalSubscriptionPlan] = useState(null);
    const [modalCountry, setModalCountry] = useState(null);

    // Fetch countries for filter dropdown
    const { data: allCountries = [] } = useQuery({
        queryKey: ['countries'],
        queryFn: async () => {
            const response = await apiClient.get('/countries');
            if (!response.ok) throw new Error('Failed to fetch countries');
            return response.json();
        },
        staleTime: 60 * 60 * 1000, // 1 hour cache
    });

    // Fetch users with pagination + search - query updates when page or search changes
    const { data: paginatedData = {
        items: [],
        totalCount: 0,
        pageNumber: 1,
        pageSize: 25,
        totalPages: 0,
        hasNextPage: false,
        hasPreviousPage: false
    }, isLoading, error } = useQuery({
        queryKey: ['adminUsers', { page: currentPage, pageSize, search: searchTerm, country: selectedCountry }],
        queryFn: async () => {
            const params = new URLSearchParams({
                pageNumber: currentPage,
                pageSize: pageSize,
                includeDeleted: 'true',
                sortBy: 'name'
            });

            if (searchTerm) {
                params.set('searchTerm', searchTerm);
            }

            if (selectedCountry) {
                params.set('countryId', selectedCountry);
            }

            const response = await apiClient.get(`/users/paged?${params.toString()}`);
            if (!response.ok) throw new Error('Failed to fetch users');
            return response.json();
        },
        staleTime: 5 * 60 * 1000, // 5 min cache
        keepPreviousData: true, // Show old data while fetching new page
    });

    // Fetch countries for all user IDs in current page
    const uniqueCountryIds = paginatedData.items
        ? [...new Set(paginatedData.items.map(u => u.countryId).filter(Boolean))]
        : [];

    const { data: countryNames = {} } = useQuery({
        queryKey: ['countries', uniqueCountryIds.join(',')],
        queryFn: async () => {
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
            return countryMap;
        },
        enabled: uniqueCountryIds.length > 0,
        staleTime: 10 * 60 * 1000, // 10 min cache
    });

    const handlePageChange = (page) => {
        setCurrentPage(page);
    };

    const handleSearchChange = (e) => {
        setSearchTerm(e.target.value);
        setCurrentPage(1); // Reset to first page on search
    };

    const handleClearSearch = () => {
        setSearchTerm('');
        setCurrentPage(1);
    };

    const handleBlock = async (userId) => {
        if (!window.confirm('Are you sure you want to block this user?')) return;

        try {
            const response = await apiClient.delete(`/users/${userId}`);
            if (response.ok) {
                queryClient.invalidateQueries(['adminUsers']);
                alert('User blocked successfully!');
            } else {
                alert('Failed to block user.');
            }
        } catch (error) {
            console.error('Error blocking user:', error);
            alert('An error occurred while blocking the user.');
        }
    };

    const handleRestore = async (userId) => {
        if (!window.confirm('Are you sure you want to restore this user?')) return;

        try {
            const response = await apiClient.post(`/users/${userId}/restore`);
            if (response.ok) {
                queryClient.invalidateQueries(['adminUsers']);
                alert('User restored successfully!');
            } else {
                alert('Failed to restore user.');
            }
        } catch (error) {
            console.error('Error restoring user:', error);
            alert('An error occurred while restoring the user.');
        }
    };

    const handleUserClick = async (user) => {
        console.log('User clicked:', user);
        setSelectedUser(user);
        setShowModal(true);

        // Fetch subscription details
        try {
            console.log('Fetching subscription for user:', user.userId);
            const subResponse = await apiClient.get(`/Subscriptions/user/${user.userId}/active`);
            console.log('Subscription response:', subResponse.status, subResponse.ok);

            // 404 means no active subscription, not an error
            if (subResponse.status === 404) {
                console.log('No active subscription found (404)');
                setModalSubscription(null);
                setModalSubscriptionPlan(null);
            } else if (subResponse.ok) {
                const subData = await subResponse.json();
                console.log('Subscription data received:', subData);
                setModalSubscription(subData);

                // Fetch subscription plan details
                const plansResponse = await apiClient.get('/Subscriptions/plans');
                if (plansResponse.ok) {
                    const plans = await plansResponse.json();
                    console.log('All subscription plans:', plans);
                    // Match by subscription name since UserSubscriptionDTO includes subscriptionName
                    const plan = plans.find(p => p.subscriptionName === subData.subscriptionName);
                    console.log('Matched plan for subscription:', plan);
                    setModalSubscriptionPlan(plan);
                }
            } else {
                console.log('Unexpected response status:', subResponse.status);
                setModalSubscription(null);
                setModalSubscriptionPlan(null);
            }
        } catch (error) {
            console.error('Error fetching subscription:', error);
            setModalSubscription(null);
            setModalSubscriptionPlan(null);
        }

        // Fetch country details
        try {
            const countryResponse = await apiClient.get(`/countries/${user.countryId}`);
            if (countryResponse.ok) {
                const countryData = await countryResponse.json();
                console.log('Country data:', countryData);
                setModalCountry(countryData.countryName);
            }
        } catch (error) {
            console.error('Error fetching country:', error);
            setModalCountry(null);
        }
    };

    const handleCloseModal = () => {
        setShowModal(false);
        setSelectedUser(null);
        setModalSubscription(null);
        setModalSubscriptionPlan(null);
        setModalCountry(null);
    };

    if (error) return <div className={styles.container} style={{ color: 'red' }}>{error.message}</div>;

    return (
        <div className={styles.container}>
            <div className={styles.header}>
                <h1 className={styles.title}>User Management</h1>
            </div>

            {/* Search and Filter Bar */}
            <div style={{ marginBottom: '1.5rem', display: 'flex', gap: '1rem', alignItems: 'flex-start', flexWrap: 'wrap' }}>
                <div style={{ flex: '1', minWidth: '250px' }}>
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
                </div>
                <div style={{ minWidth: '200px' }}>
                    <select
                        value={selectedCountry}
                        onChange={(e) => {
                            setSelectedCountry(e.target.value);
                            setCurrentPage(1);
                        }}
                        style={{
                            width: '100%',
                            padding: '0.75rem',
                            borderRadius: '6px',
                            border: '2px solid #e0e0e0',
                            fontSize: '1rem',
                            backgroundColor: 'white',
                            cursor: 'pointer'
                        }}
                    >
                        <option value="">All Countries</option>
                        {allCountries.map(country => (
                            <option key={country.countryId} value={country.countryId}>
                                {country.countryName}
                            </option>
                        ))}
                    </select>
                </div>
                {(searchTerm || selectedCountry) && (
                    <button
                        onClick={() => {
                            setSearchTerm('');
                            setSelectedCountry('');
                            setCurrentPage(1);
                        }}
                        style={{
                            padding: '0.75rem 1.5rem',
                            borderRadius: '6px',
                            border: 'none',
                            backgroundColor: '#6c757d',
                            color: 'white',
                            fontSize: '1rem',
                            cursor: 'pointer',
                            fontWeight: '500'
                        }}
                    >
                        Clear Filters
                    </button>
                )}
            </div>
            {(searchTerm || selectedCountry) && (
                <p style={{ marginBottom: '1rem', color: '#666', fontSize: '0.95rem' }}>
                    Showing results for: {searchTerm && `"${searchTerm}"`} {searchTerm && selectedCountry && '+ '}
                    {selectedCountry && `${allCountries.find(c => c.countryId === selectedCountry)?.countryName || 'Selected Country'}`}
                </p>
            )}

            {isLoading ? (
                <div style={{ textAlign: 'center', padding: '2rem' }}>Loading users...</div>
            ) : paginatedData.items.length === 0 ? (
                <div style={{ textAlign: 'center', padding: '2rem', color: '#666' }}>
                    No users found{searchTerm || selectedCountry ? ' matching your filters' : ''}.
                </div>
            ) : (
                <>
                    <div className={styles.tableContainer}>
                        <table className={styles.table}>
                            <thead>
                                <tr>
                                    <th>User Name</th>
                                    <th>Email</th>
                                    <th>Age</th>
                                    <th>Country</th>
                                    <th>Status</th>
                                    <th>Joined</th>
                                    <th>Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                {paginatedData.items.map((user) => (
                                    <tr
                                        key={user.userId}
                                        style={{ opacity: user.isDeleted ? 0.5 : 1, cursor: 'pointer' }}
                                        onClick={() => handleUserClick(user)}
                                        title="Click to view details"
                                    >
                                        <td>{user.userName}</td>
                                        <td>{user.userEmail}</td>
                                        <td>{user.age}</td>
                                        <td>
                                            <span style={{ fontWeight: '500' }}>
                                                {countryNames[user.countryId] || 'N/A'}
                                            </span>
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
                                                    onClick={(e) => {
                                                        e.stopPropagation();
                                                        handleBlock(user.userId);
                                                    }}
                                                    className={`${styles.actionButton} ${styles.deleteBtn}`}
                                                >
                                                    Block
                                                </button>
                                            ) : (
                                                <button
                                                    onClick={(e) => {
                                                        e.stopPropagation();
                                                        handleRestore(user.userId);
                                                    }}
                                                    className={`${styles.actionButton}`}
                                                    style={{ backgroundColor: '#28a745' }}
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

                    <Pagination
                        currentPage={paginatedData.pageNumber}
                        totalPages={paginatedData.totalPages}
                        onPageChange={handlePageChange}
                    />
                </>
            )}

            {showModal && (
                <UserDetailsModal
                    user={selectedUser}
                    subscription={modalSubscription}
                    subscriptionPlan={modalSubscriptionPlan}
                    country={modalCountry}
                    onClose={handleCloseModal}
                />
            )}
        </div>
    );
}

export default AdminUsers;
