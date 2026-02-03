import type {IBasketItemCart} from "../interfaces/IBasketItemCart.ts";
import styles from "./CartItem.module.css";
import {Link} from "react-router-dom";
import {X} from "lucide-react";

interface CartItemProps {
    item: IBasketItemCart;
}

const CartItem: React.FC<CartItemProps> = ({ item }) => {
    const handleRemove = () => {
        console.log('Видалено товар:', {
            clotheId: item.clotheId,
            colorId: item.colorId,
            sizeId: item.sizeId,
            clotheName: item.clotheName
        });
    };

    const handleIncrease = () => {
        console.log('Збільшено кількість:', {
            clotheId: item.clotheId,
            colorId: item.colorId,
            sizeId: item.sizeId,
            clotheName: item.clotheName,
            newQuantity: item.quantity + 1
        });
    };

    const handleDecrease = () => {
        console.log('Зменшено кількість:', {
            clotheId: item.clotheId,
            colorId: item.colorId,
            sizeId: item.sizeId,
            clotheName: item.clotheName,
            newQuantity: item.quantity - 1
        });
    };

    const itemTotal = item.price * item.quantity;

    return (
        <div className={styles.basketCard}>
            <Link
                to={`/clothe/${item.clotheSlug}/${item.colorSlug}/`}
                className={styles.clotheItem}
            >
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
                        <div
                            className={styles.clotheColor}
                            style={{ backgroundColor: item.hexCode }}
                        ></div>
                        <span className={styles.clotheSize}>Розмір: {item.sizeName}</span>
                    </div>
                </div>

                <div className={styles.priceSection}>
                    <button className={styles.close} type="button" onClick={handleRemove}>
                        <X size={20} />
                    </button>
                    <div className={styles.quantityControls}>
                        <button className={styles.minus} type="button" onClick={handleDecrease}>
                            −
                        </button>
                        <div className={styles.quantity}>{item.quantity}</div>
                        <button className={styles.plus} type="button" onClick={handleIncrease}>
                            +
                        </button>
                    </div>
                    <p className={styles.price}>{itemTotal} ₴</p>
                </div>
            </div>
        </div>
    );
};

export default CartItem;