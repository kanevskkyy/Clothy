import React, {useState} from "react";
import type {IOrderReadDTO} from "../interfaces/order/IOrderReadDTO.ts";
import styles from "./OrderList.module.css";
import {formatDate} from "../../../shared/lib/formatDate.ts";
import {Package} from "lucide-react";
import {Link} from "react-router-dom";
import Badge from "../../../features/catalog/badge/Badge.tsx";
import Select from "../../../shared/ui/Select/Select.tsx";

const ADMIN_STATUS_OPTIONS = [
    {value: "Shipped", label: "Shipped"},
    {value: "Delivered", label: "Delivered"},
];

interface OrderListItemProps {
    orders: IOrderReadDTO[];
    onStatusChange?: (orderId: string, newStatus: string) => Promise<void>;
}

export const OrderList: React.FC<OrderListItemProps> = ({orders, onStatusChange}) => {
    const [loadingId, setLoadingId] = useState<string | null>(null);

    const handleStatusChange = async (orderId: string, newStatus: string) => {
        if (!onStatusChange) return;
        setLoadingId(orderId);
        try {
            await onStatusChange(orderId, newStatus);
        } finally {
            setLoadingId(null);
        }
    };

    return (
        <div className={styles.container}>
            {orders.map((order) => (
                <div key={order.id} className={styles.orderItem}>
                    <Link
                        to={`/order/${order.id}`}
                        className={styles.linkOverlay}
                        aria-label={`Open order ${order.id}`}
                    />

                    <div className={styles.orderInfoBase}>
                        <div className={styles.logoWrapper}>
                            <Package size={24}/>
                        </div>

                        <div>
                            <div className={styles.orderId}>
                                Order #{order.id}
                            </div>

                            <p className={styles.orderDate}>
                                {formatDate(order.createdAt)}
                            </p>
                        </div>
                    </div>

                    <div className={styles.orderInfoAdditional}>
                        {onStatusChange ? (
                            <div
                                className={styles.statusSelect}
                                onClick={(e) => e.stopPropagation()}
                                onMouseDown={(e) => e.stopPropagation()}
                            >
                                <Select
                                    options={ADMIN_STATUS_OPTIONS}
                                    value={ADMIN_STATUS_OPTIONS.find(o => o.value === order.status) ?? null}
                                    onChange={(option) => {
                                        if (option) {
                                            handleStatusChange(order.id, (option as {value: string}).value);
                                        }
                                    }}
                                    isDisabled={loadingId === order.id}
                                    isLoading={loadingId === order.id}
                                    placeholder="Set status..."
                                    menuPortalTarget={document.body}
                                    menuPosition="fixed"
                                    styles={{
                                        menuPortal: (base) => ({...base, zIndex: 9999}),
                                    }}
                                />
                            </div>
                        ) : (
                            <Badge label={order.status}/>
                        )}

                        <p className={styles.orderPrice}>
                            {order.totalPrice} $
                        </p>
                    </div>
                </div>
            ))}
        </div>
    );
};

export default OrderList;