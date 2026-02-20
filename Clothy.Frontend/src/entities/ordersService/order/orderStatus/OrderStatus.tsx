import React from 'react';
import styles from "./OrderStatus.module.css";

interface IOrderStatusProps {
    name: string;
}

const OrderStatus: React.FC<IOrderStatusProps> = ({ name, }) => {
    return (
        <div className={styles.orderStatus}>
            {name}
        </div>
    );
};

export default OrderStatus;