import React, { useState, useEffect } from 'react';
import apiClient from '../../utils/apiClient';

function AdminDashboard() {
    const [stats, setStats] = useState({ totalUsers: 0, totalGames: 0, activeSubscriptions: 0 });
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchStats = async () => {
            try {
                const response = await apiClient.get('/admin/stats');
                if (response.ok) {
                    const data = await response.json();
                    setStats(data);
                }
            } catch (error) {
                console.error("Failed to fetch dashboard stats", error);
            } finally {
                setLoading(false);
            }
        };
        fetchStats();
    }, []);

    return (
        <div>
            <h1 style={{ marginBottom: '1rem', fontSize: '2rem' }}>Dashboard</h1>
            <p style={{ color: '#ccc' }}>Welcome to the Admin Portal.</p>

            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))', gap: '1rem', marginTop: '2rem' }}>
                <div style={{ background: '#1e1e1e', padding: '1.5rem', borderRadius: '8px', border: '1px solid #333' }}>
                    <h3 style={{ color: '#888', fontSize: '0.9rem', marginBottom: '0.5rem' }}>Total Users</h3>
                    <p style={{ fontSize: '1.8rem', fontWeight: 'bold' }}>
                        {loading ? '...' : stats.totalUsers}
                    </p>
                </div>
                <div style={{ background: '#1e1e1e', padding: '1.5rem', borderRadius: '8px', border: '1px solid #333' }}>
                    <h3 style={{ color: '#888', fontSize: '0.9rem', marginBottom: '0.5rem' }}>Total Games</h3>
                    <p style={{ fontSize: '1.8rem', fontWeight: 'bold' }}>
                        {loading ? '...' : stats.totalGames}
                    </p>
                </div>
                <div style={{ background: '#1e1e1e', padding: '1.5rem', borderRadius: '8px', border: '1px solid #333' }}>
                    <h3 style={{ color: '#888', fontSize: '0.9rem', marginBottom: '0.5rem' }}>Active Subscriptions</h3>
                    <p style={{ fontSize: '1.8rem', fontWeight: 'bold' }}>
                        {loading ? '...' : stats.activeSubscriptions}
                    </p>
                </div>
            </div>
        </div>
    );
}

export default AdminDashboard;
