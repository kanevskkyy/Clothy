import { Link, useNavigate } from 'react-router-dom';
import type { IBasketList } from "../../entities/basket/interfaces/IBasketList.ts";
import styles from './CartPage.module.css';
import CartItem from "../../entities/basket/cartItem/CartItem.tsx";

const CartPage = () => {
    const navigate = useNavigate();

    const mockCartItems: IBasketList = {
        userId: "user-123",
        items: [
            {
                clotheId: "cl-1",
                sizeId: "size-m",
                colorId: "color-black",
                clotheName: "Nike Air Max T-Shirt",
                clotheSlug: "nike-air-max-tshirt",
                price: 1200,
                mainPhoto: "https://res.cloudinary.com/dkdljnfja/image/upload/v1769171770/clotheThirdSecond_mljouz.jpg",
                sizeName: "M",
                hexCode: "#000000",
                colorName: "Black",
                colorSlug: "black",
                quantity: 2
            },
            {
                clotheId: "cl-2",
                sizeId: "size-l",
                colorId: "color-white",
                clotheName: "Adidas Hoodie",
                clotheSlug: "adidas-hoodie",
                price: 2500,
                mainPhoto: "https://res.cloudinary.com/dkdljnfja/image/upload/v1769171770/clotheThirdSecond_mljouz.jpg",
                sizeName: "L",
                hexCode: "#FFFFFF",
                colorName: "White",
                colorSlug: "white",
                quantity: 1
            },
            {
                clotheId: "cl-3",
                sizeId: "size-s",
                colorId: "color-blue",
                clotheName: "Puma Shorts",
                clotheSlug: "puma-shorts",
                price: 900,
                mainPhoto: "https://res.cloudinary.com/dkdljnfja/image/upload/v1769171770/clotheThirdSecond_mljouz.jpg",
                sizeName: "S",
                hexCode: "#0000FF",
                colorName: "Blue",
                colorSlug: "blue",
                quantity: 3
            },
            {
                clotheId: "cl-4",
                sizeId: "size-xl",
                colorId: "color-gray",
                clotheName: "Under Armour Jacket",
                clotheSlug: "ua-jacket",
                price: 3200,
                mainPhoto: "https://res.cloudinary.com/dkdljnfja/image/upload/v1769171770/clotheThirdSecond_mljouz.jpg",
                sizeName: "XL",
                hexCode: "#808080",
                colorName: "Gray",
                colorSlug: "gray",
                quantity: 1
            },
            {
                clotheId: "cl-5",
                sizeId: "size-m",
                colorId: "color-red",
                clotheName: "Reebok Tank Top",
                clotheSlug: "reebok-tank",
                price: 700,
                mainPhoto: "https://res.cloudinary.com/dkdljnfja/image/upload/v1769171770/clotheThirdSecond_mljouz.jpg",
                sizeName: "M",
                hexCode: "#FF0000",
                colorName: "Red",
                colorSlug: "red",
                quantity: 2
            },
            {
                clotheId: "cl-6",
                sizeId: "size-l",
                colorId: "color-green",
                clotheName: "New Balance Pants",
                clotheSlug: "nb-pants",
                price: 1800,
                mainPhoto: "https://res.cloudinary.com/dkdljnfja/image/upload/v1769171770/clotheThirdSecond_mljouz.jpg",
                sizeName: "L",
                hexCode: "#00AA00",
                colorName: "Green",
                colorSlug: "green",
                quantity: 1
            }
        ],
        totalPrice: 2900
    };

    const handleClearCart = () => {
        console.log('Корзину очищено');
    };

    const handleCheckout = () => {
        console.log('Оформлення замовлення');
        navigate('/orders');
    };

    const totalItems = mockCartItems.items.reduce((sum, item) => sum + item.quantity, 0);
    const totalPrice = mockCartItems.items.reduce((sum, item) => sum + (item.price * item.quantity), 0);

    return (
        <div className={styles.container}>
            <div className={styles.basketList}>
                {mockCartItems.items.map((item) => (
                    <CartItem
                        key={`${item.clotheId}-${item.colorId}-${item.sizeId}`}
                        item={item}
                    />
                ))}
            </div>

            <div className={styles.orderSummary}>
                <h3>Ваше замовлення</h3>

                <div className={styles.orderPriceInfo}>
                    <div className={styles.priceRow}>
                        <span className={styles.label}>Товари ({totalItems})</span>
                        <span className={styles.value}>{totalPrice} ₴</span>
                    </div>

                    <div className={styles.priceRow}>
                        <span className={styles.label}>Доставка</span>
                        <span className={styles.value}>Платна</span>
                    </div>

                    <div className={styles.divider}></div>

                    <div className={styles.totalRow}>
                        <span>До сплати</span>
                        <span>{totalPrice} ₴</span>
                    </div>
                </div>

                <button className={styles.continueBtn} onClick={handleCheckout}>
                    Оформити замовлення →
                </button>
                <Link to="/catalog">
                    <button className={styles.catalogBtn}>Продовжити покупки</button>
                </Link>
                <button className={styles.clearBtn} onClick={handleClearCart}>
                    Очистити все
                </button>
            </div>
        </div>
    );
};

export default CartPage;