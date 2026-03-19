import { defineConfig } from 'vite';

export default defineConfig({
  server: {
    proxy: {
      '/racehub': {
        target: 'http://localhost:5000',
        ws: true,
      },
    },
  },
});
