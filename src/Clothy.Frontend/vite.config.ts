import { defineConfig, loadEnv } from 'vite';
import svgr from 'vite-plugin-svgr';
import path from 'path';
import react from "@vitejs/plugin-react-swc";

export default defineConfig(({ mode }) => {
  const parentDir = path.resolve(__dirname, '../..');
  loadEnv(mode, parentDir, '');

  return {
    plugins: [react(), svgr()],
    envDir: parentDir,
    resolve: {
      alias: {
        '@': path.resolve(__dirname, 'src'),
      },
    },
    server: {
      host: true,
      allowedHosts: ['chromatographic-shanice-theogonic.ngrok-free.dev'],
    },
  };
});