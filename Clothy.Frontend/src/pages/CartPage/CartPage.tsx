import { useNavigate } from 'react-router-dom';
import type { IBasketList } from "../../entities/basketService/IBasketList.ts";
import styles from './CartPage.module.css';
import CartItem from "../../entities/basketService/cartItem/CartItem.tsx";
import { Helmet } from 'react-helmet';
import PageWrapper from "../../shared/PageWrapper/PageWrapper.tsx";
import EmptyCart from '../../features/cart/emptyCart/EmptyCart.tsx';
import Button from "../../shared/Button/Button.tsx";
import OrderSummary from "../../features/checkout/orderSummary/OrderSummary.tsx";

const CartPage = () => {
    const navigate = useNavigate();

    // #TODO: Implement API CALL

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
        totalPrice: 1400
    };

    const handleClearCart = () => {
        // TODO: API
        console.log('Cart cleared');
    };

    const handleCheckout = () => {
        navigate('/checkout');
    };

    const totalItems = mockCartItems.items.reduce((sum, item) => sum + item.quantity, 0);

    if (!mockCartItems || !mockCartItems.items || mockCartItems.items.length === 0) {
        return (
            <PageWrapper>
                <Helmet>
                    <title>Clothy — your cart is empty</title>
                    <meta name="description" content="Your cart is empty. Start shopping in the Clothy catalog." />
                </Helmet>
                <EmptyCart />
            </PageWrapper>
        );
    }

    return (
        <PageWrapper>
            <div className={styles.container}>
                <Helmet>
                    <title>{`Clothy — your cart (${totalItems} items)`}</title>
                    <meta
                        name="description"
                        content="View the items in your cart and complete your order quickly and conveniently."
                    />
                </Helmet>

                <div className={styles.basketList}>
                    {mockCartItems.items.map((item) => (
                        <CartItem
                            key={`${item.clotheId}-${item.colorId}-${item.sizeId}`}
                            item={item}
                        />
                    ))}
                </div>

                <OrderSummary
                    title="Your Order"
                    priceRows={[
                        { label: `Items (${totalItems})`, value: `${mockCartItems.totalPrice} ₴` },
                        { label: 'Delivery', value: mockCartItems.totalPrice > 1500 ? 'Free' : 'Paid' }
                    ]}
                    totalPrice={mockCartItems.totalPrice}
                    buttons={
                        <>
                            <Button
                                variant="primary"
                                size="lg"
                                fullWidth
                                onClick={handleCheckout}
                            >
                                Checkout →
                            </Button>

                            <Button
                                variant="outline"
                                fullWidth
                                to="/catalog"
                            >
                                Continue Shopping
                            </Button>

                            <Button
                                variant="outline"
                                fullWidth
                                onClick={handleClearCart}
                            >
                                Clear All
                            </Button>
                        </>
                    }
                />
            </div>
        </PageWrapper>
    );
};

export default CartPage;