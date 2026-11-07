import React from "react";
import {Network} from "lucide-react";
import Badge from "../../features/catalog/badge/Badge.tsx";
import type {IFraudRiskSummary} from "../../app/api/fraudApi.ts";
import styles from "./FraudRisk.module.css";
import {getRiskLevel, type RiskLevel} from "./riskLevel.ts";


const RISK_STYLES: Record<RiskLevel, { label: string; color: string; background: string; borderColor: string }> = {
    low: {label: "Ризик: низький", color: "#067647", background: "#ECFDF3", borderColor: "#ABEFC6"},
    review: {label: "Ризик: на перевірку", color: "#B42318", background: "#FEF3F2", borderColor: "#FECDCA"},
    unscored: {label: "Ризик: не оцінено", color: "#6B6B6B", background: "#F4F4F4", borderColor: "#E0E0E0"},
};

interface IRiskBadgeProps {
    /** undefined while loading */
    record: IFraudRiskSummary | null | undefined;
    showScore?: boolean;
}

const RiskBadge: React.FC<IRiskBadgeProps> = ({record, showScore = true}) => {
    if (record === undefined) return null;

    const level = getRiskLevel(record);
    const style = RISK_STYLES[level];

    return (
        <span className={styles.badgeRow}>
            <Badge label={style.label} color={style.color} background={style.background} borderColor={style.borderColor}/>
            {showScore && record?.score != null && (
                <span className={styles.score} title="Ймовірність шахрайства за моделлю">
                    {record.score.toFixed(3)}
                </span>
            )}
            {record?.ring_alert && (
                <span className={styles.ring} title="Сигнал кільця: з цього пристрою платили кількома новими картками">
                    <Network size={14}/> кільце
                </span>
            )}
        </span>
    );
};

export default RiskBadge;
