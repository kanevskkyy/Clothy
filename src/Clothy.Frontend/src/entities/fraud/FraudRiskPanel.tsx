import React from "react";
import {Network, ShieldAlert} from "lucide-react";
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
    if (f.value === null) return "немає даних";
    return Number.isInteger(f.value) ? String(f.value) : f.value.toFixed(2);
};

interface IFraudRiskPanelProps {
    /** undefined while loading, null when the order was not scored */
    record: IFraudTxRecord | null | undefined;
    paymentStatus?: string;
}

const FraudRiskPanel: React.FC<IFraudRiskPanelProps> = ({record, paymentStatus}) => {
    if (record === undefined) return null;

    return (
        <section className={styles.panel}>
            <div className={styles.panelHeader}>
                <h4 className={styles.panelTitle}>
                    <ShieldAlert size={20}/> Антифрод
                </h4>
                <div className={styles.extras}>
                    <RiskBadge record={record} showScore={false}/>
                    <PaymentStatusBadge status={paymentStatus}/>
                </div>
            </div>

            {!record || record.score === null ? (
                <p className={styles.meta}>
                    Оплату не оцінено: сервіс антифроду був недоступний, оплата прийшла не по черзі
                    або замовлення оплачене не карткою Stripe. На оплату це не вплинуло.
                </p>
            ) : (
                <>
                    <p className={styles.meta}>
                        Score {record.score.toFixed(3)} при порозі {record.threshold?.toFixed(3)}
                        {record.variant && <> · модель {record.variant}</>}
                    </p>

                    {record.fraud_label_at !== null && (
                        <p className={styles.meta}>Позначено як шахрайство за результатом спору.</p>
                    )}

                    {record.ring_alert && (
                        <p className={styles.meta}>
                            <Network size={14}/> Сигнал кільця: з цього пристрою за 30 днів платили
                            {" "}{record.ring_alert.other_cards_30d ?? "?"} раз(и) іншими картками
                            {record.ring_alert.share_new !== null && <>, нових карток {Math.round(record.ring_alert.share_new * 100)}%</>}.
                        </p>
                    )}

                    {record.top_features && record.top_features.length > 0 && (
                        <>
                            <p className={styles.meta}>Головні причини:</p>
                            <ul className={styles.reasons}>
                                {record.top_features.map((f) => {
                                    const value = formatValue(f);
                                    return (
                                        <li key={f.feature} className={styles.reason}>
                                            <span>
                                                {getFraudFeatureName(f.feature)}
                                                {value !== null && <>: {value}</>}
                                            </span>
                                            <span className={f.contribution > 0 ? styles.reasonUp : styles.reasonDown}>
                                                {f.contribution > 0 ? "↑ підвищує ризик" : "↓ знижує ризик"}
                                            </span>
                                        </li>
                                    );
                                })}
                            </ul>
                        </>
                    )}
                </>
            )}

            <p className={styles.note}>
                Модель навчена на наборі IEEE-CIS, а не на даних магазину: оцінки демонстраційні.
            </p>
        </section>
    );
};

export default FraudRiskPanel;
