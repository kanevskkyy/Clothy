import type {IClotheDetailDTO} from "./IClotheDetailDTO.ts";
import type {IReviewStatistic} from "../../../reviewsService/interfaces/IReviewStatistic.ts";
import type {IQuestionReadDTO} from "../../../reviewsService/interfaces/IQuestionReadDTO.ts";
import type {PagedList} from "../../../../shared/lib/pagedList.ts";
import type {IReviewReadDTO} from "../../../reviewsService/interfaces/IReviewReadDTO.ts";

export interface IClotheAggregatedDetailDTO {
    clotheDetailDTO: IClotheDetailDTO;
    reviews: PagedList<IReviewReadDTO>;
    statistics: IReviewStatistic;
    questions: PagedList<IQuestionReadDTO>;
}