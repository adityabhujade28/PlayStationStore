import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { formatPrice } from '../utils/currency';
import styles from './Subscriptions.module.css';

function Subscriptions() {
  const { token, getDecodedToken } = useAuth();
  const navigate = useNavigate();
  const [plans, setPlans] = useState([]);
  const [activeSubscription, setActiveSubscription] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [userCurrency, setUserCurrency] = useState('INR');
  const [subscribing, setSubscribing] = useState(false);
  const [selectedPlan, setSelectedPlan] = useState(null);
  const [selectedDuration, setSelectedDuration] = useState(null);

  useEffect(() => {
    fetchPlans();
    fetchActiveSubscription();
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

  const fetchPlans = async () => {
    try {
      const decoded = getDecodedToken();
      const userId = decoded?.userId;

      // First get user's country
      const userResponse = await fetch(`http://localhost:5160/api/users/${userId}`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      let userCountryId = null;
      if (userResponse.ok) {
        const userData = await userResponse.json();
        userCountryId = userData.countryId;
      }

      // Get all subscription plans
      const plansResponse = await fetch(
        `http://localhost:5160/api/Subscriptions/plans`,
        {
          headers: {
            'Authorization': `Bearer ${token}`
          }
        }
      );

      if (plansResponse.ok) {
        const plansData = await plansResponse.json();
        
        // For each plan, get pricing options for user's country
        if (userCountryId) {
          const plansWithPricing = await Promise.all(
            plansData.map(async (plan) => {
              const optionsResponse = await fetch(
                `http://localhost:5160/api/Subscriptions/plans/${plan.subscriptionId}/options?countryId=${userCountryId}`,
                {
                  headers: {
                    'Authorization': `Bearer ${token}`
                  }
                }
              );

              if (optionsResponse.ok) {
                const options = await optionsResponse.json();
                return {
                  ...plan,
                  pricingOptions: options
                };
              }
              return plan;
            })
          );
          setPlans(plansWithPricing);
        } else {
          setPlans(plansData);
        }
      } else {
        setError('Failed to load subscription plans');
      }
    } catch (err) {
      setError('Failed to load plans: ' + err.message);
    } finally {
      setLoading(false);
    }
  };

  const fetchActiveSubscription = async () => {
    try {
      const decoded = getDecodedToken();
      const userId = decoded?.userId;

      const response = await fetch(
        `http://localhost:5160/api/Subscriptions/user/${userId}/active`,
        {
          headers: {
            'Authorization': `Bearer ${token}`
          }
        }
      );

      if (response.ok) {
        const data = await response.json();

        // Enrich active subscription with plan details (name, included games count)
        try {
          const plansRes = await fetch(`http://localhost:5160/api/Subscriptions/plans`, {
            headers: { 'Authorization': `Bearer ${token}` }
          });

          if (plansRes.ok) {
            const plans = await plansRes.json();
            const matchingPlan = plans.find(p =>
              p.subscriptionId === data.subscriptionId || p.subscriptionName === data.subscriptionName
            );

            if (matchingPlan) {
              data.subscriptionName = data.subscriptionName || matchingPlan.subscriptionName;
              data.includedGamesCount = matchingPlan.includedGames?.length || 0;
            }
          }
        } catch (err) {
          console.warn('Failed to enrich active subscription:', err);
        }

        setActiveSubscription(data);
      }
    } catch (err) {
      console.error('Failed to fetch active subscription:', err);
    }
  };

  const handleSubscribe = async (subscriptionPlanCountryId) => {
    setSubscribing(true);
    try {
      const decoded = getDecodedToken();
      const userId = decoded?.userId;

      const response = await fetch(
        `http://localhost:5160/api/Subscriptions?userId=${userId}`,
        {
          method: 'POST',
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          },
          body: JSON.stringify({
            subscriptionPlanCountryId: subscriptionPlanCountryId
          })
        }
      );

      if (response.ok) {
        const result = await response.json();
        alert(result.message || 'Successfully subscribed!');
        await fetchActiveSubscription();
        setSelectedPlan(null);
      } else {
        const error = await response.json();
        alert(error.message || 'Subscription failed');
      }
    } catch (err) {
      alert('Subscription failed: ' + err.message);
    } finally {
      setSubscribing(false);
    }
  };

  if (loading) {
    return (
      <div className={styles.container}>
        <div className={styles.loading}>Loading subscription plans...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className={styles.container}>
        <div className={styles.error}>{error}</div>
      </div>
    );
  }

  // Compute games per plan for quick reference
  const gamesPerPlan = plans.reduce((acc, p) => {
    acc[p.subscriptionName] = p.includedGames?.length || 0;
    return acc;
  }, {});

  return (
    <div className={styles.container}>
      <button className={styles.backButton} onClick={() => navigate('/')}>
                        ← Back
            </button>
      <div className={styles.header}>
        <h1 className={styles.title}>PSstore Subscriptions</h1>
        <p className={styles.subtitle}>
          Get unlimited access to hundreds of games with PSstore subscription
        </p>

      </div>

      {activeSubscription && (
        <div className={styles.activeSubscription}>
          <div className={styles.activeHeader}>
            <h2>Your Active Subscription</h2>
            <span className={styles.activeBadge}>
              {activeSubscription.isActive ? 'Active' : 'Expired'}
            </span>
          </div>
          <div className={styles.activeDetails}>
            <div className={styles.activeInfo}>
              <div className={styles.planName}>{activeSubscription.subscriptionName || 'Active Subscription'}</div>
              {typeof activeSubscription.includedGamesCount !== 'undefined' && (
                <div style={{marginTop: 6, color: 'rgba(255,255,255,0.9)'}}>
                  {activeSubscription.includedGamesCount} games included
                </div>
              )}
              <div className={styles.planDetails}>
                <span>Started: {new Date(activeSubscription.planStartDate).toLocaleDateString()}</span>
                <span>Expires: {new Date(activeSubscription.planEndDate).toLocaleDateString()}</span>
              </div>
            </div>
            <button
              className={styles.manageButton}
              onClick={() => navigate('/library')}
            >
              View Library
            </button>
          </div>
        </div>
      )}

      <div className={styles.plansGrid}>
        {plans.map((plan) => {
          const pricingOptions = plan.pricingOptions || [];
          const monthlyOption = pricingOptions.find(opt => opt.durationMonths === 1);
          const monthlyPrice = monthlyOption?.price || 0;

          return (
            <div
              key={plan.subscriptionId}
              className={`${styles.planCard} ${
                plan.subscriptionName === 'Premium' ? styles.featured : ''
              }`}
            >
              {plan.subscriptionName === 'Premium' && (
                <div className={styles.bestValue}>Best Value</div>
              )}
              <div className={styles.planHeader}>
                <div>
                  <h3 className={styles.planTitle}>{plan.subscriptionName}</h3>
                  <div className={styles.includedCount}>
                    {plan.includedGames && plan.includedGames.length > 0
                      ? `${plan.includedGames.length} games included`
                      : 'No games listed'}
                  </div>
                </div>
                <div className={styles.planPrice}>
                  {formatPrice(monthlyPrice, userCurrency)}
                  <span className={styles.priceLabel}>/month</span>
                </div>
              </div>
              <p className={styles.planDescription}>
                {plan.subscriptionName === 'Essential' && 'Perfect for casual gamers'}
                {plan.subscriptionName === 'Extra' && 'Great value for regular players'}
                {plan.subscriptionName === 'Premium' && 'Ultimate gaming experience'}
              </p>
              <div className={styles.planFeatures}>
                <div className={styles.feature}>
                  ✓ {plan.includedGames?.length || 0}+ Games Included
                </div>
                <div className={styles.feature}>
                  ✓ New games added monthly
                </div>
                <div className={styles.feature}>
                  ✓ Play on all devices
                </div>
                <div className={styles.feature}>
                ✓ High Quality Games
                </div>
                {plan.subscriptionName === 'Premium' && (
                  <>
                    <div className={styles.feature}>
                      ✓ Day-one releases
                    </div>
                    <div className={styles.feature}>
                      ✓ Exclusive discounts
                    </div>
                  </>
                )}
              </div>

              {selectedPlan === plan.subscriptionId ? (
                <div className={styles.durationSelector}>
                  <h4>Select Duration:</h4>
                  <div className={styles.durationOptions}>
                    {pricingOptions.map((option) => (
                      <button
                        key={option.subscriptionPlanCountryId}
                        className={`${styles.durationButton} ${
                          selectedDuration === option.subscriptionPlanCountryId
                            ? styles.selectedDuration
                            : ''
                        }`}
                        onClick={() => setSelectedDuration(option.subscriptionPlanCountryId)}
                      >
                        {option.durationMonths} Month{option.durationMonths > 1 ? 's' : ''}
                        <br />
                        <span className={styles.durationPrice}>
                          {formatPrice(option.price, userCurrency)}
                        </span>
                        {option.durationMonths === 3 && (
                          <span className={styles.saveLabel}>Save 5%</span>
                        )}
                        {option.durationMonths === 12 && (
                          <span className={styles.saveLabel}>Save 15%</span>
                        )}
                      </button>
                    ))}
                  </div>
                  <div className={styles.confirmButtons}>
                    <button
                      className={styles.confirmButton}
                      onClick={() => handleSubscribe(selectedDuration)}
                      disabled={subscribing || !selectedDuration}
                    >
                      {subscribing ? 'Processing...' : 'Confirm Subscription'}
                    </button>
                    <button
                      className={styles.cancelButton}
                      onClick={() => {
                        setSelectedPlan(null);
                        setSelectedDuration(null);
                      }}
                    >
                      Cancel
                    </button>
                  </div>
                </div>
              ) : (
                <button
                  className={styles.subscribeButton}
                  onClick={() => {
                    setSelectedPlan(plan.subscriptionId);
                    setSelectedDuration(pricingOptions[0]?.subscriptionPlanCountryId || null);
                  }}
                  disabled={activeSubscription?.subscriptionName === plan.subscriptionName || pricingOptions.length === 0}
                >
                  {activeSubscription?.subscriptionName === plan.subscriptionName
                    ? 'Current Plan'
                    : pricingOptions.length === 0
                    ? 'Not Available'
                    : 'Subscribe Now'}
                </button>
              )}
            </div>
          );
        })}
      </div>

      {/*  */}
    </div>
  );
}

export default Subscriptions;
