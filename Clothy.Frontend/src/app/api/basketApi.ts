import apiClient from "./client.ts";

export interface IBasketItemCreateDTO {
    clotheId: string;
    sizeId: string;
    colorId: string;
    quantity: number;
}

export const basketApi = {
    addToCartAsync: async ( body: IBasketItemCreateDTO ): Promise<void> => {
        await apiClient.post<void>("/api/basket/items", body);
    }
}