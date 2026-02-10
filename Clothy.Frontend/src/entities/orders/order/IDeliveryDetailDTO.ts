import type {IPickupPointReadDTO} from "../pickupPoints/IPickupPointReadDTO.ts";
import type {IDeliveryProviderReadDTO} from "../deliveryProviders/IDeliveryProviderReadDTO.ts";
import type {ISettlementReadDTO} from "../settlement/ISettlementReadDTO.ts";
import type {IRegionReadDTO} from "../regions/IRegionReadDTO.ts";

export interface IDeliveryDetailDTO {
    id: string;
    phoneNumber: string;
    firstName: string;
    lastName: string;
    middleName: string;
    email: string;
    createdAt: string;
    updatedAt: string;
    pickupPoint: IPickupPointReadDTO;
    deliveryProvider: IDeliveryProviderReadDTO;
    settlement: ISettlementReadDTO;
    region: IRegionReadDTO;
}