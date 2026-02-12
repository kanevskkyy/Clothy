import type {IUserInfo} from "../users/IUserInfo.ts";

export interface IReviewAggregatedReadDTO {
    id: string;
    user: IUserInfo;
    rating: number;
    comment: string;
    createdAt: string;
}