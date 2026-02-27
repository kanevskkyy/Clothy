import styles from './ClotheDetailSkeleton.module.css';
import Container from "../../../../shared/layout/Container/Container.tsx";

const ClotheDetailSkeleton = () => (
    <Container>
        <div className={styles.container}>
            {/* Gallery */}
            <div className={styles.gallery}>
                <div className={styles.mainImage}/>
                <div className={styles.thumbnails}>
                    {[...Array(4)].map((_, i) => (
                        <div key={i} className={styles.thumbnail}/>
                    ))}
                </div>
            </div>

            <div className={styles.detail}>
                <div className={styles.brandName}/>

                <div className={styles.clotheName}/>

                <div className={styles.priceRow}>
                    <div className={styles.price}/>
                    <div className={styles.oldPrice}/>
                </div>

                <div className={styles.descLine}/>
                <div className={styles.descLine}/>
                <div className={styles.descLine} style={{width: '60%'}}/>

                <div className={styles.divider}/>

                <div className={styles.section}>
                    <div className={styles.sectionLabel}/>
                    <div className={styles.pillRow}>
                        <div className={styles.pill}/>
                    </div>
                </div>

                <div className={styles.section}>
                    <div className={styles.sectionLabel}/>
                    <div className={styles.pillRow}>
                        {[...Array(3)].map((_, i) => <div key={i} className={styles.pill}/>)}
                    </div>
                </div>

                <div className={styles.section}>
                    <div className={styles.sectionLabel}/>
                    <div className={styles.pillRow}>
                        {[...Array(2)].map((_, i) => <div key={i} className={styles.pillWide}/>)}
                    </div>
                </div>

                <div className={styles.section}>
                    <div className={styles.sectionLabel}/>
                    <div className={styles.pillRow}>
                        <div className={styles.pill}/>
                    </div>
                </div>

                <div className={styles.section}>
                    <div className={styles.sectionLabel}/>
                    <div className={styles.sizeRow}>
                        {[...Array(5)].map((_, i) => <div key={i} className={styles.sizeBox}/>)}
                    </div>
                </div>

                <div className={styles.section}>
                    <div className={styles.sectionLabel}/>
                    <div className={styles.colorRow}>
                        {[...Array(4)].map((_, i) => <div key={i} className={styles.colorDot}/>)}
                    </div>
                </div>

                <div className={styles.divider}/>

                <div className={styles.cartRow}>
                    <div className={styles.quantityBox}/>
                    <div className={styles.button}/>
                </div>

                <div className={styles.button}/>
            </div>
        </div>
    </Container>
);

export default ClotheDetailSkeleton;