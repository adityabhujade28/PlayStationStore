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
    const [countries, setCountries] = useState([]);

    const [formData, setFormData] = useState({
        gameName: '',
        publishedBy: '',
        price: '',
        releaseDate: '',
        freeToPlay: false,
        isMultiplayer: false,
        imageUrl: '',
        categoryIds: [], // TODO: Implement category selection
        pricing: [] // Per-country pricing
    });

    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);

    useEffect(() => {
        fetchCountries();
        if (isEditing) {
            fetchGameDetails();
        }
    }, [id]);

    const fetchCountries = async () => {
        try {
            const response = await apiClient.get('/countries');
            if (response.ok) {
                const data = await response.json();
                setCountries(data);
                // Initialize pricing for all countries when creating new game
                if (!isEditing) {
                    setFormData(prev => ({
                        ...prev,
                        pricing: data.map(c => ({
                            countryId: c.countryId,
                            price: '',
                            countryName: c.countryName,
                            currency: c.currency
                        }))
                    }));
                }
            }
        } catch (err) {
            console.error('Error fetching countries:', err);
        }
    };

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
                    imageUrl: data.imageUrl || '',
                    categoryIds: [],
                    pricing: [] // Will be populated below
                });
            } else {
                setError('Failed to load game details');
            }

            // Fetch pricing data and map it to the same format used for creation
            const pricingRes = await apiClient.get(`/games/${id}/pricing`);
            if (pricingRes.ok) {
                const pricingData = await pricingRes.json();
                setPricing(pricingData);
                
                // Map pricing data to formData format for editing
                setFormData(prev => ({
                    ...prev,
                    pricing: pricingData.map(p => ({
                        countryId: p.countryId,
                        gameCountryId: p.gameCountryId, // Include for updates
                        price: p.price,
                        countryName: p.countryName,
                        currency: p.currency
                    }))
                }));
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
                gameName: formData.gameName,
                publishedBy: formData.publishedBy,
                price: parseFloat(formData.price) || 0, // Keep for backward compatibility
                releaseDate: formData.releaseDate ? new Date(formData.releaseDate).toISOString() : null,
                freeToPlay: formData.freeToPlay,
                isMultiplayer: formData.isMultiplayer,
                imageUrl: formData.imageUrl,
                categoryIds: formData.categoryIds,
                // Add pricing data for game creation
                pricing: formData.pricing
                    ? formData.pricing
                        .filter(p => p.price !== '' && p.price !== null)
                        .map(p => ({
                            countryId: p.countryId,
                            price: parseFloat(p.price)
                        }))
                    : []
            };

            console.log("Submitting payload:", JSON.stringify(payload, null, 2));

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
                        required
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
                        required
                    />
                </div>

                <div className={styles.formGroup}>
                    <label className={styles.label}>Image URL</label>
                    <input
                        type="text"
                        name="imageUrl"
                        className={styles.input}
                        value={formData.imageUrl}
                        onChange={handleChange}
                        placeholder="/game_images/Game_Name.jpg"
                        required
                    />
                    {formData.imageUrl && (
                        <div style={{ marginTop: '1rem' }}>
                            <img
                                src={formData.imageUrl}
                                alt="Game Preview"
                                style={{
                                    maxWidth: '200px',
                                    maxHeight: '150px',
                                    borderRadius: '4px',
                                    border: '1px solid #444'
                                }}
                            />
                        </div>
                    )}
                </div>

                {/* Per-Country Pricing */}
                {!formData.freeToPlay && formData.pricing && formData.pricing.length > 0 && (
                    <div style={{ marginBottom: '2rem', padding: '1.5rem', background: '#1a1a1a', borderRadius: '8px', border: '1px solid #333' }}>
                        <h3 style={{ marginTop: 0, marginBottom: '1rem', color: '#4db8ff' }}>Regional Pricing</h3>
                        {formData.pricing.map((p, index) => (
                            <div key={p.countryId} className={styles.formGroup}>
                                <label className={styles.label}>
                                    {p.countryName} ({p.currency})
                                </label>
                                <input
                                    type="number"
                                    className={styles.input}
                                    value={p.price}
                                    onChange={(e) => {
                                        const newPricing = [...formData.pricing];
                                        // Allow empty string or valid number (including 0)
                                        const val = e.target.value;
                                        newPricing[index].price = val === '' ? '' : parseFloat(val);
                                        setFormData(prev => ({ ...prev, pricing: newPricing }));
                                    }}
                                    onBlur={async (e) => {
                                        // If editing, update the price in backend immediately
                                        if (isEditing && p.gameCountryId) {
                                            const val = parseFloat(e.target.value);
                                            if (!isNaN(val)) {
                                                try {
                                                    const res = await apiClient.put(`/games/pricing/${p.gameCountryId}`, val);
                                                    if (res.ok) {
                                                        console.log('Price updated for', p.countryName);
                                                    }
                                                } catch (err) {
                                                    console.error('Failed to update price:', err);
                                                }
                                            }
                                        }
                                    }}
                                    min="0"
                                    step="0.01"
                                    placeholder={`Enter price in ${p.currency}`}
                                    required
                                />
                            </div>
                        ))}
                    </div>
                )}

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
        </div>
    );
}

export default AdminGameForm;
