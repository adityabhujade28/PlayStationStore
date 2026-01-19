import React, { useState, useEffect } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import apiClient from '../../utils/apiClient';
import styles from './AdminGameForm.module.css';

function AdminGameForm() {
    const { id } = useParams();
    const navigate = useNavigate();
    const isEditing = !!id;
    const [pricing, setPricing] = useState([]);
    const [newPriceData, setNewPriceData] = useState({ countryId: '', price: 0 });

    const [formData, setFormData] = useState({
        gameName: '',
        publishedBy: '',
        price: '',
        releaseDate: '',
        freeToPlay: false,
        isMultiplayer: false,
        categoryIds: [] // TODO: Implement category selection
    });

    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);

    useEffect(() => {
        if (isEditing) {
            fetchGameDetails();
        }
    }, [id]);

    const fetchGameDetails = async () => {
        try {
            setLoading(true);
            const response = await apiClient.get(`/games/${id}`);
            if (response.ok) {
                const data = await response.json();
                setFormData({
                    gameName: data.gameName,
                    publishedBy: data.publishedBy || '',
                    price: data.price,
                    releaseDate: data.releaseDate ? data.releaseDate.split('T')[0] : '',
                    freeToPlay: data.freeToPlay,
                    isMultiplayer: data.isMultiplayer,
                    categoryIds: []
                });
            } else {
                setError('Failed to load game details');
            }

            // Fetch pricing if not free to play (though admin might want to configure it anyway, usually only if paid)
            // But we fetch always to show grid
            const pricingRes = await apiClient.get(`/games/${id}/pricing`);
            if (pricingRes.ok) {
                setPricing(await pricingRes.json());
            }

        } catch (err) {
            setError('Error loading game');
        } finally {
            setLoading(false);
        }
    };

    const handleChange = (e) => {
        const { name, value, type, checked } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: type === 'checkbox' ? checked : value
        }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setLoading(true);
        setError(null);

        try {
            const payload = {
                ...formData,
                price: parseFloat(formData.price) || 0,
                releaseDate: formData.releaseDate ? new Date(formData.releaseDate).toISOString() : null
            };

            let response;
            if (isEditing) {
                response = await apiClient.put(`/games/${id}`, payload);
            } else {
                response = await apiClient.post('/games', payload);
            }

            if (response.ok) {
                navigate('/admin/games');
            } else {
                const errData = await response.json();
                setError(errData.message || 'Failed to save game');
            }
        } catch (err) {
            setError('An error occurred while saving.');
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    if (loading && isEditing && !formData.gameName) return <div className={styles.container}>Loading...</div>;

    return (
        <div className={styles.container}>
            <h1 className={styles.title}>{isEditing ? 'Edit Game' : 'Add New Game'}</h1>

            {error && <div className={styles.error}>{error}</div>}

            <form onSubmit={handleSubmit}>
                <div className={styles.formGroup}>
                    <label className={styles.label}>Game Name</label>
                    <input
                        type="text"
                        name="gameName"
                        className={styles.input}
                        value={formData.gameName}
                        onChange={handleChange}
                        required
                    />
                </div>

                <div className={styles.formGroup}>
                    <label className={styles.label}>Publisher</label>
                    <input
                        type="text"
                        name="publishedBy"
                        className={styles.input}
                        value={formData.publishedBy}
                        onChange={handleChange}
                    />
                </div>

                <div className={styles.formGroup}>
                    <label className={styles.label}>Release Date</label>
                    <input
                        type="date"
                        name="releaseDate"
                        className={styles.input}
                        value={formData.releaseDate}
                        onChange={handleChange}
                    />
                </div>

                <div className={styles.formGroup}>
                    <label className={styles.label}>Price (₹)</label>
                    <input
                        type="number"
                        name="price"
                        className={styles.input}
                        value={formData.price}
                        onChange={handleChange}
                        disabled={formData.freeToPlay}
                        min="0"
                        step="0.01"
                    />
                </div>

                <div className={styles.checkboxGroup}>
                    <input
                        type="checkbox"
                        name="freeToPlay"
                        id="freeToPlay"
                        className={styles.checkbox}
                        checked={formData.freeToPlay}
                        onChange={handleChange}
                    />
                    <label htmlFor="freeToPlay" className={styles.label} style={{ marginBottom: 0, cursor: 'pointer' }}>Free to Play</label>
                </div>

                <div className={styles.checkboxGroup}>
                    <input
                        type="checkbox"
                        name="isMultiplayer"
                        id="isMultiplayer"
                        className={styles.checkbox}
                        checked={formData.isMultiplayer}
                        onChange={handleChange}
                    />
                    <label htmlFor="isMultiplayer" className={styles.label} style={{ marginBottom: 0, cursor: 'pointer' }}>Multiplayer Supported</label>
                </div>

                <div className={styles.actions}>
                    <Link to="/admin/games" className={styles.cancelButton}>Cancel</Link>
                    <button type="submit" className={styles.saveButton} disabled={loading}>
                        {loading ? 'Saving...' : (isEditing ? 'Update Game' : 'Create Game')}
                    </button>
                </div>
            </form>

            {isEditing && !formData.freeToPlay && (
                <div style={{ marginTop: '3rem', borderTop: '1px solid #444', paddingTop: '2rem' }}>
                    <h2 style={{ marginBottom: '1rem' }}>Regional Pricing</h2>
                    <div className={styles.tableContainer}>
                        <table className={styles.table}>
                            <thead>
                                <tr>
                                    <th>Region / Country</th>
                                    <th>Currency</th>
                                    <th>Price</th>
                                    <th>Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                {pricing.map((p) => (
                                    <tr key={p.gameCountryId}>
                                        <td>{p.countryName}</td>
                                        <td>{p.currency}</td>
                                        <td>
                                            <input
                                                type="number"
                                                defaultValue={p.price}
                                                onBlur={async (e) => {
                                                    const val = parseFloat(e.target.value);
                                                    if (val !== p.price) {
                                                        try {
                                                            const res = await apiClient.put(`/games/pricing/${p.gameCountryId}`, val);
                                                            if (res.ok) alert('Price updated');
                                                        } catch (err) { console.error(err); }
                                                    }
                                                }}
                                                style={{ background: '#333', border: '1px solid #555', color: 'white', padding: '5px', width: '100px' }}
                                            />
                                        </td>
                                        <td>
                                            {/* Delete? Maybe later */}
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>

                    <div style={{ marginTop: '2rem', background: '#252525', padding: '15px', borderRadius: '8px' }}>
                        <h3 style={{ marginBottom: '1rem', fontSize: '1.2rem' }}>Add Regional Price</h3>
                        <div style={{ display: 'flex', gap: '10px', alignItems: 'flex-end' }}>
                            <div style={{ display: 'flex', flexDirection: 'column' }}>
                                <label style={{ fontSize: '0.8rem', marginBottom: '5px' }}>Country ID</label>
                                <input
                                    type="text"
                                    placeholder="Guid"
                                    value={newPriceData.countryId}
                                    onChange={(e) => setNewPriceData({ ...newPriceData, countryId: e.target.value })}
                                    style={{ padding: '8px', borderRadius: '4px', border: '1px solid #555', background: '#333', color: 'white' }}
                                />
                            </div>
                            <div style={{ display: 'flex', flexDirection: 'column' }}>
                                <label style={{ fontSize: '0.8rem', marginBottom: '5px' }}>Price</label>
                                <input
                                    type="number"
                                    placeholder="0.00"
                                    value={newPriceData.price}
                                    onChange={(e) => setNewPriceData({ ...newPriceData, price: e.target.value })}
                                    style={{ padding: '8px', borderRadius: '4px', border: '1px solid #555', background: '#333', color: 'white' }}
                                />
                            </div>
                            <button
                                onClick={async () => {
                                    if (!newPriceData.countryId || !newPriceData.price) return alert('Fill all fields');
                                    try {
                                        const res = await apiClient.post('/games/pricing', {
                                            gameId: id,
                                            countryId: newPriceData.countryId,
                                            price: parseFloat(newPriceData.price)
                                        });
                                        if (res.ok) {
                                            const created = await res.json();
                                            setPricing(prev => [...prev, created]);
                                            setNewPriceData({ countryId: '', price: 0 });
                                            alert('Added!');
                                        } else {
                                            alert('Failed. Check Country ID.');
                                        }
                                    } catch (e) { console.error(e); }
                                }}
                                className={styles.saveButton}
                                style={{ height: '38px', marginTop: 0 }}
                            >
                                Add
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

export default AdminGameForm;
