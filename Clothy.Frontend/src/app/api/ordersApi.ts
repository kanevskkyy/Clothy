import type { PagedList } from "../../shared/lib/pagedList.ts";
import type { IOrderReadDTO } from "../../entities/ordersService/interfaces/order/IOrderReadDTO.ts";
import apiClient from "./client.ts";
import type { IOrderDetailDTO } from "../../entities/ordersService/interfaces/order/IOrderDetailDTO.ts";
import type { IDeliveryProviderReadDTO } from "../../entities/ordersService/interfaces/IDeliveryProviderReadDTO.ts";
import type { IRegionReadDTO } from "../../entities/ordersService/interfaces/IRegionReadDTO.ts";
import type { ISettlementReadDTO } from "../../entities/ordersService/interfaces/ISettlementReadDTO.ts";
import type { IPickupPointReadDTO } from "../../entities/ordersService/interfaces/IPickupPointReadDTO.ts";
import type {IOrderCreateDTO} from "../../entities/ordersService/interfaces/order/IOrderCreateDTO.ts";

export interface ISettlementFilters {
    regionId?: string;
    name?: string;
    pageNumber?: number;
}

export interface IPickupPointsFilters {
    deliveryProviderId?: string;
    settlementId?: string;
    address?: string;
    pageNumber?: number;
}

export const ordersApi = {
    getMyOrdersAsync: async (pageNumber: number): Promise<PagedList<IOrderReadDTO>> => {
        const { data } = await apiClient.get<PagedList<IOrderReadDTO>>(`/api/orders/my?pageNumber=${pageNumber}`);
        return data;
    },

    getAllOrdersAsync: async (pageNumber: number, status?: string): Promise<PagedList<IOrderReadDTO>> => {
        let url = `/api/orders/?pageNumber=${pageNumber}`;

        if (status) url += `&status=${status}`;

        const { data } = await apiClient.get<PagedList<IOrderReadDTO>>(url);
        return data;
    },

    updateOrderAsync: async (id: string, status: string): Promise<void> => {
        await apiClient.put(`/api/orders/${id}`, {
            status: status
        });
    },

    createOrderAsync: async (body : IOrderCreateDTO): Promise<IOrderDetailDTO> => {
        const { data } = await apiClient.post<IOrderDetailDTO>("/api/orders", body);
        return data;
    },

    getOrderByIdAsync: async (orderId: string): Promise<IOrderDetailDTO> => {
        const { data } = await apiClient.get<IOrderDetailDTO>(`/api/orders/${orderId}`);
        return data;
    },

    getDeliveryProvidersAsync: async (): Promise<IDeliveryProviderReadDTO[]> => {
        const { data } = await apiClient.get<IDeliveryProviderReadDTO[]>("/api/delivery-providers");
        return data;
    },

    getAllRegionsAsync: async (): Promise<IRegionReadDTO[]> => {
        const { data } = await apiClient.get<IRegionReadDTO[]>("/api/regions");
        return data;
    },

    getSettlementsAsync: async (filters: ISettlementFilters): Promise<PagedList<ISettlementReadDTO>> => {
        const params = new URLSearchParams({ pageSize: "50" });
        if (filters.regionId) params.set("regionId", filters.regionId);
        if (filters.name) params.set("name", filters.name);
        if (filters.pageNumber) params.set("pageNumber", String(filters.pageNumber));

        const { data } = await apiClient.get<PagedList<ISettlementReadDTO>>(`/api/settlements?${params}`);
        return data;
    },

    getPickupPointsAsync: async (filters: IPickupPointsFilters): Promise<PagedList<IPickupPointReadDTO>> => {
        const params = new URLSearchParams({ pageSize: "50" });
        if (filters.deliveryProviderId) params.set("deliveryProviderId", filters.deliveryProviderId);
        if (filters.settlementId) params.set("settlementId", filters.settlementId);
        if (filters.address) params.set("address", filters.address);
        if (filters.pageNumber) params.set("pageNumber", String(filters.pageNumber));

        const { data } = await apiClient.get<PagedList<IPickupPointReadDTO>>(`/api/pickup-points?${params}`);
        return data;
    },
};