import type {IBrandReadDTO} from "../brand/IBrandReadDTO.ts";
import type { IClotheColorSummaryDTO } from "../color/IClotheColorSummaryDTO.ts";

export interface IClotheSummaryDTO{
    id: string;
    name: string;
    slug: string;
    price: number;
    oldPrice?: number;
    discountPercent?: number;
    brand: IBrandReadDTO;
    colors: IClotheColorSummaryDTO[];
    isAvailable: boolean;
}