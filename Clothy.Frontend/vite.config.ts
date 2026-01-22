import { defineConfig } from 'vite';
import svgr from 'vite-plugin-svgr';
import path from 'path';
import react from "@vitejs/plugin-react-swc";

export default defineConfig({
  plugins: [react(), svgr()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, 'src'),
    },
  },
  server: {
    host: true,
    allowedHosts: ['chromatographic-shanice-theogonic.ngrok-free.dev'],
  },
});