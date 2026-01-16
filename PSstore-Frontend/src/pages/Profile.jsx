import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import Navbar from '../components/Navbar';
import styles from './Profile.module.css';

function Profile() {
  const { token, getDecodedToken } = useAuth();
  const [user, setUser] = useState(null);
  const [country, setCountry] = useState(null);
  const [countries, setCountries] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [editMode, setEditMode] = useState(false);
  const [saving, setSaving] = useState(false);

  const [formData, setFormData] = useState({
    userName: '',
    email: '',
    age: '',
    countryId: ''
  });

  useEffect(() => {
    fetchUserProfile();
    fetchCountries();
  }, []);

  const fetchUserProfile = async () => {
    setLoading(true);
    try {
      const decoded = getDecodedToken();
      const userId = decoded?.userId;

      const response = await fetch(`http://localhost:5160/api/users/${userId}`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.ok) {
        const data = await response.json();
        setUser(data);
        setFormData({
          userName: data.userName,
          email: data.userEmail || data.email,
          age: data.age,
          countryId: data.countryId
        });

        if (data.countryId) {
          const countryResponse = await fetch(
            `http://localhost:5160/api/countries/${data.countryId}`,
            {
              headers: {
                'Authorization': `Bearer ${token}`
              }
            }
          );
          if (countryResponse.ok) {
            const countryData = await countryResponse.json();
            setCountry(countryData);
          }
        }
      } else {
        setError('Failed to load profile');
      }
    } catch (err) {
      setError('Error loading profile: ' + err.message);
    } finally {
      setLoading(false);
    }
  };

  const fetchCountries = async () => {
    try {
      const response = await fetch('http://localhost:5160/api/countries', {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.ok) {
        const data = await response.json();
        setCountries(data);
      }
    } catch (err) {
      console.error('Failed to fetch countries:', err);
    }
  };

  const handleEditProfile = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      const decoded = getDecodedToken();
      const userId = decoded?.userId;

      const response = await fetch(`http://localhost:5160/api/users/${userId}`, {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(formData)
      });

      if (response.ok) {
        alert('Profile updated successfully!');
        setEditMode(false);
        await fetchUserProfile();
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to update profile');
      }
    } catch (err) {
      alert('Error updating profile: ' + err.message);
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <>
        <Navbar />
        <div className={styles.container}>
          <div className={styles.loading}>Loading profile...</div>
        </div>
      </>
    );
  }

  if (error) {
    return (
      <>
        <Navbar />
        <div className={styles.container}>
          <div className={styles.error}>{error}</div>
        </div>
      </>
    );
  }

  return (
    <>
      <Navbar />
      <div className={styles.container}>
        <h1 className={styles.title}>My Profile</h1>

        <div className={styles.profileGrid}>
          <div className={styles.profileCard}>
            <div className={styles.cardHeader}>
              <h2>Profile Information</h2>
              {!editMode && (
                <button
                  className={styles.editButton}
                  onClick={() => setEditMode(true)}
                >
                  Edit Profile
                </button>
              )}
            </div>

            {editMode ? (
              <form onSubmit={handleEditProfile} className={styles.form}>
                <div className={styles.formGroup}>
                  <label>Username</label>
                  <input
                    type="text"
                    value={formData.userName}
                    onChange={(e) =>
                      setFormData({ ...formData, userName: e.target.value })
                    }
                    required
                  />
                </div>

                <div className={styles.formGroup}>
                  <label>Email</label>
                  <input
                    type="email"
                    value={formData.email}
                    onChange={(e) =>
                      setFormData({ ...formData, email: e.target.value })
                    }
                    required
                  />
                </div>

                <div className={styles.formGroup}>
                  <label>Age</label>
                  <input
                    type="number"
                    value={formData.age}
                    onChange={(e) =>
                      setFormData({ ...formData, age: parseInt(e.target.value) })
                    }
                    required
                    min="1"
                  />
                </div>

                <div className={styles.formGroup}>
                  <label>Country</label>
                  <select
                    value={formData.countryId}
                    onChange={(e) =>
                      setFormData({ ...formData, countryId: e.target.value })
                    }
                    required
                  >
                    <option value="">Select Country</option>
                    {countries.map((c) => (
                      <option key={c.countryId} value={c.countryId}>
                        {c.countryName || c.name}
                      </option>
                    ))}
                  </select>
                </div>

                <div className={styles.formActions}>
                  <button
                    type="submit"
                    className={styles.saveButton}
                    disabled={saving}
                  >
                    {saving ? 'Saving...' : 'Save Changes'}
                  </button>
                  <button
                    type="button"
                    className={styles.cancelButton}
                    onClick={() => {
                      setEditMode(false);
                      setFormData({
                        userName: user.userName,
                        email: user.userEmail || user.email,
                        age: user.age,
                        countryId: user.countryId
                      });
                    }}
                  >
                    Cancel
                  </button>
                </div>
              </form>
            ) : (
              <div className={styles.infoList}>
                <div className={styles.infoItem}>
                  <span className={styles.infoLabel}>Username:</span>
                  <span className={styles.infoValue}>{user.userName}</span>
                </div>
                <div className={styles.infoItem}>
                  <span className={styles.infoLabel}>Email:</span>
                  <span className={styles.infoValue}>{user.userEmail || user.email}</span>
                </div>
                <div className={styles.infoItem}>
                  <span className={styles.infoLabel}>Age:</span>
                  <span className={styles.infoValue}>{user.age}</span>
                </div>
                <div className={styles.infoItem}>
                  <span className={styles.infoLabel}>Country:</span>
                  <span className={styles.infoValue}>
                    {country ? `${country.countryName || country.name} (${country.currency})` : 'N/A'}
                  </span>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </>
  );
}

export default Profile;
