import OrderSummary from '../../../features/checkout/orderSummary/OrderSummary.tsx';
import Button from "../../../shared/ui/Button/Button.tsx";
import CheckoutForm from '../../../features/forms/checkoutForm/CheckoutForm.tsx';
import styles from "./CheckoutPage.module.css";
import type {CheckoutFormData} from '../../../app/schemas/checkoutFormSchema.ts';
import {useEffect, useState} from "react";
import {basketApi} from "../../../app/api/basketApi.ts";
import {useNavigate} from "react-router-dom";
import {useAuthStore} from "../../../app/api/stores/authStore.ts";
import {toast} from "sonner";
import {ordersApi} from "../../../app/api/ordersApi.ts";
import {getErrorMessage} from "../../../shared/lib/errorHandler.ts";
import {paymentApi} from "../../../app/api/paymentApi.ts";
import {useQueryClient} from "@tanstack/react-query";
import Loader from "../../../shared/ui/Loader/Loader.tsx";
import Container from "../../../shared/layout/Container/Container.tsx";

const CheckoutPage = () => {
    const queryClient = useQueryClient();
    const [originalPrice, setOriginalPrice] = useState<number>(0);
    const [totalPrice, setTotalPrice] = useState<number>(0);
    const [totalItems, setTotalItems] = useState<number>(0);
    const [isFirstOrder, setIsFirstOrder] = useState<boolean>(false);
    const [isLoading, setIsLoading] = useState(true);
    const [isCreatingOrder, setIsCreatingOrder] = useState(false);

    const user = useAuthStore(state => state.user);
    const emailVerified = user?.emailVerified;

    const navigate = useNavigate();

    const handleValidFormSubmit = async (formData: CheckoutFormData) => {
        if (!emailVerified) {
            toast.error("Please confirm your email to create an order");
            return;
        }

        try {
            setIsCreatingOrder(true);
            const orderData = await ordersApi.createOrderAsync({
                pickupPointId: formData.pickupPointId,
                phoneNumber: formData.phoneNumber,
                firstName: formData.firstName,
                lastName: formData.lastName,
                email: formData.email,
                comment: formData.comment,
            });

            await queryClient.invalidateQueries({queryKey: ["clothe-top8"]});
            const paymentData = await paymentApi.payForOrderAsync(orderData.id, formData.paymentMethod);
            window.location.href = paymentData.paymentUrl;
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setIsCreatingOrder(false);
        }
    };

    const handlePlaceOrder = () => {
        const form = document.getElementById('checkout-form') as HTMLFormElement;
        if (form) form.requestSubmit();
    };

    useEffect(() => {
        const getCartInfo = async () => {
            try {
                const data = await basketApi.getMyCartAsync();
                if (!data || !data.items || data.items.length === 0) {
                    navigate('/catalog');
                    return;
                }
                if (data.unAvailableItemsCount > 0) {
                    navigate('/cart');
                    return;
                }
                setOriginalPrice(data.originalPrice);
                setTotalPrice(data.totalPrice);
                setTotalItems(data.items.length);
                setIsFirstOrder(data.isFirstOrder);
            } catch (error) {
                toast.error(getErrorMessage(error));
                navigate('/cart');
            } finally {
                setIsLoading(false);
            }
        };

        getCartInfo();
    }, []);

    if (isLoading) return <Loader/>;

    return (
        <Container paddingY={30}>
            <div className={styles.container}>
                <div className={styles.formSection}>
                    <h1 className={styles.pageTitle}>Checkout</h1>
                    <CheckoutForm onValidSubmit={handleValidFormSubmit}/>
                </div>

                <OrderSummary
                    unAvailableItemsCount={0}
                    title="Your order"
                    priceRows={[
                        {label: `Items (${totalItems})`, value: `$${originalPrice.toFixed(2)}`},
                        ...(isFirstOrder ? [{
                            label: 'First order discount (10%)',
                            value: `-$${(originalPrice - totalPrice).toFixed(2)}`,
                            isDiscount: true
                        }] : []),
                        {label: 'Delivery', value: totalPrice > 1500 ? 'Free' : 'Paid'}
                    ]}
                    totalPrice={totalPrice}
                    buttons={
                        <>
                            <Button
                                variant="primary"
                                size="lg"
                                fullWidth
                                disabled={isCreatingOrder}
                                onClick={handlePlaceOrder}
                            >
                                {isCreatingOrder ? 'Creating order...' : 'Create an order'}
                            </Button>
                            <Button
                                variant="outline"
                                fullWidth
                                to="/cart"
                            >
                                Back to cart
                            </Button>
                        </>
                    }
                >
                    <div className={styles.warning}>
                        <strong>Attention!</strong><br/>
                        After creating an order, you will have <strong>10 minutes</strong> to pay.
                        If payment is not made on time, the order will be automatically deleted.
                    </div>
                </OrderSummary>
            </div>
        </Container>
    );
};

export default CheckoutPage;