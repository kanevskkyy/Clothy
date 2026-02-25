import type {ISizeReadDTO} from "../size/ISizeReadDTO.ts";
import type {IColorReadDTO} from "../color/IColorReadDTO.ts";

export interface IStockReadDTO {
    id: string;
    size: ISizeReadDTO;
    color: IColorReadDTO;
    quantity: number;
}