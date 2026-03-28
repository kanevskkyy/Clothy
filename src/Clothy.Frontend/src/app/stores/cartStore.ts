import {create} from 'zustand';

interface CartStore {
    totalItems: number;
    setTotalItems: (count: number) => void;
    increment: (by?: number) => void;
}

export const useCartStore = create<CartStore>((set) => ({
    totalItems: 0,
    setTotalItems: (count) => set({totalItems: count}),
    increment: (by = 1) => set((state) => ({totalItems: state.totalItems + by})),
}));