import { BrowserRouter, Routes, Route } from "react-router-dom";
import ScrollToTop from "../scroll/ScrollToTop";
import { Toaster } from "sonner";
import AdminLayout from "../../features/auth/admin/adminLayout/AdminLayout";
import GuestRoute from "../routes/GuestRoute";
import ProtectedRoute from "../routes/ProtectedRoute";
import AdminRoute from "../routes/AdminRoute";
import HomePage from "../../pages/home/HomePage/HomePage";
import AboutUsPage from "../../pages/info/AboutUsPage/AboutUsPage";
import DeliveryInfoPage from "../../pages/info/DeliveryInfoPage/DeliveryInfoPage";
import CatalogPage from "../../pages/catalog/CatalogPage/CatalogPage";
import ClotheDetailPage from "../../pages/catalog/ClotheDetailPage/ClotheDetailPage";
import PaymentSuccessfulPage from "../../pages/payment/PaymentSuccessfulPage/PaymentSuccessfulPage";
import PaymentCancelledPage from "../../pages/payment/PaymentCancelledPage/PaymentCancelledPage";
import ForbiddenPage from "../../pages/system/ForbiddenPage/ForbiddenPage";
import LoginPage from "../../pages/auth/LoginPage/LoginPage";
import RegisterPage from "../../pages/auth/RegisterPage/RegisterPage";
import ForgotPasswordPage from "../../pages/auth/ForgotPasswordPage/ForgotPasswordPage";
import ResetPasswordPage from "../../pages/auth/ResetPasswordPage/ResetPasswordPage";
import CartPage from "../../pages/cart/CartPage/CartPage";
import CheckoutPage from "../../pages/checkout/CheckoutPage/CheckoutPage";
import OrderDetailPage from "../../pages/account/orders/OrderDetailPage/OrderDetailPage";
import AccountLayout from "../../features/auth/user/accountLayout/AccountLayout";
import AccountProfilePage from "../../pages/account/profile/AccountProfilePage/AccountProfilePage";
import AccountOrderPage from "../../pages/account/orders/AccountOrderPage/AccountOrderPage";
import AccountReviewsPage from "../../pages/account/reviews/AccountReviewsPage/AccountReviewsPage";
import AdminDashboardPage from "../../pages/admin/dashboard/AdminDashboardPage";
import NotFoundPage from "../../pages/system/NotFoundPage/NotFoundPage";
import UserLayout from "../layout/userLayout/UserLayout.tsx";
import EmailVerifyRoute from "../routes/EmailVerifyRoute.tsx";
import VerifyEmailPage from "../../pages/auth/VerifyEmailPage/VerifyEmailPage.tsx";
import AdminOrdersPage from "../../pages/admin/orders/AdminOrdersPage.tsx";
import AdminReviewsPage from "../../pages/admin/reviews/AdminReviewsPage.tsx";
import AdminQuestionsPage from "../../pages/admin/questions/AdminQuestionsPage.tsx";
import AdminBrandsPage from "../../pages/admin/brand/AdminBrandsPage.tsx";
import AdminCollectionsPage from "../../pages/admin/collection/AdminCollectionsPage.tsx";
import AdminColorsPage from "../../pages/admin/colors/AdminColorsPage.tsx";
import AdminTagsPage from "../../pages/admin/tags/AdminTagsPage.tsx";
import AdminSizesPage from "../../pages/admin/sizes/AdminSizesPage.tsx";
import AdminDeliveryProvidersPage from "../../pages/admin/deliveryProvider/AdminDeliveryProvidersPage.tsx";
import AdminClothesPage from "../../pages/admin/clothes/page/AdminClothesPage.tsx";
import AdminClotheCreatePage from "../../pages/admin/clothes/create/AdminClotheCreatePage.tsx";
import AdminClotheEditPage from "../../pages/admin/clothes/update/AdminClotheEditPage.tsx";

export const AppRouter = () => {
    return (
        <BrowserRouter>
            <ScrollToTop />

            <Routes>
                <Route element={<UserLayout />}>
                    <Route path="/" element={<HomePage />} />
                    <Route path="/about-us" element={<AboutUsPage />} />
                    <Route path="/delivery-info" element={<DeliveryInfoPage />} />
                    <Route path="/catalog" element={<CatalogPage />} />
                    <Route path="/clothe/:slug/:colorSlug" element={<ClotheDetailPage />} />
                    <Route path="/payment/success" element={<PaymentSuccessfulPage />} />
                    <Route path="/payment/cancelled" element={<PaymentCancelledPage />} />
                    <Route path="/forbidden" element={<ForbiddenPage />} />

                    <Route element={<GuestRoute />}>
                        <Route path="/login" element={<LoginPage />} />
                        <Route path="/register" element={<RegisterPage />} />
                        <Route path="/forgot-password" element={<ForgotPasswordPage />} />
                    </Route>

                    <Route element={<EmailVerifyRoute />}>
                        <Route path="/email-verification" element={<VerifyEmailPage />} />
                    </Route>

                    <Route element={<ProtectedRoute />}>
                        <Route path="/cart" element={<CartPage />} />
                        <Route path="/checkout" element={<CheckoutPage />} />
                        <Route path="/order/:orderId" element={<OrderDetailPage />} />
                        <Route path="/reset-password" element={<ResetPasswordPage />} />

                        <Route path="/account" element={<AccountLayout />}>
                            <Route index element={<AccountProfilePage />} />
                            <Route path="orders" element={<AccountOrderPage />} />
                            <Route path="reviews" element={<AccountReviewsPage />} />
                        </Route>
                    </Route>

                    <Route path="*" element={<NotFoundPage />} />

                </Route>

                <Route element={<AdminRoute />}>
                    <Route path="/admin" element={<AdminLayout />}>
                        <Route index element={<AdminDashboardPage />} />
                        <Route path="orders" element={<AdminOrdersPage />} />
                        <Route path="reviews" element={<AdminReviewsPage />} />
                        <Route path="questions" element={<AdminQuestionsPage />} />
                        <Route path="brands" element={<AdminBrandsPage />} />
                        <Route path="collections" element={<AdminCollectionsPage />} />
                        <Route path="colors" element={<AdminColorsPage />} />
                        <Route path="tags" element={<AdminTagsPage />} />
                        <Route path="sizes" element={<AdminSizesPage />} />
                        <Route path="delivery" element={<AdminDeliveryProvidersPage />} />
                        <Route path="clothes" element={<AdminClothesPage  />} />
                        <Route path="clothes/create" element={<AdminClotheCreatePage  />} />
                        <Route path="clothes/edit/:id" element={<AdminClotheEditPage  />} />
                    </Route>
                </Route>

            </Routes>

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
        </BrowserRouter>
    );
};