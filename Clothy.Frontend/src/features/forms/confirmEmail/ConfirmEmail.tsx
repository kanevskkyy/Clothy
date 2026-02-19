import styles from "./ConfirmEmail.module.css";
import { ArrowRight } from "lucide-react";
import Button from "../../../shared/Button/Button.tsx";
import { useState, useEffect } from "react";
import StepList, { type Step } from "../../onboarding/stepList/StepList.tsx";
import { useAuthStore } from "../../../app/api/stores/authStore.ts";
import { authApi, type IResendVerificationEmailRequest } from "../../../app/api/authApi.ts";
import { toast } from "sonner";
import { getErrorMessage } from "../../../shared/utils/errorHandler.ts";
import { useNavigate } from "react-router-dom";
import {formatTime} from "../../../shared/utils/formatTime.ts";

interface ConfirmEmailProps {
    fromBanner?: boolean;
}

const ConfirmEmail = ({ fromBanner = false }: ConfirmEmailProps) => {
    const [resendTimer, setResendTimer] = useState(0);
    const [isResending, setIsResending] = useState(false);
    const user = useAuthStore((state) => state.user);
    const navigate = useNavigate();

    useEffect(() => {
        if (resendTimer > 0) {
            const interval = setInterval(() => {
                setResendTimer((prev) => prev - 1);
            }, 1000);

            return () => clearInterval(interval);
        }
    }, [resendTimer]);

    const handleResendEmail = async () => {
        setIsResending(true);

        try {
            if (!user?.email) {
                toast.error("Email not found");
                return;
            }

            const body: IResendVerificationEmailRequest = {
                email: user.email,
            };

            await authApi.resendVerificationEmailAsync(body);
            toast.success("We have successfully sent you a confirmation email");
            setResendTimer(60);
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setIsResending(false);
        }
    };

    const handleSkip = () => {
        navigate(fromBanner ? "/account" : "/");
    };

    const confirmEmailSteps: Step[] = [
        {
            stepNumber: 1,
            stepInstruction: "Open the letter from us in your mailbox",
        },
        {
            stepNumber: 2,
            stepInstruction: "Click on the link to confirm",
        },
        {
            stepNumber: 3,
            stepInstruction: "All done! You can now use your account",
        },
    ];

    return (
        <div className={styles.wrapper}>
            <StepList steps={confirmEmailSteps} circleColor="#070707" numberColor="#fff" />

            <div className={styles.emailNotReceived}>
                Didn't receive the email? Check your spam folder
            </div>

            <div className={styles.buttonGroup}>
                <Button
                    variant="outline"
                    fullWidth
                    onClick={handleResendEmail}
                    disabled={resendTimer > 0 || isResending}
                >
                    {isResending
                        ? "Sending..."
                        : resendTimer > 0
                            ? `Resend in ${formatTime(resendTimer)}`
                            : "Resend email"}
                </Button>

                <Button
                    variant="primary"
                    icon={<ArrowRight size={20} />}
                    fullWidth
                    onClick={handleSkip}
                >
                    {fromBanner ? "Back to account" : "Skip and continue"}
                </Button>
            </div>
        </div>
    );
};

export default ConfirmEmail;