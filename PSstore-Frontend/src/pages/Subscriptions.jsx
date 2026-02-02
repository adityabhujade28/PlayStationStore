import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useAuth } from '../context/AuthContext';
import { formatPrice } from '../utils/currency';
import styles from './Subscriptions.module.css';
import apiClient from '../utils/apiClient';

function Subscriptions() {
  const { getDecodedToken } = useAuth();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [subscribing, setSubscribing] = useState(false);
  const [selectedPlan, setSelectedPlan] = useState(null);
  const [selectedDuration, setSelectedDuration] = useState(null);

  const decoded = getDecodedToken();
  const userId = decoded?.userId;

  // Query 1: Fetch user data - CACHED for 5 minutes
  const { data: userInfo, isLoading: userLoading } = useQuery({
    queryKey: ['user', userId],
    queryFn: async () => {
      const response = await apiClient.get(`/users/${userId}`);
      if (!response.ok) throw new Error('Failed to fetch user');
      return response.json();
    },
    enabled: !!userId,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });

  // Query 2: Fetch country/currency - CACHED for 24 hours (rarely changes)
  const { data: countryData } = useQuery({
    queryKey: ['country', userInfo?.countryId],
    queryFn: async () => {
      const response = await apiClient.get(`/countries/${userInfo.countryId}`);
      if (!response.ok) throw new Error('Failed to fetch country');
      return response.json();
    },
    enabled: !!userInfo?.countryId,
    staleTime: 24 * 60 * 60 * 1000, // 24 hours
  });

  // Query 3: Fetch subscription plans - CACHED for 1 hour
  const { data: plansData = [], isLoading: plansLoading } = useQuery({
    queryKey: ['subscriptionPlans'],
    queryFn: async () => {
      const response = await apiClient.get(`/Subscriptions/plans`);
      if (!response.ok) throw new Error('Failed to fetch plans');
      return response.json();
    },
    staleTime: 60 * 60 * 1000, // 1 hour
  });

  // Query 4: Fetch pricing options for each plan - CACHED for 1 hour
  const { data: plansWithOptions = [] } = useQuery({
    queryKey: ['plansWithOptions', plansData, userInfo?.countryId],
    queryFn: async () => {
      if (!userInfo?.countryId || plansData.length === 0) return plansData;

      const plansWithPricing = await Promise.all(
        plansData.map(async (plan) => {
          const response = await apiClient.get(
            `/Subscriptions/plans/${plan.subscriptionId}/options?countryId=${userInfo.countryId}`
          );
          if (response.ok) {
            const options = await response.json();
            return { ...plan, pricingOptions: options };
          }
          return plan;
        })
      );
      return plansWithPricing;
    },
    enabled: !!userInfo?.countryId && plansData.length > 0,
    staleTime: 60 * 60 * 1000, // 1 hour
  });

  // Query 5: Fetch active subscription - CACHED for 5 minutes (might change frequently)
  const { data: activeSubscription } = useQuery({
    queryKey: ['activeSubscription', userId],
    queryFn: async () => {
      const response = await apiClient.get(`/Subscriptions/user/${userId}/active`);
      if (!response.ok) return null;
      const data = await response.json();

      // Enrich with plan details
      if (plansData.length > 0) {
        const matchingPlan = plansData.find(
          (p) =>
            p.subscriptionId === data.subscriptionId ||
            p.subscriptionName === data.subscriptionName
        );
        if (matchingPlan) {
          data.subscriptionName =
            data.subscriptionName || matchingPlan.subscriptionName;
          data.includedGamesCount = matchingPlan.includedGames?.length || 0;
        }
      }
      return data;
    },
    enabled: !!userId,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });

  const userCurrency = countryData?.currency || 'INR';
  const loading = userLoading || plansLoading;

  const handleSubscribe = async (subscriptionPlanCountryId) => {
    setSubscribing(true);
    try {
      const response = await apiClient.post(`/Subscriptions?userId=${userId}`, {
        subscriptionPlanCountryId: subscriptionPlanCountryId
      });

      if (response.ok) {
        const result = await response.json();
        alert(result.message || 'Successfully subscribed!');
        // Invalidate the cache so it refetches fresh data
        queryClient.invalidateQueries({ queryKey: ['activeSubscription', userId] });
        setSelectedPlan(null);
        setSelectedDuration(null);
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
                <div style={{ marginTop: 6, color: 'rgba(255,255,255,0.9)' }}>
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
        {plansWithOptions.map((plan) => {
          const pricingOptions = plan.pricingOptions || [];
          const monthlyOption = pricingOptions.find(opt => opt.durationMonths === 1);
          const monthlyPrice = monthlyOption?.price || 0;

          return (
            <div
              key={plan.subscriptionId}
              className={`${styles.planCard} ${plan.subscriptionName === 'Premium' ? styles.featured : ''
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
                        className={`${styles.durationButton} ${selectedDuration === option.subscriptionPlanCountryId
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
