import apiClient from "./client.ts";
import type {PagedList} from "../../shared/utils/pagedList.ts";
import type {IQuestionAggregatedReadDTO} from "../../entities/reviewsService/questions/IQuestionAggregatedReadDTO.ts";

export const questionApi = {
    getQuestionsAsync: async ( pageNumber: number, clotheId: string ): Promise<IQuestionAggregatedReadDTO[]> => {
        const { data } = await apiClient.get<PagedList<IQuestionAggregatedReadDTO>>(`/api/questions/?pageNumber=${pageNumber}&clotheItemId=${clotheId}`);

        return data.items;
    }
}