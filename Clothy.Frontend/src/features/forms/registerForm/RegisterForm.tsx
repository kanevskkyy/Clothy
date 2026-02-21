import React, { useState } from 'react';
import { type RegisterFormData, registerSchema } from "../../../app/schemas/registerSchema.ts";
import FormField from "../../../shared/form/FormField/FormField.tsx";
import styles from "./RegisterForm.module.css";
import Input from "../../../shared/ui/Input/Input.tsx";
import { Link, useNavigate } from "react-router-dom";
import Button from "../../../shared/ui/Button/Button.tsx";
import { authApi } from "../../../app/api/authApi.ts";
import { toast } from "sonner";
import { getErrorMessage } from "../../../shared/lib/errorHandler.ts";
import { getZodFieldErrors } from "../../../shared/lib/getZodFieldErrors.ts";
import PasswordInput from "../../../shared/ui/PasswordInput/PasswordInput.tsx";

const RegisterForm = () => {
    const navigate = useNavigate();

    const [formData, setFormData] = useState<RegisterFormData>({
        email: "",
        password: "",
        firstName: "",
        lastName: "",
        phoneNumber: "",
    });
    const [errors, setErrors] = useState<Partial<Record<keyof RegisterFormData, string>>>({});
    const [isTryingRegister, setIsTryingRegister] = useState(false);

    const handleChange = (field: keyof RegisterFormData) => (
        e: React.ChangeEvent<HTMLInputElement>
    ) => {
        setFormData((prev) => ({ ...prev, [field]: e.target.value }));
        if (errors[field]) setErrors((prev) => ({ ...prev, [field]: undefined }));
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsTryingRegister(true);

        const result = registerSchema.safeParse(formData);

        if (!result.success) {
            setErrors(getZodFieldErrors(result.error));
            setIsTryingRegister(false);
            return;
        }

        try {
            await authApi.registerAsync(formData);
            toast.success("Registration successful!");
            navigate("/email-verification");
        } catch (error) {
            const msg = getErrorMessage(error);
            if (msg?.includes("User exists")) {
                toast.error("An account with this email already exists.");
            } else {
                toast.error("Registration failed. Please try again.");
            }
        } finally {
            setIsTryingRegister(false);
        }
    };

    return (
        <form className={styles.form} onSubmit={handleSubmit}>
            <div className={styles.formGroupWrapper}>
                <FormField label="First name" htmlFor="firstName" required error={errors.firstName}>
                    <Input
                        type="text"
                        id="firstName"
                        placeholder="enter your first name"
                        value={formData.firstName}
                        onChange={handleChange("firstName")}
                        error={!!errors.firstName}
                    />
                </FormField>

                <FormField label="Last name" htmlFor="lastName" required error={errors.lastName}>
                    <Input
                        type="text"
                        id="lastName"
                        placeholder="enter your last name"
                        value={formData.lastName}
                        onChange={handleChange("lastName")}
                        error={!!errors.lastName}
                    />
                </FormField>
            </div>

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

            <FormField label="Phone number" htmlFor="phoneNumber" required error={errors.phoneNumber}>
                <Input
                    type="tel"
                    id="phoneNumber"
                    placeholder="enter your phone number (+380671234567)"
                    value={formData.phoneNumber}
                    onChange={handleChange("phoneNumber")}
                    error={!!errors.phoneNumber}
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

            <div className={styles.actions}>
                <Button type="submit" variant="primary" disabled={isTryingRegister} size="lg" fullWidth>
                    Register
                </Button>
                <div className={styles.login}>
                    <span>Already have an account?</span>
                    <Link to="/login" className={styles.loginLink}>Login</Link>
                </div>
            </div>
        </form>
    );
};

export default RegisterForm;