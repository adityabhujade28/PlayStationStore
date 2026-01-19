import React, { useState, useEffect } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import apiClient from '../../utils/apiClient';
import styles from './AdminGameForm.module.css';

const PriceCell = ({ initialPrice, planCountryId }) => {
    const [price, setPrice] = useState(initialPrice);
    const [isDirty, setIsDirty] = useState(false);
    const [updating, setUpdating] = useState(false);

    const handleChange = (e) => {
        const val = e.target.value;
        setPrice(val);
        setIsDirty(parseFloat(val) !== initialPrice);
    };

    const handleUpdate = async () => {
        if (!price) return;
        setUpdating(true);
        try {
            const res = await apiClient.put(`/subscriptions/pricing/${planCountryId}`, parseFloat(price));
            if (res.ok) {
                setIsDirty(false);
                // consistent visually
            } else {
                alert('Failed to update price');
            }
        } catch (err) {
            console.error(err);
            alert('Error updating price');
        } finally {
            setUpdating(false);
        }
    };

    return (
        <div style={{ display: 'flex', alignItems: 'center', gap: '5px' }}>
            <input
                type="number"
                value={price}
                onChange={handleChange}
                style={{
                    background: '#333',
                    border: isDirty ? '1px solid #0070d1' : '1px solid #555',
                    color: 'white',
                    padding: '6px',
                    width: '70px',
                    borderRadius: '4px'
                }}
            />
            <button
                onClick={handleUpdate}
                disabled={!isDirty || updating}
                style={{
                    background: isDirty ? '#0070d1' : '#444',
                    color: isDirty ? 'white' : '#888',
                    border: 'none',
                    borderRadius: '4px',
                    padding: '6px 8px',
                    cursor: isDirty ? 'pointer' : 'default',
                    fontSize: '0.75rem',
                    transition: 'all 0.2s'
                }}
            >
                {updating ? '...' : 'Save'}
            </button>
        </div>
    );
};

