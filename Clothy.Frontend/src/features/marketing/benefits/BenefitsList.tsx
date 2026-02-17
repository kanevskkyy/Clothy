import {BadgePercent, type LucideIcon, RefreshCw, Shield, Sparkles, Truck} from "lucide-react";
import styles from "./BenefitsList.module.css";

interface IBenefitItem {
    icon: LucideIcon;
    title: string;
    description: string;
}

const benefits: IBenefitItem[] = [
    {
        icon: Truck,
        title: "Free shipping",
        description: "from ₴1500",
    },
    {
        icon: RefreshCw,
        title: "Returns",
        description: "14 days",
    },
    {
        icon: Shield,
        title: "Quality guarantee",
        description: "100% original",
    },
    {
        icon: Sparkles,
        title: "Fast delivery",
        description: "1–3 days",
    },
    {
        icon: BadgePercent,
        title: "10% discount",
        description: "on first order",
    },
];

const BenefitsList = () => {
    return (
        <section className={styles.wrapper}>
            <div className={styles.container}>
                {benefits.map(({ icon: Icon, title, description }) => (
                    <div className={styles.item} key={title}>
                        <div className={styles.iconWrap}>
                            <Icon className={styles.icon} />
                        </div>
                        <div>
                            <p className={styles.title}>{title}</p>
                            <p className={styles.subtitle}>{description}</p>
                        </div>
                    </div>
                ))}
            </div>
        </section>
    );
};

export default BenefitsList;
