import { Helmet } from "react-helmet";
import styles from "./DeliveryInfoPage.module.css";
import { Clock, type LucideIcon, Package, RotateCcw, Truck } from "lucide-react";
import PageWrapper from "../../../shared/layout/PageWrapper/PageWrapper.tsx";

interface DeliveryOption {
    icon: LucideIcon;
    title: string;
    description: string;
    time: string;
    price: string;
}

const deliveryOptions: DeliveryOption[] = [
    {
        icon: Truck,
        title: 'Nova Poshta',
        description: 'Delivery to branch or parcel locker',
        time: '1-3 days',
        price: 'from ₴70',
    },
    {
        icon: Package,
        title: "Nova Poshta Courier",
        description: 'Delivery to your address',
        time: '1-3 days',
        price: 'from ₴100',
    },
];

const DeliveryInfoPage = () => {
    return (
        <PageWrapper>
            <section className={styles.storySection}>
                <Helmet>
                    <title>Clothy — Delivery and Returns</title>
                    <meta
                        name="description"
                        content="Learn how to receive your Clothy orders quickly and reliably. Delivery and return policies."
                    />
                </Helmet>

                <h2 className={styles.sectionTitle}>Delivery and Payment</h2>

                <div className={styles.deliveryOptions}>
                    {deliveryOptions.map((option, index) => (
                        <div key={index} className={styles.deliveryItem}>
                            <div className={styles.icon}>
                                <option.icon size={24} />
                            </div>
                            <div className={styles.deliveryInfo}>
                                <h4>{option.title}</h4>
                                <p>{option.description}</p>
                                <div className={styles.details}>
                                    <div className={styles.detailItem}>
                                        <Clock size={20} />
                                        <p>{option.time}</p>
                                    </div>
                                    <div className={styles.detailPrice}>{option.price}</div>
                                </div>
                            </div>
                        </div>
                    ))}
                </div>

                <div className={styles.returnPolicy}>
                    <div className={styles.returnHeader}>
                        <RotateCcw size={24} />
                        <h3>Returns and Exchanges</h3>
                    </div>

                    <div className={styles.returnContent}>
                        <p>
                            You can return or exchange items within <span className={styles.highlight}>14 days </span>
                            from the date of receipt. The items must be in original packaging, with all tags attached,
                            and without signs of wear. To initiate a return, contact us by phone or email —
                            we will help you resolve the issue quickly.
                        </p>
                    </div>
                </div>
            </section>
        </PageWrapper>
    );
};

export default DeliveryInfoPage;