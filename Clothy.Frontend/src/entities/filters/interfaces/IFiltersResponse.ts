import type {IPriceRange} from "./IPriceRange.ts";
import type {IGender} from "./IGender.ts";
import type {IFilterTagResponse} from "../../tags/interfaces/IFilterTagResponse.ts";
import type {IFilterSizeResponse} from "../../sizes/interfaces/IFilterSizeResponse.ts";
import type {IMaterialFilterResponse} from "../../materials/interfaces/IMaterialFilterResponse.ts";
import type {IFilterColorResponse} from "../../colors/interfaces/IFilterColorResponse.ts";
import type {ICollectionFilterResponse} from "../../collection/interfaces/ICollectionFilterResponse.ts";
import type {IClothingTypeFilterResponse} from "../../clothyType/interfaces/IClothingTypeFilterResponse.ts";
import type {IBrandFilterResponse} from "../../brand/interfaces/IBrandFilterResponse.ts";

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