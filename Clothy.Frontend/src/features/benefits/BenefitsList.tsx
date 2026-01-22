import {type LucideIcon, RefreshCw, Shield, Sparkles, Truck} from "lucide-react";
import styles from "./BenefitsList.module.css";

interface IBenefitItem {
    icon: LucideIcon;
    title: string;
    description: string;
}

const benefits: IBenefitItem[] = [
    {
        icon: Truck,
        title: "Безкоштовна доставка",
        description: "від 1500 ₴",
    },
    {
        icon: RefreshCw,
        title: "Повернення",
        description: "14 днів",
    },
    {
        icon: Shield,
        title: "Гарантія якості",
        description: "100% оригінал",
    },
    {
        icon: Sparkles,
        title: "Швидка доставка",
        description: "1–3 дні",
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
