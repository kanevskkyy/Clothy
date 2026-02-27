import apiClient from "./client.ts";
import type {PagedList} from "../../shared/lib/pagedList.ts";
import type {IClotheSummaryDTO} from "../../entities/catalogService/interfaces/clothe/IClotheSummaryDTO.ts";
import type {IBrandReadDTO} from "../../entities/catalogService/interfaces/brand/IBrandReadDTO.ts";
import type {IFiltersResponse} from "../../entities/catalogService/interfaces/filters/IFiltersResponse.ts";
import type {BrandSchema} from "../schemas/brandSchema.ts";
import type { ICollectionReadDTO } from "../../entities/catalogService/interfaces/collection/ICollectionReadDTO.ts";
import type { CollectionSchema } from "../schemas/collectionSchema.ts";
import type {ColorSchema} from "../schemas/colorSchema.ts";
import type { IColorReadDTO } from "../../entities/catalogService/interfaces/color/IColorReadDTO.ts";
import type {ITagReadDTO} from "../../entities/catalogService/interfaces/tag/ITagReadDTO.ts";
import type { ISizeReadDTO } from "../../entities/catalogService/interfaces/size/ISizeReadDTO.ts";
import type { SizeSchema } from "../schemas/sizeSchema.ts";
import type {
    IClotheByIdDTO,
    IClotheDetailDTO
} from "../../entities/catalogService/interfaces/clothe/IClotheDetailDTO.ts";
import type {ClotheUpdateSchema} from "../schemas/ clotheUpdateSchema.ts";

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

    getBrandByIdAsync: async (id: string): Promise<IBrandReadDTO> => {
        const { data } = await apiClient.get<IBrandReadDTO>(`/api/catalog/brands/${id}`);
        return data;
    },

    createBrandAsync: async (body: BrandSchema): Promise<IBrandReadDTO> => {
        const { data } = await apiClient.post<IBrandReadDTO>("/api/catalog/brands", body);
        return data;
    },

    updateBrandAsync: async (id: string, body: BrandSchema): Promise<IBrandReadDTO> => {
        const { data } = await apiClient.put<IBrandReadDTO>(`/api/catalog/brands/${id}`, body);
        return data;
    },

    deleteBrandAsync: async (id: string): Promise<void> => {
        await apiClient.delete(`/api/catalog/brands/${id}`);
    },

    getAllCollectionsAsync: async (): Promise<ICollectionReadDTO[]> => {
        const { data } = await apiClient.get<ICollectionReadDTO[]>("/api/catalog/collections");
        return data;
    },

    getCollectionByIdAsync: async (id: string): Promise<ICollectionReadDTO> => {
        const { data } = await apiClient.get<ICollectionReadDTO>(`/api/catalog/collections/${id}`);
        return data;
    },

    createCollectionAsync: async (body: CollectionSchema): Promise<ICollectionReadDTO> => {
        const { data } = await apiClient.post<ICollectionReadDTO>("/api/catalog/collections", body);
        return data;
    },

    updateCollectionAsync: async (id: string, body: CollectionSchema): Promise<ICollectionReadDTO> => {
        const { data } = await apiClient.put<ICollectionReadDTO>(`/api/catalog/collections/${id}`, body);
        return data;
    },

    deleteCollectionAsync: async (id: string): Promise<void> => {
        await apiClient.delete(`/api/catalog/collections/${id}`);
    },

    getAllColorsAsync: async (): Promise<IColorReadDTO[]> => {
        const { data } = await apiClient.get("/api/catalog/colors");
        return data;
    },

    getColorByIdAsync: async (id: string): Promise<IColorReadDTO> => {
        const { data } = await apiClient.get(`/api/catalog/colors/${id}`);
        return data;
    },

    createColorAsync: async (body: ColorSchema): Promise<IColorReadDTO> => {
        const { data } = await apiClient.post("/api/catalog/colors", body);
        return data;
    },

    updateColorAsync: async (id: string, body: ColorSchema): Promise<IColorReadDTO> => {
        const { data } = await apiClient.put(`/api/catalog/colors/${id}`, body);
        return data;
    },

    deleteColorAsync: async (id: string): Promise<void> => {
        await apiClient.delete(`/api/catalog/colors/${id}`);
    },

    getAllTagsAsync: async (): Promise<ITagReadDTO[]> => {
        const { data } = await apiClient.get("/api/catalog/tags");
        return data;
    },

    getTagByIdAsync: async (id: string): Promise<ITagReadDTO> => {
        const { data } = await apiClient.get(`/api/catalog/tags/${id}`);
        return data;
    },

    createTagAsync: async (body: { name: string; slug: string }): Promise<ITagReadDTO> => {
        const { data } = await apiClient.post("/api/catalog/tags", body);
        return data;
    },

    updateTagAsync: async (id: string, body: { name: string; slug: string }): Promise<ITagReadDTO> => {
        const { data } = await apiClient.put(`/api/catalog/tags/${id}`, body);
        return data;
    },

    deleteTagAsync: async (id: string) => {
        await apiClient.delete(`/api/catalog/tags/${id}`);
    },

    getAllSizesAsync: async (): Promise<ISizeReadDTO[]> => {
        const { data } = await apiClient.get<ISizeReadDTO[]>("/api/catalog/sizes");
        return data;
    },

    getSizeByIdAsync: async (id: string): Promise<ISizeReadDTO> => {
        const { data } = await apiClient.get<ISizeReadDTO>(`/api/catalog/sizes/${id}`);
        return data;
    },

    createSizeAsync: async (body: SizeSchema): Promise<ISizeReadDTO> => {
        const { data } = await apiClient.post<ISizeReadDTO>("/api/catalog/sizes", body);
        return data;
    },

    updateSizeAsync: async (id: string, body: SizeSchema): Promise<ISizeReadDTO> => {
        const { data } = await apiClient.put<ISizeReadDTO>(`/api/catalog/sizes/${id}`, body);
        return data;
    },

    deleteSizeAsync: async (id: string): Promise<void> => {
        await apiClient.delete(`/api/catalog/sizes/${id}`);
    },

    getTop8MostSaleAsync: async (): Promise<IClotheSummaryDTO[]> => {
        const { data } = await apiClient.get<IClotheSummaryDTO[]>("/api/catalog/clothes/top8");
        return data;
    },

    getClotheBySlugAggregatorAsync: async (slug: string | undefined) => {
        const { data } = await apiClient.get(`/api/clothe/${slug}`);
        return data;
    },

    getClotheByIdAsync: async (id: string | undefined): Promise<IClotheByIdDTO> => {
        const { data } = await apiClient.get<IClotheByIdDTO>(`/api/catalog/clothes/${id}`);
        return data;
    },

    createClotheAsync: async (formData: FormData): Promise<IClotheDetailDTO> => {
        const { data } = await apiClient.post<IClotheDetailDTO>("/api/catalog/clothes", formData, {
            headers: { "Content-Type": "multipart/form-data" },
        });
        return data;
    },

    updateClotheAsync: async (id: string, body: ClotheUpdateSchema): Promise<IClotheByIdDTO> => {
        const fd = new FormData();
        fd.append("Name", body.name);
        fd.append("Slug", body.slug);
        fd.append("Description", body.description);
        fd.append("Price", body.price.toFixed(2).replace(".", ","));
        fd.append("Gender", body.gender);
        fd.append("BrandId", body.brandId);
        fd.append("ClothingTypeId", body.clothingTypeId);
        fd.append("CollectionId", body.collectionId);

        const { data } = await apiClient.put<IClotheByIdDTO>(`/api/catalog/clothes/${id}`, fd, {
            headers: { "Content-Type": "multipart/form-data" },
        });
        return data;
    },

    deleteClotheAsync: async (id: string): Promise<void> => {
        await apiClient.delete(`/api/catalog/clothes/${id}`);
    },

    updateStockAsync: async (stockId: string, quantity: number): Promise<void> => {
        await apiClient.put(`/api/catalog/clothes-stock/${stockId}`, { quantity });
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