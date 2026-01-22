import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import Navbar from '../components/Navbar';
import SearchBar from '../components/SearchBar';
import CategoryFilter from '../components/CategoryFilter';
import GameCard from '../components/GameCard';
import Pagination from '../components/Pagination';
import styles from './GamesStore.module.css';
import apiClient from '../utils/apiClient';

function GamesStore() {
  const { getDecodedToken } = useAuth();
  const [categories, setCategories] = useState([]);
  const [selectedCategory, setSelectedCategory] = useState(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [userCurrency, setUserCurrency] = useState('INR');
  
  // Pagination states
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize] = useState(20);
  const [paginatedData, setPaginatedData] = useState({
    items: [],
    totalCount: 0,
    pageNumber: 1,
    pageSize: 20,
    totalPages: 0,
    hasNextPage: false,
    hasPreviousPage: false
  });

  useEffect(() => {
    fetchCategories();
    fetchUserCurrency();
  }, []);

  useEffect(() => {
    setCurrentPage(1); // Reset to page 1 when filters change
  }, [selectedCategory, searchQuery]);

  useEffect(() => {
    fetchGames();
  }, [currentPage, selectedCategory, searchQuery]);

  const fetchCategories = async () => {
    try {
      const response = await apiClient.get('/categories');
      if (response.ok) {
        const data = await response.json();
        setCategories(data);
      }
    } catch (err) {
      console.error('Failed to fetch categories:', err);
    }
  };

  const fetchUserCurrency = async () => {
    try {
      const decoded = getDecodedToken();
      const userId = decoded?.userId;

      const response = await apiClient.get(`/users/${userId}`);
      if (response.ok) {
        const userData = await response.json();
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

  const fetchGames = async () => {
    setLoading(true);
    setError('');

    try {
      const decoded = getDecodedToken();
      const userId = decoded?.userId;

      let endpoint = '';
      let params = new URLSearchParams({
        pageNumber: currentPage,
        pageSize: pageSize,
        userId: userId || ''
      });

      if (searchQuery) {
        // Search with pagination
        endpoint = `/games/search/paged`;
        params.set('query', searchQuery);
      } else if (selectedCategory) {
        // Category with pagination
        endpoint = `/games/category/${selectedCategory}/paged`;
      } else {
        // All games with pagination
        endpoint = `/games/paged`;
      }

      const response = await apiClient.get(`${endpoint}?${params.toString()}`);

      if (!response.ok) {
        throw new Error('Failed to fetch games');
      }

      const data = await response.json();
      setPaginatedData(data);
    } catch (err) {
      setError(err.message);
      setPaginatedData({
        items: [],
        totalCount: 0,
        pageNumber: 1,
        pageSize: 20,
        totalPages: 0,
        hasNextPage: false,
        hasPreviousPage: false
      });
    } finally {
      setLoading(false);
    }
  };

  const handleCategoryChange = (categoryId) => {
    setSelectedCategory(categoryId);
  };

  const handleSearchChange = (query) => {
    setSearchQuery(query);
  };

  const handleClearSearch = () => {
    setSearchQuery('');
  };

  const handlePageChange = (pageNumber) => {
    setCurrentPage(pageNumber);
    // Scroll to top
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const freeGames = paginatedData.items.filter(game => game.freeToPlay);
  const displayTitle = selectedCategory
    ? categories.find(c => c.categoryId === selectedCategory)?.categoryName
    : searchQuery
      ? `Search Results (${paginatedData.totalCount})`
      : 'All Games';

  return (
    <>
      <Navbar />
      <div className={styles.store}>
        <div className={styles.container}>
          <div className={styles.header}>
            <h1 className={styles.title}>Game Store</h1>
            <p className={styles.subtitle}>Browse our collection of amazing games</p>
          </div>

          <SearchBar
            value={searchQuery}
            onChange={handleSearchChange}
            onClear={handleClearSearch}
          />

          <CategoryFilter
            categories={categories}
            selectedCategory={selectedCategory}
            onSelectCategory={handleCategoryChange}
          />

          {error && <div className={styles.error}>{error}</div>}

          {/* Free to Play Section - Only on first page without filters */}
          {!selectedCategory && !searchQuery && currentPage === 1 && freeGames.length > 0 && (
            <section className={styles.section}>
              <h2 className={styles.sectionTitle}>Free to Play</h2>
              <div className={styles.gamesGrid}>
                {freeGames.map((game) => (
                  <GameCard key={game.gameId} game={game} currency={userCurrency} />
                ))}
              </div>
            </section>
          )}

          {/* All Games Section */}
          <section className={styles.section}>
            <h2 className={styles.sectionTitle}>{displayTitle}</h2>

            {loading ? (
              <div className={styles.loading}>Loading games...</div>
            ) : paginatedData.items.length === 0 ? (
              <div className={styles.empty}>
                <p>No games found</p>
              </div>
            ) : (
              <>
                <div className={styles.gamesGrid}>
                  {paginatedData.items.map((game) => (
                    <GameCard key={game.gameId} game={game} currency={userCurrency} />
                  ))}
                </div>

                {/* Pagination Controls */}
                <Pagination
                  currentPage={paginatedData.pageNumber}
                  totalPages={paginatedData.totalPages}
                  hasNextPage={paginatedData.hasNextPage}
                  hasPreviousPage={paginatedData.hasPreviousPage}
                  onPageChange={handlePageChange}
                  isLoading={loading}
                />
              </>
            )}
          </section>
        </div>
      </div>
    </>
  );
}

export default GamesStore;
