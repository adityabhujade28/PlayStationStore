import React from 'react';
import styles from './Pagination.module.css';

/**
 * Reusable Pagination Component
 * Displays pagination controls for paginated data
 * 
 * Props:
 * - currentPage: number (current page number)
 * - totalPages: number (total pages)
 * - hasNextPage: boolean
 * - hasPreviousPage: boolean
 * - onPageChange: function(pageNumber) - callback when page changes
 * - isLoading: boolean (optional, default: false)
 */
function Pagination({
  currentPage,
  totalPages,
  hasNextPage,
  hasPreviousPage,
  onPageChange,
  isLoading = false
}) {
  const handlePrevious = () => {
    if (hasPreviousPage && !isLoading) {
      onPageChange(currentPage - 1);
    }
  };

  const handleNext = () => {
    if (hasNextPage && !isLoading) {
      onPageChange(currentPage + 1);
    }
  };

  const handlePageInput = (e) => {
    const value = parseInt(e.target.value);
    if (value >= 1 && value <= totalPages && !isLoading) {
      onPageChange(value);
    }
  };

  if (totalPages <= 1) {
    return null; // Don't show pagination if only one page
  }

  return (
    <div className={styles.pagination}>
      <button
        className={styles.button}
        onClick={handlePrevious}
        disabled={!hasPreviousPage || isLoading}
        title="Previous page"
      >
        ← Previous
      </button>

      <div className={styles.pageInfo}>
        <label htmlFor="pageInput">Page</label>
        <input
          id="pageInput"
          type="number"
          min="1"
          max={totalPages}
          value={currentPage}
          onChange={handlePageInput}
          disabled={isLoading}
          className={styles.pageInput}
        />
        <span className={styles.pageTotal}>of {totalPages}</span>
      </div>

      <button
        className={styles.button}
        onClick={handleNext}
        disabled={!hasNextPage || isLoading}
        title="Next page"
      >
        Next →
      </button>

      {isLoading && <span className={styles.loading}>Loading...</span>}
    </div>
  );
}

export default Pagination;
