import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import Navbar from '../components/Navbar';
import SearchBar from '../components/SearchBar';
import CategoryFilter from '../components/CategoryFilter';
import GameCard from '../components/GameCard';
import Pagination from '../components/Pagination';
import styles from './GamesStore.module.css';
import apiClient from '../utils/apiClient';
import { useQuery } from '@tanstack/react-query';

function GamesStore() {
  const { getDecodedToken } = useAuth();
  const decoded = getDecodedToken();
  const userId = decoded?.userId;

  const [selectedCategory, setSelectedCategory] = useState(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 20;

  // Categories (cached)
  const { data: categories = [] } = useQuery({
    queryKey: ['categories'],
    queryFn: async () => {
      const res = await apiClient.get('/categories');
      if (!res.ok) throw new Error('Failed to load categories');
      return res.json();
    },
    staleTime: 24 * 60 * 60 * 1000,
  });

  // User + Country for currency
  const { data: user } = useQuery({
    queryKey: ['user', userId],
    queryFn: async () => {
      const res = await apiClient.get(`/users/${userId}`);
      if (!res.ok) throw new Error('Failed to fetch user');
      return res.json();
    },
    enabled: !!userId,
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

  // Paginated games query - key includes filters and page
  const { data: paginatedData = { items: [], totalCount: 0, pageNumber: 1, pageSize: 20, totalPages: 0, hasNextPage: false, hasPreviousPage: false }, isLoading, error } = useQuery({
    queryKey: ['games', { page: currentPage, pageSize, category: selectedCategory, q: searchQuery, userId }],
    queryFn: async () => {
      const params = new URLSearchParams({
        pageNumber: currentPage,
        pageSize,
        includeDeleted: 'false' // Hide soft-deleted games from users
      });

      // Add userId if available
      if (userId) {
        params.set('userId', userId);
      }

      // Add search term if present
      if (searchQuery) {
        params.set('searchTerm', searchQuery);
      }

      // Add category filter if selected
      if (selectedCategory) {
        params.set('categoryId', selectedCategory);
      }

      const res = await apiClient.get(`/games/paged?${params.toString()}`);
      if (!res.ok) throw new Error('Failed to fetch games');
      return res.json();
    },
    keepPreviousData: true,
    staleTime: 2 * 60 * 1000,
  });

  useEffect(() => {
    setCurrentPage(1);
  }, [selectedCategory, searchQuery]);

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
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const freeGames = paginatedData.items.filter(game => game.freeToPlay);
  const userCurrency = country?.currency || 'USD';
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

            {isLoading ? (
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
                  isLoading={isLoading}
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
