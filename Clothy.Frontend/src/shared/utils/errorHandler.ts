import type { AxiosError } from 'axios';

interface BackendErrorResponse {
    error: string;
    type?: string;
    detail?: string;
    message?: string;
}

export const getErrorMessage = (error: unknown): string => {
    if (error && typeof error === 'object' && 'response' in error) {
        const axiosError = error as AxiosError<BackendErrorResponse>;

        if (axiosError.response?.data) {
            const data = axiosError.response.data;

            if (data.error) return data.error;
            if (data.detail) return data.detail;
            if (data.message) return data.message;
        }

        if (axiosError.response?.status) {
            return `Server error: ${axiosError.response.status}`;
        }

        if (axiosError.message) {
            return axiosError.message;
        }
    }

    if (error instanceof Error) return error.message;

    if (typeof error === 'string') return error;

    return "An unexpected error occurred";
};