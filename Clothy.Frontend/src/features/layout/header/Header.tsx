import { useState } from 'react';
import { Link } from 'react-router-dom';
import { User, ShoppingBag, Menu, X } from 'lucide-react';
import styles from './Header.module.css';
import Container from "../../../shared/layout/Container/Container.tsx";

const Header = () => {
    const [menuOpen, setMenuOpen] = useState<boolean>(false);

    return (
        <header className={styles.header}>
            <Container className={styles.inner}>
                <Link to="/" className={styles.logo}>
                    Clothy
                </Link>

                <nav className={`${styles.nav} ${menuOpen ? styles.navOpen : ''}`}>
                    <Link to="/catalog?gender=Male" className={styles.navLink} onClick={() => setMenuOpen(false)}>
                        Men
                    </Link>
                    <Link to="/catalog?gender=Female" className={styles.navLink} onClick={() => setMenuOpen(false)}>
                        Woman
                    </Link>
                    <Link to="/about-us" className={styles.navLink} onClick={() => setMenuOpen(false)}>
                        About us
                    </Link>
                    <Link to="/delivery-info" className={styles.navLink} onClick={() => setMenuOpen(false)}>
                        Delivery
                    </Link>
                </nav>

                <div className={styles.right}>
                    <Link to="/cart" className={styles.iconButton}>
                        <ShoppingBag size={20} />
                    </Link>
                    <Link to="/account" className={styles.iconButton}>
                        <User size={20} />
                    </Link>

                    <button className={styles.menuButton} onClick={() => setMenuOpen(!menuOpen)}>
                        {menuOpen ? <X size={24} /> : <Menu size={24} />}
                    </button>
                </div>
            </Container>
        </header>
    );
};

export default Header;