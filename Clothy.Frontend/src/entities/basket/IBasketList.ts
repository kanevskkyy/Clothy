import type {IBasketItemCart} from "./IBasketItemCart.ts";

export interface IBasketList {
    userId: string;
    items: IBasketItemCart[];
    totalPrice: number;
}