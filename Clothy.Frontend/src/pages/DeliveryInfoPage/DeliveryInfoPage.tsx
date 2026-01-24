import styles from "./DeliveryInfoPage.module.css";
import {Clock, type LucideIcon, Package, RotateCcw, Truck} from "lucide-react";

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
        title: 'Нова Пошта',
        description: 'Доставка у відділення або поштомат',
        time: '1-3 дні',
        price: 'від 70 ₴',
    },
    {
        icon: Package,
        title: "Кур'єр Нової Пошти",
        description: 'Доставка за вашою адресою',
        time: '1-3 дні',
        price: 'від 100 ₴',
    },
];

const DeliveryInfoPage = () => {
    return (
        <section className={styles.storySection}>
            <h2 className={styles.sectionTitle}>Доставка та оплата</h2>

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

            <div className={styles.freeShippingBanner}>
                <p>Безкоштовна доставка при замовленні від 1500 ₴</p>
            </div>

            <div className={styles.returnPolicy}>
                <div className={styles.returnHeader}>
                    <RotateCcw size={24} />
                    <h3>Повернення та обмін</h3>
                </div>

                <div className={styles.returnContent}>
                    <p>
                        Ви можете повернути або обміняти товар протягом <span className={styles.highlight}>14 днів</span> з
                        моменту отримання замовлення. Для повернення товар має бути в оригінальній упаковці, з усіма бірками та без слідів
                        носіння. Для оформлення повернення зв'яжіться з нами за телефоном або напишіть на пошту —
                        ми допоможемо швидко вирішити питання.
                    </p>
                </div>
            </div>
        </section>
    );
};

export default DeliveryInfoPage;