import styles from './CategoryFilter.module.css';

function CategoryFilter({ categories, selectedCategory, onSelectCategory }) {
  return (
    <div className={styles.filterContainer}>
      <button
        className={`${styles.categoryButton} ${!selectedCategory ? styles.active : ''}`}
        onClick={() => onSelectCategory(null)}
      >
        All Games
      </button>
      {categories.map((category) => (
        <button
          key={category.categoryId}
          className={`${styles.categoryButton} ${selectedCategory === category.categoryId ? styles.active : ''}`}
          onClick={() => onSelectCategory(category.categoryId)}
        >
          {category.categoryName}
        </button>
      ))}
    </div>
  );
}

export default CategoryFilter;
