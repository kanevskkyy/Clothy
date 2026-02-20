import { createRoot } from 'react-dom/client'
import './index.css'
import {AppRouter} from "./app/providers/AppRouter.tsx";
import {QueryClient, QueryClientProvider} from "@tanstack/react-query";

const queryClient = new QueryClient({
    defaultOptions: {
        queries: {
            staleTime: 1000 * 60 * 5,
            gcTime: 1000 * 60 * 10,
            retry: 2,
            refetchOnWindowFocus: false
        }
    }
});

createRoot(document.getElementById('root')!).render(
    <QueryClientProvider client={queryClient}>
        <AppRouter />
    </QueryClientProvider>
)