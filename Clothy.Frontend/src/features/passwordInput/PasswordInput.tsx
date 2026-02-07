import { useState } from "react";
import { Eye, EyeOff } from "lucide-react";
import styles from "./PasswordInput.module.css";
import Input from "../../shared/Input/Input.tsx";

interface PasswordInputProps {
    id: string;
    placeholder?: string;
    value: string;
    onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
    error?: boolean;
}

const PasswordInput = ({ id, placeholder, value, onChange, error }: PasswordInputProps) => {
    const [showPassword, setShowPassword] = useState(false);

    return (
        <div className={styles.wrapper}>
            <Input
                type={showPassword ? "text" : "password"}
                id={id}
                placeholder={placeholder}
                value={value}
                onChange={onChange}
                error={error}
                className={styles.input}
            />
            <button
                type="button"
                className={styles.toggle}
                onClick={() => setShowPassword(!showPassword)}
                aria-label={showPassword ? "Hide password" : "Show password"}
            >
                {showPassword ? <EyeOff size={20} /> : <Eye size={20} />}
            </button>
        </div>
    );
};

export default PasswordInput;