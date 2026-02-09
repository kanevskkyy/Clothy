import { Link } from "react-router-dom";
import styles from "./LoginForm.module.css";
import { useState } from "react";
import {loginSchema, type LoginFormData } from "../../app/schemas/loginSchema";
import FormField from "../../shared/FormField/FormField";
import Input from "../../shared/Input/Input";
import PasswordInput from "../passwordInput/PasswordInput";
import Button from "../../shared/Button/Button.tsx";

const LoginForm = () => {
    const [formData, setFormData] = useState<LoginFormData>({
        email: "",
        password: "",
    });
    const [errors, setErrors] = useState<Partial<Record<keyof LoginFormData, string>>>({});

    const handleChange = (field: keyof LoginFormData) => (
        e: React.ChangeEvent<HTMLInputElement>
    ) => {
        setFormData((prev) => ({ ...prev, [field]: e.target.value }));
        if (errors[field]) {
            setErrors((prev) => ({ ...prev, [field]: undefined }));
        }
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();

        const result = loginSchema.safeParse(formData);

        if (!result.success) {
            const fieldErrors: Partial<Record<keyof LoginFormData, string>> = {};
            result.error.issues.forEach((issue) => {
                const field = issue.path[0] as keyof LoginFormData;
                fieldErrors[field] = issue.message;
            });
            setErrors(fieldErrors);
            return;
        }

        // TODO: Connect to API
        console.log("Login successful:", result.data);
    };

    return (
        <form className={styles.form} onSubmit={handleSubmit}>
            <FormField
                label="Email"
                htmlFor="email"
                required
                error={errors.email}
            >
                <Input
                    type="email"
                    id="email"
                    placeholder="enter your email"
                    value={formData.email}
                    onChange={handleChange("email")}
                    error={!!errors.email}
                />
            </FormField>

            <FormField
                label="Password"
                htmlFor="password"
                required
                error={errors.password}
            >
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
                <Button type="submit" variant="primary" size="lg" fullWidth>
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