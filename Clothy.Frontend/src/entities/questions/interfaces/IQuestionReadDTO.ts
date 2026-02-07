import type {IUserInfo} from "../../users/interfaces/IUserInfo.ts";
import type {IAnswerReadDTO} from "../../answers/interfaces/IAnswerReadDTO.ts";

export interface IQuestionReadDTO {
    id: string;
    user: IUserInfo;
    questionText: string;
    createdAt: string;
    answers: IAnswerReadDTO[];
}