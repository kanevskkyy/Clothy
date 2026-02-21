import {Link} from 'react-router-dom';
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
                    </div>

                    <div className={styles.sections}>

                        <div className={styles.section}>
                            <h4 className={styles.sectionTitle}>Catalog</h4>
                            <ul className={styles.list}>
                                <li className={styles.listItem}>
                                    <Link to="/catalog" className={styles.link}>All products</Link>
                                </li>
                                <li className={styles.listItem}>
                                    <Link to="/catalog?gender=Male" className={styles.link}>Men</Link>
                                </li>
                                <li className={styles.listItem}>
                                    <Link to="/catalog?gender=Female" className={styles.link}>Women</Link>
                                </li>
                            </ul>
                        </div>

                        <div className={styles.section}>
                            <h4 className={styles.sectionTitle}>Information</h4>
                            <ul className={styles.list}>
                                <li className={styles.listItem}>
                                    <Link to="/about-us" className={styles.link}>About us</Link>
                                </li>
                                <li className={styles.listItem}>
                                    <Link to="/delivery-info" className={styles.link}>Delivery & Shipping</Link>
                                </li>
                                <li className={styles.listItem}>
                                    <Link to="/account" className={styles.link}>My Account</Link>
                                </li>
                            </ul>
                        </div>

                        <div className={styles.section}>
                            <h4 className={styles.sectionTitle}>Contacts</h4>
                            <ul className={styles.list}>
                                <li className={styles.listItem}>
                                    <a href="tel:+18001234567" className={styles.link}>+1 (800) 123-4567</a>
                                </li>
                                <li className={styles.listItem}>
                                    <a href="mailto:info@atelier.com" className={styles.link}>info@atelier.com</a>
                                </li>
                                <li className={styles.listItem}>
                                    <a href="https://maps.google.com/?q=5th+Avenue+New+York" target="_blank" rel="noreferrer" className={styles.link}>New York, 5th Avenue</a>
                                </li>
                            </ul>
                        </div>
                    </div>
                </div>
            </div>
        </footer>
    );
};

export default Footer;