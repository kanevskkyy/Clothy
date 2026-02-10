import PageWrapper from "../../shared/PageWrapper/PageWrapper.tsx";
import {Truck, User} from "lucide-react";
import styles from "./OrderDetailPage.module.css";
import type {IOrderDetailDTO} from "../../entities/orders/order/IOrderDetailDTO.ts";
import {formatDate} from "../../shared/formatDate.ts";
import {useParams} from "react-router-dom";

const OrderDetailPage = () => {
    const mockOrderDetail: IOrderDetailDTO = {
        id: "A1B2C3D4",
        status: {
            id: "status-1",
            name: "Awaiting payment",
            createdAt: "2026-02-27T10:00:00.000Z",
            updatedAt: "2026-02-27T10:00:00.000Z"
        },
        userFirstName: "Олена",
        userLastName: "Коваленко",
        userEmail: "olena@example.com",
        comment: "If possible, please contact me before shipping to confirm the details. Thank you!",
        isFreeDelivery: true,
        totalPrice: 1200,
        items: [
            {
                id: "item-1",
                clotheId: "clothe-1",
                clotheName: "Off-White Hoodie",
                price: 1299,
                quantity: 1,
                mainPhoto: "https://www.off---white.com/dw/image/v2/BGDG_PRD/on/demandware.static/-/Sites-51/default/dw6325cf8e/images/zoom/44MBB12MS26F007_001_0.jpg?sw=960&sh=1275",
                colorId: "color-1",
                hexCode: "#000000",
                sizeId: "size-1",
                sizeName: "M",
                isClotheDeleted: false,
                isClotheUpdated: false
            }
        ],
        deliveryDetail: {
            id: "delivery-1",
            phoneNumber: "+380501234567",
            firstName: "Elena",
            lastName: "Kovalenko",
            middleName: "Ivanovna",
            email: "olena@example.com",
            createdAt: "2026-02-27T10:00:00.000Z",
            updatedAt: "2026-02-27T10:00:00.000Z",
            pickupPoint: {
                id: "pickup-1",
                address: "First cargo compartment (Ruska 248)",
                ref: "warehouse-ref-1",
                isActive: true,
                deliveryProviderId: "provider-1",
                settlementId: "settlement-1",
                createdAt: "2026-01-01T00:00:00.000Z",
                updatedAt: "2026-01-01T00:00:00.000Z"
            },
            deliveryProvider: {
                id: "provider-1",
                name: "New Mail",
                iconUrl: "https://example.com/nova-poshta-icon.png",
                createdAt: "2026-01-01T00:00:00.000Z",
                updatedAt: "2026-01-01T00:00:00.000Z"
            },
            settlement: {
                id: "settlement-1",
                name: "Kyiv",
                ref: "kyiv-ref-1",
                type: "місто",
                regionId: "region-1",
                createdAt: "2026-01-01T00:00:00.000Z",
                updatedAt: "2026-01-01T00:00:00.000Z"
            },
            region: {
                id: "region-1",
                name: "Kyiv region",
                ref: "kyiv-region-ref-1",
                createdAt: "2026-01-01T00:00:00.000Z",
                updatedAt: "2026-01-01T00:00:00.000Z"
            }
        },
        createdAt: "2026-02-27T10:00:00.000Z",
        updatedAt: "2026-02-27T10:00:00.000Z"
    };

    // TODO: Connect to api, get from url order id if 403 => redirect to account, else show

    const {orderId} = useParams<{ orderId: string; }>();

    return (
        <PageWrapper>
            <div className={styles.container}>
                <div className={styles.orderHeader}>
                    <div className={styles.orderHeaderInfo}>
                        <h3 className={styles.orderHeaderTitle}>Order #{mockOrderDetail.id}</h3>
                        <p className={styles.orderDate}>{formatDate(mockOrderDetail.createdAt)}</p>
                    </div>
                    <div className={styles.orderStatus}>{mockOrderDetail.status.name}</div>
                </div>

                <div className={styles.orderItemsList}>
                    <h4 className={styles.productTitle}>Products</h4>
                    <div className={styles.orderItems}>

                        {mockOrderDetail.items.map((item) => (
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

                        {mockOrderDetail.comment && mockOrderDetail.comment.trim() !== "" && (
                            <div className={styles.commentSection}>
                                <h5 className={styles.commentTitle}>Comment</h5>
                                <p className={styles.commentText}>{mockOrderDetail.comment}</p>
                            </div>
                        )}

                        <div className={styles.orderSummary}>
                            <div className={`${styles.summaryRow} ${styles.total}`}>
                                <span>Total</span>
                                <span className={styles.totalPrice}>{mockOrderDetail.totalPrice} ₴</span>
                            </div>
                            <div
                                className={`${styles.freeDelivery} ${
                                    mockOrderDetail.isFreeDelivery
                                        ? styles.free
                                        : styles.paid
                                }`}
                            >
                                <Truck size={24}/>
                                <span>
                                    {mockOrderDetail.isFreeDelivery
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
                            <p className={styles.deliveryMethod}>{mockOrderDetail.deliveryDetail.deliveryProvider.name}</p>
                            <p className={styles.deliveryAddress}>{mockOrderDetail.deliveryDetail.region.name}</p>
                            <p className={styles.deliveryAddress}>{mockOrderDetail.deliveryDetail.settlement.name}</p>
                            <p className={styles.deliveryAddress}>{mockOrderDetail.deliveryDetail.pickupPoint.address}</p>
                        </div>
                    </div>

                    <div className={`${styles.section} ${styles.recipientSection}`}>
                        <h4 className={styles.sectionTitle}>
                            <User size={20}/>
                            Receiver
                        </h4>
                        <div className={styles.sectionContent}>
                            <p className={styles.recipientName}>{mockOrderDetail.deliveryDetail.lastName} {mockOrderDetail.deliveryDetail.firstName} {mockOrderDetail.deliveryDetail.middleName}</p>
                            <p className={styles.recipientPhone}>{mockOrderDetail.deliveryDetail.phoneNumber}</p>
                            <p className={styles.recipientEmail}>{mockOrderDetail.deliveryDetail.email}</p>
                        </div>
                    </div>
                </div>
            </div>
        </PageWrapper>
    );
};

export default OrderDetailPage;