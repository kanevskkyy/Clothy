import { useNavigate } from "react-router-dom";
import type {IClotheSummaryDTO} from "../clothe.ts";
import styles from "./ClotheSummaryCard.module.css";

interface ClotheSummaryCardProps {
    product: IClotheSummaryDTO;
}

const ClotheSummaryCard = ({ product }: ClotheSummaryCardProps) => {
    const navigate = useNavigate();

    const handleClick = () => {
        navigate(`/clothe/${product.slug}/${product.id}`);
    };

    return (
        <div className={styles.card} onClick={handleClick}>
            <div className={styles.imageContainer}>
                <img
                    src={product.colors[0].mainPhotoURL}
                    alt={product.name}
                    className={styles.image}
                />
                {product.discountPercent && (
                    <div className={styles.discountBadge}>
                        -{product.discountPercent}%
                    </div>
                )}
                {!product.isAvailable && (
                    <div className={styles.unavailableBadge}>
                        Немає в наявності
                    </div>
                )}
            </div>

            <div className={styles.content}>
                <div className={styles.brandInfo}>
                    <img src={product.brand.photoURL}
                         alt={product.brand.name}
                         width="20px"
                         height="20px"
                    />
                    <div className={styles.brand}>{product.brand.name}</div>
                </div>
                <div className={styles.name}>{product.name}</div>

                <div className={styles.colors}>
                    {product.colors.map((color) => (
                        <div
                            key={color.id}
                            className={styles.colorDot}
                            style={{ backgroundColor: color.hexCode }}
                            title={color.colorId}
                        />
                    ))}
                </div>

                <div className={styles.priceContainer}>
                    <span className={styles.price}>{product.price} $</span>
                    {product.oldPrice && (
                        <span className={styles.oldPrice}>{product.oldPrice} $</span>
                    )}
                </div>
            </div>
        </div>
    );
};

export default ClotheSummaryCard;