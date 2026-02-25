import type {IPriceRange} from "./IPriceRange.ts";
import type {IGender} from "../shared/IGender.ts";
import type {IFilterTagResponse} from "../tag/IFilterTagResponse.ts";
import type {IFilterSizeResponse} from "../size/IFilterSizeResponse.ts";
import type {IMaterialFilterResponse} from "../material/IMaterialFilterResponse.ts";
import type {IFilterColorResponse} from "../color/IFilterColorResponse.ts";
import type {ICollectionFilterResponse} from "../collection/ICollectionFilterResponse.ts";
import type {IClothingTypeFilterResponse} from "../clothingType/IClothingTypeFilterResponse.ts";
import type {IBrandFilterResponse} from "../brand/IBrandFilterResponse.ts";

export interface IFiltersResponse {
    priceRange: IPriceRange;
    gender: IGender;
    tags: IFilterTagResponse[];
    sizes: IFilterSizeResponse[];
    materials: IMaterialFilterResponse[];
    colors: IFilterColorResponse[];
    collections: ICollectionFilterResponse[];
    clothingTypes: IClothingTypeFilterResponse[];
    brands: IBrandFilterResponse[];
}