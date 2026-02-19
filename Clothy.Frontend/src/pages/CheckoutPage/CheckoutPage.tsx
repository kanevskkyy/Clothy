import PageWrapper from "../../shared/PageWrapper/PageWrapper.tsx";
import OrderSummary from '../../features/checkout/orderSummary/OrderSummary.tsx';
import Button from "../../shared/Button/Button.tsx";
import CheckoutForm from '../../features/forms/checkoutForm/CheckoutForm.tsx';
import styles from "./CheckoutPage.module.css";
import type { CheckoutFormData } from '../../app/schemas/checkoutFormSchema';
import {useEffect, useState} from "react";
import {basketApi} from "../../app/api/basketApi.ts";
import {useNavigate} from "react-router-dom";
import {useAuthStore} from "../../app/api/stores/authStore.ts";
import {toast} from "sonner";
import {ordersApi} from "../../app/api/ordersApi.ts";
import {getErrorMessage} from "../../shared/utils/errorHandler.ts";
import {paymentApi} from "../../app/api/paymentApi.ts";

const CheckoutPage = () => {
    const [totalPrice, setTotalPrice] = useState<number>();
    const [totalItems, setTotalItems] = useState<number>();

    const user = useAuthStore(state => state.user);
    const emailVerified = user?.emailVerified;

    const [isCreatingOrder, setIsCreatingOrder] = useState(false);

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

            console.groupEnd();

            const paymentData = await paymentApi.payForOrderAsync(orderData.id, formData.paymentMethod);
            window.location.href = paymentData.paymentUrl;
        }
        catch (error: any) {
            toast.error(getErrorMessage(error));
        }
        finally {
            setIsCreatingOrder(false);
        }
    };


    const handlePlaceOrder = () => {
        const form = document.getElementById('checkout-form') as HTMLFormElement;
        if (form) {
            form.requestSubmit();
        }
    };

    useEffect(() => {
        const getCartInfo = async () => {
            const data = await basketApi.getMyCartAsync();
            if (!data || !data.items || data.items.length === 0) {
                navigate('/catalog');
                return;
            }

            setTotalItems(data.items.length);
            setTotalPrice(data.totalPrice);
        };

        getCartInfo();
    }, []);

    return (
        <PageWrapper>
            <div className={styles.container}>
                <div className={styles.formSection}>
                    <h1 className={styles.pageTitle}>Placing an order</h1>
                    <CheckoutForm onValidSubmit={handleValidFormSubmit} />
                </div>

                <OrderSummary
                    title="Your order"
                    priceRows={[
                        { label: `Items (${totalItems})`, value: `${totalPrice} ₴` },
                        { label: 'Delivery', value: totalPrice! > 1500 ? 'Free' : 'Paid' }
                    ]}
                    totalPrice={totalPrice!}
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
                        <strong>Attention!</strong><br />
                        After creating an order, you will have <strong>10 minutes</strong> to pay.
                        If payment is not made on time, the order will be automatically deleted.
                    </div>
                </OrderSummary>
            </div>
        </PageWrapper>
    );
};

export default CheckoutPage;