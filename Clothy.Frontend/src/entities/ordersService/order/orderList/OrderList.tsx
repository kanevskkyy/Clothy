import React from "react";
import type { IOrderReadDTO } from "../IOrderReadDTO.ts";
import styles from "./OrderList.module.css";
import { formatDate } from "../../../../shared/lib/formatDate.ts";
import { Package } from "lucide-react";
import { Link } from "react-router-dom";
import Badge from "../../../../features/catalog/badge/Badge.tsx";

interface OrderListItemProps {
    orders: IOrderReadDTO[];
}

export const OrderList: React.FC<OrderListItemProps> = ({ orders }) => {
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
                            <Package size={24} />
                        </div>

                        <div>
                            <h3 className={styles.orderId}>
                                Order #{order.id}
                            </h3>

                            <p className={styles.orderDate}>
                                {formatDate(order.createdAt)}
                            </p>
                        </div>
                    </div>

                    <div className={styles.orderInfoAdditional}>
                        <Badge label={order.status}/>

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