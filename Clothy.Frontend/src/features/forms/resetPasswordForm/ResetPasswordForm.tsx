import React, { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { resetPasswordSchema, type ResetPasswordSchema } from "../../../app/schemas/resetPasswordSchema.ts";
import FormField from "../../../shared/FormField/FormField.tsx";
import Button from "../../../shared/Button/Button.tsx";
import styles from "./ResetPassword.module.css";
import { authApi } from "../../../app/api/authApi.ts";
import { toast } from "sonner";
import { getErrorMessage } from "../../../shared/utils/errorHandler.ts";
import { getZodFieldErrors } from "../../../shared/utils/getZodFieldErrors.ts";
import PasswordInput from "../../../shared/PasswordInput/PasswordInput.tsx";

const ResetPasswordForm = () => {
    const navigate = useNavigate();

    const [formData, setFormData] = useState<ResetPasswordSchema>({
        currentPassword: "",
        newPassword: "",
    });
    const [errors, setErrors] = useState<Partial<Record<keyof ResetPasswordSchema, string>>>({});
    const [isTryingChangePassword, setIsTryingChangePassword] = useState(false);

    const handleChange = (field: keyof ResetPasswordSchema) => (
        e: React.ChangeEvent<HTMLInputElement>
    ) => {
        setFormData((prev) => ({ ...prev, [field]: e.target.value }));
        if (errors[field]) setErrors((prev) => ({ ...prev, [field]: undefined }));
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsTryingChangePassword(true);

        const result = resetPasswordSchema.safeParse(formData);

        if (!result.success) {
            setErrors(getZodFieldErrors(result.error));
            setIsTryingChangePassword(false);
            return;
        }

        try {
            await authApi.resetPasswordAsync(formData);
            toast.success("Your password has been successfully changed!");
            navigate("/account");
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setIsTryingChangePassword(false);
        }
    };

    return (
        <form className={styles.form} onSubmit={handleSubmit}>
            <FormField label="Current password" htmlFor="currentPassword" required error={errors.currentPassword}>
                <PasswordInput
                    id="currentPassword"
                    placeholder="enter your current password"
                    value={formData.currentPassword}
                    onChange={handleChange("currentPassword")}
                    error={!!errors.currentPassword}
                />
            </FormField>

            <FormField label="New password" htmlFor="newPassword" required error={errors.newPassword}>
                <PasswordInput
                    id="newPassword"
                    placeholder="enter your new password"
                    value={formData.newPassword}
                    onChange={handleChange("newPassword")}
                    error={!!errors.newPassword}
                />
            </FormField>

            <div className={styles.actions}>
                <Button disabled={isTryingChangePassword} type="submit" variant="primary" size="lg" fullWidth>
                    Change password
                </Button>
                <div className={styles.account}>
                    <Link to="/account" className={styles.accountLink}>Return to account</Link>
                </div>
            </div>
        </form>
    );
};

export default ResetPasswordForm;