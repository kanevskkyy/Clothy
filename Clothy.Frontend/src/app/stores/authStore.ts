import { create } from "zustand/react";
import { persist, createJSONStorage } from "zustand/middleware";
import type { IUserReadDTO } from "../../entities/usersService/IUserReadDTO.ts";

export interface IAuthStore {
    accessToken: string | null;
    refreshToken: string | null;
    user: IUserReadDTO | null;

    setTokens: (accessToken: string, refreshToken: string) => void;
    setUser: (user: IUserReadDTO | null) => void;
    logout: () => void;
    isAuthenticated: () => boolean;
}

export const useAuthStore = create<IAuthStore>()(
    persist(
        (set, get) => ({
            accessToken: null,
            refreshToken: null,
            user: null,

            setTokens: (accessToken, refreshToken) => {
                set({
                    accessToken,
                    refreshToken,
                });
            },

            setUser: (user) => {
                set({
                    user,
                });
            },

            logout: () => {
                set({
                    accessToken: null,
                    refreshToken: null,
                    user: null,
                });
            },

            isAuthenticated: () => !!get().accessToken,
        }),
        {
            name: "auth-storage",
            storage: createJSONStorage(() => localStorage),
        }
    )
);