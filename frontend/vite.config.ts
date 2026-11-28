import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
  plugins: [react(), tailwindcss(), {
    name: 'signalr-pure-annotations',
    apply: 'build',
    // SignalR 8 coloca estas anotaciones en declaraciones; Rollup solo las admite en llamadas.
    transform(code, id) {
      if (id.endsWith('/@microsoft/signalr/dist/esm/Utils.js'))
        return code.replace(/\/\*#__PURE__\*\/ (?=function (getOsName|getRuntimeVersion)\()/g, '')
    },
  }],
  server: { port: 5173, strictPort: true },
})
