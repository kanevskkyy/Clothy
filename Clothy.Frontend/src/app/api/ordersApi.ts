import type {PagedList} from "../../shared/utils/pagedList.ts";
import type {IOrderReadDTO} from "../../entities/ordersService/order/IOrderReadDTO.ts";
import apiClient from "./client.ts";

export const ordersApi = {
    getMyOrdersAsync: async ( pageNumber: number ): Promise<PagedList<IOrderReadDTO>> => {
        const { data } = await apiClient.get<PagedList<IOrderReadDTO>>(`/api/orders/my?pageNumber=${pageNumber}`);
        return data;
    }
};