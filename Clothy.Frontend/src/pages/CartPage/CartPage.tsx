import { useNavigate } from 'react-router-dom';
import type { IBasketList } from "../../entities/basketService/IBasketList.ts";
import styles from './CartPage.module.css';
import CartItem from "../../entities/basketService/cartItem/CartItem.tsx";
import { Helmet } from 'react-helmet';
import PageWrapper from "../../shared/PageWrapper/PageWrapper.tsx";
import EmptyCart from '../../features/cart/emptyCart/EmptyCart.tsx';
import Button from "../../shared/Button/Button.tsx";
import OrderSummary from "../../features/checkout/orderSummary/OrderSummary.tsx";
import { useEffect, useState } from "react";
import { basketApi } from "../../app/api/basketApi.ts";
import { getErrorMessage } from "../../shared/utils/errorHandler.ts";
import { toast } from 'sonner';
import Loader from "../../shared/Loader/Loader.tsx";

const CartPage = () => {
    const navigate = useNavigate();
    const [cartItems, setCartItems] = useState<IBasketList | null>(null);
    const [isInitialLoading, setIsInitialLoading] = useState(true);

    const fetchUserCart = async (silent = false) => {
        try {
            if (!silent) setIsInitialLoading(true);
            const data = await basketApi.getMyCartAsync();
            setCartItems(data);
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            if (!silent) setIsInitialLoading(false);
        }
    };

    useEffect(() => {
        fetchUserCart();
    }, []);

    const handleClearCart = async () => {
        try {
            await basketApi.clearCartAsync();
            setCartItems({ userId: "", items: [], totalPrice: 0 });
        } catch (error) {
            toast.error(getErrorMessage(error));
        }
    };

    const handleCheckout = () => {
        navigate('/checkout');
    };

    if (isInitialLoading) {
        return <Loader marginTop="75px" />;
    }

    const items = cartItems?.items ?? [];
    const totalItems = items.reduce((sum, item) => sum + item.quantity, 0);

    if (items.length === 0) {
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
                    {items.map((item) => (
                        <CartItem
                            key={`${item.clotheId}-${item.colorId}-${item.sizeId}`}
                            item={item}
                            onUpdate={() => fetchUserCart(true)}
                        />
                    ))}
                </div>

                <OrderSummary
                    title="Your Order"
                    priceRows={[
                        { label: `Items (${totalItems})`, value: `${cartItems!.totalPrice} ₴` },
                        { label: 'Delivery', value: cartItems!.totalPrice > 1500 ? 'Free' : 'Paid' }
                    ]}
                    totalPrice={cartItems!.totalPrice}
                    buttons={
                        <>
                            <Button variant="primary" size="lg" fullWidth onClick={handleCheckout}>
                                Checkout →
                            </Button>
                            <Button variant="outline" fullWidth to="/catalog">
                                Continue Shopping
                            </Button>
                            <Button variant="outline" fullWidth onClick={handleClearCart}>
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