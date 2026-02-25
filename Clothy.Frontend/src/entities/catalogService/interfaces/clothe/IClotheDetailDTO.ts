import type {IBrandReadDTO} from "../brand/IBrandReadDTO.ts";
import type {IClothingTypeReadDTO} from "../clothingType/IClothingTypeReadDTO.ts";
import type {ICollectionReadDTO} from "../collection/ICollectionReadDTO.ts";
import type {ITagReadDTO} from "../tag/ITagReadDTO.ts";
import type {IMaterialPercentage} from "../material/IMaterialPercentage.ts";
import type {IStockReadDTO} from "./IStockReadDTO.ts";
import type {IClothePhotos} from "./IClothePhotos.ts";

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