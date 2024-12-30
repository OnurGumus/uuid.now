import { defineConfig  } from 'vite'

export default defineConfig({
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