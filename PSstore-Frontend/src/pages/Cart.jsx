import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useCart } from '../context/CartContext';
import { formatPrice } from '../utils/currency';
import styles from './Cart.module.css';

function Cart() {
  const { token, getDecodedToken } = useAuth();
  const { cart, fetchCart, removeFromCart, updateQuantity, loading } = useCart();
  const navigate = useNavigate();
  const [userCurrency, setUserCurrency] = useState('INR');
  const [processingCheckout, setProcessingCheckout] = useState(false);

  useEffect(() => {
    fetchCart();
    fetchUserCurrency();
  }, []);

  const fetchUserCurrency = async () => {
    try {
      const decoded = getDecodedToken();
      const userId = decoded?.userId;

      if (!userId) return;

      const userResponse = await fetch(`http://localhost:5160/api/users/${userId}`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (userResponse.ok) {
        const userData = await userResponse.json();
        
        if (userData.countryId) {
          const countryResponse = await fetch(
            `http://localhost:5160/api/countries/${userData.countryId}`,
            {
              headers: {
                'Authorization': `Bearer ${token}`
              }
            }
          );

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

  const handleUpdateQuantity = async (cartItemId, newQuantity) => {
    if (newQuantity < 1) return;
    const result = await updateQuantity(cartItemId, newQuantity);
    if (!result.success) {
      alert(result.error || 'Failed to update quantity');
    }
  };

  const handleCheckout = async () => {
    setProcessingCheckout(true);
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

  if (!cart || cart.items?.length === 0) {
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

  const subtotal = cart.items.reduce((sum, item) => sum + (item.price * item.quantity), 0);
  const tax = cart.items[0]?.taxRate ? (subtotal * cart.items[0].taxRate) : 0;
  const total = subtotal + tax;
  console.log('Cart totals:', { subtotal, tax, total });

  return (
    <div className={styles.container}>
      <h1 className={styles.title}>Shopping Cart</h1>
      
      <div className={styles.cartContent}>
        <div className={styles.cartItems}>
          {cart.items.map((item) => (
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
                {formatPrice(item.price, userCurrency)}
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
            <span>Tax ({(cart.items[0]?.taxRate * 100).toFixed(0)}%):</span>
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
