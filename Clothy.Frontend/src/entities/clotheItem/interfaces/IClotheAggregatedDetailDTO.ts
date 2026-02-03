import type {IReviewReadDTO} from "../../reviews/interfaces/IReviewReadDTO.ts";
import type {IClotheDetailDTO} from "./IClotheDetailDTO.ts";
import type {IReviewStatistic} from "../../reviews/interfaces/IReviewStatistic.ts";
import type {IQuestionReadDTO} from "../../questions/interfaces/IQuestionReadDTO.ts";

export interface IClotheAggregatedDetailDTO {
    clotheDetailDTO: IClotheDetailDTO;
    reviews: IReviewReadDTO[];
    statistics: IReviewStatistic;
    questions: IQuestionReadDTO[];
}