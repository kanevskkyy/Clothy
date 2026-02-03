import type {IBrandReadDTO} from "../../brand/interfaces/IBrandReadDTO.ts";
import type {IClothingTypeReadDTO} from "../../clothyType/interfaces/IClothingTypeReadDTO.ts";
import type {ICollectionReadDTO} from "../../collection/interfaces/ICollectionReadDTO.ts";
import type {ITagReadDTO} from "../../tags/interfaces/ITagReadDTO.ts";
import type {IMaterialPercentage} from "../../materials/interfaces/IMaterialPercentage.ts";
import type {IStockReadDTO} from "../../stocks/interfaces/IStockReadDTO.ts";
import type {IClothePhotos} from "../../photos/interfaces/IClothePhotos.ts";

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