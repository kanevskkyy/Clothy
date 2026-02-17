import {BrowserRouter, Route, Routes } from "react-router-dom";
import NotFoundPage from "../../pages/NotFoundPage/NotFoundPage";
import styles from "./AppRouter.module.css";
import ScrollToTop from "../scroll/ScrollToTop";
import Header from "../../features/layout/header/Header";
import HomePage from "../../pages/HomePage/HomePage";
import AboutUsPage from "../../pages/AboutUsPage/AboutUsPage";
import CatalogPage from "../../pages/CatalogPage/CatalogPage";
import ClotheDetailPage from "../../pages/ClotheDetailPage/ClotheDetailPage";
import PaymentSuccessfulPage from "../../pages/PaymentSuccessfulPage/PaymentSuccessfulPage";
import PaymentCancelledPage from "../../pages/PaymentCancelledPage/PaymentCancelledPage.tsx";
import ForbiddenPage from "../../pages/ForbiddenPage/ForbiddenPage.tsx";
import DeliveryInfoPage from "../../pages/DeliveryInfoPage/DeliveryInfoPage.tsx";
import GuestRoute from "../routes/GuestRoute.tsx";
import LoginPage from "../../pages/LoginPage/LoginPage.tsx";
import RegisterPage from "../../pages/RegisterPage/RegisterPage.tsx";
import EmailVerifyRoute from "../routes/EmailVerifyRoute.tsx";
import VerifyEmailPage from "../../pages/VerifyEmailPage/VerifyEmailPage.tsx";
import ForgotPasswordPage from "../../pages/ForgotPasswordPage/ForgotPasswordPage.tsx";
import ProtectedRoute from "../routes/ProtectedRoute.tsx";
import CartPage from "../../pages/CartPage/CartPage.tsx";
import CheckoutPage from "../../pages/CheckoutPage/CheckoutPage.tsx";
import OrderDetailPage from "../../pages/OrderDetailPage/OrderDetailPage.tsx";
import AccountLayout from "../../features/auth/accountLayout/AccountLayout.tsx";
import AccountProfilePage from "../../pages/AccountProfilePage/AccountProfilePage.tsx";
import {Toaster} from "sonner";
import Footer from "../../features/layout/footer/Footer.tsx";
import AccountReviewsPage from "../../pages/AccountReviewsPage/AccountReviewsPage.tsx";
import AccountOrderPage from "../../pages/AccountOrderPage/AccountOrderPage.tsx";
import ResetPasswordPage from "../../pages/ResetPasswordPage/ResetPasswordPage.tsx";

export const AppRouter = () => {
    return (
        <BrowserRouter>
            <ScrollToTop/>
            <div className={styles.appWrapper}>
                <Header/>

                <main className={styles.mainContent}>
                    <Routes>
                        <Route path="/" element={<HomePage/>}/>
                        <Route path="/about-us" element={<AboutUsPage/>}/>
                        <Route path="/delivery-info" element={<DeliveryInfoPage/>}/>
                        <Route path="/catalog" element={<CatalogPage/>}/>
                        <Route path="/clothe/:slug/:colorSlug" element={<ClotheDetailPage/>}/>
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

                            <Route path="/account" element={<AccountLayout />}>
                                <Route index element={<AccountProfilePage />} />
                                <Route path="orders" element={<AccountOrderPage />} />
                                <Route path="reviews" element={<AccountReviewsPage />} />
                            </Route>
                        </Route>

                        <Route path="*" element={<NotFoundPage/>}/>
                    </Routes>
                </main>

                <Toaster
                    position="bottom-right"
                    expand
                    richColors
                    closeButton
                    toastOptions={{
                        duration: 4000,
                        style: { zIndex: 9999 },
                    }}
                />

                <Footer/>
            </div>
        </BrowserRouter>
    );
};