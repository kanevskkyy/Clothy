import type {IBasketItemCart} from "./IBasketItemCart.ts";

export interface IBasketList {
    userId: string;
    items: IBasketItemCart[];
    originalPrice: number;
    totalPrice: number;
    totalItems: number;
    unAvailableItemsCount: number;
    isFirstOrder: boolean;
}