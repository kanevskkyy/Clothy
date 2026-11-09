import React from "react";
import {Gavel, Network, ShieldAlert} from "lucide-react";
import type {IFraudTopFeature, IFraudTxRecord} from "../../app/api/fraudApi.ts";
import {getFraudFeatureName} from "./fraudFeatureNames.ts";
import RiskBadge from "./RiskBadge.tsx";
import PaymentStatusBadge from "./PaymentStatusBadge.tsx";
import styles from "./FraudRisk.module.css";

// The model gets these as encoded categories or a hash: their raw value means nothing to a person.
const HIDDEN_VALUE_FEATURES = new Set([
    "card1", "ProductCD", "card4", "card6", "P_emaildomain", "R_emaildomain",
    "DeviceType", "DeviceInfo", "id_30", "id_31", "id_33",
]);

const formatValue = (f: IFraudTopFeature): string | null => {
    if (HIDDEN_VALUE_FEATURES.has(f.feature)) return null;
    if (f.value === null) return "no data";
    return Number.isInteger(f.value) ? String(f.value) : f.value.toFixed(2);
};

const toPercent = (value: number) => `${Math.min(1, Math.max(0, value)) * 100}%`;

interface IScoreMeterProps {
    score: number;
    threshold: number | null;
    flagged: boolean;
}

/** Score on a 0..1 track with a mark at the decision threshold. */
const ScoreMeter: React.FC<IScoreMeterProps> = ({score, threshold, flagged}) => {
    const relation = threshold === null ? "" : score >= threshold ? ", above threshold" : ", below threshold";
    return (
        <div>
            <div className={styles.scoreHead}>
                <span className={styles.scoreValue}>{score.toFixed(3)}</span>
                <span className={styles.scoreCaption}>
                    {threshold !== null ? <>threshold {threshold.toFixed(3)}</> : "threshold unknown"}
                </span>
            </div>
            <div
                className={styles.meter}
                role="meter"
                aria-label="Model score"
                aria-valuemin={0}
                aria-valuemax={1}
                aria-valuenow={score}
                aria-valuetext={`${score.toFixed(3)}${threshold !== null ? ` at threshold ${threshold.toFixed(3)}` : ""}${relation}`}
            >
                <span
                    className={`${styles.meterFill} ${flagged ? styles.fillReview : styles.fillLow}`}
                    style={{width: toPercent(score)}}
                />
                {threshold !== null && (
                    <span className={styles.threshold} style={{left: toPercent(threshold)}} aria-hidden="true">
                        <span className={styles.thresholdLabel}>threshold</span>
                    </span>
                )}
            </div>
            <div className={styles.scale} aria-hidden="true">
                <span>0</span>
                <span>1</span>
            </div>
        </div>
    );
};

interface IFraudRiskPanelProps {
    /** undefined while loading, null when the order was not scored */
    record: IFraudTxRecord | null | undefined;
    paymentStatus?: string;
}

const FraudRiskPanel: React.FC<IFraudRiskPanelProps> = ({record, paymentStatus}) => {
    if (record === undefined) return null;

    const features = record?.top_features ?? [];
    const maxImpact = Math.max(...features.map((f) => Math.abs(f.contribution)), 0);

    return (
        <section className={styles.panel} aria-labelledby="fraud-panel-title">
            <div className={styles.panelHeader}>
                <h2 id="fraud-panel-title" className={styles.panelTitle}>
                    <ShieldAlert size={18} aria-hidden="true"/> Fraud check
                </h2>
                <div className={styles.extras}>
                    <RiskBadge record={record} showScore={false}/>
                    <PaymentStatusBadge status={paymentStatus}/>
                </div>
            </div>

            <div className={styles.panelBody}>
                {!record || record.score === null ? (
                    <p className={styles.meta}>
                        Payment not scored: the fraud service was unavailable, the payment arrived out of order,
                        or the order wasn't paid with a Stripe card. The payment itself was not affected.
                    </p>
                ) : (
                    <>
                        <div>
                            <p className={styles.sectionLabel}>Score</p>
                            <ScoreMeter score={record.score} threshold={record.threshold} flagged={record.flag === true}/>
                            {record.variant && <p className={styles.meta}>Model {record.variant}</p>}
                        </div>

                        {record.fraud_label_at !== null && (
                            <div className={`${styles.callout} ${styles.calloutLabel}`}>
                                <span className={styles.calloutIcon} aria-hidden="true"><Gavel size={16}/></span>
                                <span>Marked as fraud based on the dispute outcome.</span>
                            </div>
                        )}

                        {record.ring_alert && (
                            <div className={`${styles.callout} ${styles.calloutRing}`}>
                                <span className={styles.calloutIcon} aria-hidden="true"><Network size={16}/></span>
                                <span>
                                    <span className={styles.calloutTitle}>Ring signal</span>
                                    This device made
                                    {" "}{record.ring_alert.other_cards_30d ?? "?"} payment(s) with other cards in the last 30 days
                                    {record.ring_alert.share_new !== null && <>, {Math.round(record.ring_alert.share_new * 100)}% of them new</>}.
                                </span>
                            </div>
                        )}

                        {features.length > 0 && (
                            <div>
                                <p className={styles.sectionLabel} id="fraud-reasons-title">Top reasons</p>
                                <ul className={styles.reasons} aria-labelledby="fraud-reasons-title">
                                    {features.map((f) => {
                                        const value = formatValue(f);
                                        const up = f.contribution > 0;
                                        const width = maxImpact > 0 ? `${(Math.abs(f.contribution) / maxImpact) * 100}%` : "0%";
                                        return (
                                            <li key={f.feature} className={styles.reason}>
                                                <span className={styles.reasonName}>
                                                    {getFraudFeatureName(f.feature)}
                                                    {value !== null && <span className={styles.reasonValue}>: {value}</span>}
                                                </span>
                                                <span
                                                    className={styles.impact}
                                                    title={`Contribution ${f.contribution > 0 ? "+" : ""}${f.contribution.toFixed(3)}`}
                                                    aria-hidden="true"
                                                >
                                                    <span
                                                        className={`${styles.impactFill} ${up ? styles.impactUp : styles.impactDown}`}
                                                        style={{width}}
                                                    />
                                                </span>
                                                <span className={up ? styles.reasonUp : styles.reasonDown}>
                                                    <span className={styles.arrow} aria-hidden="true">{up ? "↑" : "↓"}</span>
                                                    {up ? "raises risk" : "lowers risk"}
                                                </span>
                                            </li>
                                        );
                                    })}
                                </ul>
                            </div>
                        )}
                    </>
                )}
            </div>

            <p className={styles.note}>
                The model is trained on the IEEE-CIS dataset, not on store data: scores are for demo purposes only.
            </p>
        </section>
    );
};

export default FraudRiskPanel;
