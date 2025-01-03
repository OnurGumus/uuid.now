import { defineConfig  } from 'vite'

export default defineConfig({
  build: {
    emptyOutDir: true,
    rollupOptions: {
    output: {
        manualChunks(id:string) {
          if (id.includes('fable')) {
            return 'fable';
          }
        }
      }
    }
  },
    server: {
        watch: {
          ignored: [ "**/*.fs"]
        },
        hmr: {
          clientPort: 5173,
          protocol: 'ws'
        }
      }
})