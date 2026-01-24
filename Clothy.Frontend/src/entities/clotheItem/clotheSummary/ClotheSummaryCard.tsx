import { Link } from "react-router-dom";
import { memo, useState } from "react";
import type { IClotheSummaryDTO } from "../clothe.ts";
import styles from "./ClotheSummaryCard.module.css";

interface ClotheSummaryCardProps {
    product: IClotheSummaryDTO;
}

const ClotheSummaryCard = memo(({ product }: ClotheSummaryCardProps) => {
    const [selectedColor, setSelectedColor] = useState(product.colors[0]);

    return (
        <Link
            to={`/clothe/${product.slug}/colorId=${selectedColor.colorId}`}
            className={styles.card}
        >
            <div className={styles.imageContainer}>
                <img
                    src={selectedColor.mainPhotoURL}
                    alt={product.name}
                    className={styles.image}
                    loading="lazy"
                    decoding="async"
                />

                {product.discountPercent && (
                    <div className={styles.discountBadge}>-{product.discountPercent}%</div>
                )}

                {!product.isAvailable && (
                    <div className={styles.unavailableBadge}>
                        Немає в наявності
                    </div>
                )}
            </div>

            <div className={styles.content}>
                <div className={styles.brandInfo}>
                    <img
                        src={product.brand.photoURL}
                        alt={product.brand.name}
                        width="20"
                        height="20"
                        loading="lazy"
                        decoding="async"
                    />
                    <div className={styles.brand}>{product.brand.name}</div>
                </div>

                <div className={styles.name}>{product.name}</div>

                <div className={styles.colors}>
                    {product.colors.map((color) => (
                        <button
                            key={color.id}
                            type="button"
                            className={`${styles.colorDot} ${
                                selectedColor.id === color.id
                                    ? styles.activeColor
                                    : ""
                            }`}
                            style={{ backgroundColor: color.hexCode }}
                            title={color.colorId}
                            onClick={(e) => {
                                e.preventDefault();
                                e.stopPropagation();
                                setSelectedColor(color);
                            }}
                        />
                    ))}
                </div>

                <div className={styles.priceContainer}>
                    <span className={styles.price}>{product.price} $</span>
                    {product.oldPrice && (
                        <span className={styles.oldPrice}>
                            {product.oldPrice} $
                        </span>
                    )}
                </div>
            </div>
        </Link>
    );
});

ClotheSummaryCard.displayName = "ClotheSummaryCard";
export default ClotheSummaryCard;