import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import styles from './Signup.module.css';

function Signup() {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    userName: '',
    email: '',
    password: '',
    confirmPassword: '',
    dateOfBirth: '',
    countryId: ''
  });
  const [countries, setCountries] = useState([]);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { signup } = useAuth();

  // Calculate max date (16 years ago from today)
  const getMaxDate = () => {
    const today = new Date();
    const maxDate = new Date(today.getFullYear() - 16, today.getMonth(), today.getDate());
    return maxDate.toISOString().split('T')[0];
  };

  // Calculate age from date of birth
  const calculateAge = (dobStr) => {
    if (!dobStr) return 0;
    const dob = new Date(dobStr);
    const today = new Date();
    let age = today.getFullYear() - dob.getFullYear();
    const monthDiff = today.getMonth() - dob.getMonth();
    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < dob.getDate())) {
      age--;
    }
    return age;
  };

  // Fetch countries from API
  useEffect(() => {
    const fetchCountries = async () => {
      try {
        const response = await fetch('http://localhost:5160/api/countries');
        if (response.ok) {
          const data = await response.json();
          setCountries(data);
        } else {
          console.error('Failed to fetch countries:', response.status);
        }
      } catch (err) {
        console.error('Error fetching countries:', err);
        // Fallback to hardcoded countries if API fails
        setCountries([
          { countryId: 1, countryCode: 'IN', countryName: 'India', currency: 'INR' },
          { countryId: 2, countryCode: 'UK', countryName: 'United Kingdom', currency: 'GBP' }
        ]);
      }
    };
    fetchCountries();
  }, []);

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

    // Validate date of birth
    if (!formData.dateOfBirth) {
      setError('Date of birth is required');
      return;
    }

    const age = calculateAge(formData.dateOfBirth);
    if (age < 16) {
      setError('You must be at least 16 years old to register');
      return;
    }

    if (!formData.countryId) {
      setError('Please select a country');
      return;
    }

    setLoading(true);

    // Calculate age from date of birth to send to backend
    const calculatedAge = calculateAge(formData.dateOfBirth);

    const result = await signup(
      formData.userName,
      formData.email,
      formData.password,
      calculatedAge,
      parseInt(formData.countryId)
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
            <label className={styles.formLabel}>Date of Birth</label>
            <input
              type="date"
              name="dateOfBirth"
              className={styles.formInput}
              value={formData.dateOfBirth}
              onChange={handleChange}
              max={getMaxDate()}
              required
            />
            <small className={styles.formHint}>You must be at least 16 years old</small>
          </div>

          <div className={styles.formGroup}>
            <label className={styles.formLabel}>Country</label>
            <select
              name="countryId"
              className={styles.formSelect}
              value={formData.countryId}
              onChange={handleChange}
              required
            >
              <option value="">Select a country</option>
              {countries.map(country => (
                <option key={country.countryId} value={country.countryId}>
                  {country.countryName} ({country.countryCode})
                </option>
              ))}
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
          Already have an account? <a onClick={() => navigate('/login')}>Login</a>
        </div>
      </div>
    </div>
  );
}

export default Signup;
