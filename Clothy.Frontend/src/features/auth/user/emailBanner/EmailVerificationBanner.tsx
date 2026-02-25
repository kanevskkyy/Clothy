import styles from "./EmailVerificationBanner.module.css";
import Button from "../../../../shared/ui/Button/Button.tsx";
import { AlertCircle, RefreshCw } from "lucide-react";
import { useAuthStore } from "../../../../app/api/stores/authStore.ts";
import { toast } from "sonner";
import { authApi, type IResendVerificationEmailRequest } from "../../../../app/api/authApi.ts";
import { getErrorMessage } from "../../../../shared/lib/errorHandler.ts";
import { useState } from "react";
import { useNavigate } from "react-router-dom";

interface EmailVerificationBannerProps {
    emailVerified?: boolean;
}

const EmailVerificationBanner = ({ emailVerified }: EmailVerificationBannerProps) => {
    const { user, setUser } = useAuthStore();
    const navigate = useNavigate();
    const [isSending, setIsSending] = useState(false);
    const [isRefreshing, setIsRefreshing] = useState(false);
    const [emailSent, setEmailSent] = useState(false);

    const handleSendEmail = async () => {
        setIsSending(true);

        try {
            if (!user?.email) {
                toast.error("Email not found in user profile");
                return;
            }

            const body: IResendVerificationEmailRequest = {
                email: user.email,
            };

            await authApi.resendVerificationEmailAsync(body);
            toast.success("Verification email sent! Please check your inbox.");
            setEmailSent(true);
            navigate("/email-verification", { state: { fromBanner: true } });
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setIsSending(false);
        }
    };

    const handleRefreshUser = async () => {
        setIsRefreshing(true);

        try {
            const updatedUser = await authApi.getInfoAboutMeAsync();
            setUser(updatedUser);

            if (updatedUser.emailVerified) {
                toast.success("Email verified successfully!");
            } else {
                toast.info("Email not verified yet. Please check your inbox and click the verification link.");
            }
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setIsRefreshing(false);
        }
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
                        {emailSent
                            ? "We've sent you a verification email. Please check your inbox and click the link."
                            : "Your email has not been confirmed yet. Please confirm it to be able to place orders."}
                    </p>
                </div>
                <div className={styles.actions}>
                    {!emailSent ? (
                        <Button
                            onClick={handleSendEmail}
                            variant="primary"
                            size="sm"
                            disabled={isSending}
                        >
                            {isSending ? "Sending..." : "Send verification email"}
                        </Button>
                    ) : (
                        <Button
                            onClick={handleRefreshUser}
                            variant="primary"
                            size="sm"
                            disabled={isRefreshing}
                            icon={<RefreshCw size={16} />}
                        >
                            {isRefreshing ? "Checking..." : "I verified my email"}
                        </Button>
                    )}
                </div>
            </div>
        </div>
    );
};

export default EmailVerificationBanner;