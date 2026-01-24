import { useState } from 'react';
import { Link } from 'react-router-dom';
import { User, ShoppingBag, Menu, X } from 'lucide-react';
import styles from './Header.module.css';

const Header = () => {
    const [menuOpen, setMenuOpen] = useState<boolean>(false);

    return (
        <header className={styles.header}>
            <div className={styles.container}>
                <Link to="/" className={styles.logo}>Clothy</Link>

                <nav className={`${styles.nav} ${menuOpen ? styles.navOpen : ''}`}>
                    <Link to="/catalog" className={styles.navLink} onClick={() => setMenuOpen(false)}>Каталог</Link>
                    <Link to="/about-us" className={styles.navLink} onClick={() => setMenuOpen(false)}>Про нас</Link>
                    <Link to="/delivery-info" className={styles.navLink} onClick={() => setMenuOpen(false)}>Доставка</Link>
                </nav>

                <div className={styles.right}>
                    <Link to="/cart" className={styles.iconButton}><ShoppingBag size={20} /></Link>
                    <Link to="/account" className={styles.iconButton}><User size={20} /></Link>

                    <button className={styles.menuButton} onClick={() => setMenuOpen(!menuOpen)}>
                        {menuOpen ? <X size={24} /> : <Menu size={24} />}
                    </button>
                </div>
            </div>
        </header>
    );
};

export default Header;