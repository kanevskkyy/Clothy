import type {IUserInfo} from "../../usersService/IUserInfo.ts";

export interface IReviewAggregatedReadDTO {
    id: string;
    user: IUserInfo;
    rating: number;
    comment: string;
    createdAt: string;
}