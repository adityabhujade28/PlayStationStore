// Currency utility functions

export const getCurrencySymbol = (currencyCode) => {
  const symbols = {
    'INR': '₹',
    'GBP': '£',
    'USD': '$',
    'EUR': '€',
    'JPY': '¥',
    'AUD': 'A$',
    'CAD': 'C$',
    'CNY': '¥'
  };
  return symbols[currencyCode] || currencyCode;
};

export const formatPrice = (price, currencyCode) => {
  if (price === null || price === undefined || isNaN(price)) {
    return 'N/A';
  }
  const symbol = getCurrencySymbol(currencyCode);
  return `${symbol}${parseFloat(price).toFixed(2)}`;
};
