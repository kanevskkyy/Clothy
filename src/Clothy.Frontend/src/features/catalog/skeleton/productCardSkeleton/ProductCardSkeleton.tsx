import styles from "../../../../entities/catalogService/clotheSummary/ClotheSummaryCard.module.css";
import skeletonStyles from "./ProductCardSkeleton.module.css";

const ProductCardSkeleton = () => {
    return (
        <div className={styles.card}>
            <div className={`${styles.imageContainer} ${skeletonStyles.shimmer}`} />

            <div className={styles.content}>
                <div className={skeletonStyles.nameLine} />

                <div className={styles.footer}>
                    <div className={styles.colors}>
                        <div className={skeletonStyles.colorDot} />
                        <div className={skeletonStyles.colorDot} />
                        <div className={skeletonStyles.colorDot} />
                    </div>
                    <div className={skeletonStyles.priceLine} />
                </div>
            </div>
        </div>
    );
};

export default ProductCardSkeleton;