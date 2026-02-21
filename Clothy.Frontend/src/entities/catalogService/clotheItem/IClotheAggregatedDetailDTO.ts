import type {IClotheDetailDTO} from "./IClotheDetailDTO.ts";
import type {IReviewStatistic} from "../../reviewsService/reviews/IReviewStatistic.ts";
import type {IQuestionReadDTO} from "../../reviewsService/questions/IQuestionReadDTO.ts";
import type {PagedList} from "../../../shared/lib/pagedList.ts";
import type {IReviewReadDTO} from "../../reviewsService/reviews/IReviewReadDTO.ts";

export interface IClotheAggregatedDetailDTO {
    clotheDetailDTO: IClotheDetailDTO;
    reviews: PagedList<IReviewReadDTO>;
    statistics: IReviewStatistic;
    questions: PagedList<IQuestionReadDTO>;
}