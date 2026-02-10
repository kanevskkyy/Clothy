import styles from "./StepList.module.css";

export interface Step {
    stepNumber: number;
    stepInstruction: string;
}

interface StepListProps {
    steps: Step[];
    circleColor?: string;
    numberColor?: string;
}

const StepList = ({ steps, circleColor, numberColor }: StepListProps) => {
    return (
        <div className={styles.stepList}>
            {steps.map((step) => (
                <div key={step.stepNumber} className={styles.stepItem}>
                    <div
                        className={styles.stepNumber}
                        style={{
                            backgroundColor: circleColor,
                            color: numberColor
                        }}
                    >
                        {step.stepNumber}
                    </div>
                    <div className={styles.stepInstruction}>{step.stepInstruction}</div>
                </div>
            ))}
        </div>
    );
};

export default StepList;