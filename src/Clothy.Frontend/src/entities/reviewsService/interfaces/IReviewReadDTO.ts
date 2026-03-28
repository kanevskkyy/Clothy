import type { IClotheInfo } from "../../catalogService/interfaces/clothe/IClotheInfo.ts";
import type { IUserInfo } from "../../usersService/IUserInfo.ts";

export interface IReviewReadDTO {
    clotheInfo?: IClotheInfo;
    user: IUserInfo;
    rating: number;
    comment: string;
    status?: string;
    id: string;
    createdAt: string;
    updatedAt?: string;
}