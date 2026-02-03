import type {IUserInfo} from "../../users/interfaces/IUserInfo.ts";

export interface IReviewReadDTO {
    id: string;
    user: IUserInfo;
    rating: number;
    comment: string;
    createdAt: string;
}