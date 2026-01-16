import styles from './SearchBar.module.css';

function SearchBar({ value, onChange, onClear }) {
  return (
    <div className={styles.searchContainer}>
      <input
        type="text"
        className={styles.searchInput}
        placeholder="Search games..."
        value={value}
        onChange={(e) => onChange(e.target.value)}
      />
      {value && (
        <button className={styles.clearButton} onClick={onClear}>
          ✕
        </button>
      )}
      <span className={styles.searchIcon}></span>
    </div>
  );
}

export default SearchBar;
