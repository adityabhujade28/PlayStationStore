import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  build: {
    // Optimize chunk splitting and bundling
    rollupOptions: {
      output: {
        // Group CSS files together
        assetFileNames: (assetInfo) => {
          if (assetInfo.name.endsWith('.css')) {
            return 'assets/styles.[hash].css';
          }
          return 'assets/[name].[hash][extname]';
        },
        // Better chunk naming for debugging
        chunkFileNames: 'assets/[name].[hash].js',
        entryFileNames: 'assets/[name].[hash].js',
        // Optimize chunk sizes
        manualChunks: {
          // Vendor chunk
          'vendor': [
            'react',
            'react-dom',
            'react-router-dom'
          ]
        }
      }
    },
    // Increase chunk size warnings threshold
    chunkSizeWarningLimit: 500,
    // DISABLE CSS code splitting - bundle all CSS into one file
    cssCodeSplit: false,
    // Use esbuild for minification (default, faster than terser)
    minify: 'esbuild',
    // Source maps for debugging (optional, remove for smaller build)
    sourcemap: false
  },
  // Optimize dependencies
  optimizeDeps: {
    include: ['react', 'react-dom', 'react-router-dom']
  }
})
