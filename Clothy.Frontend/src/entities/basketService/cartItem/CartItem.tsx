import type { IBasketItemCart } from "../IBasketItemCart.ts";
import styles from "./CartItem.module.css";
import { Link } from "react-router-dom";
import { X, AlertTriangle } from "lucide-react";
import { basketApi } from "../../../app/api/basketApi.ts";
import { useEffect, useRef, useState } from "react";
import { toast } from "sonner";
import { getErrorMessage } from "../../../shared/lib/errorHandler.ts";
import { formatMoney } from "../../../shared/lib/formatMoney.ts";

interface CartItemProps {
    item: IBasketItemCart;
    onUpdate: () => Promise<void>;
}

const DEBOUNCE_MS = 600;

const CartItem: React.FC<CartItemProps> = ({ item, onUpdate }) => {
    const [optimisticQuantity, setOptimisticQuantity] = useState(item.quantity);
    const [isRemoving, setIsRemoving] = useState(false);

    const quantityRef = useRef(item.quantity);
    const debounceTimer = useRef<ReturnType<typeof setTimeout> | null>(null);

    const scheduleUpdate = (newQuantity: number) => {
        quantityRef.current = newQuantity;

        if (debounceTimer.current) clearTimeout(debounceTimer.current);

        debounceTimer.current = setTimeout(async () => {
            debounceTimer.current = null;
            try {
                await basketApi.updateCartAsync({
                    clotheId: item.clotheId,
                    sizeId: item.sizeId,
                    colorId: item.colorId,
                    quantity: quantityRef.current,
                });
                await onUpdate();
            } catch (error) {
                setOptimisticQuantity(item.quantity);
                quantityRef.current = item.quantity;
                toast.error(getErrorMessage(error));
            }
        }, DEBOUNCE_MS);
    };

    useEffect(() => {
        if (!debounceTimer.current) {
            setOptimisticQuantity(item.quantity);
            quantityRef.current = item.quantity;
        }
    }, [item.quantity]);

    useEffect(() => {
        return () => {
            if (debounceTimer.current) clearTimeout(debounceTimer.current);
        };
    }, []);

    const handleIncrease = () => {
        const next = optimisticQuantity + 1;
        setOptimisticQuantity(next);
        scheduleUpdate(next);
    };

    const handleDecrease = () => {
        if (optimisticQuantity <= 1) return;
        const next = optimisticQuantity - 1;
        setOptimisticQuantity(next);
        scheduleUpdate(next);
    };

    const handleRemove = async () => {
        if (isRemoving) return;
        if (debounceTimer.current) clearTimeout(debounceTimer.current);
        setIsRemoving(true);
        try {
            await basketApi.deleteFromCartAsync(item.clotheId, item.sizeId, item.colorId);
            await onUpdate();
        } catch (error) {
            setIsRemoving(false);
            toast.error(getErrorMessage(error));
        }
    };

    const itemTotal = item.price * optimisticQuantity;

    return (
        <div className={`${styles.basketCard} ${isRemoving ? styles.removing : ""} ${!item.isAvailable ? styles.unavailable : ""}`}>
            <Link to={`/clothe/${item.clotheSlug}/${item.colorSlug}/`} className={styles.clotheItem}>
                <img
                    src={item.mainPhoto}
                    alt={item.clotheName}
                    width="130"
                    height="150"
                    loading="lazy"
                />
            </Link>

            <div className={styles.clotheInfo}>
                <div className={styles.clotheName}>
                    <span>{item.clotheName}</span>
                    <div className={styles.clotheAdditionalInfo}>
                        <div className={styles.clotheColor} style={{ backgroundColor: item.hexCode }} />
                        <span className={styles.clotheSize}>Size: {item.sizeName}</span>
                    </div>
                    {!item.isAvailable && item.validationMessage && (
                        <span className={styles.validationMessage}>
                            <AlertTriangle size={14} />
                            {item.validationMessage}
                        </span>
                    )}
                </div>

                <div className={styles.priceSection}>
                    <button
                        className={styles.close}
                        type="button"
                        onClick={handleRemove}
                        disabled={isRemoving}
                    >
                        <X size={20} />
                    </button>

                    {item.isAvailable ? (
                        <>
                            <div className={styles.quantityControls}>
                                <button
                                    className={styles.minus}
                                    type="button"
                                    onClick={handleDecrease}
                                    disabled={optimisticQuantity <= 1}
                                >
                                    −
                                </button>
                                <div className={styles.quantity}>{optimisticQuantity}</div>
                                <button
                                    className={styles.plus}
                                    type="button"
                                    onClick={handleIncrease}
                                >
                                    +
                                </button>
                            </div>
                            <p className={styles.price}>{formatMoney(itemTotal)} ₴</p>
                        </>
                    ) : (
                        <p className={styles.unavailablePrice}>{formatMoney(itemTotal)} ₴</p>
                    )}
                </div>
            </div>
        </div>
    );
};

export default CartItem;