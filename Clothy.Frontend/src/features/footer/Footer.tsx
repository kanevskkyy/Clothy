import { Link } from 'react-router-dom';
import { Facebook, Instagram, Send } from "lucide-react";
import styles from './Footer.module.css';

const Footer = () => {
    return (
        <footer className={styles.footer}>
            <div className={styles.container}>
                <div className={styles.grid}>
                    <div className={styles.brandSection}>
                        <Link to="/" className={styles.brandTitle}>Clothy</Link>
                        <p className={styles.brandDescription}>
                            Ваш стильний гардероб. Найкращі бренди та актуальні колекції.
                        </p>
                        <div className={styles.socialLinks}>
                            <Link to="https://www.instagram.com/clothy-shop" className={styles.socialLink}>
                                <Instagram size={20} />
                            </Link>
                            <Link to="https://www.facebook.com/clothy-shop" className={styles.socialLink}>
                                <Facebook size={20} />
                            </Link>
                            <Link to="https://www.telegram.com/clothy-shop" className={styles.socialLink}>
                                <Send size={20} />
                            </Link>
                        </div>
                    </div>

                    <div className={styles.section}>
                        <h4 className={styles.sectionTitle}>Інформація</h4>
                        <ul className={styles.list}>
                            <li className={styles.listItem}>
                                <Link to="/about-us" className={styles.link}>Про нас</Link>
                            </li>
                            <li className={styles.listItem}>
                                <Link to="/delivery-info" className={styles.link}>Доставка та оплата</Link>
                            </li>
                            <li className={styles.listItem}>
                                <a href="#" className={styles.link}>Повернення та обмін</a>
                            </li>
                        </ul>
                    </div>

                    <div className={styles.section}>
                        <h4 className={styles.sectionTitle}>Контакти</h4>
                        <ul className={styles.list}>
                            <li className={styles.listItem}>+38 (050) 238-19-39</li>
                            <li className={styles.listItem}>info@clothy.ua</li>
                            <li className={styles.listItem}>Пн-Пт: 9:00 - 21:00</li>
                        </ul>
                    </div>
                </div>
            </div>
        </footer>
    );
};

export default Footer;