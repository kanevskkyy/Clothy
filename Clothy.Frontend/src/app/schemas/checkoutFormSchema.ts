import { z } from "zod";

export const checkoutFormSchema = z.object({
    firstName: z.string()
        .min(1, "FirstName is required")
        .max(100, "FirstName cannot exceed 100 characters"),

    lastName: z.string()
        .min(1, "LastName is required")
        .max(100, "LastName cannot exceed 100 characters"),

    email: z.string()
        .min(1, "Email is required")
        .email("Email should be valid!")
        .max(100, "Email cannot exceed 100 characters"),

    phoneNumber: z.string()
        .min(1, "PhoneNumber is required")
        .max(20, "PhoneNumber cannot exceed 20 characters")
        .regex(/^(\+380|0)\d{9}$/, "PhoneNumber must be a valid Ukrainian number"),

    pickupPointId: z.string()
        .min(1, "Select a pickup point"),

    comment: z.string()
        .max(80, "Comment cannot exceed 80 characters")
        .optional(),

    deliveryProviderId: z.string()
        .min(1, "Select a delivery provider"),

    regionId: z.string()
        .min(1, "Select a region"),

    settlementId: z.string()
        .min(1, "Select a settlement"),

    paymentMethod: z.enum(["Card", "Crypto"])
});

export type CheckoutFormData = z.infer<typeof checkoutFormSchema>;