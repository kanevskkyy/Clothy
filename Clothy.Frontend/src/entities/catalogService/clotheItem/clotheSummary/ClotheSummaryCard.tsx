import { Link } from "react-router-dom";
import { memo, useState } from "react";
import styles from "./ClotheSummaryCard.module.css";
import type {IClotheSummaryDTO} from "../IClotheSummaryDTO.ts";

interface ClotheSummaryCardProps {
    product: IClotheSummaryDTO;
}

const ClotheSummaryCard = memo(({ product }: ClotheSummaryCardProps) => {
    const [selectedColor, setSelectedColor] = useState(product.colors[0]);

    return (
        <Link
            to={`/clothe/${product.slug}/${selectedColor.colorSlug}`}
            className={styles.card}
        >
            <div className={styles.imageContainer}>
                <img
                    src={selectedColor.photoURL}
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
                        Not available
                    </div>
                )}
            </div>

            <div className={styles.content}>
                <div className={styles.brand}>{product.brand.name}</div>

                <h4 className={styles.name}>{product.name}</h4>

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

export default ClotheSummaryCard;