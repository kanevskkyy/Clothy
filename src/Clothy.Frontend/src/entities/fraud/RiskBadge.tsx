import React from "react";
import {AlertTriangle, CircleSlash, Network, ShieldCheck} from "lucide-react";
import type {IFraudRiskSummary} from "../../app/api/fraudApi.ts";
import styles from "./FraudRisk.module.css";
import {getRiskLevel, type RiskLevel} from "./riskLevel.ts";

// every level has its own icon and text, so the badge does not rely on color alone
const RISK_STYLES: Record<RiskLevel, { label: string; className: string; icon: React.ReactNode }> = {
    low: {label: "Low risk", className: styles.toneSuccess, icon: <ShieldCheck size={13}/>},
    review: {label: "Needs review", className: styles.toneWarning, icon: <AlertTriangle size={13}/>},
    unscored: {label: "Not scored", className: styles.toneNeutral, icon: <CircleSlash size={13}/>},
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
            <span className={`${styles.badge} ${style.className}`}>
                <span className={styles.badgeIcon} aria-hidden="true">{style.icon}</span>
                {style.label}
            </span>
            {showScore && record?.score != null && (
                <span className={styles.score} title="Fraud probability (model score)">
                    {record.score.toFixed(3)}
                </span>
            )}
            {record?.ring_alert && (
                <span className={styles.ring} title="Ring signal: this device paid with several new cards">
                    <Network size={12} aria-hidden="true"/> ring
                </span>
            )}
        </span>
    );
};

export default RiskBadge;
