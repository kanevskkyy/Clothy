import type {PagedList} from "../../shared/utils/pagedList.ts";
import apiClient from "./client.ts";
import type {IReviewReadDTO} from "../../entities/reviewsService/reviews/IReviewReadDTO.ts";

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
    }
};