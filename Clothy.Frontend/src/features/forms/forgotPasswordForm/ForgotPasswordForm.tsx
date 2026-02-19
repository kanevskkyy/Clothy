import React, { useState, useEffect } from 'react';
import Button from "../../../shared/Button/Button.tsx";
import { Link } from "react-router-dom";
import { type ForgotPasswordFormData, forgotPasswordSchema } from "../../../app/schemas/forgotPasswordSchema.ts";
import FormField from '../../../shared/FormField/FormField.tsx';
import Input from '../../../shared/Input/Input.tsx';
import styles from "./ForgotPasswordForm.module.css";
import { authApi } from "../../../app/api/authApi.ts";
import { toast } from "sonner";
import { getErrorMessage } from "../../../shared/utils/errorHandler.ts";
import {formatTime} from "../../../shared/utils/formatTime.ts";

const ForgotPasswordForm = () => {
    const [resendTimer, setResendTimer] = useState(0);
    const [isSubmitting, setIsSubmitting] = useState(false);

    const [formData, setFormData] = useState<ForgotPasswordFormData>({
        email: "",
    });

    const [errors, setErrors] = useState<Partial<Record<keyof ForgotPasswordFormData, string>>>({});

    useEffect(() => {
        if (resendTimer > 0) {
            const interval = setInterval(() => {
                setResendTimer((prev) => prev - 1);
            }, 1000);

            return () => clearInterval(interval);
        }
    }, [resendTimer]);

    const handleChange = (field: keyof ForgotPasswordFormData) => (
        e: React.ChangeEvent<HTMLInputElement>
    ) => {
        setFormData((prev) => ({ ...prev, [field]: e.target.value }));
        if (errors[field]) {
            setErrors((prev) => ({ ...prev, [field]: undefined }));
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
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

        setIsSubmitting(true);
        try {
            await authApi.forgotPasswordAsync(formData);
            toast.success("Instructions have been sent to your email");
            setResendTimer(60);
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setIsSubmitting(false);
        }
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
                <Button
                    type="submit"
                    variant="primary"
                    size="lg"
                    fullWidth
                    disabled={resendTimer > 0 || isSubmitting}
                >
                    {isSubmitting
                        ? "Sending..."
                        : resendTimer > 0
                            ? `Resend in ${formatTime(resendTimer)}`
                            : "Send instructions"}
                </Button>
                <div className={styles.login}>
                    <Link to="/login" className={styles.loginLink}>Return to login</Link>
                </div>
            </div>
        </form>
    );
};

export default ForgotPasswordForm;