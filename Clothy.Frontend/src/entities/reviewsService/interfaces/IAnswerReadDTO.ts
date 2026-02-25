import type {IUserInfo} from "../../usersService/IUserInfo.ts";

export interface IAnswerReadDTO {
    id: string;
    user: IUserInfo;
    answerText: string;
    createdAt: string;
}