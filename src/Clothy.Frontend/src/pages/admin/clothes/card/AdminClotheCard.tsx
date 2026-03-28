import { useNavigate } from "react-router-dom";
import { memo, useState } from "react";
import { Pencil } from "lucide-react";
import styles from "./AdminClotheCard.module.css";
import type {IClotheSummaryDTO} from "../../../../entities/catalogService/interfaces/clothe/IClotheSummaryDTO.ts";
import { formatMoney } from "../../../../shared/lib/formatMoney.ts";

interface AdminClotheCardProps {
    product: IClotheSummaryDTO;
}

const AdminClotheCard = memo(({ product }: AdminClotheCardProps) => {
    const [selectedColor, setSelectedColor] = useState(product.colors[0]);
    const navigate = useNavigate();

    return (
        <div
            className={styles.card}
            onClick={() => navigate(`/admin/clothes/edit/${product.id}`)}
        >
            <div className={styles.imageContainer}>
                <img
                    src={selectedColor.photoURL}
                    alt={product.name}
                    className={styles.image}
                    loading="lazy"
                    decoding="async"
                />
                {!product.isAvailable && (
                    <div className={styles.unavailableBadge}>Not available</div>
                )}
                <div className={styles.editOverlay}>
                    <Pencil size={18} />
                    <span>Edit</span>
                </div>
            </div>

            <div className={styles.content}>
                <h4 className={styles.name}>{product.name}</h4>
                <div className={styles.footer}>
                    <div className={styles.colors}>
                        {product.colors.map((color) => (
                            <button
                                key={color.id}
                                type="button"
                                className={`${styles.colorDot} ${selectedColor.id === color.id ? styles.activeColor : ""}`}
                                style={{ backgroundColor: color.hexCode }}
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
        </div>
    );
});

export default AdminClotheCard;