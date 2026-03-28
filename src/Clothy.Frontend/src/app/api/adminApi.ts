import apiClient from "./client.ts";

export interface IAdminDashboard {
    newOrdersCount: number;
    totalPrice: number;
    pendingOrdersCount: number;
    totalItemsCount: number;
    pendingReviewCount: number;
    questionsWithoutAnswerCount: number;
}

export const adminApi = {
    getDashboardAsync: async (): Promise<IAdminDashboard> => {
        const { data } = await apiClient.get<IAdminDashboard>("/api/dashboard");
        return data;
    }
}