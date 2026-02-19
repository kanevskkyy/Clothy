import {useState, useRef, useEffect} from "react";
import {Link, Navigate, useOutletContext} from "react-router-dom";
import styles from "./AccountProfilePage.module.css";
import { Upload } from "lucide-react";
import type { IUserReadDTO } from "../../entities/usersService/IUserReadDTO";
import { type UserUpdateFormData, userUpdateSchema, } from "../../app/schemas/userUpdateSchema.ts";
import Button from "../../shared/Button/Button.tsx";
import FormField from "../../shared/FormField/FormField.tsx";
import Input from "../../shared/Input/Input.tsx";
import { Helmet } from "react-helmet";
import {authApi} from "../../app/api/authApi.ts";
import {toast} from "sonner";
import {getErrorMessage} from "../../shared/utils/errorHandler.ts";
import {useAuthStore} from "../../app/api/stores/authStore.ts";

interface OutletContext {
    user: IUserReadDTO;
}

const AccountProfilePage = () => {
    const [isLoading, setIsLoading] = useState(false);

    const { user } = useOutletContext<OutletContext>();
    const { setUser } = useAuthStore();

    useEffect(() => {
        const refreshUser = async () => {
            try {
                const updatedUser = await authApi.getInfoAboutMeAsync();
                setUser(updatedUser);
            } catch (error) {
                console.error(getErrorMessage(error));
            }
        };

        refreshUser();
    }, []);

    const [formData, setFormData] = useState({
        firstName: user?.firstName ?? "",
        lastName: user?.lastName ?? "",
        phoneNumber: user?.phoneNumber ?? "",
    });

    const [photoFile, setPhotoFile] = useState<File | undefined>(undefined);
    const [photoPreview, setPhotoPreview] = useState<string>(user?.photoUrl ?? "");
    const [errors, setErrors] = useState<Partial<Record<keyof UserUpdateFormData, string>>>({});
    const fileInputRef = useRef<HTMLInputElement>(null);

    if (!user) {
        return <Navigate to="/login" replace />;
    }

    const handleChange =
        (field: keyof Omit<UserUpdateFormData, "photo">) =>
            (e: React.ChangeEvent<HTMLInputElement>) => {
                setFormData((prev) => ({ ...prev, [field]: e.target.value }));

                if (errors[field]) {
                    setErrors((prev) => ({ ...prev, [field]: undefined }));
                }
            };

    const handlePhotoChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file) return;

        setPhotoFile(file);

        const reader = new FileReader();
        reader.onloadend = () => {
            setPhotoPreview(reader.result as string);
        };
        reader.readAsDataURL(file);

        if (errors.photo) {
            setErrors((prev) => ({ ...prev, photo: undefined }));
        }
    };

    const triggerFileSelect = () => {
        fileInputRef.current?.click();
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        const dataToValidate = {
            ...formData,
            photo: photoFile,
        };

        const result = userUpdateSchema.safeParse(dataToValidate);

        if (!result.success) {
            const fieldErrors: Partial<Record<keyof UserUpdateFormData, string>> = {};
            result.error.issues.forEach((issue) => {
                const field = issue.path[0] as keyof UserUpdateFormData;
                fieldErrors[field] = issue.message;
            });
            setErrors(fieldErrors);
            return;
        }

        try {
            setIsLoading(true);
            const updatedUser = await authApi.updateMyAccountAsync(result.data);
            setUser(updatedUser);
            toast.success("Successfully updated your account");
        } catch (error) {
            toast.error(getErrorMessage(error))
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className={styles.page}>
            <Helmet>
                <title>Profile Settings | Account</title>
                <meta
                    name="description"
                    content="Manage your personal information, profile photo, and contact details in your account settings."
                />
            </Helmet>

            <h2 className={styles.title}>Personal Information</h2>

            <form className={styles.form} onSubmit={handleSubmit}>
                <div className={styles.photoSection}>
                    <img
                        src={photoPreview}
                        alt="Profile"
                        className={styles.photo}
                    />

                    <input
                        ref={fileInputRef}
                        type="file"
                        accept=".jpg,.jpeg,.png,.gif,.svg"
                        onChange={handlePhotoChange}
                        className={styles.fileInput}
                    />

                    <Button
                        type="button"
                        onClick={triggerFileSelect}
                        variant="outline"
                        size="sm"
                        icon={<Upload size={18} />}
                    >
                        Upload Photo
                    </Button>

                    {errors.photo && (
                        <p className={styles.photoError}>{errors.photo}</p>
                    )}
                </div>

                <div className={styles.row}>
                    <FormField
                        label="First Name"
                        htmlFor="firstName"
                        required
                        error={errors.firstName}
                    >
                        <Input
                            type="text"
                            id="firstName"
                            value={formData.firstName}
                            onChange={handleChange("firstName")}
                            error={!!errors.firstName}
                        />
                    </FormField>

                    <FormField
                        label="Last Name"
                        htmlFor="lastName"
                        required
                        error={errors.lastName}
                    >
                        <Input
                            type="text"
                            id="lastName"
                            value={formData.lastName}
                            onChange={handleChange("lastName")}
                            error={!!errors.lastName}
                        />
                    </FormField>
                </div>

                <FormField
                    label="Phone Number"
                    htmlFor="phoneNumber"
                    required
                    error={errors.phoneNumber}
                >
                    <Input
                        type="tel"
                        id="phoneNumber"
                        placeholder="+380XXXXXXXXX"
                        value={formData.phoneNumber}
                        onChange={handleChange("phoneNumber")}
                        error={!!errors.phoneNumber}
                    />
                </FormField>

                <Button type="submit" variant="primary" size="lg" fullWidth disabled={isLoading}>
                    {isLoading ? "Saving..." : "Save Changes"}
                </Button>

                <div className={styles.passwordSection}>
                    <span>Want to change your password?</span>
                    <Link to="/reset-password" className={styles.passwordLink}>
                        Change password
                    </Link>
                </div>
            </form>
        </div>
    );
};

export default AccountProfilePage;