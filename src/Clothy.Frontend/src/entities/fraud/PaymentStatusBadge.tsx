import React from "react";
import Badge from "../../features/catalog/badge/Badge.tsx";

const PAYMENT_STATUS_STYLES: Record<string, { label: string; color: string; background: string; borderColor: string }> = {
    Pending: {label: "Не оплачено", color: "#6B6B6B", background: "#F4F4F4", borderColor: "#E0E0E0"},
    Paid: {label: "Оплачено", color: "#067647", background: "#ECFDF3", borderColor: "#ABEFC6"},
    Disputed: {label: "Спір відкрито", color: "#B42318", background: "#FEF3F2", borderColor: "#FECDCA"},
    DisputeWon: {label: "Спір виграно", color: "#175CD3", background: "#EFF8FF", borderColor: "#B2DDFF"},
    DisputeLost: {label: "Спір програно", color: "#B42318", background: "#FEF3F2", borderColor: "#FECDCA"},
};

interface IPaymentStatusBadgeProps {
    status: string | undefined;
    /** show only dispute statuses (for the orders list) */
    disputesOnly?: boolean;
}

const PaymentStatusBadge: React.FC<IPaymentStatusBadgeProps> = ({status, disputesOnly = false}) => {
    if (!status) return null;
    if (disputesOnly && !status.startsWith("Dispute")) return null;

    const style = PAYMENT_STATUS_STYLES[status] ?? {label: status, color: "#6B6B6B", background: "#F4F4F4", borderColor: "#E0E0E0"};
    return <Badge label={style.label} color={style.color} background={style.background} borderColor={style.borderColor}/>;
};

export default PaymentStatusBadge;
