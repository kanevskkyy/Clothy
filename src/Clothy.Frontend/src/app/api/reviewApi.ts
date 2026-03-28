import type {PagedList} from "../../shared/lib/pagedList.ts";
import apiClient from "./client.ts";
import type {IReviewReadDTO} from "../../entities/reviewsService/interfaces/IReviewReadDTO.ts";
import type {ReviewSchemaData} from "../schemas/reviewSchema.ts";

export type ReviewsStatusEnum = "Pending" | "Confirmed";

export interface IReviewFilters {
    pageNumber?: number;
    userId?: string;
    clotheItemId?: string;
    rating?: number;
    status?: ReviewsStatusEnum;
}

export const reviewApi = {
    getReviewsAsync: async (filters: IReviewFilters): Promise<PagedList<IReviewReadDTO>> => {
        const params = new URLSearchParams();

        if (filters.pageNumber) params.append("pageNumber", filters.pageNumber.toString());
        if (filters.userId) params.append("userId", filters.userId);
        if (filters.clotheItemId) params.append("clotheItemId", filters.clotheItemId);
        if (filters.rating) params.append("rating", filters.rating.toString());
        if (filters.status) params.append("status", filters.status);

        const { data } = await apiClient.get<PagedList<IReviewReadDTO>>(`/api/reviews?${params.toString()}`);
        return data;
    },

    createReviewAsync: async (body : ReviewSchemaData): Promise<void> => {
        await apiClient.post<void>("/api/reviews", body);
    },

    getReviewByIdAsync: async (id: string): Promise<IReviewReadDTO> => {
        const { data } = await apiClient.get<IReviewReadDTO>(`/api/reviews/${id}`);
        return data;
    },

    updateReviewAsync: async (id: string, body: ReviewSchemaData): Promise<IReviewReadDTO> => {
        const updateBody = {
            comment: body.comment,
            rating: body.rating,
        };

        const { data } = await apiClient.put<IReviewReadDTO>(`/api/reviews/${id}`, updateBody);
        return data;
    },

    confirmReviewAsync: async (id: string): Promise<void> => {
        await apiClient.patch<void>(`/api/reviews/status/${id}/confirm`)
    },

    deleteReviewAsync: async (id: string): Promise<void> => {
        await apiClient.delete<void>(`/api/reviews/${id}`);
    }
};