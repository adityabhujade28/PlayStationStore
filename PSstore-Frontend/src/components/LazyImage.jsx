import { useState, useRef, useEffect } from 'react';
import styles from './LazyImage.module.css';

function LazyImage({ src, alt, className = '', placeholderSize = 'small' }) {
  const [isLoaded, setIsLoaded] = useState(false);
  const [isVisible, setIsVisible] = useState(false);
  const imgRef = useRef(null);

  useEffect(() => {
    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          setIsVisible(true);
          observer.unobserve(entry.target);
        }
      },
      {
        rootMargin: '50px', // Start loading 50px before image enters viewport
      }
    );

    if (imgRef.current) {
      observer.observe(imgRef.current);
    }

    return () => {
      if (imgRef.current) {
        observer.unobserve(imgRef.current);
      }
    };
  }, []);

  return (
    <div ref={imgRef} className={`${styles.lazyImageContainer} ${className}`}>
      {isVisible ? (
        <img
          src={src}
          alt={alt}
          className={`${styles.image} ${isLoaded ? styles.loaded : styles.loading}`}
          onLoad={() => setIsLoaded(true)}
          onError={() => setIsLoaded(true)} // Mark as loaded even on error
        />
      ) : (
        <div className={styles.placeholder}>
          <div className={styles.blurPlaceholder}>🎮</div>
        </div>
      )}
    </div>
  );
}

export default LazyImage;
