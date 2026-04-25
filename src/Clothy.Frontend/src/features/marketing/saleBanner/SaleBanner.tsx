import styles from "./SaleBanner.module.css";
import Button from "../../../shared/ui/Button/Button.tsx";
import { ArrowRight } from "lucide-react";
import { useEffect, useRef } from "react";

const SaleBanner = () => {
    const sectionRef = useRef<HTMLElement>(null);

    useEffect(() => {
        const el = sectionRef.current;
        if (!el) return;

        const observer = new IntersectionObserver(
            ([entry]) => {
                if (entry.isIntersecting) {
                    el.classList.add(styles.visible);
                    observer.disconnect();
                }
            },
            { threshold: 0.15 }
        );

        observer.observe(el);
        return () => observer.disconnect();
    }, []);

    return (
        <section className={styles.section} ref={sectionRef}>
            <div className={styles.promoInner}>
                <p className={styles.promoEyebrow}>Special offer</p>
                <h2 className={styles.promoTitle}>30% Off Outerwear</h2>
                <p className={styles.promoDescription}>
                    Limited time only. Don't miss out!
                </p>
                <div className={styles.buttonWrapper}>
                    <Button
                        to="/catalog"
                        variant="secondary"
                        icon={<ArrowRight size={20} />}
                    >
                        Shop now
                    </Button>
                </div>
            </div>
            <div className={styles.promoMedia}>
                <img
                    src="assets/images/outerwear.jpg"
                    alt="Outerwear collection"
                    className={styles.promoImage}
                />
            </div>
        </section>
    );
};

export default SaleBanner;