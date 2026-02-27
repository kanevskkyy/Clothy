import type { IClotheColorSummaryDTO } from "../color/IClotheColorSummaryDTO.ts";

export interface IClotheSummaryDTO{
    id: string;
    name: string;
    slug: string;
    price: number;
    oldPrice?: number;
    discountPercent?: number;
    colors: IClotheColorSummaryDTO[];
    isAvailable: boolean;
}