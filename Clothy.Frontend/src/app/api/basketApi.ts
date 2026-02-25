import apiClient from "./client.ts";
import type {IBasketList} from "../../entities/basketService/interfaces/IBasketList.ts";

export interface IBasketItemCreateDTO {
    clotheId: string;
    sizeId: string;
    colorId: string;
    quantity: number;
}

export const basketApi = {
    addToCartAsync: async ( body: IBasketItemCreateDTO ): Promise<void> => {
        await apiClient.post<void>("/api/basket/items", body);
    },

    getMyCartAsync: async () : Promise<IBasketList> => {
        const { data } = await apiClient.get<IBasketList>("/api/basket");
        return data;
    },

    clearCartAsync: async (): Promise<void> => {
        await apiClient.delete("/api/basket");
    },

    updateCartAsync: async ( body: IBasketItemCreateDTO ): Promise<void> => {
        await apiClient.put("/api/basket/items", body);
    },

    deleteFromCartAsync: async ( clotheId: string, sizeId: string, colorId: string ): Promise<void> => {
        await apiClient.delete(`/api/basket/items/${clotheId}/${sizeId}/${colorId}`);
    }
}