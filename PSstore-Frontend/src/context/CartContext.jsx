import { createContext, useState, useContext, useEffect } from 'react';
import { useAuth } from './AuthContext';
import apiClient from '../utils/apiClient';

const CartContext = createContext(null);

export const CartProvider = ({ children }) => {
  const { token, getDecodedToken, isAuthenticated } = useAuth();
  const [cartCount, setCartCount] = useState(0);
  const [cartItems, setCartItems] = useState([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (isAuthenticated()) {
      fetchCartCount();
    } else {
      setCartCount(0);
      setCartItems([]);
    }
  }, [token, isAuthenticated]); // Added isAuthenticated to dependency array

  const fetchCartCount = async () => {
    try {
      const decoded = getDecodedToken();
      if (!decoded) return;

      const userId = decoded.userId;

      // Use the correct endpoint for getting user cart
      const response = await apiClient.get(`/cart/user/${userId}`);

      if (response.ok) {
        const data = await response.json();
        // The API returns CartDTO { items: [...] }
        const count = data.items?.length || 0;
        setCartCount(count);
      }
    } catch (err) {
      console.error('Failed to fetch cart count:', err);
    }
  };

  const fetchCartItems = async () => {
    setLoading(true);
    try {
      const decoded = getDecodedToken();
      if (!decoded) return;

      const userId = decoded.userId;

      // Corrected endpoint: POST /cart/user/{userId}/items usually adds, GET /cart/user/{userId} gets the whole cart
      // The backend has GET /api/Cart/user/{userId} which returns CartDTO
      const response = await apiClient.get(`/cart/user/${userId}`);

      if (response.ok) {
        const data = await response.json();
        const items = data.items || [];
        setCartItems(items);
        setCartCount(items.length);
      } else {
        console.error('Failed to fetch cart:', response.status);
      }
    } catch (err) {
      console.error('Failed to fetch cart items:', err);
    } finally {
      setLoading(false);
    }
  };

  const addToCart = async (gameId) => {
    if (!isAuthenticated()) return { success: false, message: 'Please login to add to cart' };

    try {
      const decoded = getDecodedToken();
      const userId = decoded.userId;

      // Corrected endpoint: POST /api/Cart/user/{userId}/items
      const response = await apiClient.post(`/cart/user/${userId}/items`, {
        userId, // Redundant if in URL, but backend might expect it in DTO
        gameId,
        quantity: 1
      });

      if (response.ok) {
        await fetchCartCount();
        return { success: true };
      } else {
        let errorMsg = 'Failed to add to cart';
        try {
          const error = await response.json();
          errorMsg = error.message || errorMsg;
        } catch (e) { /* ignore json parse error */ }
        return { success: false, message: errorMsg };
      }
    } catch (err) {
      return { success: false, message: err.message };
    }
  };

  const removeFromCart = async (cartItemId) => {
    try {
      const decoded = getDecodedToken();
      const userId = decoded?.userId;

      if (!userId) return { success: false, message: "User not found" };

      // Corrected endpoint: DELETE /api/Cart/user/{userId}/items/{cartItemId}
      const response = await apiClient.delete(`/cart/user/${userId}/items/${cartItemId}`);

      if (response.ok) {
        await fetchCartItems(); // Refresh items and count
        return { success: true };
      } else {
        return { success: false, message: 'Failed to remove item' };
      }
    } catch (err) {
      return { success: false, message: err.message };
    }
  };

  // updateQuantity is removed as per the provided edit.

  const clearCart = async () => {
    try {
      const decoded = getDecodedToken();
      const userId = decoded.userId;

      const response = await apiClient.delete(`/cart/user/${userId}`);

      if (response.ok) {
        setCartItems([]);
        setCartCount(0);
        return { success: true };
      }
      return { success: false };
    } catch (err) {
      console.error('Failed to clear cart:', err);
      return { success: false };
    }
  };

  const checkout = async () => {
    try {
      const decoded = getDecodedToken();
      const userId = decoded.userId;

      const response = await apiClient.post(`/cart/user/${userId}/checkout`, {});

      if (response.ok) {
        const data = await response.json();
        setCartItems([]);
        setCartCount(0);
        return { success: true, data }; // Return data for checkout-success
      } else {
        let errorMsg = 'Checkout failed';
        try {
          const error = await response.json();
          errorMsg = error.message || errorMsg;
        } catch (e) { /* ignore */ }
        return { success: false, message: errorMsg };
      }
    } catch (err) {
      return { success: false, message: err.message };
    }
  };

  const value = {
    cartCount,
    cartItems,
    loading,
    fetchCartItems,
    addToCart,
    removeFromCart,
    clearCart,
    checkout
  };

  return (
    <CartContext.Provider value={value}>
      {children}
    </CartContext.Provider>
  );
};

export const useCart = () => {
  const context = useContext(CartContext);
  if (!context) {
    throw new Error('useCart must be used within a CartProvider');
  }
  return context;
};
