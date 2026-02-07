import type {IReviewReadDTO} from "../../reviews/interfaces/IReviewReadDTO.ts";
import type {IClotheDetailDTO} from "./IClotheDetailDTO.ts";
import type {IReviewStatistic} from "../../reviews/interfaces/IReviewStatistic.ts";
import type {IQuestionReadDTO} from "../../questions/interfaces/IQuestionReadDTO.ts";
import type {PagedList} from "../../../shared/pagedList.ts";

export interface IClotheAggregatedDetailDTO {
    clotheDetailDTO: IClotheDetailDTO;
    reviews: PagedList<IReviewReadDTO>;
    statistics: IReviewStatistic;
    questions: PagedList<IQuestionReadDTO>;
}