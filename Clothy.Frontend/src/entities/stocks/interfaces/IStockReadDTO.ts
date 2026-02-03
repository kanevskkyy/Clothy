import type {ISizeReadDTO} from "../../sizes/interfaces/ISizeReadDTO.ts";
import type {IColorReadDTO} from "../../colors/interfaces/IColorReadDTO.ts";

export interface IStockReadDTO {
    id: string;
    size: ISizeReadDTO;
    color: IColorReadDTO;
    quantity: number;
}