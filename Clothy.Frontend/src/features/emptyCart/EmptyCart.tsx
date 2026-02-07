import { ShoppingCart } from 'lucide-react';
import { Link } from 'react-router-dom';
import styles from './EmptyCart.module.css';

const EmptyCart = () => {
    return (
        <div className={styles.wrapper}>
            <div className={styles.container}>
                <div className={styles.mainIcon}>
                    <ShoppingCart size={24} color="#6B6B6B" />
                </div>
                <h1>Your cart is empty</h1>
                <p>
                    Start shopping and find something special for yourself
                </p>
                <Link to="/catalog" className={styles.catalogButton}>
                    Go to catalog
                </Link>
            </div>
        </div>
    );
};

export default EmptyCart;