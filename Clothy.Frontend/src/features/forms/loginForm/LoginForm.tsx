import { Link, useNavigate } from "react-router-dom";
import styles from "./LoginForm.module.css";
import { useState } from "react";
import { loginSchema, type LoginFormData } from "../../../app/schemas/loginSchema.ts";
import FormField from "../../../shared/form/FormField/FormField.tsx";
import Input from "../../../shared/ui/Input/Input.tsx";
import Button from "../../../shared/ui/Button/Button.tsx";
import { authApi } from "../../../app/api/authApi.ts";
import { toast } from "sonner";
import { getErrorMessage } from "../../../shared/lib/errorHandler.ts";
import { getZodFieldErrors } from "../../../shared/lib/getZodFieldErrors.ts";
import PasswordInput from "../../../shared/ui/PasswordInput/PasswordInput.tsx";

const LoginForm = () => {
    const navigate = useNavigate();

    const [formData, setFormData] = useState<LoginFormData>({
        email: "",
        password: "",
    });
    const [errors, setErrors] = useState<Partial<Record<keyof LoginFormData, string>>>({});
    const [isTryingLogin, setIsTryingLogin] = useState(false);

    const handleChange = (field: keyof LoginFormData) => (
        e: React.ChangeEvent<HTMLInputElement>
    ) => {
        setFormData((prev) => ({ ...prev, [field]: e.target.value }));
        if (errors[field]) setErrors((prev) => ({ ...prev, [field]: undefined }));
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsTryingLogin(true);

        const result = loginSchema.safeParse(formData);

        if (!result.success) {
            setErrors(getZodFieldErrors(result.error));
            setIsTryingLogin(false);
            return;
        }

        try {
            await authApi.loginAsync(formData);
            toast.success("Successfully signed in. Welcome back!");
            navigate("/account");
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setIsTryingLogin(false);
        }
    };

    return (
        <form className={styles.form} onSubmit={handleSubmit}>
            <FormField label="Email" htmlFor="email" required error={errors.email}>
                <Input
                    type="email"
                    id="email"
                    placeholder="enter your email"
                    value={formData.email}
                    onChange={handleChange("email")}
                    error={!!errors.email}
                />
            </FormField>

            <FormField label="Password" htmlFor="password" required error={errors.password}>
                <PasswordInput
                    id="password"
                    placeholder="enter your password"
                    value={formData.password}
                    onChange={handleChange("password")}
                    error={!!errors.password}
                />
            </FormField>

            <div className={styles.forgotPassword}>
                <Link to="/forgot-password" className={styles.forgotLink}>
                    Forgot your password?
                </Link>
            </div>

            <div className={styles.actions}>
                <Button type="submit" variant="primary" size="lg" disabled={isTryingLogin} fullWidth>
                    Sign in
                </Button>
                <div className={styles.login}>
                    <span>Don't have an account?</span>
                    <Link to="/register" className={styles.registerLink}>Register</Link>
                </div>
            </div>
        </form>
    );
};

export default LoginForm;