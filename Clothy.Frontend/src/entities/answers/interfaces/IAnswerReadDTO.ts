import type {IUserInfo} from "../../users/interfaces/IUserInfo.ts";

export interface IAnswerReadDTO {
    id: string;
    user: IUserInfo;
    answerText: string;
    createdAt: string;
}