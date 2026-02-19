export interface IOrderCreateDTO {
    pickupPointId: string;
    phoneNumber: string;
    firstName: string;
    lastName: string;
    email: string;
    comment?: string;
}