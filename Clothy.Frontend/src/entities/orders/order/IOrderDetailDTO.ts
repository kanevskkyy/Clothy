import type {IOrderStatusReadDTO} from "../orderStatus/IOrderStatusReadDTO.ts";
import type {IOrderItemDTO} from "./IOrderItemDTO.ts";
import type {IDeliveryDetailDTO} from "./IDeliveryDetailDTO.ts";

export interface IOrderDetailDTO {
    id: string;
    status: IOrderStatusReadDTO;
    userFirstName: string;
    userLastName: string;
    userEmail: string;
    comment?: string;
    isFreeDelivery: boolean;
    totalPrice: number;
    items: IOrderItemDTO[];
    deliveryDetail: IDeliveryDetailDTO;
    createdAt: string;
    updatedAt?: string;
}