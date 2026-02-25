export interface IOrderItemDTO {
    id: string;
    clotheId: string;
    clotheName: string;
    price: number;
    quantity: number;
    mainPhoto: string;
    colorId: string;
    hexCode: string;
    sizeId: string;
    sizeName: string;
    isClotheDeleted: boolean;
    isClotheUpdated: boolean;
}