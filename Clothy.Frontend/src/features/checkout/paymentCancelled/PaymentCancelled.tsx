import StepList, {type Step} from "../../onboarding/stepList/StepList.tsx";
import { Home, Info, RotateCcw } from "lucide-react";
import styles from "./PaymentCancelled.module.css";
import Button from "../../../shared/Button/Button.tsx";

const PaymentCancelled = () => {
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

    const handleRetryButtonClick = () => {
        // #TODO: Conect to API
        // find paid id from query

        console.log('Retrying...');
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
                >
                    Try again
                </Button>
            </div>
        </div>
    );
};

export default PaymentCancelled;