function AdminSubscriptionForm() {
    const { id } = useParams();
    const navigate = useNavigate();
    const isEditing = !!id;

    const [formData, setFormData] = useState({
        subscriptionType: ''
    });
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);

    const [pricing, setPricing] = useState([]);
    const [includedGames, setIncludedGames] = useState([]);
    const [allGames, setAllGames] = useState([]);

    // UI State
    const [showAddRow, setShowAddRow] = useState(false);

    const [newPriceData, setNewPriceData] = useState({
        countryName: '',
        price1: '',
        price3: '',
        price12: ''
    });

    const handleAddRow = async () => {
        if (!newPriceData.countryName) return alert('Please enter a country name');

        let successCount = 0;
        const pricesToAdd = [
            { duration: 1, price: newPriceData.price1 },
            { duration: 3, price: newPriceData.price3 },
            { duration: 12, price: newPriceData.price12 }
        ];

        setLoading(true);
        try {
            setLoading(true);
            try {
                const validItems = pricesToAdd.filter(item => item.price && parseFloat(item.price) > 0);

                const results = await Promise.all(validItems.map(async (item) => {
                    const payload = {
                        subscriptionId: id,
                        countryName: newPriceData.countryName,
                        durationMonths: item.duration,
                        price: parseFloat(item.price)
                    };

                    try {
                        const res = await apiClient.post('/subscriptions/pricing', payload);
                        if (res.ok) {
                            return await res.json();
                        }
                    } catch (e) {
                        console.error('Failed to add ' + item.duration, e);
                    }
                    return null;
                }));

                const successful = results.filter(r => r !== null);
                if (successful.length > 0) {
                    setPricing(prev => [...prev, ...successful]);
                    setNewPriceData({ countryName: '', price1: '', price3: '', price12: '' });
                    alert(`Successfully added ${successful.length} prices for ${newPriceData.countryName}`);
                } else {
                    alert('No prices were added. Please ensure at least one price is valid and the country exists.');
                }
            } catch (err) {
                console.error(err);
                alert('An error occurred.');
            } finally {
                setLoading(false);
            }
        }
        catch (err) {
            console.error(err);
            alert('An error occurred.');
        }
        finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        if (isEditing) {
            fetchPlanDetails();
        }
    }, [id]);

    const fetchPlanDetails = async () => {
            try {
                setLoading(true);
                // Fetch Plan Basic Info
                const response = await apiClient.get('/subscriptions/plans');
                if (response.ok) {
                    const plans = await response.json();
                    const plan = plans.find(p => p.subscriptionId === id);
                    if (plan) {
                        setFormData({
                            subscriptionType: plan.subscriptionName
                        });
                    } else {
                        setError('Plan not found in list');
                    }
                } else {
                    setError('Failed to load plans');
                }

                // Fetch Pricing
                const pricingRes = await apiClient.get(`/subscriptions/pricing/${id}`);
                if (pricingRes.ok) {
                    setPricing(await pricingRes.json());
                }

                // Fetch Included Games
                const includedRes = await apiClient.get(`/subscriptions/${id}/games`);
                if (includedRes.ok) {
                    setIncludedGames(await includedRes.json());
                }

                // Fetch All Games (for selection)
                const allGamesRes = await apiClient.get('/games?includeDeleted=false');
                if (allGamesRes.ok) {
                    setAllGames(await allGamesRes.json());
                }

            } catch (err) {
                setError('Error loading plan');
            } finally {
                setLoading(false);
            }
        };

        const handleChange = (e) => {
            const { name, value } = e.target;
            setFormData(prev => ({
                ...prev,
                [name]: value
            }));
        };

        const handleSubmit = async (e) => {
            e.preventDefault();
            setLoading(true);
            setError(null);

            try {
                let response;
                if (isEditing) {
                    response = await apiClient.put(`/subscriptions/plans/${id}`, formData);
                } else {
                    response = await apiClient.post('/subscriptions/plans', formData);
                }

                if (response.ok) {
                    if (isEditing) {
                        alert('Plan updated successfully!');
                    } else {
                        // If created, navigate to edit page to add prices
                        const createdPlan = await response.json();
                        navigate(`/admin/subscriptions/edit/${createdPlan.subscriptionId}`);
                    }
                } else {
                    const errData = await response.json();
                    setError(errData.message || 'Failed to save plan');
                }
            } catch (err) {
                setError('An error occurred while saving.');
                console.error(err);
            } finally {
                setLoading(false);
            }
        };

        // Group pricing by Country
        const grouped = pricing.reduce((acc, curr) => {
            if (!acc[curr.countryName]) acc[curr.countryName] = [];
            acc[curr.countryName].push(curr);
            return acc;
        }, {});

        if (loading && isEditing && !formData.subscriptionType) return <div className={styles.container}>Loading...</div>;

        return (
            <div className={styles.container}>
                <div className={styles.header}>
                    <h1 className={styles.title}>{isEditing ? 'Edit Subscription Plan' : 'Add New Plan'}</h1>
                    <button onClick={() => navigate('/admin/subscriptions')} className={styles.actionButton}>Back</button>
                </div>

                {error && <div className={styles.error}>{error}</div>}

                <form onSubmit={handleSubmit} style={{ marginBottom: '2rem' }}>
                    <div className={styles.formGroup}>
                        <label className={styles.label}>Plan Name (e.g., Ultra)</label>
                        <input
                            type="text"
                            name="subscriptionType"
                            className={styles.input}
                            value={formData.subscriptionType}
                            onChange={handleChange}
                            required
                            maxLength={50}
                        />
                    </div>

                    <div className={styles.actions}>
                        <button type="submit" className={styles.saveButton} disabled={loading}>
                            {loading ? 'Saving...' : (isEditing ? 'Update Name' : 'Create & Add Prices')}
                        </button>
                    </div>
                </form>

                {isEditing && (
                    <div style={{ marginTop: '3rem', borderTop: '1px solid #444', paddingTop: '2rem' }}>
                        <h2 style={{ marginBottom: '1rem' }}>Regional Pricing & Durations</h2>

                        <div className={styles.tableContainer}>
                            <table className={styles.table}>
                                <thead>
                                    <tr>
                                        <th>Region / Country</th>
                                        <th>Currency</th>
                                        <th>1 Month</th>
                                        <th>3 Months</th>
                                        <th>12 Months</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {Object.entries(grouped).map(([country, prices]) => {
                                        const currency = prices[0].currency;

                                        const getPriceCell = (months) => {
                                            const p = prices.find(x => x.durationMonths === months);
                                            if (!p) return <td key={months}><span style={{ color: '#555' }}>-</span></td>;

                                            return (
                                                <td key={months}>
                                                    <PriceCell
                                                        initialPrice={p.price}
                                                        planCountryId={p.planCountryId}
                                                    />
                                                </td>
                                            );
                                        };

                                        return (
                                            <tr key={country}>
                                                <td>{country}</td>
                                                <td>{currency}</td>
                                                {getPriceCell(1)}
                                                {getPriceCell(3)}
                                                {getPriceCell(12)}
                                            </tr>
                                        );
                                    })}
                                    {pricing.length === 0 && <tr><td colSpan="5" style={{ textAlign: 'center', color: '#888', padding: '20px' }}>No pricing configured yet. Add a country below.</td></tr>}

                                    {showAddRow && (
                                        <tr style={{ borderTop: '2px solid #444', background: '#2a2a2a' }}>
                                            <td>
                                                <input
                                                    type="text"
                                                    placeholder="Country Name (e.g. Japan)"
                                                    value={newPriceData.countryName}
                                                    onChange={(e) => setNewPriceData({ ...newPriceData, countryName: e.target.value })}
                                                    style={{ background: '#333', border: '1px solid #555', color: 'white', padding: '8px', width: '100%', borderRadius: '4px' }}
                                                />
                                            </td>
                                            <td style={{ color: '#888', fontSize: '0.8rem' }}>Auto</td>
                                            <td>
                                                <input
                                                    type="number"
                                                    placeholder="1 Mo"
                                                    value={newPriceData.price1}
                                                    onChange={(e) => setNewPriceData({ ...newPriceData, price1: e.target.value })}
                                                    style={{ background: '#333', border: '1px solid #555', color: 'white', padding: '8px', width: '80px', borderRadius: '4px' }}
                                                />
                                            </td>
                                            <td>
                                                <input
                                                    type="number"
                                                    placeholder="3 Mo"
                                                    value={newPriceData.price3}
                                                    onChange={(e) => setNewPriceData({ ...newPriceData, price3: e.target.value })}
                                                    style={{ background: '#333', border: '1px solid #555', color: 'white', padding: '8px', width: '80px', borderRadius: '4px' }}
                                                />
                                            </td>
                                            <td>
                                                <div style={{ display: 'flex', gap: '10px', alignItems: 'center' }}>
                                                    <input
                                                        type="number"
                                                        placeholder="12 Mo"
                                                        value={newPriceData.price12}
                                                        onChange={(e) => setNewPriceData({ ...newPriceData, price12: e.target.value })}
                                                        style={{ background: '#333', border: '1px solid #555', color: 'white', padding: '8px', width: '80px', borderRadius: '4px' }}
                                                    />
                                                    <button
                                                        onClick={handleAddRow}
                                                        disabled={!newPriceData.countryName}
                                                        style={{
                                                            background: newPriceData.countryName ? '#0070d1' : '#444',
                                                            color: 'white',
                                                            border: 'none',
                                                            padding: '8px 15px',
                                                            borderRadius: '4px',
                                                            cursor: newPriceData.countryName ? 'pointer' : 'not-allowed',
                                                            whiteSpace: 'nowrap'
                                                        }}
                                                    >
                                                        Save
                                                    </button>
                                                </div>
                                            </td>
                                        </tr>
                                    )}
                                </tbody>
                            </table>
                            {!showAddRow && (
                                <button
                                    onClick={() => setShowAddRow(true)}
                                    style={{
                                        marginTop: '10px',
                                        background: 'transparent',
                                        border: '1px dashed #555',
                                        color: '#888',
                                        width: '100%',
                                        padding: '10px',
                                        borderRadius: '4px',
                                        cursor: 'pointer',
                                        fontSize: '0.9rem'
                                    }}
                                >
                                    + Add Region Pricing
                                </button>
                            )}
                        </div>
                    </div>
                )}

                {isEditing && (
                    <div style={{ marginTop: '3rem', borderTop: '1px solid #444', paddingTop: '2rem' }}>
                        <h2 style={{ marginBottom: '1rem' }}>Included Games</h2>

                        <div style={{ display: 'flex', gap: '10px', marginBottom: '1rem' }}>
                            <select
                                id="gameSelect"
                                style={{ padding: '10px', flex: 1, background: '#333', color: 'white', border: '1px solid #555', borderRadius: '4px' }}
                            >
                                <option value="">Select a game to add...</option>
                                {allGames.filter(g => !includedGames.some(ig => ig.gameId === g.gameId)).map(g => (
                                    <option key={g.gameId} value={g.gameId}>{g.gameName}</option>
                                ))}
                            </select>
                            <button
                                onClick={async () => {
                                    const select = document.getElementById('gameSelect');
                                    const gameId = select.value;
                                    if (!gameId) return;

                                    try {
                                        const res = await apiClient.post(`/subscriptions/${id}/games/${gameId}`);
                                        if (res.ok) {
                                            fetchPlanDetails(); // Refresh
                                            select.value = '';
                                        } else {
                                            alert('Failed to add game');
                                        }
                                    } catch (e) { console.error(e); }
                                }}
                                className={styles.saveButton}
                                style={{ width: 'auto', marginTop: 0 }}
                            >
                                Add Game
                            </button>
                        </div>

                        <div className={styles.tableContainer}>
                            <table className={styles.table}>
                                <thead>
                                    <tr>
                                        <th>Game Name</th>
                                        <th>Developer</th>
                                        <th>Actions</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {includedGames.map(game => (
                                        <tr key={game.gameId}>
                                            <td>{game.gameName}</td>
                                            <td>{game.publishedBy || '-'}</td>
                                            <td>
                                                <button
                                                    onClick={async () => {
                                                        if (!window.confirm('Remove from plan?')) return;
                                                        try {
                                                            const res = await apiClient.delete(`/subscriptions/${id}/games/${game.gameId}`);
                                                            if (res.ok) fetchPlanDetails();
                                                        } catch (e) { console.error(e); }
                                                    }}
                                                    style={{ background: '#d32f2f', color: 'white', border: 'none', padding: '5px 10px', borderRadius: '4px', cursor: 'pointer' }}
                                                >
                                                    Remove
                                                </button>
                                            </td>
                                        </tr>
                                    ))}
                                    {includedGames.length === 0 && <tr><td colSpan="3" style={{ textAlign: 'center', color: '#888', padding: '20px' }}>No games included in this plan.</td></tr>}
                                </tbody>
                            </table>
                        </div>
                    </div>
                )}
            </div>
        );
    }

    export default AdminSubscriptionForm;
