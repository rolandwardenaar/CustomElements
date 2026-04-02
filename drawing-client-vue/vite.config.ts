import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import { resolve } from 'path';

export default defineConfig({
  plugins: [
    vue({
      // Enable custom element mode for *.ce.vue files
      customElement: true,
    }),
  ],
  define: {
    'process.env.NODE_ENV': JSON.stringify('production'),
  },
  build: {
    lib: {
      entry: resolve(__dirname, 'src/main.ts'),
      name: 'VueDrawingClient',
      formats: ['iife'],
      fileName: () => 'vue-drawing-client.js',
    },
    rollupOptions: {
      // Bundle everything — no external dependencies for the custom element
      external: [],
    },
    // Output to dist/ as a single self-contained JS file
    outDir: 'dist',
    emptyOutDir: true,
  },
  resolve: {
    alias: {
      '@': resolve(__dirname, 'src'),
    },
  },
});
