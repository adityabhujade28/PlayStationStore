const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5160/api';

/**
 * Helper to get the JWT token from localStorage
 */
const getToken = () => localStorage.getItem('token');

/**
 * Generic fetch wrapper that handles auth headers and base URL
 */
const apiFetch = async (endpoint, options = {}) => {
  const token = getToken();
  
  const headers = {
    'Content-Type': 'application/json',
    ...options.headers,
  };

  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const config = {
    ...options,
    headers,
  };

  // Ensure endpoint starts with / if not absolute
  const url = endpoint.startsWith('http') 
    ? endpoint 
    : `${API_BASE_URL}${endpoint.startsWith('/') ? endpoint : '/' + endpoint}`;

  try {
    const response = await fetch(url, config);
    
    // Handle 401 Unauthorized (optional: could emit event to logout)
    if (response.status === 401) {
      console.warn('Unauthorized access. Token might be invalid.');
      // Optional: localStorage.removeItem('token');
      // Optional: window.location.href = '/login';
    }

    return response;
  } catch (error) {
    console.error('API Request Failed:', error);
    throw error;
  }
};

const apiClient = {
  get: (endpoint, options = {}) => {
    // Add cache-busting parameter to prevent browser caching
    const separator = endpoint.includes('?') ? '&' : '?';
    const cacheBuster = `${separator}_t=${Date.now()}`;
    const endpointWithCacheBuster = endpoint + cacheBuster;
    
    return apiFetch(endpointWithCacheBuster, { 
      ...options, 
      method: 'GET',
      headers: {
        ...options.headers,
        'Cache-Control': 'no-cache, no-store, must-revalidate',
        'Pragma': 'no-cache',
        'Expires': '0'
      }
    });
  },
  
  post: (endpoint, body, options = {}) => apiFetch(endpoint, { 
    ...options, 
    method: 'POST', 
    body: JSON.stringify(body) 
  }),

  put: (endpoint, body, options = {}) => apiFetch(endpoint, { 
    ...options, 
    method: 'PUT', 
    body: JSON.stringify(body) 
  }),

  delete: (endpoint, options = {}) => apiFetch(endpoint, { ...options, method: 'DELETE' }),
};

export default apiClient;
