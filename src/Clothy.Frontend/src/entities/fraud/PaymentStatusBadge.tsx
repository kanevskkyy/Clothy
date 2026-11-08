import React from "react";
import {AlertCircle, Check, Clock, CreditCard, X} from "lucide-react";
import styles from "./FraudRisk.module.css";

const PAYMENT_STATUS_STYLES: Record<string, { label: string; className: string; icon: React.ReactNode }> = {
    Pending: {label: "Не оплачено", className: styles.toneNeutral, icon: <Clock size={13}/>},
    Paid: {label: "Оплачено", className: styles.toneSuccess, icon: <CreditCard size={13}/>},
    Disputed: {label: "Спір: відкрито", className: styles.toneWarning, icon: <AlertCircle size={13}/>},
    DisputeWon: {label: "Спір: виграно", className: styles.toneInfo, icon: <Check size={13}/>},
    DisputeLost: {label: "Спір: програно", className: styles.toneDanger, icon: <X size={13}/>},
};

interface IPaymentStatusBadgeProps {
    status: string | undefined;
    /** show only dispute statuses (for the orders list) */
    disputesOnly?: boolean;
}

const PaymentStatusBadge: React.FC<IPaymentStatusBadgeProps> = ({status, disputesOnly = false}) => {
    if (!status) return null;
    if (disputesOnly && !status.startsWith("Dispute")) return null;

    const style = PAYMENT_STATUS_STYLES[status] ?? {label: status, className: styles.toneNeutral, icon: null};
    return (
        <span className={`${styles.badge} ${style.className}`}>
            {style.icon && <span className={styles.badgeIcon} aria-hidden="true">{style.icon}</span>}
            {style.label}
        </span>
    );
};

export default PaymentStatusBadge;
