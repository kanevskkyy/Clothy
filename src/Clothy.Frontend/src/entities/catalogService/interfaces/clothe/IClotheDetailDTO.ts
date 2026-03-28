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

interface IClotheByIdPhotoDTO {
    id: string;
    photoURL: string;
    colorId: string;
    isMain: boolean;
}

interface IClotheByIdClothingTypeDTO {
    id: string;
    name: string;
    slug: string;
    createdAt?: string;
}

interface IClotheStockColorDTO {
    id: string;
    name: string;
    hexCode: string;
    slug: string;
    createdAt?: string;
}

interface IClotheStockSizeDTO {
    id: string;
    name: string;
    createdAt?: string;
}

export interface IClotheStockDTO {
    stockId: string;
    size: IClotheStockSizeDTO;
    color: IClotheStockColorDTO;
    quantity: number;
}

export interface IClotheByIdDTO {
    id: string;
    name: string;
    slug: string;
    description: string;
    price: number;
    gender: string;
    brand: IBrandReadDTO;
    clothyType: IClotheByIdClothingTypeDTO;
    collection: ICollectionReadDTO;
    additionalPhotos: IClotheByIdPhotoDTO[];
    tags: ITagReadDTO[];
    materials: IMaterialPercentage[];
    stocks: IClotheStockDTO[];
}