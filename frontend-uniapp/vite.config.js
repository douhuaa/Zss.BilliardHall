import { defineConfig } from 'vite';
import uni from '@dcloudio/vite-plugin-uni';

const apiTarget =
  process.env['services__api__https__0'] ||
  process.env['services__api__http__0'] ||
  'http://localhost:5000'

export default defineConfig({
  plugins: [uni()],
  server: {
    port: Number(process.env.PORT) || 5173,
    proxy: {
      '/api': {
        target: apiTarget,
        changeOrigin: true,
        rewrite: p => p.replace(/^\/api/, '')
      }
    }
  }
})