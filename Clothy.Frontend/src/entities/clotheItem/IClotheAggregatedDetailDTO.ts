import type {IReviewAggregatedReadDTO} from "../reviews/IReviewAggregatedReadDTO.ts";
import type {IClotheDetailDTO} from "./IClotheDetailDTO.ts";
import type {IReviewStatistic} from "../reviews/IReviewStatistic.ts";
import type {IQuestionAggregatedReadDTO} from "../questions/IQuestionAggregatedReadDTO.ts";
import type {PagedList} from "../../shared/pagedList.ts";

export interface IClotheAggregatedDetailDTO {
    clotheDetailDTO: IClotheDetailDTO;
    reviews: PagedList<IReviewAggregatedReadDTO>;
    statistics: IReviewStatistic;
    questions: PagedList<IQuestionAggregatedReadDTO>;
}