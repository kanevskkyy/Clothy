import StepList, {type Step} from "../../onboarding/stepList/StepList.tsx";
import { Home, Info, RotateCcw } from "lucide-react";
import styles from "./PaymentCancelled.module.css";
import Button from "../../../shared/ui/Button/Button.tsx";
import {paymentApi} from "../../../app/api/paymentApi.ts";
import {getErrorMessage} from "../../../shared/lib/errorHandler.ts";
import {toast} from "sonner";
import {useState} from "react";

interface PaymentProps {
    paymentId: string;
}

const PaymentCancelled: React.FC<PaymentProps> = ( { paymentId } ) => {
    const [isRetrying, setIsRetrying] = useState(false);

    const paymentFailureSteps: Step[] = [
        {
            stepNumber: 1,
            stepInstruction: "The payment was cancelled by the user",
        },
        {
            stepNumber: 2,
            stepInstruction: "The payment session expired after 10 minutes",
        },
        {
            stepNumber: 3,
            stepInstruction: "There were insufficient funds to complete the payment",
        },
        {
            stepNumber: 4,
            stepInstruction: "A technical issue occurred while processing the transaction",
        },
    ];

    const handleRetryButtonClick = async () => {
        try{
            setIsRetrying(true)
            const paymentResponse = await paymentApi.retryPaymentAsync(paymentId);
            window.location.href = paymentResponse.paymentUrl;
        }
        catch (error) {
            toast.error(getErrorMessage(error));
        }
        finally {
            setIsRetrying(false);
        }
    };

    return (
        <div className={styles.wrapper}>
            <div className={styles.sectionHeader}>
                <Info size={24} />
                <h2 className={styles.sectionTitle}>Possible causes</h2>
            </div>

            <StepList steps={paymentFailureSteps} circleColor="#EF4444" numberColor="#fff" />

            <div className={styles.buttonGroup}>
                <Button
                    to="/"
                    variant="outline"
                    icon={<Home size={20} />}
                    fullWidth
                >
                    Home
                </Button>

                <Button
                    variant="primary"
                    icon={<RotateCcw  size={20} />}
                    onClick={handleRetryButtonClick}
                    fullWidth
                    disabled={isRetrying}
                >
                    {isRetrying ? (
                        "Retrying..."
                    ) : "Try again"}
                </Button>
            </div>
        </div>
    );
};

export default PaymentCancelled;