import type {IBrandReadDTO} from "../brand/IBrandReadDTO.ts";
import type {IClothingTypeReadDTO} from "../clothyType/IClothingTypeReadDTO.ts";
import type {ICollectionReadDTO} from "../collection/ICollectionReadDTO.ts";
import type {ITagReadDTO} from "../tags/ITagReadDTO.ts";
import type {IMaterialPercentage} from "../materials/IMaterialPercentage.ts";
import type {IStockReadDTO} from "../stocks/IStockReadDTO.ts";
import type {IClothePhotos} from "../photos/IClothePhotos.ts";

export interface IClotheDetailDTO {
    id: string;
    name: string;
    slug: string;
    description: string;
    price: string;
    gender: string;
    oldPrice?: string;
    hasOldPrice: boolean;
    discountPercentage: number;
    hasDiscountPercentage: boolean;
    brand: IBrandReadDTO;
    clothingType: IClothingTypeReadDTO;
    collection: ICollectionReadDTO;
    additionalPhotos: IClothePhotos[];
    tags: ITagReadDTO[];
    materials: IMaterialPercentage[];
    stocks: IStockReadDTO[];
}