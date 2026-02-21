import apiClient from "./client.ts";
import type {PagedList} from "../../shared/lib/pagedList.ts";
import type {IQuestionReadDTO} from "../../entities/reviewsService/questions/IQuestionReadDTO.ts";
import type {QuestionSchema} from "../schemas/questionSchema.ts";
import type {AnswerSchema} from "../schemas/answerSchemas.ts";
import type {IAnswerReadDTO} from "../../entities/reviewsService/answers/IAnswerReadDTO.ts";

export const questionApi = {
    getQuestionsAsync: async ( pageNumber: number, clotheId: string ): Promise<IQuestionReadDTO[]> => {
        const { data } = await apiClient.get<PagedList<IQuestionReadDTO>>(`/api/questions/?pageNumber=${pageNumber}&clotheItemId=${clotheId}`);

        return data.items;
    },

    createQuestionAsync: async ( body: QuestionSchema ): Promise<IQuestionReadDTO> => {
        const { data } = await apiClient.post<IQuestionReadDTO>('/api/questions', body);
        return data;
    },

    createAnswerAsync: async ( questionId: string, body: AnswerSchema ): Promise<IAnswerReadDTO> => {
        const { data } = await apiClient.post<IAnswerReadDTO>(`/api/questions/${questionId}/answers`, body);
        return data;
    },
}