import type {IReviewReadDTO} from "../reviews/IReviewReadDTO.ts";
import type {IClotheDetailDTO} from "./IClotheDetailDTO.ts";
import type {IReviewStatistic} from "../reviews/IReviewStatistic.ts";
import type {IQuestionReadDTO} from "../questions/IQuestionReadDTO.ts";
import type {PagedList} from "../../shared/pagedList.ts";

export interface IClotheAggregatedDetailDTO {
    clotheDetailDTO: IClotheDetailDTO;
    reviews: PagedList<IReviewReadDTO>;
    statistics: IReviewStatistic;
    questions: PagedList<IQuestionReadDTO>;
}