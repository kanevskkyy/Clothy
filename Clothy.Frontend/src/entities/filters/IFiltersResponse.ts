import type {IPriceRange} from "./IPriceRange.ts";
import type {IGender} from "./IGender.ts";
import type {IFilterTagResponse} from "../tags/IFilterTagResponse.ts";
import type {IFilterSizeResponse} from "../sizes/IFilterSizeResponse.ts";
import type {IMaterialFilterResponse} from "../materials/IMaterialFilterResponse.ts";
import type {IFilterColorResponse} from "../colors/IFilterColorResponse.ts";
import type {ICollectionFilterResponse} from "../collection/ICollectionFilterResponse.ts";
import type {IClothingTypeFilterResponse} from "../clothyType/IClothingTypeFilterResponse.ts";
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