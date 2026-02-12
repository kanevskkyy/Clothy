import type {IUserInfo} from "../users/IUserInfo.ts";
import type {IAnswerReadDTO} from "../answers/IAnswerReadDTO.ts";

export interface IQuestionAggregatedReadDTO {
    id: string;
    user: IUserInfo;
    questionText: string;
    createdAt: string;
    answers: IAnswerReadDTO[];
}