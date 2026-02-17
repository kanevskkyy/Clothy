import { Helmet } from "react-helmet";
import AuthLayout from "../../features/forms/authLayout/AuthLayout.tsx";
import PageWrapper from "../../shared/PageWrapper/PageWrapper.tsx";
import ConfirmEmail from "../../features/forms/confirmEmail/ConfirmEmail.tsx";
import styles from "./VerifyEmailPage.module.css";

const VerifyEmailPage = () => {
    /*
    * TODO: With the access token, extract the confirmed email, check if it is confirmed,
    *  and if so, redirect immediately to the directory.
    *  We also extract the email and display it.
    * */
    const userEmail = "test@gmail.com";

    return (
        <PageWrapper>
            <div className={styles.page}>
                <Helmet>
                    <title>Verify Your Email – Clothy</title>
                    <meta name="description" content="Confirm your email to activate your Clothy account and start shopping securely."/>
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
                    <ConfirmEmail />
                </AuthLayout>
            </div>
        </PageWrapper>
    );
};

export default VerifyEmailPage;