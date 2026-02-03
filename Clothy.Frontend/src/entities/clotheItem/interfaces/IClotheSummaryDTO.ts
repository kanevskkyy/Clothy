import type {IBrandReadDTO} from "../../brand/interfaces/IBrandReadDTO.ts";
import type { IClotheColorSummaryDTO } from "../../colors/interfaces/IClotheColorSummaryDTO.ts";

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