import apiClient from "./client.ts";
import type {PagedList} from "../../shared/lib/pagedList.ts";
import type {IQuestionReadDTO} from "../../entities/reviewsService/interfaces/IQuestionReadDTO.ts";
import type {QuestionSchema} from "../schemas/questionSchema.ts";
import type {AnswerSchema} from "../schemas/answerSchemas.ts";
import type {IAnswerReadDTO} from "../../entities/reviewsService/interfaces/IAnswerReadDTO.ts";

export interface IQuestionFilters {
    pageNumber: number;
    clotheId?: string;
    userId?: string;
    withoutAnswer?: boolean;
}

export const questionApi = {
    getQuestionsAsync: async (filters: IQuestionFilters): Promise<PagedList<IQuestionReadDTO>> => {
        const params = new URLSearchParams();
        params.append("pageNumber", filters.pageNumber.toString());

        if (filters.clotheId) params.append("clotheItemId", filters.clotheId);
        if (filters.userId) params.append("userId", filters.userId);
        if (filters.withoutAnswer !== undefined) params.append("withoutAnswer", filters.withoutAnswer.toString());

        const { data } = await apiClient.get<PagedList<IQuestionReadDTO>>(`/api/questions?${params.toString()}`);
        return data;
    },

    createQuestionAsync: async ( body: QuestionSchema ): Promise<IQuestionReadDTO> => {
        const { data } = await apiClient.post<IQuestionReadDTO>('/api/questions', body);
        return data;
    },

    deleteQuestionAsync: async ( id: string ): Promise<void> => {
        await apiClient.delete(`/api/questions/${id}`);
    },

    deleteAnswerAsync: async (questionId: string, answerId: string): Promise<void> => {
        await apiClient.delete(`/api/questions/${questionId}/answers/${answerId}`);
    },

    createAnswerAsync: async ( questionId: string, body: AnswerSchema ): Promise<IAnswerReadDTO> => {
        const { data } = await apiClient.post<IAnswerReadDTO>(`/api/questions/${questionId}/answers`, body);
        return data;
    },
}