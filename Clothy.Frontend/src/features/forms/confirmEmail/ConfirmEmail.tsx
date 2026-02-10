import styles from "./ConfirmEmail.module.css";
import { ArrowRight } from "lucide-react";
import Button from "../../../shared/Button/Button.tsx";
import { useState, useEffect } from "react";
import StepList, {type Step} from "../../onboarding/stepList/StepList.tsx";

const ConfirmEmail = () => {
    const [resendTimer, setResendTimer] = useState(0);
    const [isResending, setIsResending] = useState(false);

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
            // #TODO: integrate API call

            await new Promise(resolve => setTimeout(resolve, 1000));

            setResendTimer(60);
        } catch (error) {
            console.error("Failed to resend email:", error);
        } finally {
            setIsResending(false);
        }
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

    const formatTime = (seconds: number) => {
        const mins = Math.floor(seconds / 60);
        const secs = seconds % 60;
        return `${mins}:${secs.toString().padStart(2, '0')}`;
    };

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
                    to="/account"
                    variant="primary"
                    icon={<ArrowRight size={20} />}
                    fullWidth
                >
                    Skip and continue
                </Button>
            </div>
        </div>
    );
};

export default ConfirmEmail;