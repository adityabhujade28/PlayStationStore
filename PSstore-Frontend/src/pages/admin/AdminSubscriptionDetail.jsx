import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import apiClient from '../../utils/apiClient';
import styles from './AdminGames.module.css'; // Reusing styles

function AdminSubscriptionDetail() {
    const { id } = useParams();
    const navigate = useNavigate();
    const [plan, setPlan] = useState(null);
    const [pricing, setPricing] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    // State for new price form
    const [isAdding, setIsAdding] = useState(false);
    const [newPriceData, setNewPriceData] = useState({ countryId: '', duration: 1, price: 0 });
    const [countries, setCountries] = useState([]);

    useEffect(() => {
        const fetchData = async () => {
            try {
                // Fetch Plan Basic Info (Reuse generic list for now or generic ID endpoint if exists)
                // We'll just fetch pricing and assume we know the plan name or fetch generic Details if needed.
                // Actually, let's fetch generic plans and find ours to get the name.
                const plansRes = await apiClient.get('/subscriptions/plans');
                if (plansRes.ok) {
                    const plans = await plansRes.json();
                    const p = plans.find(p => p.subscriptionId === id);
                    if (p) setPlan(p);
                }

                // Fetch Pricing
                const pricingRes = await apiClient.get(`/subscriptions/pricing/${id}`);
                if (pricingRes.ok) {
                    setPricing(await pricingRes.json());
                }

                // Fetch Countries for dropdown (We assume we have an endpoint or hardcode for now if missing)
                // We don't have a direct "GetAllCountries" for frontend yet in public API?
                // Let's assume we can add it or just use existing ones found in pricing to add more.
                // For now, I'll hardcode a few IDs or try to fetch.
                // Wait, we need country IDs to add new pricing. 
                // Let's SKIP adding new countries for this iteration and just allow editing existing.
                // OR add a quick endpoint for countries.

            } catch (err) {
                setError('Failed to load data');
                console.error(err);
            } finally {
                setLoading(false);
            }
        };
        fetchData();
    }, [id]);

    const handlePriceChange = async (planCountryId, newPrice) => {
        try {
            const res = await apiClient.put(`/subscriptions/pricing/${planCountryId}`, parseFloat(newPrice));
            if (res.ok) {
                // Update local state
                setPricing(prev => prev.map(p => p.planCountryId === planCountryId ? { ...p, price: newPrice } : p));
                alert('Price updated!');
            } else {
                alert('Failed to update price');
            }
        } catch (err) {
            console.error(err);
        }
    };

    if (loading) return <div className={styles.container}>Loading...</div>;
    if (!plan) return <div className={styles.container}>Plan not found</div>;

    // Group pricing by Country
    const grouped = pricing.reduce((acc, curr) => {
        if (!acc[curr.countryName]) acc[curr.countryName] = [];
        acc[curr.countryName].push(curr);
        return acc;
    }, {});

    return (
        <div className={styles.container}>
            <div className={styles.header}>
                <h1 className={styles.title}>{plan.subscriptionName} - Management</h1>
                <button onClick={() => navigate('/admin/subscriptions')} className={styles.actionButton}>Back</button>
            </div>

            <h2 style={{ marginTop: '2rem', marginBottom: '1rem' }}>Regional Pricing</h2>

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

                            // Find prices for specific durations
                            const getPriceInput = (months) => {
                                const p = prices.find(x => x.durationMonths === months);
                                if (!p) return <span style={{ color: '#555' }}>N/A</span>;

                                return (
                                    <input
                                        type="number"
                                        defaultValue={p.price}
                                        onBlur={(e) => {
                                            if (parseFloat(e.target.value) !== p.price) {
                                                handlePriceChange(p.planCountryId, e.target.value);
                                            }
                                        }}
                                        style={{
                                            background: '#333',
                                            border: '1px solid #555',
                                            color: 'white',
                                            padding: '5px',
                                            width: '80px'
                                        }}
                                    />
                                );
                            };

                            return (
                                <tr key={country}>
                                    <td>{country}</td>
                                    <td>{currency}</td>
                                    <td>{getPriceInput(1)}</td>
                                    <td>{getPriceInput(3)}</td>
                                    <td>{getPriceInput(12)}</td>
                                </tr>
                            );
                        })}
                    </tbody>
                </table>
            </div>
            <div style={{ marginTop: '2rem' }}>
                <h3 style={{ marginBottom: '1rem' }}>Add New Price</h3>
                <div style={{ display: 'flex', gap: '10px', alignItems: 'flex-end', background: '#333', padding: '15px', borderRadius: '8px' }}>

                    <div style={{ display: 'flex', flexDirection: 'column' }}>
                        <label style={{ fontSize: '0.8rem', marginBottom: '5px' }}>Country ID</label>
                        <input
                            type="text"
                            placeholder="Guid of Country"
                            value={newPriceData.countryId}
                            onChange={(e) => setNewPriceData({ ...newPriceData, countryId: e.target.value })}
                            style={{ padding: '8px', borderRadius: '4px', border: '1px solid #555', background: '#222', color: 'white' }}
                        />
                    </div>

                    <div style={{ display: 'flex', flexDirection: 'column' }}>
                        <label style={{ fontSize: '0.8rem', marginBottom: '5px' }}>Duration</label>
                        <select
                            value={newPriceData.duration}
                            onChange={(e) => setNewPriceData({ ...newPriceData, duration: parseInt(e.target.value) })}
                            style={{ padding: '8px', borderRadius: '4px', border: '1px solid #555', background: '#222', color: 'white' }}
                        >
                            <option value={1}>1 Month</option>
                            <option value={3}>3 Months</option>
                            <option value={12}>12 Months</option>
                        </select>
                    </div>

                    <div style={{ display: 'flex', flexDirection: 'column' }}>
                        <label style={{ fontSize: '0.8rem', marginBottom: '5px' }}>Price</label>
                        <input
                            type="number"
                            placeholder="0.00"
                            value={newPriceData.price}
                            onChange={(e) => setNewPriceData({ ...newPriceData, price: e.target.value })}
                            style={{ padding: '8px', borderRadius: '4px', border: '1px solid #555', background: '#222', color: 'white' }}
                        />
                    </div>

                    <button
                        onClick={async () => {
                            if (!newPriceData.countryId || !newPriceData.price) return alert('Please fill all fields');
                            try {
                                const res = await apiClient.post('/subscriptions/pricing', {
                                    subscriptionId: id,
                                    countryId: newPriceData.countryId,
                                    durationMonths: newPriceData.duration,
                                    price: parseFloat(newPriceData.price)
                                });
                                if (res.ok) {
                                    const created = await res.json();
                                    setPricing(prev => [...prev, created]);
                                    setNewPriceData({ countryId: '', duration: 1, price: 0 });
                                    alert('Price added!');
                                } else {
                                    const err = await res.json();
                                    alert(err.message || 'Failed to add price');
                                }
                            } catch (e) { console.error(e); alert('Error adding price'); }
                        }}
                        style={{ padding: '8px 16px', borderRadius: '4px', background: '#4CAF50', color: 'white', border: 'none', cursor: 'pointer', height: '36px' }}
                    >
                        Add Price
                    </button>
                </div>
                <p style={{ fontSize: '0.8rem', color: '#888', marginTop: '5px' }}>
                    *Note: You need the Region/Country Guid to add. In a real app this would be a dropdown.
                </p>
            </div>
        </div>
    );
}

export default AdminSubscriptionDetail;
