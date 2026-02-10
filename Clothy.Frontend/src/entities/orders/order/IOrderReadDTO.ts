import type {IOrderStatusReadDTO} from "../orderStatus/IOrderStatusReadDTO.ts";

export interface IOrderReadDTO {
    id: number;
    status: IOrderStatusReadDTO;
    userId: string;
    userFirstName: string;
    userLastName: string;
    userEmail: string;
    totalPrice: number;
    isFreeDelivery: boolean;
    createdAt?: string;
    updatedAt?: string;
}