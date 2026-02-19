import apiClient from "./client.ts";

export interface IPaymentResponse {
    paymentId: string;
    paymentUrl: string;
    status: string;
}

export const paymentApi = {
    payForOrderAsync: async (orderId: string, method: string): Promise<IPaymentResponse> => {
        const { data } = await apiClient.post<IPaymentResponse>(`api/payments/create?method=${method}`, {
            orderId: orderId,
        });
        return data;
    },

    retryPaymentAsync: async (paymentId: string): Promise<IPaymentResponse> => {
        const { data } = await apiClient.post<IPaymentResponse>(`api/payments/retry/${paymentId}`);
        return data;
    }
}