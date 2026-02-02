import { createContext, useState, useContext, useEffect } from 'react';
import { jwtDecode } from 'jwt-decode';
import apiClient from '../utils/apiClient';

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
  const [token, setToken] = useState(localStorage.getItem('token'));
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Check if token exists on mount and validate it
    const storedToken = localStorage.getItem('token');

    if (storedToken) {
      try {
        const decoded = jwtDecode(storedToken);
        const currentTime = Date.now() / 1000;

        if (decoded.exp < currentTime) {
          // Token expired
          console.log('Token expired, logging out');
          localStorage.removeItem('token');
          localStorage.removeItem('user');
          setToken(null);
        } else {
          // Token valid
          setToken(storedToken);
        }
      } catch (error) {
        // Invalid token format
        console.error('Invalid token found, clearing storage', error);
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        setToken(null);
      }
    }
    setLoading(false);
  }, []);

  // Decode token to get user info
  const getDecodedToken = () => {
    if (!token) return null;

    try {
      const decoded = jwtDecode(token);
      return {
        userId: decoded.sub, // Guid as string
        role: decoded.role || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'],
        jti: decoded.jti,
        exp: decoded.exp,
        iat: decoded.iat
      };
    } catch (error) {
      console.error('Failed to decode token:', error);
      return null;
    }
  };

  const login = async (email, password) => {
    try {
      const payload = {
        UserEmail: email,
        UserPassword: password,
      };
      
      console.log('Login attempt for:', email);
      
      const response = await apiClient.post('/users/login', payload);

      if (!response.ok) {
        const errorData = await response.json();
        console.error('Login failed:', response.status, errorData);
        throw new Error(errorData.message || errorData.title || 'Login failed');
      }

      const data = await response.json();

      // Only store token in localStorage (remove any old user data)
      localStorage.removeItem('user'); // Clean up old user data
      localStorage.setItem('token', data.token);
      setToken(data.token);

      return { success: true };
    } catch (error) {
      console.error('Login error:', error);
      return { success: false, error: error.message };
    }
  };

  const adminLogin = async (email, password) => {
    try {
      const response = await apiClient.post('/admin/login', {
        UserEmail: email,
        UserPassword: password,
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || 'Admin login failed');
      }

      const data = await response.json();

      localStorage.removeItem('user');
      localStorage.setItem('token', data.token);
      setToken(data.token);

      return { success: true };
    } catch (error) {
      return { success: false, error: error.message };
    }
  };

  const signup = async (userName, email, password, age, countryId) => {
    try {
      // Backend expects PascalCase field names directly (not wrapped)
      const payload = {
        UserName: userName,
        UserEmail: email,
        UserPassword: password,
        Age: age,
        CountryId: String(countryId), // Ensure countryId is a string (GUID)
      };
      
      console.log('Signup payload:', payload);
      
      const response = await apiClient.post('/users', payload);

      if (!response.ok) {
        const errorData = await response.json();
        console.error('Signup failed:', response.status, errorData);
        
        // Extract validation errors if present
        if (errorData.errors) {
          console.error('Validation errors:', JSON.stringify(errorData.errors, null, 2));
          const errorMessages = Object.entries(errorData.errors)
            .map(([field, messages]) => `${field}: ${Array.isArray(messages) ? messages.join(', ') : messages}`)
            .join('\n');
          throw new Error(errorMessages || 'Validation failed');
        }
        
        throw new Error(errorData.message || errorData.title || 'Signup failed');
      }

      // Auto-login after successful signup
      return await login(email, password);
    } catch (error) {
      console.error('Signup error:', error);
      return { success: false, error: error.message };
    }
  };

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user'); // Clean up old user data
    setToken(null);
  };

  const isAuthenticated = () => {
    return !!token;
  };

  const value = {
    token,
    getDecodedToken,
    login,
    adminLogin,
    signup,
    logout,
    isAuthenticated,
    loading,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
