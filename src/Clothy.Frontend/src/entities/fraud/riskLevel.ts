import type {IFraudRiskSummary} from "../../app/api/fraudApi.ts";

export type RiskLevel = "low" | "review" | "unscored";

export const getRiskLevel = (record: IFraudRiskSummary | null): RiskLevel => {
    if (!record || record.score === null || record.flag === null) return "unscored";
    return record.flag ? "review" : "low";
};
