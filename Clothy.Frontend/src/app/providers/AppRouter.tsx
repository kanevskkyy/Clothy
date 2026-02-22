import {BrowserRouter, Route, Routes} from "react-router-dom";
import NotFoundPage from "../../pages/system/NotFoundPage/NotFoundPage";
import styles from "./AppRouter.module.css";
import ScrollToTop from "../scroll/ScrollToTop";
import Header from "../../features/layout/header/Header";
import HomePage from "../../pages/home/HomePage/HomePage";
import AboutUsPage from "../../pages/info/AboutUsPage/AboutUsPage";
import CatalogPage from "../../pages/catalog/CatalogPage/CatalogPage";
import ClotheDetailPage from "../../pages/catalog/ClotheDetailPage/ClotheDetailPage";
import PaymentSuccessfulPage from "../../pages/payment/PaymentSuccessfulPage/PaymentSuccessfulPage";
import PaymentCancelledPage from "../../pages/payment/PaymentCancelledPage/PaymentCancelledPage.tsx";
import ForbiddenPage from "../../pages/system/ForbiddenPage/ForbiddenPage.tsx";
import DeliveryInfoPage from "../../pages/info/DeliveryInfoPage/DeliveryInfoPage.tsx";
import GuestRoute from "../routes/GuestRoute.tsx";
import LoginPage from "../../pages/auth/LoginPage/LoginPage.tsx";
import RegisterPage from "../../pages/auth/RegisterPage/RegisterPage.tsx";
import EmailVerifyRoute from "../routes/EmailVerifyRoute.tsx";
import VerifyEmailPage from "../../pages/auth/VerifyEmailPage/VerifyEmailPage.tsx";
import ForgotPasswordPage from "../../pages/auth/ForgotPasswordPage/ForgotPasswordPage.tsx";
import ProtectedRoute from "../routes/ProtectedRoute.tsx";
import CartPage from "../../pages/cart/CartPage/CartPage.tsx";
import CheckoutPage from "../../pages/checkout/CheckoutPage/CheckoutPage.tsx";
import OrderDetailPage from "../../pages/account/orders/OrderDetailPage/OrderDetailPage.tsx";
import AccountLayout from "../../features/auth/accountLayout/AccountLayout.tsx";
import AccountProfilePage from "../../pages/account/profile/AccountProfilePage/AccountProfilePage.tsx";
import {Toaster} from "sonner";
import Footer from "../../features/layout/footer/Footer.tsx";
import AccountReviewsPage from "../../pages/account/reviews/AccountReviewsPage/AccountReviewsPage.tsx";
import AccountOrderPage from "../../pages/account/orders/AccountOrderPage/AccountOrderPage.tsx";
import ResetPasswordPage from "../../pages/auth/ResetPasswordPage/ResetPasswordPage.tsx";

export const AppRouter = () => {
    return (
        <BrowserRouter>
            <ScrollToTop/>
            <div className={styles.appWrapper}>
                <Header/>

                <main className={styles.mainContent}>
                    <Routes>
                        <Route path="/" element={<HomePage/>}/>
                        <Route path="/clothe/:slug/:colorSlug" element={<ClotheDetailPage/>}/>

                        <Route>
                            <Route path="/about-us" element={<AboutUsPage/>}/>
                            <Route path="/delivery-info" element={<DeliveryInfoPage/>}/>
                            <Route path="/catalog" element={<CatalogPage/>}/>
                            <Route path="/payment/success" element={<PaymentSuccessfulPage/>}/>
                            <Route path="/payment/cancelled" element={<PaymentCancelledPage/>}/>
                            <Route path="/forbidden" element={<ForbiddenPage/>}/>

                            <Route element={<GuestRoute/>}>
                                <Route path="/login" element={<LoginPage/>}/>
                                <Route path="/register" element={<RegisterPage/>}/>
                                <Route path="/forgot-password" element={<ForgotPasswordPage/>}/>
                            </Route>

                            <Route element={<EmailVerifyRoute/>}>
                                <Route path="/email-verification" element={<VerifyEmailPage/>}/>
                            </Route>

                            <Route element={<ProtectedRoute/>}>
                                <Route path="/cart" element={<CartPage/>}/>
                                <Route path="/checkout" element={<CheckoutPage/>}/>
                                <Route path="/order/:orderId" element={<OrderDetailPage/>}/>
                                <Route path="/reset-password" element={<ResetPasswordPage/>}/>

                                <Route path="/account" element={<AccountLayout/>}>
                                    <Route index element={<AccountProfilePage/>}/>
                                    <Route path="orders" element={<AccountOrderPage/>}/>
                                    <Route path="reviews" element={<AccountReviewsPage/>}/>
                                </Route>
                            </Route>

                            <Route path="*" element={<NotFoundPage/>}/>
                        </Route>

                    </Routes>
                </main>

                <Toaster
                    position="bottom-right"
                    expand
                    richColors
                    closeButton
                    toastOptions={{
                        duration: 4000,
                        style: {zIndex: 9999},
                    }}
                />

                <Footer/>
            </div>
        </BrowserRouter>
    );
};