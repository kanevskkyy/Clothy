import React, {useState} from 'react';
import Button from "../../shared/Button/Button.tsx";
import {Link} from "react-router-dom";
import {type ForgotPasswordFormData, forgotPasswordSchema} from "../../app/schemas/forgotPasswordSchema.ts";
import FormField from '../../shared/FormField/FormField.tsx';
import Input from '../../shared/Input/Input.tsx';
import styles from "./ForgotPasswordForm.module.css";

const ForgotPasswordForm = () => {
    const [formData, setFormData] = useState<ForgotPasswordFormData>({
        email: "",
    });

    const [errors, setErrors] = useState<Partial<Record<keyof ForgotPasswordFormData, string>>>({});

    const handleChange = (field: keyof ForgotPasswordFormData) => (
        e: React.ChangeEvent<HTMLInputElement>
    ) => {
        setFormData((prev) => ({ ...prev, [field]: e.target.value }));
        if (errors[field]) {
            setErrors((prev) => ({ ...prev, [field]: undefined }));
        }
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();

        const result = forgotPasswordSchema.safeParse(formData);

        if (!result.success) {
            const fieldErrors: Partial<Record<keyof ForgotPasswordFormData, string>> = {};
            result.error.issues.forEach((issue) => {
                const field = issue.path[0] as keyof ForgotPasswordFormData;
                fieldErrors[field] = issue.message;
            });
            setErrors(fieldErrors);
            return;
        }

        // TODO: Connect to API
        console.log("Send forgot password link:", result.data);
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

            <div className={styles.actions}>
                <Button type="submit" variant="primary" size="lg" fullWidth>
                    Send instructions
                </Button>
                <div className={styles.login}>
                    <Link to="/login" className={styles.loginLink}>Return to login</Link>
                </div>
            </div>
        </form>
    );
};

export default ForgotPasswordForm;