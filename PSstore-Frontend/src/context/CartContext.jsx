import { createContext, useState, useContext, useEffect } from 'react';
import { useAuth } from './AuthContext';

const CartContext = createContext(null);

export const CartProvider = ({ children }) => {
  const { token, getDecodedToken } = useAuth();
  const [cart, setCart] = useState(null);
  const [cartItemCount, setCartItemCount] = useState(0);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (token) {
      fetchCart();
    }
  }, [token]);

  const fetchCart = async () => {
    try {
      const decoded = getDecodedToken();
      const userId = decoded?.userId;

      if (!userId) return;

      const response = await fetch(`http://localhost:5160/api/cart/user/${userId}`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.ok) {
        const data = await response.json();
        setCart(data);
        setCartItemCount(data.items?.length || 0);
      }
    } catch (err) {
      console.error('Failed to fetch cart:', err);
    }
  };

  const addToCart = async (gameId) => {
    setLoading(true);
    try {
      const decoded = getDecodedToken();
      const userId = decoded?.userId;

      const response = await fetch(`http://localhost:5160/api/cart/user/${userId}/items`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          gameId: gameId,
          quantity: 1
        })
      });

      if (response.ok) {
        await fetchCart();
        return { success: true };
      } else {
        const error = await response.json();
        return { success: false, error: error.message || 'Failed to add to cart' };
      }
    } catch (err) {
      return { success: false, error: err.message };
    } finally {
      setLoading(false);
    }
  };

  const removeFromCart = async (cartItemId) => {
    setLoading(true);
    try {
      const decoded = getDecodedToken();
      const userId = decoded?.userId;

      const response = await fetch(
        `http://localhost:5160/api/cart/user/${userId}/items/${cartItemId}`,
        {
          method: 'DELETE',
          headers: {
            'Authorization': `Bearer ${token}`
          }
        }
      );

      if (response.ok) {
        await fetchCart();
        return { success: true };
      } else {
        return { success: false, error: 'Failed to remove item' };
      }
    } catch (err) {
      return { success: false, error: err.message };
    } finally {
      setLoading(false);
    }
  };

  const updateQuantity = async (cartItemId, quantity) => {
    setLoading(true);
    try {
      const decoded = getDecodedToken();
      const userId = decoded?.userId;

      const response = await fetch(
        `http://localhost:5160/api/cart/user/${userId}/items/${cartItemId}`,
        {
          method: 'PUT',
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          },
          body: JSON.stringify(quantity)
        }
      );

      if (response.ok) {
        await fetchCart();
        return { success: true };
      } else {
        return { success: false, error: 'Failed to update quantity' };
      }
    } catch (err) {
      return { success: false, error: err.message };
    } finally {
      setLoading(false);
    }
  };

  const clearCart = async () => {
    setLoading(true);
    try {
      const decoded = getDecodedToken();
      const userId = decoded?.userId;

      const response = await fetch(`http://localhost:5160/api/cart/user/${userId}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.ok) {
        setCart(null);
        setCartItemCount(0);
        return { success: true };
      }
      return { success: false };
    } catch (err) {
      return { success: false, error: err.message };
    } finally {
      setLoading(false);
    }
  };

  const checkout = async () => {
    setLoading(true);
    try {
      const decoded = getDecodedToken();
      const userId = decoded?.userId;

      const response = await fetch(`http://localhost:5160/api/cart/user/${userId}/checkout`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (response.ok) {
        const data = await response.json();
        await fetchCart();
        return { success: true, data };
      } else {
        const error = await response.json();
        return { success: false, error: error.message || 'Checkout failed' };
      }
    } catch (err) {
      return { success: false, error: err.message };
    } finally {
      setLoading(false);
    }
  };

  const value = {
    cart,
    cartItemCount,
    loading,
    fetchCart,
    addToCart,
    removeFromCart,
    updateQuantity,
    clearCart,
    checkout
  };

  return <CartContext.Provider value={value}>{children}</CartContext.Provider>;
};

export const useCart = () => {
  const context = useContext(CartContext);
  if (!context) {
    throw new Error('useCart must be used within a CartProvider');
  }
  return context;
};
