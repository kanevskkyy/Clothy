import {Helmet} from "react-helmet";
import AuthLayout from "../../../features/auth/authLayout/AuthLayout.tsx";
import ConfirmEmail from "../../../features/forms/confirmEmail/ConfirmEmail.tsx";
import styles from "./VerifyEmailPage.module.css";
import {useEffect, useState} from "react";
import {useLocation} from "react-router-dom";
import {decodeJwt} from "../../../shared/lib/decodeJwt.ts";
import {useAuthStore} from "../../../app/api/stores/authStore.ts";
import Container from "../../../shared/layout/Container/Container.tsx";

const VerifyEmailPage = () => {
    const [userEmail, setUserEmail] = useState<string>();
    const accessToken = useAuthStore.getState().accessToken;
    const location = useLocation();
    const fromBanner = location.state?.fromBanner ?? false;

    useEffect(() => {
        if (!accessToken) return;
        const data = decodeJwt(accessToken);
        setUserEmail(data.email);
    }, [accessToken]);

    return (
        <Container paddingY={0}>
            <div className={styles.page}>
                <Helmet>
                    <title>Verify Your Email – Clothy</title>
                    <meta
                        name="description"
                        content="Confirm your email to activate your Clothy account and start shopping securely."
                    />
                </Helmet>

                <AuthLayout
                    title="Confirm your email"
                    subtitle={
                        <>
                            We have sent a confirmation letter to <br/>
                            <span className={styles.userEmail}>{userEmail}</span>
                        </>
                    }
                >
                    <ConfirmEmail fromBanner={fromBanner}/>
                </AuthLayout>
            </div>
        </Container>
    );
};

export default VerifyEmailPage;