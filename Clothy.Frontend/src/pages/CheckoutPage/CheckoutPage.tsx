import PageWrapper from "../../shared/PageWrapper/PageWrapper.tsx";
import OrderSummary from '../../features/checkout/orderSummary/OrderSummary.tsx';
import Button from "../../shared/Button/Button.tsx";
import CheckoutForm from '../../features/checkout/checkoutForm/CheckoutForm.tsx';
import styles from "./CheckoutPage.module.css";
import type { CheckoutFormData } from '../../app/schemas/checkoutFormSchema';

import {useState} from "react";

const CheckoutPage = () => {
    // TODO: connect to API get basketService, set totalPrice, totalItems
    // TODO: if basketService empty redirect to catalog

    const [totalPrice] = useState(14500);
    const totalItems = 6;

    const handleValidFormSubmit = (formData: CheckoutFormData) => {
        console.log('Order placed with valid data:', formData);
        // TODO: Connect to API to create order
    };

    const handlePlaceOrder = () => {
        const form = document.getElementById('checkout-form') as HTMLFormElement;
        if (form) {
            form.requestSubmit();
        }
    };

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
                        { label: 'Delivery', value: totalPrice > 1500 ? 'Free' : 'Paid' }
                    ]}
                    totalPrice={totalPrice}
                    buttons={
                        <>
                            <Button
                                variant="primary"
                                size="lg"
                                fullWidth
                                onClick={handlePlaceOrder}
                            >
                                Create an order
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