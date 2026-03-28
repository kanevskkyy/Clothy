export interface IBasketItemCart {
    clotheId: string;
    sizeId: string;
    colorId: string;
    clotheName: string;
    clotheSlug: string;
    price: number;
    mainPhoto: string;
    sizeName: string;
    hexCode: string;
    colorName: string;
    colorSlug: string;
    quantity: number;
    isAvailable: boolean;
    validationMessage: string;
}