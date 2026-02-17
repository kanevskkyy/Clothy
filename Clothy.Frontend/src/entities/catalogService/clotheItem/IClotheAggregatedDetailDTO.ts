import type {IReviewAggregatedReadDTO} from "../../reviewsService/reviews/IReviewAggregatedReadDTO.ts";
import type {IClotheDetailDTO} from "./IClotheDetailDTO.ts";
import type {IReviewStatistic} from "../../reviewsService/reviews/IReviewStatistic.ts";
import type {IQuestionAggregatedReadDTO} from "../../reviewsService/questions/IQuestionAggregatedReadDTO.ts";
import type {PagedList} from "../../../shared/utils/pagedList.ts";

export interface IClotheAggregatedDetailDTO {
    clotheDetailDTO: IClotheDetailDTO;
    reviews: PagedList<IReviewAggregatedReadDTO>;
    statistics: IReviewStatistic;
    questions: PagedList<IQuestionAggregatedReadDTO>;
}