import axios, { type InternalAxiosRequestConfig, type AxiosError } from 'axios';
import { useAuthStore } from '../stores/authStore.ts';

const API_BASE_URL: string = import.meta.env.VITE_API_URL || 'http://localhost:5000';

interface IRefreshTokenRequest {
    refreshToken: string;
}

interface IRefreshTokenResponse {
    accessToken: string;
    refreshToken: string;
}

interface BackendErrorResponse {
    error: string;
    type?: string;
}

export const apiClient = axios.create({
    baseURL: API_BASE_URL,
    timeout: 30000,
    headers: {
        'Content-Type': 'application/json',
    },
});

let isRefreshing = false;
let failedQueue: Array<{
    resolve: (value: unknown) => void;
    reject: (reason?: unknown) => void;
}> = [];

const processQueue = (error: Error | null, token: string | null = null) => {
    failedQueue.forEach((prom) => {
        if (error) {
            prom.reject(error);
        } else {
            prom.resolve(token);
        }
    });

    failedQueue = [];
};

apiClient.interceptors.request.use(
    (config: InternalAxiosRequestConfig) => {
        const { accessToken } = useAuthStore.getState();

        if (accessToken && config.headers) {
            config.headers.Authorization = `Bearer ${accessToken}`;
        }

        if (config.data instanceof FormData) {
            delete config.headers['Content-Type'];
        }

        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

apiClient.interceptors.response.use(
    (response) => response,
    async (error: AxiosError<BackendErrorResponse>) => {
        const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

        if (error.response?.status === 404) {
            if (typeof window !== 'undefined' && !window.location.pathname.includes('/not-found')) {
                window.location.href = '/not-found';
            }
            return Promise.reject(error);
        }

        if (error.response?.status === 403) {
            if (typeof window !== 'undefined' && !window.location.pathname.includes('/forbidden')) {
                window.location.href = '/forbidden';
            }
            return Promise.reject(error);
        }

        if (
            error.response?.status === 401 &&
            !originalRequest._retry &&
            !originalRequest.url?.includes('/auth/refresh')
        ) {
            if (
                originalRequest.url?.includes('/auth/login') ||
                originalRequest.url?.includes('/auth/register')
            ) {
                return Promise.reject(error);
            }

            if (isRefreshing) {
                return new Promise((resolve, reject) => {
                    failedQueue.push({ resolve, reject });
                })
                    .then((token) => {
                        if (originalRequest.headers) {
                            originalRequest.headers.Authorization = `Bearer ${token}`;
                        }
                        return apiClient(originalRequest);
                    })
                    .catch((err) => {
                        return Promise.reject(err);
                    });
            }

            originalRequest._retry = true;
            isRefreshing = true;

            const { refreshToken, logout, setTokens } = useAuthStore.getState();

            if (refreshToken) {
                try {
                    const response = await axios.post<IRefreshTokenResponse>(
                        `${API_BASE_URL}/api/auth/refresh`,
                        {
                            refreshToken: refreshToken,
                        } as IRefreshTokenRequest,
                        {
                            headers: {
                                'Content-Type': 'application/json',
                            },
                        }
                    );

                    const { accessToken: newAccessToken, refreshToken: newRefreshToken } = response.data;

                    setTokens(newAccessToken, newRefreshToken);

                    if (originalRequest.headers) {
                        originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
                    }

                    processQueue(null, newAccessToken);

                    return apiClient(originalRequest);
                } catch (refreshError) {
                    processQueue(refreshError as Error, null);
                    logout();

                    if (typeof window !== 'undefined' && !window.location.pathname.includes('/login')) {
                        window.location.href = '/login';
                    }

                    return Promise.reject(refreshError);
                } finally {
                    isRefreshing = false;
                }
            } else {
                isRefreshing = false;
                logout();

                if (typeof window !== 'undefined' && !window.location.pathname.includes('/login')) {
                    window.location.href = '/login';
                }

                return Promise.reject(error);
            }
        }

        return Promise.reject(error);
    }
);

export default apiClient;