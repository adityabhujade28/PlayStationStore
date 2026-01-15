import { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import styles from './Signup.module.css';

function Signup({ onSwitchToLogin }) {
  const [formData, setFormData] = useState({
    userName: '',
    email: '',
    password: '',
    confirmPassword: '',
    age: '',
    regionId: '1'
  });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { signup } = useAuth();

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    // Validation
    if (formData.password !== formData.confirmPassword) {
      setError('Passwords do not match');
      return;
    }

    if (formData.password.length < 6) {
      setError('Password must be at least 6 characters');
      return;
    }

    setLoading(true);

    const result = await signup(
      formData.userName,
      formData.email,
      formData.password,
      parseInt(formData.age) || null,
      parseInt(formData.regionId)
    );

    if (!result.success) {
      setError(result.error || 'Signup failed');
    }

    setLoading(false);
  };

  return (
    <div className={styles.signupContainer}>
      <div className={styles.signupCard}>
        <h2 className={styles.signupTitle}>Create Account</h2>
        
        {error && <div className={styles.errorMessage}>{error}</div>}
        
        <form onSubmit={handleSubmit} className={styles.signupForm}>
          <div className={styles.formGroup}>
            <label className={styles.formLabel}>Username</label>
            <input
              type="text"
              name="userName"
              className={styles.formInput}
              value={formData.userName}
              onChange={handleChange}
              placeholder="Enter your username"
              required
            />
          </div>

          <div className={styles.formGroup}>
            <label className={styles.formLabel}>Email</label>
            <input
              type="email"
              name="email"
              className={styles.formInput}
              value={formData.email}
              onChange={handleChange}
              placeholder="Enter your email"
              required
            />
          </div>

          <div className={styles.formGroup}>
            <label className={styles.formLabel}>Password</label>
            <input
              type="password"
              name="password"
              className={styles.formInput}
              value={formData.password}
              onChange={handleChange}
              placeholder="Enter password (min 6 characters)"
              required
            />
          </div>

          <div className={styles.formGroup}>
            <label className={styles.formLabel}>Confirm Password</label>
            <input
              type="password"
              name="confirmPassword"
              className={styles.formInput}
              value={formData.confirmPassword}
              onChange={handleChange}
              placeholder="Confirm your password"
              required
            />
          </div>

          <div className={styles.formGroup}>
            <label className={styles.formLabel}>Age (Optional)</label>
            <input
              type="number"
              name="age"
              className={styles.formInput}
              value={formData.age}
              onChange={handleChange}
              placeholder="Enter your age"
              min="0"
              max="150"
            />
          </div>

          <div className={styles.formGroup}>
            <label className={styles.formLabel}>Region</label>
            <select
              name="regionId"
              className={styles.formSelect}
              value={formData.regionId}
              onChange={handleChange}
              required
            >
              <option value="1">United States</option>
              <option value="2">Europe</option>
              <option value="3">Asia</option>
              <option value="4">Other</option>
            </select>
          </div>

          <button 
            type="submit" 
            className={styles.signupButton}
            disabled={loading}
          >
            {loading ? 'Creating Account...' : 'Sign Up'}
          </button>
        </form>

        <div className={styles.loginLink}>
          Already have an account? <a onClick={onSwitchToLogin}>Login</a>
        </div>
      </div>
    </div>
  );
}

export default Signup;
