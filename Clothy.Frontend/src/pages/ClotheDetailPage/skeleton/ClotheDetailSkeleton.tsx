import styles from './ClotheDetailSkeleton.module.css';

const ClotheDetailSkeleton = () => (
    <div className={styles.container}>
        <div className={styles.gallery}>
            <div className={styles.mainImage} />
            <div className={styles.thumbnails}>
                {[...Array(4)].map((_, i) => (
                    <div key={i} className={styles.thumbnail} />
                ))}
            </div>
        </div>

        <div className={styles.detail}>
            <div className={styles.titleLarge} />
            <div className={styles.titleSmall} />
            <div className={styles.line} />
            <div className={styles.line} style={{ width: '45%' }} />

            <div className={styles.colorRow}>
                {[...Array(4)].map((_, i) => (
                    <div key={i} className={styles.colorDot} />
                ))}
            </div>

            <div className={styles.sizeRow}>
                {[...Array(5)].map((_, i) => (
                    <div key={i} className={styles.sizeBox} />
                ))}
            </div>

            <div className={styles.price} />
            <div className={styles.button} />
            <div className={styles.button} />
        </div>
    </div>
);

export default ClotheDetailSkeleton;