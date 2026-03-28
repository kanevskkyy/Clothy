import type {ReactNode} from "react";
import styles from "./OrderSummary.module.css";
import {AlertTriangle} from "lucide-react";
import {formatMoney} from "../../../shared/lib/formatMoney.ts";

interface PriceRow {
    label: string;
    value: string | number;
    isDiscount?: boolean;
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

const FREE_SHIPPING_THRESHOLD = 1500;

const OrderSummary = ({
                          title = "Your Order",
                          priceRows = [],
                          totalPrice,
                          unAvailableItemsCount,
                          currency = "$",
                          children,
                          buttons
                      }: OrderSummaryProps) => {
    const amountToFreeShipping = FREE_SHIPPING_THRESHOLD - totalPrice;

    return (
        <div className={styles.orderSummary}>
            <h3>{title}</h3>

            <div className={styles.orderPriceInfo}>
                {priceRows.map((row, index) => (
                    <div key={index} className={`${styles.priceRow} ${row.isDiscount ? styles.discountRow : ''}`}>
                        <span className={styles.label}>{row.label}</span>
                        <span className={styles.value}>{row.value}</span>
                    </div>
                ))}

                {priceRows.length > 0 && <div className={styles.divider}></div>}

                {unAvailableItemsCount > 0 && (
                    <div className={styles.warningBox}>
                        <AlertTriangle size={18}/>
                        <span>Unavailable items are not included in the total</span>
                    </div>
                )}

                <div className={styles.totalRow}>
                    <span>Total</span>
                    <span>{currency}{formatMoney(totalPrice)}</span>
                </div>
            </div>

            {children}

            {amountToFreeShipping > 0 && (
                <div className={styles.freeShipping}>
                    Add <span className={styles.freeShippingAmount}>
                    ${formatMoney(amountToFreeShipping)}
                </span> more for free shipping
                </div>
            )}

            {buttons && (
                <div className={styles.buttonWrapper}>
                    {buttons}
                </div>
            )}
        </div>
    );
};

export default OrderSummary;