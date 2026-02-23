import {Helmet} from "react-helmet";
import styles from "./DeliveryInfoPage.module.css";
import {Truck, Clock, MapPin} from "lucide-react";
import Container from "../../../shared/layout/Container/Container.tsx";

interface IDeliveryOption {
    icon: React.ReactNode;
    title: string;
    days: string;
    price: string;
    note: string;
}

const DeliveryInfoPage = () => {
    const deliveryOptions: IDeliveryOption[] = [
        {
            icon: <Truck size={28}/>,
            title: "Standard Delivery",
            days: "3–5 business days",
            price: "$15.00",
            note: "Free on orders over $1,500",
        },
        {
            icon: <Clock size={28}/>,
            title: "Express Delivery",
            days: "1–2 business days",
            price: "$35.00",
            note: "Order before 2 PM for same-day dispatch",
        },
        {
            icon: <MapPin size={28}/>,
            title: "Pickup Point",
            days: "2–4 business days",
            price: "$10.00",
            note: "Over 500 pickup locations available",
        },
    ];

    return (
        <Container>
            <div>
                <Helmet>
                    <title>Clothy — Delivery and Returns</title>
                    <meta
                        name="description"
                        content="Learn how to receive your Clothy orders quickly and reliably. Delivery and return policies."
                    />
                </Helmet>

                <section className={styles.header}>
                    <h2 className={styles.title}>Delivery & Shipping</h2>
                    <p className={styles.description}>
                        We offer multiple shipping options to get your order to you as quickly and conveniently as possible.
                    </p>
                </section>

                <div className={styles.cards}>
                    {deliveryOptions.map(({icon, title, days, price, note}) => (
                        <div key={title} className={styles.card}>
                            <span className={styles.icon}>{icon}</span>
                            <h3 className={styles.cardTitle}>{title}</h3>
                            <p className={styles.days}>{days}</p>
                            <p className={styles.price}>{price}</p>
                            <p className={styles.note}>{note}</p>
                        </div>
                    ))}
                </div>

                <section className={styles.shippingPolicyHeader}>
                    <h2 className={styles.shippingPolicyTitle}>Shipping Policy</h2>
                    <p className={styles.shippingPolicyDescription}>
                        All orders are processed within one business day. Once your order ships,
                        you’ll receive a confirmation email with tracking details.
                        We offer complimentary standard shipping on orders over $1,500.
                    </p>
                </section>

                <section className={styles.returnsSection}>
                    <h2 className={styles.returnsTitle}>Returns & Exchanges</h2>
                    <p className={styles.returnsDescription}>
                        Returns are accepted within 14 days of delivery. Items must be unworn,
                        unwashed, and returned in their original condition and packaging.
                        If an item arrives damaged or defective, return shipping is covered by us.
                    </p>
                </section>
            </div>
        </Container>
    );
};

export default DeliveryInfoPage;