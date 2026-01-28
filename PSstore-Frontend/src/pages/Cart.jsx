import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { formatPrice } from '../utils/currency';
import LazyImage from '../components/LazyImage';
import styles from './Cart.module.css';
import apiClient from '../utils/apiClient';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';

function Cart() {
  const { getDecodedToken, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [processingCheckout, setProcessingCheckout] = useState(false);

  const decoded = getDecodedToken();
  const userId = decoded?.userId;

  // Fetch user data + currency
  const { data: user } = useQuery({
    queryKey: ['user', userId],
    queryFn: async () => {
      const res = await apiClient.get(`/users/${userId}`);
      if (!res.ok) throw new Error('Failed to fetch user');
      return res.json();
    },
    enabled: !!userId,
    staleTime: 5 * 60 * 1000,
  });

  const { data: country } = useQuery({
    queryKey: ['country', user?.countryId],
    queryFn: async () => {
      const res = await apiClient.get(`/countries/${user.countryId}`);
      if (!res.ok) throw new Error('Failed to fetch country');
      return res.json();
    },
    enabled: !!user?.countryId,
    staleTime: 24 * 60 * 60 * 1000,
  });

  // Fetch cart items
  const { data: cartData = { items: [], totalPrice: 0, taxRate: 0 }, isLoading, error } = useQuery({
    queryKey: ['cart', userId],
    queryFn: async () => {
      if (!userId) throw new Error('User not authenticated');
      const res = await apiClient.get(`/cart/user/${userId}`);
      if (!res.ok) throw new Error('Failed to fetch cart');
      const data = await res.json();
      // Normalize response - ensure it has items array
      if (Array.isArray(data)) {
        return { items: data, totalPrice: 0, taxRate: 0 };
      }
      return data;
    },
    enabled: !!userId,
    staleTime: 2 * 60 * 1000, // Cart: 2 min cache (frequent changes)
    refetchOnWindowFocus: false,
  });

  const cartItems = cartData.items || [];

  // Remove item mutation
  const { mutate: removeFromCart, isPending: isRemoving } = useMutation({
    mutationFn: async (cartItemId) => {
      const res = await apiClient.delete(`/cart/user/${userId}/items/${cartItemId}`);
      if (!res.ok) throw new Error('Failed to remove item');
      return res.status === 204 ? { success: true } : res.json();
    },
    onSuccess: () => {
      // Invalidate cart query to refetch updated cart
      queryClient.invalidateQueries({ queryKey: ['cart', userId] });
    },
    onError: (error) => {
      alert(error.message || 'Failed to remove item from cart');
    }
  });

  // Checkout mutation
  const { mutate: checkout } = useMutation({
    mutationFn: async () => {
      const res = await apiClient.post(`/cart/user/${userId}/checkout`, {});
      if (!res.ok) throw new Error('Checkout failed');
      return res.json();
    },
    onSuccess: (data) => {
      // Invalidate cart after successful checkout
      queryClient.invalidateQueries({ queryKey: ['cart', userId] });
      navigate('/checkout-success', { state: { purchase: data } });
    },
    onError: (error) => {
      alert(error.message || 'Checkout failed');
    }
  });

  const userCurrency = country?.currencyCode || 'INR';

  if (!isLoading && (!cartItems || cartItems.length === 0)) {
    return (
      <div className={styles.container}>
        <div className={styles.emptyCart}>
          <div className={styles.emptyIcon}>🛒</div>
          <h2>Your cart is empty</h2>
          <p>Add some games to get started!</p>
          <button className={styles.browseButton} onClick={() => navigate('/')}>
            Browse Store
          </button>
        </div>
      </div>
    );
  }

  const subtotal = cartItems.reduce((sum, item) => sum + (item.totalPrice || 0), 0);
  const tax = cartItems[0]?.taxRate ? (subtotal * cartItems[0].taxRate) : cartData.taxRate ? (subtotal * cartData.taxRate) : 0;
  const total = subtotal + tax;

  return (
    <div className={styles.container}>
      <button className={styles.backButton} onClick={() => navigate('/')}>
        ← Back
      </button>
      <h1 className={styles.title}>Shopping Cart</h1>

      <div className={styles.cartContent}>
        <div className={styles.cartItems}>
          {cartItems.map((item) => (
            <div key={item.cartItemId} className={styles.cartItem}>
              <div className={styles.itemImage}>
                {item.imageUrl ? (
                  <LazyImage src={item.imageUrl} alt={item.gameName} className={styles.imageWrapper} />
                ) : (
                  <div className={styles.placeholder}>🎮</div>
                )}
              </div>
              <div className={styles.itemDetails}>
                <h3 className={styles.itemName}>{item.gameName}</h3>
                <p className={styles.itemPublisher}>{item.publisher}</p>
              </div>
              {/*  */}
              <div className={styles.itemPrice}>
                {formatPrice(item.unitPrice, userCurrency)}
              </div>
              <button
                className={styles.removeButton}
                onClick={() => removeFromCart(item.cartItemId)}
                disabled={isRemoving}
              >
                ✕
              </button>
            </div>
          ))}
        </div>

        <div className={styles.cartSummary}>
          <h2 className={styles.summaryTitle}>Order Summary</h2>
          <div className={styles.summaryRow}>
            <span>Subtotal:</span>
            <span>{formatPrice(subtotal, userCurrency)}</span>
          </div>
          <div className={styles.summaryRow}>
            <span>Tax ({(cartItems[0]?.taxRate * 100).toFixed(0)}%):</span>
            <span>{formatPrice(tax, userCurrency)}</span>
          </div>
          <div className={styles.summaryDivider}></div>
          <div className={styles.summaryRow}>
            <span className={styles.totalLabel}>Total:</span>
            <span className={styles.totalAmount}>{formatPrice(total, userCurrency)}</span>
          </div>
          <button
            className={styles.checkoutButton}
            onClick={() => checkout()}
            disabled={processingCheckout || isLoading}
          >
            {processingCheckout ? 'Processing...' : 'Proceed to Checkout'}
          </button>
          <button
            className={styles.continueButton}
            onClick={() => navigate('/')}
          >
            Continue Shopping
          </button>
        </div>
      </div>
    </div>
  );
}

export default Cart;
