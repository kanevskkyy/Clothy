import styles from "./PaymentSuccessful.module.css";
import { Home, ArrowRight, Package } from "lucide-react";
import Button from "../../shared/Button/Button.tsx";
import StepList, {type Step} from "../stepList/StepList.tsx";

const PaymentSuccessful = () => {
    const orderSteps: Step[] = [
        {
            stepNumber: 1,
            stepInstruction: "We are processing your order and preparing the goods for shipment",
        },
        {
            stepNumber: 2,
            stepInstruction: "You will receive tracking details from the delivery provider you selected to follow your order status",
        },
        {
            stepNumber: 3,
            stepInstruction: "Pick up your order at the selected branch",
        },
    ];

    return (
        <div className={styles.wrapper}>
            <div className={styles.sectionHeader}>
                <Package size={24} />
                <h2 className={styles.sectionTitle}>What's next?</h2>
            </div>

            <StepList steps={orderSteps} circleColor="#DCFCE7" numberColor="#20A34A" />

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
                    to="/account"
                    variant="primary"
                    icon={<ArrowRight size={20} />}
                    fullWidth
                >
                    My Orders
                </Button>
            </div>
        </div>
    );
};

export default PaymentSuccessful;