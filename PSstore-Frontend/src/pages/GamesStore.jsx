import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import Navbar from '../components/Navbar';
import SearchBar from '../components/SearchBar';
import CategoryFilter from '../components/CategoryFilter';
import GameCard from '../components/GameCard';
import styles from './GamesStore.module.css';
import apiClient from '../utils/apiClient';

function GamesStore() {
  const { getDecodedToken } = useAuth();
  const [games, setGames] = useState([]);
  const [categories, setCategories] = useState([]);
  const [filteredGames, setFilteredGames] = useState([]);
  const [selectedCategory, setSelectedCategory] = useState(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [userCurrency, setUserCurrency] = useState('INR');

  useEffect(() => {
    fetchCategories();
    fetchGames();
    fetchUserCurrency();
  }, []);

  useEffect(() => {
    filterGames();
  }, [games, selectedCategory, searchQuery]);

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

  const fetchUserLibrary = async (userId) => {
    if (!userId) return new Set();
    try {
      const response = await apiClient.get(`/users/${userId}/library`);
      if (response.ok) {
        const data = await response.json();
        // Return set of game IDs that user can access
        return new Set(data.accessibleGames.map(g => g.gameId));
      }
    } catch (err) {
      console.error('Failed to fetch user library:', err);
    }
    return new Set();
  };

  const fetchUserCurrency = async () => {
    try {
      const decoded = getDecodedToken();
      const userId = decoded?.userId;

      const response = await apiClient.get(`/users/${userId}`);

      if (response.ok) {
        const userData = await response.json();
        // console.log('User data:', userData);
        // Fetch country details to get currency
        if (userData.countryId) {
          const countryResponse = await apiClient.get(`/countries/${userData.countryId}`);

          if (countryResponse.ok) {
            const countryData = await countryResponse.json();
            // console.log('Country data:', countryData);
            // console.log('Setting currency to:', countryData.currency);
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

      const response = await apiClient.get(`/games?userId=${userId}`);

      if (!response.ok) {
        throw new Error('Failed to fetch games');
      }

      const data = await response.json();

      // Fetch library to check access
      let accessibleGameIds = new Set();
      if (userId) {
        accessibleGameIds = await fetchUserLibrary(userId);
      }

      // Merge access info
      const gamesWithAccess = data.map(game => ({
        ...game,
        canAccess: accessibleGameIds.has(game.gameId),
        // Use a generic logic for access type if needed, or just boolean
        accessType: accessibleGameIds.has(game.gameId) ? 'OWNED' : null
      }));

      setGames(gamesWithAccess);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const filterGames = () => {
    let filtered = [...games];

    // Filter by category
    if (selectedCategory) {
      // Note: Need to fetch games with categories or add category filtering to API
      // For now, we'll need to update the API call when category is selected
      fetchGamesByCategory(selectedCategory);
      return;
    }

    // Filter by search query
    if (searchQuery) {
      filtered = filtered.filter(game =>
        game.gameName.toLowerCase().includes(searchQuery.toLowerCase()) ||
        game.publishedBy?.toLowerCase().includes(searchQuery.toLowerCase())
      );
    }

    setFilteredGames(filtered);
  };

  const fetchGamesByCategory = async (categoryId) => {
    setLoading(true);
    try {
      const decoded = getDecodedToken();
      const userId = decoded?.userId;

      const response = await apiClient.get(`/games/category/${categoryId}?userId=${userId}`);

      if (response.ok) {
        const data = await response.json();

        // Fetch library to check access if userId is present
        // We can potentially cache this, but for now fetch again to be safe/simple
        let accessibleGameIds = new Set();
        if (userId) {
          accessibleGameIds = await fetchUserLibrary(userId);
        }

        const gamesWithAccess = data.map(game => ({
          ...game,
          canAccess: accessibleGameIds.has(game.gameId),
          accessType: accessibleGameIds.has(game.gameId) ? 'OWNED' : null
        }));

        // Apply search filter if needed
        if (searchQuery) {
          const filtered = gamesWithAccess.filter(game =>
            game.gameName.toLowerCase().includes(searchQuery.toLowerCase()) ||
            game.publishedBy?.toLowerCase().includes(searchQuery.toLowerCase())
          );
          setFilteredGames(filtered);
        } else {
          setFilteredGames(gamesWithAccess);
        }
      }
    } catch (err) {
      console.error('Failed to fetch games by category:', err);
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

  const freeGames = games.filter(game => game.freeToPlay);

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

          {/* Free to Play Section */}
          {!selectedCategory && !searchQuery && freeGames.length > 0 && (
            <section className={styles.section}>
              <h2 className={styles.sectionTitle}> Free to Play</h2>
              <div className={styles.gamesGrid}>
                {freeGames.map((game) => (
                  <GameCard key={game.gameId} game={game} currency={userCurrency} />
                ))}
              </div>
            </section>
          )}

          {/* All Games Section */}
          <section className={styles.section}>
            <h2 className={styles.sectionTitle}>
              {selectedCategory
                ? categories.find(c => c.categoryId === selectedCategory)?.categoryName
                : searchQuery
                  ? `Search Results (${filteredGames.length})`
                  : 'All Games'}
            </h2>

            {loading ? (
              <div className={styles.loading}>Loading games...</div>
            ) : filteredGames.length === 0 ? (
              <div className={styles.empty}>
                <p>No games found</p>
              </div>
            ) : (
              <div className={styles.gamesGrid}>
                {filteredGames.map((game) => (
                  <GameCard key={game.gameId} game={game} currency={userCurrency} />
                ))}
              </div>
            )}
          </section>
        </div>
      </div>
    </>
  );
}

export default GamesStore;
