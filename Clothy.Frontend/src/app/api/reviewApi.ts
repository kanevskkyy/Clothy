import type {PagedList} from "../../shared/utils/pagedList.ts";
import type {IReviewAggregatedReadDTO} from "../../entities/reviewsService/reviews/IReviewAggregatedReadDTO.ts";
import apiClient from "./client.ts";

export const reviewApi = {
    getReviewsAsync: async ( pageNumber: number, clotheId: string ): Promise<IReviewAggregatedReadDTO[]> => {
        const { data } = await apiClient.get<PagedList<IReviewAggregatedReadDTO>>(`/api/reviews/?pageNumber=${pageNumber}&clotheItemId=${clotheId}`);

        return data.items;
    }
}