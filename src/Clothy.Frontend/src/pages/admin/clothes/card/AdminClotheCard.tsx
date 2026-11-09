import { Link } from "react-router-dom";
import { memo, useState } from "react";
import { Pencil } from "lucide-react";
import styles from "./AdminClotheCard.module.css";
import type {IClotheSummaryDTO} from "../../../../entities/catalogService/interfaces/clothe/IClotheSummaryDTO.ts";
import { formatMoney } from "../../../../shared/lib/formatMoney.ts";

interface AdminClotheCardProps {
    product: IClotheSummaryDTO;
}

// The whole card opens the edit page: the name link stretches over it, color dots sit above.
const AdminClotheCard = memo(({ product }: AdminClotheCardProps) => {
    const [selectedColor, setSelectedColor] = useState(product.colors[0]);

    return (
        <article className={styles.card}>
            <div className={styles.imageContainer}>
                <img
                    src={selectedColor.photoURL}
                    alt=""
                    className={styles.image}
                    loading="lazy"
                    decoding="async"
                />
                {!product.isAvailable && (
                    <div className={styles.unavailableBadge}>Not available</div>
                )}
                <div className={styles.editChip} aria-hidden="true">
                    <Pencil size={14} />
                    <span>Edit</span>
                </div>
            </div>

            <div className={styles.content}>
                <h3 className={styles.name}>
                    <Link
                        to={`/admin/clothes/edit/${product.id}`}
                        className={styles.nameLink}
                        title={product.name}
                    >
                        {product.name}
                    </Link>
                </h3>
                <div className={styles.footer}>
                    <div className={styles.colors}>
                        {product.colors.map((color) => (
                            <button
                                key={color.id}
                                type="button"
                                className={`${styles.colorDot} ${selectedColor.id === color.id ? styles.activeColor : ""}`}
                                style={{ backgroundColor: color.hexCode }}
                                aria-label={`Show color ${color.colorSlug}`}
                                aria-pressed={selectedColor.id === color.id}
                                onClick={(e) => {
                                    e.stopPropagation();
                                    setSelectedColor(color);
                                }}
                            />
                        ))}
                    </div>
                    <div className={styles.priceTag}>
                        ${formatMoney(product.price)}
                        {product.oldPrice && (
                            <span className={styles.oldPrice}>${formatMoney(product.oldPrice)}</span>
                        )}
                    </div>
                </div>
            </div>
        </article>
    );
});

export default AdminClotheCard;
