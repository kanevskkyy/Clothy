export interface IPickupPointReadDTO {
    id: string;
    address: string;
    ref: string;
    isActive: boolean;
    deliveryProviderId: string;
    settlementId: string;
    createdAt?: string;
    updatedAt?: string;
}