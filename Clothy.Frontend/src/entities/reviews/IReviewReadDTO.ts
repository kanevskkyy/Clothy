import type { IClotheInfo } from "../clotheItem/IClotheInfo";
import type { IUserInfo } from "../users/IUserInfo";

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