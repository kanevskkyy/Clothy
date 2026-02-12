import styles from "./EmailVerificationBanner.module.css";
import Button from "../../../shared/Button/Button.tsx";
import { AlertCircle } from "lucide-react";

interface EmailVerificationBannerProps {
    emailVerified: boolean;
}

const EmailVerificationBanner = ({ emailVerified }: EmailVerificationBannerProps) => {
    const handleVerify = () => {
        // TODO: Connect to API
        // TODO: send request to API, and then redirect to email verification

        console.log("Verify email clicked");
    };

    if (emailVerified) {
        return null;
    }

    return (
        <div className={styles.banner}>
            <div className={styles.content}>
                <div className={styles.iconWrapper}>
                    <AlertCircle size={20} />
                </div>
                <div className={styles.textWrapper}>
                    <p className={styles.title}>Confirm your email</p>
                    <p className={styles.message}>
                        Your email has not been confirmed yet. Please confirm it to be able to place orders.
                    </p>
                </div>
                <div className={styles.actions}>
                    <Button onClick={handleVerify} variant="primary" size="sm">
                        Confirm your email
                    </Button>
                </div>
            </div>
        </div>
    );
};

export default EmailVerificationBanner;