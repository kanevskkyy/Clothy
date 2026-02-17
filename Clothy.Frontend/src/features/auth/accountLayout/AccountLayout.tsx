import { Outlet, Navigate } from "react-router-dom";
import styles from "./AccountLayout.module.css";
import AccountHeader from "../accountHeader/AccountHeader.tsx";
import EmailVerificationBanner from "../emailBanner/EmailVerificationBanner.tsx";
import AccountSidebar from "../accountSidebar/AccountSidebar.tsx";
import PageWrapper from "../../../shared/PageWrapper/PageWrapper.tsx";
import { useEffect, useState } from "react";
import { useAuthStore } from "../../../app/api/stores/authStore.ts";
import {Loader} from "lucide-react";

const AccountLayout = () => {
    const { user, isAuthenticated } = useAuthStore();
    const [isInitialized, setIsInitialized] = useState(false);

    useEffect(() => {
        const timer = setTimeout(() => {
            setIsInitialized(true);
        }, 100);

        return () => clearTimeout(timer);
    }, []);

    if (!isInitialized) {
        return <Loader />
    }

    if (!isAuthenticated() || !user) {
        return <Navigate to="/login" replace />;
    }

    return (
        <PageWrapper>
            <div className={styles.container}>
                <EmailVerificationBanner emailVerified={user.emailVerified} />
                <AccountHeader user={user} />
                <div className={styles.content}>
                    <AccountSidebar />
                    <main className={styles.main}>
                        <Outlet context={{ user }} />
                    </main>
                </div>
            </div>
        </PageWrapper>
    );
};

export default AccountLayout;