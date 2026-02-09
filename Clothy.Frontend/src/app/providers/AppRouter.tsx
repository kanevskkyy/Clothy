import {BrowserRouter, Route, Routes} from 'react-router-dom';
import HomePage from '../../pages/HomePage/HomePage';
import AboutUsPage from '../../pages/AboutUsPage/AboutUsPage.tsx';
import Header from '../../features/header/Header';
import Footer from '../../features/footer/Footer';
import styles from './AppRouter.module.css';
import DeliveryInfoPage from "../../pages/DeliveryInfoPage/DeliveryInfoPage.tsx";
import ScrollToTop from "../scroll/ScrollToTop.tsx";
import ClotheDetailPage from "../../pages/ClotheDetailPage/ClotheDetailPage.tsx";
import CartPage from "../../pages/CartPage/CartPage.tsx";
import NotFoundPage from "../../pages/NotFoundPage/NotFoundPage.tsx";
import CatalogPage from "../../pages/CatalogPage/CatalogPage.tsx";
import LoginPage from "../../pages/LoginPage/LoginPage.tsx";
import RegisterPage from "../../pages/RegisterPage/RegisterPage.tsx";
import ForgotPasswordPage from "../../pages/ForgotPasswordPage/ForgotPasswordPage.tsx";
import VerifyEmailPage from "../../pages/VerifyEmailPage/VerifyEmailPage.tsx";
import PaymentSuccessfulPage from "../../pages/PaymentSuccessfulPage/PaymentSuccessfulPage.tsx";
import PaymentCancelledPage from "../../pages/PaymentCancelledPage/PaymentCancelledPage.tsx";

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
                        <Route path="/clothe/:slug/:colorSlug" element={<ClotheDetailPage/>}/>
                        <Route path="/cart" element={<CartPage/>}/>
                        <Route path="/catalog" element={<CatalogPage/>}/>
                        <Route path="/login" element={<LoginPage/>}/>
                        <Route path="/register" element={<RegisterPage/>}/>
                        <Route path="/forgot-password" element={<ForgotPasswordPage/>}/>
                        <Route path="/email-verification" element={<VerifyEmailPage/>}/>
                        <Route path="/payment/success" element={<PaymentSuccessfulPage/>}/>
                        <Route path="/payment/cancelled" element={<PaymentCancelledPage/>}/>
                        <Route path="*" element={<NotFoundPage/>}/>
                    </Routes>
                </main>

                <Footer/>
            </div>
        </BrowserRouter>
    );
};