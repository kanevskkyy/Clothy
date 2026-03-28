import type {IUserInfo} from "../../usersService/IUserInfo.ts";
import type {IAnswerReadDTO} from "./IAnswerReadDTO.ts";
import type {IClotheInfo} from "../../catalogService/interfaces/clothe/IClotheInfo.ts";

export interface IQuestionReadDTO {
    id: string;
    user: IUserInfo;
    clotheInfo?: IClotheInfo;
    questionText: string;
    createdAt: string;
    answers: IAnswerReadDTO[];
}