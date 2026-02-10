import type {IUserInfo} from "../users/IUserInfo.ts";

export interface IAnswerReadDTO {
    id: string;
    user: IUserInfo;
    answerText: string;
    createdAt: string;
}