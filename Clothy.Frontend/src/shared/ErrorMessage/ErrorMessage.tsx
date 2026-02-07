import styles from "./ErrorMessage.module.css";

interface ErrorMessageProps {
    message?: string;
}

const ErrorMessage = ({ message }: ErrorMessageProps) => {
    if (!message) return null;

    return <div className={styles.error}>{message}</div>;
};

export default ErrorMessage;