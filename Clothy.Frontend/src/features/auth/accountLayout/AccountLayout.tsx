import {Outlet, Navigate} from "react-router-dom";
import styles from "./AccountLayout.module.css";
import EmailVerificationBanner from "../emailBanner/EmailVerificationBanner.tsx";
import AccountSidebar from "../accountSidebar/AccountSidebar.tsx";
import {useEffect, useState} from "react";
import {useAuthStore} from "../../../app/api/stores/authStore.ts";
import {Loader} from "lucide-react";
import Container from "../../../shared/layout/Container/Container.tsx";

const AccountLayout = () => {
    const {user, isAuthenticated} = useAuthStore();
    const [isInitialized, setIsInitialized] = useState(false);

    useEffect(() => {
        const timer = setTimeout(() => {
            setIsInitialized(true);
        }, 100);
        return () => clearTimeout(timer);
    }, []);

    if (!isInitialized) return <Loader/>;
    if (!isAuthenticated() || !user) return <Navigate to="/login" replace/>;

    return (
        <Container paddingY={20}>
            <EmailVerificationBanner emailVerified={user.emailVerified}/>
            <AccountSidebar/>
            <main className={styles.main}>
                <Outlet context={{user}}/>
            </main>
        </Container>
    );
};

export default AccountLayout;