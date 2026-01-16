import { createContext, useState, useContext, useEffect } from 'react';
import { jwtDecode } from 'jwt-decode';

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
  const [token, setToken] = useState(localStorage.getItem('token'));
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Check if token exists on mount
    const storedToken = localStorage.getItem('token');
    
    if (storedToken) {
      setToken(storedToken);
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
      const response = await fetch('http://localhost:5160/api/users/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          userEmail: email,
          userPassword: password,
        }),
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || 'Login failed');
      }

      const data = await response.json();
      
      // Only store token in localStorage (remove any old user data)
      localStorage.removeItem('user'); // Clean up old user data
      localStorage.setItem('token', data.token);
      setToken(data.token);

      return { success: true };
    } catch (error) {
      return { success: false, error: error.message };
    }
  };

  const signup = async (userName, email, password, age, countryId) => {
    try {
      const response = await fetch('http://localhost:5160/api/users', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          userName,
          userEmail: email,
          userPassword: password,
          age,
          countryId,
        }),
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || 'Signup failed');
      }

      // Auto-login after successful signup
      return await login(email, password);
    } catch (error) {
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
