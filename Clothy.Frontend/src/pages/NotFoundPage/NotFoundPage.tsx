import { Home, SearchIcon, ShoppingBasket } from 'lucide-react';
import styles from './NotFound.module.css';
import { Helmet } from "react-helmet";
import PageWrapper from "../../shared/PageWrapper/PageWrapper.tsx";

const NotFoundPage = () => {
    return (
        <PageWrapper>
            <div className={styles.wrapper}>
                <Helmet>
                    <title>Page Not Found | Clothy</title>
                </Helmet>

                <div className={styles.container}>
                    <div className={styles.mainIcon}>
                        <SearchIcon size={24} color="#6B6B6B" />
                    </div>
                    <h1>Page Not Found</h1>
                    <p>
                        It seems the page got lost along the way. No worries — we still have plenty of great products for you!
                    </p>
                    <div className={styles.actions}>
                        <a href="/" className={styles.primaryButton}>
                            <Home size={20} />
                            Home
                        </a>
                        <a href="/catalog" className={styles.secondaryButton}>
                            <ShoppingBasket size={20} />
                            To Catalog
                        </a>
                    </div>
                </div>
            </div>
        </PageWrapper>
    );
};

export default NotFoundPage;