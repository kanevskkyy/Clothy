import ProductCardSkeleton from "../productCardSkeleton/ProductCardSkeleton";
import styles from "./PopularProductsSection.module.css";

const SKELETON_COUNT = 8;

const ProductGridSkeleton = () => {
    return (
        <div className={styles.popularGrid}>
            {Array.from({ length: SKELETON_COUNT }).map((_, i) => (
                <ProductCardSkeleton key={i} />
            ))}
        </div>
    );
};

export default ProductGridSkeleton;