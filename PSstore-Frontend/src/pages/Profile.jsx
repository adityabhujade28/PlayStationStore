import { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import Navbar from '../components/Navbar';
import styles from './Profile.module.css';
import apiClient from '../utils/apiClient';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';

function Profile() {
  const { getDecodedToken } = useAuth();
  const queryClient = useQueryClient();
  const [editMode, setEditMode] = useState(false);

  const decoded = getDecodedToken();
  const userId = decoded?.userId;

  // Fetch user profile
  const { data: user, isLoading: userLoading, error: userError } = useQuery({
    queryKey: ['user', userId],
    queryFn: async () => {
      const res = await apiClient.get(`/users/${userId}`);
      if (!res.ok) throw new Error('Failed to fetch user');
      return res.json();
    },
    enabled: !!userId,
    staleTime: 5 * 60 * 1000, // 5 min cache - shares with Navbar
  });

  // Fetch user's country
  const { data: country } = useQuery({
    queryKey: ['country', user?.countryId],
    queryFn: async () => {
      const res = await apiClient.get(`/countries/${user.countryId}`);
      if (!res.ok) throw new Error('Failed to fetch country');
      return res.json();
    },
    enabled: !!user?.countryId,
    staleTime: 24 * 60 * 60 * 1000, // 24 hr - shares with other pages
  });

  // Fetch all countries for dropdown (24hr cache)
  const { data: countries = [] } = useQuery({
    queryKey: ['countries'],
    queryFn: async () => {
      const res = await apiClient.get('/countries');
      if (!res.ok) throw new Error('Failed to fetch countries');
      return res.json();
    },
    staleTime: 24 * 60 * 60 * 1000, // 24 hr - countries rarely change
  });

  // Form state
  const [formData, setFormData] = useState({
    userName: user?.userName || '',
    email: user?.userEmail || user?.email || '',
    age: user?.age || '',
    countryId: user?.countryId || ''
  });

  // Update form when user data loads
  if (user && (formData.userName === '' || formData.email === '')) {
    setFormData({
      userName: user.userName,
      email: user.userEmail || user.email,
      age: user.age,
      countryId: user.countryId
    });
  }

  // Update profile mutation
  const { mutate: updateProfile, isPending: isSaving } = useMutation({
    mutationFn: async (updatedData) => {
      const res = await apiClient.put(`/users/${userId}`, updatedData);
      if (!res.ok) throw new Error('Failed to update profile');
      return res.json();
    },
    onSuccess: (updatedUser) => {
      // Update user cache with new data
      queryClient.setQueryData(['user', userId], updatedUser);
      alert('Profile updated successfully!');
      setEditMode(false);
    },
    onError: (error) => {
      alert(error.message || 'Failed to update profile');
    }
  });

  const handleEditProfile = async (e) => {
    e.preventDefault();
    updateProfile(formData);
  };

  const handleCancel = () => {
    setEditMode(false);
    // Reset form to current user data
    setFormData({
      userName: user.userName,
      email: user.userEmail || user.email,
      age: user.age,
      countryId: user.countryId
    });
  };

  const isLoading = userLoading;

  if (isLoading) {
    return (
      <>
        <Navbar />
        <div className={styles.container}>
          <div className={styles.loading}>Loading profile...</div>
        </div>
      </>
    );
  }

  if (userError) {
    return (
      <>
        <Navbar />
        <div className={styles.container}>
          <div className={styles.error}>{userError.message}</div>
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
                    disabled={isSaving}
                  >
                    {isSaving ? 'Saving...' : 'Save Changes'}
                  </button>
                  <button
                    type="button"
                    className={styles.cancelButton}
                    onClick={handleCancel}
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
