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
                            Your stylish wardrobe. The best brands and latest collections.
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
                        <h4 className={styles.sectionTitle}>Information</h4>
                        <ul className={styles.list}>
                            <li className={styles.listItem}>
                                <Link to="/catalog" className={styles.link}>Catalog</Link>
                            </li>
                            <li className={styles.listItem}>
                                <Link to="/about-us" className={styles.link}>About us</Link>
                            </li>
                            <li className={styles.listItem}>
                                <Link to="/delivery-info" className={styles.link}>Delivery and payment</Link>
                            </li>
                        </ul>
                    </div>

                    <div className={styles.section}>
                        <h4 className={styles.sectionTitle}>Contacts</h4>
                        <ul className={styles.list}>
                            <li className={styles.listItem}>+38 (050) 238-19-39</li>
                            <li className={styles.listItem}>info@clothy.ua</li>
                            <li className={styles.listItem}>Mon–Fri: 9:00 - 21:00</li>
                        </ul>
                    </div>
                </div>
            </div>
        </footer>
    );
};

export default Footer;