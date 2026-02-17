import type { LoginFormData } from "../schemas/loginSchema.ts";
import apiClient from "./client.ts";
import { useAuthStore } from "./stores/authStore.ts";
import type { RegisterFormData } from "../schemas/registerSchema.ts";
import type { IUserReadDTO } from "../../entities/usersService/IUserReadDTO.ts";
import type { ResetPasswordSchema } from "../schemas/resetPasswordSchema.ts";

interface ITokenResponse {
    accessToken: string;
    refreshToken: string;
}

export interface IResendVerificationEmailRequest {
    email: string;
}

interface ILoginResponse {
    tokens: ITokenResponse;
    user: IUserReadDTO;
}

export interface IRegisterResponse {
    user: IUserReadDTO;
    tokens: ITokenResponse;
}

export const authApi = {
    loginAsync: async (body: LoginFormData): Promise<void> => {
        const response = await apiClient.post<ILoginResponse>("/api/auth/login", body);
        const { tokens, user } = response.data;

        const { setTokens, setUser } = useAuthStore.getState();
        setTokens(tokens.accessToken, tokens.refreshToken);
        setUser(user);
    },

    registerAsync: async (body: RegisterFormData): Promise<void> => {
        const response = await apiClient.post<IRegisterResponse>("/api/auth/register", body);
        const { tokens, user } = response.data;

        const { setTokens, setUser } = useAuthStore.getState();
        setTokens(tokens.accessToken, tokens.refreshToken);
        setUser(user);
    },

    logoutAsync: async (): Promise<void> => {
        const { refreshToken, logout } = useAuthStore.getState();

        try {
            if (refreshToken) {
                await apiClient.post("/api/auth/logout", { refreshToken });
            }
        } catch (error) {
            console.error("Logout request failed:", error);
        } finally {
            logout();
        }
    },

    resendVerificationEmailAsync: async (body: IResendVerificationEmailRequest): Promise<void> => {
        await apiClient.post("/api/auth/resend-verification", body);
    },

    resetPasswordAsync: async (body: ResetPasswordSchema): Promise<void> => {
        await apiClient.post("/api/auth/reset-password", body);
    },

    getInfoAboutMeAsync: async (): Promise<IUserReadDTO> => {
        const response = await apiClient.get<IUserReadDTO>("/api/users/me");
        return response.data;
    },
};