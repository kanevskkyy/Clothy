import type {IBrandReadDTO} from "../brand/brand.ts";
import type {IClotheColorSummaryDTO} from "../colors/colors.ts";

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