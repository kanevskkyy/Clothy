import apiClient from "./client.ts";
import type {IPaymentDeviceDTO} from "../../shared/lib/deviceInfo.ts";

export interface IPaymentResponse {
    paymentId: string;
    paymentUrl: string;
    status: string;
}

export interface IOrderPaymentStatus {
    orderId: string;
    status: "Pending" | "Paid" | "Disputed" | "DisputeWon" | "DisputeLost" | string;
    paymentMethod: string;
    updatedAt: string;
}

export const paymentApi = {
    payForOrderAsync: async (orderId: string, method: string, device?: IPaymentDeviceDTO): Promise<IPaymentResponse> => {
        const { data } = await apiClient.post<IPaymentResponse>(`api/payments/create?method=${method}`, {
            orderId: orderId,
            device: device,
        });
        return data;
    },

    /** Admin only. Up to 100 orders per call. */
    getOrderPaymentStatusesAsync: async (orderIds: string[]): Promise<IOrderPaymentStatus[]> => {
        if (orderIds.length === 0) return [];
        const params = new URLSearchParams();
        orderIds.forEach((id) => params.append("orderIds", id));
        const { data } = await apiClient.get<IOrderPaymentStatus[]>(`/api/payments/admin/statuses?${params.toString()}`);
        return data;
    },

    retryPaymentAsync: async (paymentId: string): Promise<IPaymentResponse> => {
        const { data } = await apiClient.post<IPaymentResponse>(`api/payments/retry/${paymentId}`);
        return data;
    }
}