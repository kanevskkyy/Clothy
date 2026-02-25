import type {IPickupPointReadDTO} from "../IPickupPointReadDTO.ts";
import type {IDeliveryProviderReadDTO} from "../IDeliveryProviderReadDTO.ts";
import type {ISettlementReadDTO} from "../ISettlementReadDTO.ts";
import type {IRegionReadDTO} from "../IRegionReadDTO.ts";

export interface IDeliveryDetailDTO {
    id: string;
    phoneNumber: string;
    firstName: string;
    lastName: string;
    email: string;
    createdAt: string;
    updatedAt: string;
    pickupPoint: IPickupPointReadDTO;
    deliveryProvider: IDeliveryProviderReadDTO;
    settlement: ISettlementReadDTO;
    region: IRegionReadDTO;
}