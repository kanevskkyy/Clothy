import type { IClotheInfo } from "../../catalogService/clotheItem/IClotheInfo.ts";
import type { IUserInfo } from "../../usersService/IUserInfo.ts";

export interface IReviewReadDTO {
    clotheInfo: IClotheInfo;
    user: IUserInfo;
    rating: number;
    comment: string;
    status: string;
    id: string;
    createdAt: string;
    updatedAt: string;
}