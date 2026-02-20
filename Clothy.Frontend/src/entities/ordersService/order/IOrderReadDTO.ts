export interface IOrderReadDTO {
    id: string;
    status: string;
    userId: string;
    userFirstName: string;
    userLastName: string;
    userEmail: string;
    totalPrice: number;
    isFreeDelivery: boolean;
    createdAt: string;
    updatedAt?: string;
}