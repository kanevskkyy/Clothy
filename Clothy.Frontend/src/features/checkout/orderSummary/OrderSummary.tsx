import type { ReactNode } from "react";
import styles from "./OrderSummary.module.css";
import {AlertTriangle} from "lucide-react";

interface PriceRow {
    label: string;
    value: string | number;
}

interface OrderSummaryProps {
    title?: string;
    priceRows?: PriceRow[];
    totalPrice: number;
    currency?: string;
    children?: ReactNode;
    buttons?: ReactNode;
    unAvailableItemsCount: number;
}

const OrderSummary = ({
                          title = "Your Order",
                          priceRows = [],
                          totalPrice,
                          unAvailableItemsCount,
                          currency = "₴",
                          children,
                          buttons
                      }: OrderSummaryProps) => {
    return (
        <div className={styles.orderSummary}>
            <h3>{title}</h3>

            <div className={styles.orderPriceInfo}>
                {priceRows.map((row, index) => (
                    <div key={index} className={styles.priceRow}>
                        <span className={styles.label}>{row.label}</span>
                        <span className={styles.value}>{row.value}</span>
                    </div>
                ))}

                {priceRows.length > 0 && <div className={styles.divider}></div>}

                {unAvailableItemsCount > 0 && (
                    <div className={styles.warningBox}>
                        <AlertTriangle  size={18} />
                        <span>Unavailable items are not included in the total</span>
                    </div>
                )}

                <div className={styles.totalRow}>
                    <span>Total</span>
                    <span>{totalPrice} {currency}</span>
                </div>
            </div>

            {children}

            {buttons && (
                <div className={styles.buttonWrapper}>
                    {buttons}
                </div>
            )}
        </div>
    );
};

export default OrderSummary;