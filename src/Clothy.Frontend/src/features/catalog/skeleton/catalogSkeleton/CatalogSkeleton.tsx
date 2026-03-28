import styles from "../../../../pages/catalog/CatalogPage/CatalogPage.module.css";
import skeletonStyles from "./CatalogSkeleton.module.css";
import ProductGridSkeleton from "../gridSkeleton/ProductGridSkeleton.tsx";

const FILTER_ITEMS = [3, 4, 3, 2, 5, 3, 2, 1];

const CatalogSkeleton = () => {
    return (
        <div className={styles.catalogContainer}>
            <aside className={styles.filterSidebar}>
                <div className={skeletonStyles.filterSkeleton}>
                    {FILTER_ITEMS.map((count, sectionIdx) => (
                        <div key={sectionIdx} className={skeletonStyles.filterSection}>
                            <div className={skeletonStyles.sectionTitle} />
                            {Array.from({ length: count }).map((_, i) => (
                                <div key={i} className={skeletonStyles.checkboxRow}>
                                    <div className={skeletonStyles.checkbox} />
                                    <div
                                        className={skeletonStyles.checkboxLabel}
                                        style={{ width: `${55 + ((sectionIdx * 3 + i * 7) % 35)}%` }}
                                    />
                                </div>
                            ))}
                        </div>
                    ))}
                </div>
            </aside>

            <main className={styles.catalogMain}>
                <div className={styles.catalogHeader}>
                    <div className={skeletonStyles.sortBar} />
                </div>
                <ProductGridSkeleton />
            </main>
        </div>
    );
};

export default CatalogSkeleton;