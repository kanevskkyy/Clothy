import apiClient from "./client.ts";
import type {PagedList} from "../../shared/lib/pagedList.ts";
import type {IClotheSummaryDTO} from "../../entities/catalogService/interfaces/clothe/IClotheSummaryDTO.ts";
import type {IBrandReadDTO} from "../../entities/catalogService/interfaces/brand/IBrandReadDTO.ts";
import type {IFiltersResponse} from "../../entities/catalogService/interfaces/filters/IFiltersResponse.ts";

interface GetClothesParams {
    pageNumber?: number;
    pageSize?: number;
    brands?: string[];
    clothingTypes?: string[];
    colors?: string[];
    materials?: string[];
    sizes?: string[];
    tags?: string[];
    collections?: string[];
    gender?: string[];
    minPrice?: number;
    maxPrice?: number;
    sortBy?: 'price' | 'name';
    sortDescending?: boolean;
}

const buildQueryString = (params: GetClothesParams): string => {
    const queryParams = new URLSearchParams();

    if (params.pageNumber) queryParams.append('pageNumber', params.pageNumber.toString());
    if (params.pageSize) queryParams.append('pageSize', params.pageSize.toString());

    params.brands?.forEach(id => queryParams.append('brands', id));
    params.clothingTypes?.forEach(id => queryParams.append('clothingTypes', id));
    params.colors?.forEach(id => queryParams.append('colors', id));
    params.materials?.forEach(id => queryParams.append('materials', id));
    params.sizes?.forEach(id => queryParams.append('sizes', id));
    params.tags?.forEach(id => queryParams.append('tags', id));
    params.collections?.forEach(id => queryParams.append('collections', id));

    params.gender?.forEach(g => queryParams.append('gender', g));

    if (params.minPrice !== undefined && !isNaN(params.minPrice)) {
        queryParams.append('minPrice', params.minPrice.toString());
    }
    if (params.maxPrice !== undefined && !isNaN(params.maxPrice)) {
        queryParams.append('maxPrice', params.maxPrice.toString());
    }

    if (params.sortBy) {
        queryParams.append('sortBy', params.sortBy);
        if (params.sortDescending) queryParams.append('sortDescending', 'true');
    }

    const queryString = queryParams.toString();
    return queryString ? `?${queryString}` : '';
};

export const catalogApi = {
    getAllBrandsAsync: async (): Promise<IBrandReadDTO[]> => {
        const { data } = await apiClient.get<IBrandReadDTO[]>("/api/catalog/brands");
        return data;
    },

    getTop8MostSaleAsync: async (): Promise<IClotheSummaryDTO[]> => {
        const { data } = await apiClient.get<IClotheSummaryDTO[]>("/api/catalog/clothes/top8");
        return data;
    },

    getClotheBySlugAsync: async (slug: string | undefined) => {
        const { data } = await apiClient.get(`/api/clothe/${slug}`);
        return data;
    },

    getFiltersAsync: async (): Promise<IFiltersResponse> => {
        const { data } = await apiClient.get<IFiltersResponse>("/api/filters");
        return data;
    },

    getClothesPagedAsync: async (params: GetClothesParams): Promise<PagedList<IClotheSummaryDTO>> => {
        const queryString = buildQueryString(params);
        const { data } = await apiClient.get<PagedList<IClotheSummaryDTO>>(`/api/catalog/clothes${queryString}`);
        return data;
    },

    subscribeOnUpdatesAsync: async (stockId: string): Promise<void> => {
        await apiClient.post<void>(`/api/catalog/stocks/notifications/subscribe/${stockId}`);
    }
};