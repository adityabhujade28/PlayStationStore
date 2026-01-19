import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useCart } from '../context/CartContext';
import { formatPrice } from '../utils/currency';
import styles from './Cart.module.css';
import apiClient from '../utils/apiClient';

function Cart() {
  const { getDecodedToken, isAuthenticated } = useAuth();
  const { cartItems, fetchCartItems, removeFromCart, loading } = useCart();
  const navigate = useNavigate();
  const [userCurrency, setUserCurrency] = useState('INR');
  const [processingCheckout, setProcessingCheckout] = useState(false);

  useEffect(() => {
    fetchCartItems();
    fetchUserCurrency();
  }, [isAuthenticated]);

  const fetchUserCurrency = async () => {
    try {
      const decoded = getDecodedToken();
      const userId = decoded?.userId;

      if (!userId) return;

      const userResponse = await apiClient.get(`/users/${userId}`);

      if (userResponse.ok) {
        const userData = await userResponse.json();

        if (userData.countryId) {
          const countryResponse = await apiClient.get(`/countries/${userData.countryId}`);

          if (countryResponse.ok) {
            const countryData = await countryResponse.json();
            setUserCurrency(countryData.currency);
          }
        }
      }
    } catch (err) {
      console.error('Failed to fetch user currency:', err);
    }
  };

  const handleRemoveItem = async (cartItemId) => {
    const result = await removeFromCart(cartItemId);
    if (!result.success) {
      alert(result.error || 'Failed to remove item from cart');
    }
  };

  const handleCheckout = async () => {
    setProcessingCheckout(true);
    try {
      const decoded = getDecodedToken();
      const userId = decoded?.userId;

      const response = await apiClient.post(`/cart/user/${userId}/checkout`, {});

      if (response.ok) {
        const data = await response.json();
        navigate('/checkout-success', { state: { purchase: data } });
      } else {
        const error = await response.json();
        alert(error.message || 'Checkout failed');
      }
    } catch (err) {
      alert('Checkout failed: ' + err.message);
    } finally {
      setProcessingCheckout(false);
    }
  };

  // Fixed rendering condition using cartItems
  if (!loading && (!cartItems || cartItems.length === 0)) {
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
  const tax = cartItems[0]?.taxRate ? (subtotal * cartItems[0].taxRate) : 0;
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
                <div className={styles.placeholder}>🎮</div>
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
                onClick={() => handleRemoveItem(item.cartItemId)}
                disabled={loading}
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
            onClick={handleCheckout}
            disabled={processingCheckout || loading}
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
