import { createRoot } from 'react-dom/client'
import { CinepolisProvider } from './context/CinepolisContext'
import { App } from './App'
import './styles.css'
import './cine.css'

createRoot(document.getElementById('root')!).render(<CinepolisProvider><App /></CinepolisProvider>)
