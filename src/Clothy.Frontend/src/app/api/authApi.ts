import type { LoginFormData } from "../schemas/loginSchema.ts";
import apiClient from "./client.ts";
import { useAuthStore } from "../stores/authStore.ts";
import type { RegisterFormData } from "../schemas/registerSchema.ts";
import type { IUserReadDTO } from "../../entities/usersService/IUserReadDTO.ts";
import type { ResetPasswordSchema } from "../schemas/resetPasswordSchema.ts";
import type {UserUpdateFormData} from "../schemas/userUpdateSchema.ts";
import type {ForgotPasswordFormData} from "../schemas/forgotPasswordSchema.ts";

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
    loginAsync: async (body: LoginFormData): Promise<ILoginResponse> => {
        const response = await apiClient.post<ILoginResponse>("/api/auth/login", body);
        const { tokens, user } = response.data;

        const { setTokens, setUser } = useAuthStore.getState();

        setTokens(tokens.accessToken, tokens.refreshToken);
        setUser(user);

        return response.data;
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

    updateMyAccountAsync: async (body: UserUpdateFormData): Promise<IUserReadDTO> => {
        const formData = new FormData();
        formData.append('FirstName', body.firstName);
        formData.append('LastName', body.lastName);
        formData.append('PhoneNumber', body.phoneNumber);

        if (body.photo) {
            formData.append("Photo", body.photo);
        }

        const response = await apiClient.put<IUserReadDTO>("api/users/me", formData, {
            headers: {
                "Content-Type": "multipart/form-data",
            },
        });

        return response.data;
    },

    forgotPasswordAsync: async (body: ForgotPasswordFormData): Promise<void> => {
        await apiClient.post("/api/auth/forgot-password", body);
    }
};