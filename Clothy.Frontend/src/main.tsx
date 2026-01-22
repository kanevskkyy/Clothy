import { createRoot } from 'react-dom/client'
import './index.css'
import {AppRouter} from "./app/providers/AppRouter.tsx";

createRoot(document.getElementById('root')!).render(
  <AppRouter/>
)
