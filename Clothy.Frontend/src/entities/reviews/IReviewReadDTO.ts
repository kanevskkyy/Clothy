import type {IUserInfo} from "../users/IUserInfo.ts";

export interface IReviewReadDTO {
    id: string;
    user: IUserInfo;
    rating: number;
    comment: string;
    createdAt: string;
}