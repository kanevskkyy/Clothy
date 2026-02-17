import type {ISizeReadDTO} from "../sizes/ISizeReadDTO.ts";
import type {IColorReadDTO} from "../colors/IColorReadDTO.ts";

export interface IStockReadDTO {
    id: string;
    size: ISizeReadDTO;
    color: IColorReadDTO;
    quantity: number;
}