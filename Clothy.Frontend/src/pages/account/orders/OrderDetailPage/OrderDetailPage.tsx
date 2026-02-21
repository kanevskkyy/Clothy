import PageWrapper from "../../../../shared/layout/PageWrapper/PageWrapper.tsx";
import {Truck, User} from "lucide-react";
import styles from "./OrderDetailPage.module.css";
import type {IOrderDetailDTO} from "../../../../entities/ordersService/order/IOrderDetailDTO.ts";
import {formatDate} from "../../../../shared/lib/formatDate.ts";
import {useParams} from "react-router-dom";
import {useEffect, useState} from "react";
import {ordersApi} from "../../../../app/api/ordersApi.ts";
import {toast} from "sonner";
import {getErrorMessage} from "../../../../shared/lib/errorHandler.ts";
import Loader from "../../../../shared/ui/Loader/Loader.tsx";
import OrderStatus from "../../../../entities/ordersService/order/orderStatus/OrderStatus.tsx";

const OrderDetailPage = () => {
    const {orderId} = useParams<{ orderId: string; }>();
    const [orderDetail, setOrderDetail] = useState<IOrderDetailDTO>();

    const [isLoading, setIsLoading] = useState<boolean>(false);

    useEffect(() => {
        if (!orderId) return;

        const fetchOrderDetail = async () => {
            try {
                setIsLoading(true);
                const response = await ordersApi.getOrderByIdAsync(orderId);
                setOrderDetail(response);
            } catch (error) {
                toast.error(getErrorMessage(error));
            } finally {
                setIsLoading(false);
            }
        };

        fetchOrderDetail();
    }, [orderId]);

    if (isLoading || !orderDetail) {
        return <Loader />;
    }

    return (
        <PageWrapper>
            <div className={styles.container}>
                <div className={styles.orderHeader}>
                    <div className={styles.orderHeaderInfo}>
                        <h3 className={styles.orderHeaderTitle}>Order #{orderDetail?.id}</h3>
                        <p className={styles.orderDate}>{formatDate(orderDetail!.createdAt)}</p>
                    </div>
                    <OrderStatus name={orderDetail.status} />
                </div>

                <div className={styles.orderItemsList}>
                    <h4 className={styles.productTitle}>Products</h4>
                    <div className={styles.orderItems}>

                        {orderDetail?.items.map((item) => (
                            <div key={item.id} className={styles.orderItem}>
                                <img
                                    src={item.mainPhoto}
                                    alt={item.clotheName} className={styles.itemImage}/>
                                <div className={styles.itemDetails}>
                                    <p className={styles.itemName}>{item.clotheName}</p>
                                    <p className={styles.itemVariant}>{item.sizeName} • x{item.quantity}</p>
                                </div>
                                <p className={styles.itemPrice}>{item.price} ₴</p>
                            </div>
                        ))}

                        {orderDetail?.comment && orderDetail?.comment.trim() !== "" && (
                            <div className={styles.commentSection}>
                                <h5 className={styles.commentTitle}>Comment</h5>
                                <p className={styles.commentText}>{orderDetail.comment}</p>
                            </div>
                        )}

                        <div className={styles.orderSummary}>
                            <div className={`${styles.summaryRow} ${styles.total}`}>
                                <span>Total</span>
                                <span className={styles.totalPrice}>{orderDetail?.totalPrice} ₴</span>
                            </div>
                            <div
                                className={`${styles.freeDelivery} ${
                                    orderDetail?.isFreeDelivery
                                        ? styles.free
                                        : styles.paid
                                }`}
                            >
                                <Truck size={24}/>
                                <span>
                                    {orderDetail?.isFreeDelivery
                                        ? "Free delivery"
                                        : "Paid"}
                                </span>
                            </div>

                        </div>
                    </div>
                </div>

                <div className={styles.orderSections}>
                    <div className={`${styles.section} ${styles.deliverySection}`}>
                        <h4 className={styles.sectionTitle}>
                            <Truck size={24}/>
                            Delivery
                        </h4>
                        <div className={styles.sectionContent}>
                            <p className={styles.deliveryMethod}>Delivery provider: {orderDetail?.deliveryDetail.deliveryProvider.name}</p>
                            <p className={styles.deliveryAddress}>Region: {orderDetail?.deliveryDetail.region.name}</p>
                            <p className={styles.deliveryAddress}>Settlement: {orderDetail?.deliveryDetail.settlement.name}</p>
                            <p className={styles.deliveryAddress}>Address: {orderDetail?.deliveryDetail.pickupPoint.address}</p>
                        </div>
                    </div>

                    <div className={`${styles.section} ${styles.recipientSection}`}>
                        <h4 className={styles.sectionTitle}>
                            <User size={20}/>
                            Receiver
                        </h4>
                        <div className={styles.sectionContent}>
                            <p className={styles.recipientName}>{orderDetail?.deliveryDetail.lastName} {orderDetail?.deliveryDetail.firstName}</p>
                            <p className={styles.recipientPhone}>{orderDetail?.deliveryDetail.phoneNumber}</p>
                            <p className={styles.recipientEmail}>{orderDetail?.deliveryDetail.email}</p>
                        </div>
                    </div>
                </div>
            </div>
        </PageWrapper>
    );
};

export default OrderDetailPage;