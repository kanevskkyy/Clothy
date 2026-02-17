import React, { useState } from 'react';
import { CreditCard, Bitcoin } from 'lucide-react';
import styles from './CheckoutForm.module.css';
import type { IRegionReadDTO } from '../../../entities/ordersService/regions/IRegionReadDTO.ts';
import type { ISettlementReadDTO } from '../../../entities/ordersService/settlement/ISettlementReadDTO.ts';
import type { IDeliveryProviderReadDTO } from '../../../entities/ordersService/deliveryProviders/IDeliveryProviderReadDTO.ts';
import type { IPickupPointReadDTO } from '../../../entities/ordersService/pickupPoints/IPickupPointReadDTO.ts';
import {checkoutFormSchema, type CheckoutFormData } from '../../../app/schemas/checkoutFormSchema';
import FormField from '../../../shared/FormField/FormField';
import RadioOption from "../../../shared/RadioOption/RadioOption.tsx";
import Textarea from '../../../shared/Textarea/Textarea.tsx';
import Input from '../../../shared/Input/Input';
import Select from "../../../shared/Select/Select.tsx";

interface CheckoutFormProps {
    onValidSubmit: (data: CheckoutFormData) => void;
}

const CheckoutForm = ({ onValidSubmit }: CheckoutFormProps) => {
    const mockDeliveryProviders: IDeliveryProviderReadDTO[] = [
        {
            id: "dp-1",
            name: "NovaPoshta",
            iconUrl: "https://img.crm-onebox.com//media/97/ff/97ff04254dde5d65aab1c4ca90d17a95.png",
            createdAt: "2024-01-01T00:00:00Z",
            updatedAt: "2024-01-01T00:00:00Z"
        },
        {
            id: "dp-2",
            name: "UkrPoshta",
            iconUrl: "https://upload.wikimedia.org/wikipedia/commons/thumb/a/a0/Ukrposhta-ua-icon.svg/1920px-Ukrposhta-ua-icon.svg.png",
            createdAt: "2024-01-01T00:00:00Z",
            updatedAt: "2024-01-01T00:00:00Z"
        },
        {
            id: "dp-3",
            name: "Meest",
            iconUrl: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQqvdlpx0JGh07avsMk_h3wAP23JkubvQsH8g&s",
            createdAt: "2024-01-01T00:00:00Z",
            updatedAt: "2024-01-01T00:00:00Z"
        }
    ];

    // TODO: Connect to API

    const mockRegions: IRegionReadDTO[] = [
        { id: "reg-1", name: "Київська область", ref: "kyiv-oblast", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" },
        { id: "reg-2", name: "Львівська область", ref: "lviv-oblast", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" },
        { id: "reg-3", name: "Одеська область", ref: "odesa-oblast", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" },
        { id: "reg-4", name: "Харківська область", ref: "kharkiv-oblast", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" },
        { id: "reg-5", name: "Дніпропетровська область", ref: "dnipro-oblast", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" },
        { id: "reg-6", name: "Чернівецька область", ref: "chernivtsi-oblast", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" }
    ];

    // TODO: Connect to API

    const mockSettlements: ISettlementReadDTO[] = [
        { id: "set-1", name: "Київ", ref: "kyiv", type: "місто", regionId: "reg-1", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" },
        { id: "set-2", name: "Бориспіль", ref: "boryspil", type: "місто", regionId: "reg-1", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" },
        { id: "set-3", name: "Біла Церква", ref: "bila-tserkva", type: "місто", regionId: "reg-1", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" },
        { id: "set-4", name: "Львів", ref: "lviv", type: "місто", regionId: "reg-2", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" },
        { id: "set-5", name: "Дрогобич", ref: "drohobych", type: "місто", regionId: "reg-2", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" },
        { id: "set-6", name: "Одеса", ref: "odesa", type: "місто", regionId: "reg-3", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" },
        { id: "set-7", name: "Ізмаїл", ref: "izmail", type: "місто", regionId: "reg-3", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" },
        { id: "set-8", name: "Харків", ref: "kharkiv", type: "місто", regionId: "reg-4", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" },
        { id: "set-9", name: "Дніпро", ref: "dnipro", type: "місто", regionId: "reg-5", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" },
        { id: "set-10", name: "Кривий Ріг", ref: "kryvyi-rih", type: "місто", regionId: "reg-5", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" },
        { id: "set-11", name: "Чернівці", ref: "chernivtsi", type: "місто", regionId: "reg-6", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" }
    ];

    // TODO: Connect to API

    const mockPickupPoints: IPickupPointReadDTO[] = [
        { id: "pp-1", address: "Відділення №1: вул. Хрещатик, 1", ref: "kyiv-np-1", isActive: true, deliveryProviderId: "dp-1", settlementId: "set-1", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" },
        { id: "pp-2", address: "Відділення №5: вул. Саксаганського, 25", ref: "kyiv-np-5", isActive: true, deliveryProviderId: "dp-1", settlementId: "set-1", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" },
        { id: "pp-3", address: "Відділення №12: вул. Антоновича, 48", ref: "kyiv-np-12", isActive: true, deliveryProviderId: "dp-1", settlementId: "set-1", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" },
        { id: "pp-4", address: "Поштове відділення №01001: Майдан Незалежності, 2", ref: "kyiv-up-01001", isActive: true, deliveryProviderId: "dp-2", settlementId: "set-1", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" },
        { id: "pp-5", address: "Поштове відділення №01033: вул. Велика Васильківська, 12", ref: "kyiv-up-01033", isActive: true, deliveryProviderId: "dp-2", settlementId: "set-1", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" },
        { id: "pp-6", address: "Відділення №1: вул. Прорізна, 8", ref: "kyiv-meest-1", isActive: true, deliveryProviderId: "dp-3", settlementId: "set-1", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" },
        { id: "pp-7", address: "Відділення №1: пр. Свободи, 15", ref: "lviv-np-1", isActive: true, deliveryProviderId: "dp-1", settlementId: "set-4", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" },
        { id: "pp-8", address: "Відділення №7: вул. Городоцька, 178", ref: "lviv-np-7", isActive: true, deliveryProviderId: "dp-1", settlementId: "set-4", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" },
        { id: "pp-9", address: "Відділення №3: вул. Дерибасівська, 10", ref: "odesa-np-3", isActive: true, deliveryProviderId: "dp-1", settlementId: "set-6", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" },
        { id: "pp-10", address: "Відділення №2: вул. Сумська, 25", ref: "kharkiv-np-2", isActive: true, deliveryProviderId: "dp-1", settlementId: "set-8", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" },
        { id: "pp-11", address: "Відділення №4: пр. Дмитра Яворницького, 12", ref: "dnipro-np-4", isActive: true, deliveryProviderId: "dp-1", settlementId: "set-9", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" },
        { id: "pp-12", address: "Відділення №1: вул. Головна, 85", ref: "chernivtsi-np-1", isActive: true, deliveryProviderId: "dp-1", settlementId: "set-11", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" },
        { id: "pp-13", address: "Відділення №5: вул. Небесної Сотні, 42", ref: "chernivtsi-np-5", isActive: true, deliveryProviderId: "dp-1", settlementId: "set-11", createdAt: "2024-01-01T00:00:00Z", updatedAt: "2024-01-01T00:00:00Z" }
    ];

    // TODO: Connect to API

    const [formData, setFormData] = useState<CheckoutFormData>({
        lastName: "",
        firstName: "",
        middleName: "",
        email: "",
        phoneNumber: "+380",
        comment: "",
        deliveryProviderId: mockDeliveryProviders[0].id,
        regionId: "",
        settlementId: "",
        pickupPointId: "",
        paymentMethod: "Card"
    });

    const [errors, setErrors] = useState<Partial<Record<keyof CheckoutFormData, string>>>({});

    const handleChange = (field: keyof CheckoutFormData) => (
        e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
    ) => {
        setFormData((prev) => ({ ...prev, [field]: e.target.value }));
        if (errors[field]) {
            setErrors((prev) => ({ ...prev, [field]: undefined }));
        }
    };

    const handleDeliveryProviderChange = (value: string) => {
        setFormData((prev) => ({
            ...prev,
            deliveryProviderId: value,
            pickupPointId: ""
        }));
        if (errors.deliveryProviderId) {
            setErrors((prev) => ({ ...prev, deliveryProviderId: undefined }));
        }
    };

    const handleRegionChange = (value: string) => {
        setFormData((prev) => ({
            ...prev,
            regionId: value,
            settlementId: "",
            pickupPointId: ""
        }));
        if (errors.regionId) {
            setErrors((prev) => ({ ...prev, regionId: undefined }));
        }
    };

    const handleSettlementChange = (value: string) => {
        setFormData((prev) => ({
            ...prev,
            settlementId: value,
            pickupPointId: ""
        }));
        if (errors.settlementId) {
            setErrors((prev) => ({ ...prev, settlementId: undefined }));
        }
    };

    const handlePickupPointChange = (value: string) => {
        setFormData((prev) => ({ ...prev, pickupPointId: value }));
        if (errors.pickupPointId) {
            setErrors((prev) => ({ ...prev, pickupPointId: undefined }));
        }
    };

    const handlePaymentMethodChange = (value: string) => {
        setFormData((prev) => ({ ...prev, paymentMethod: value as "Card" | "Crypto" }));
        if (errors.paymentMethod) {
            setErrors((prev) => ({ ...prev, paymentMethod: undefined }));
        }
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();

        const result = checkoutFormSchema.safeParse(formData);

        if (!result.success) {
            const fieldErrors: Partial<Record<keyof CheckoutFormData, string>> = {};
            result.error.issues.forEach((issue) => {
                const field = issue.path[0] as keyof CheckoutFormData;
                fieldErrors[field] = issue.message;
            });
            setErrors(fieldErrors);
            return;
        }

        onValidSubmit(result.data);
    };

    const regionOptions = mockRegions.map(region => ({
        value: region.id,
        label: region.name
    }));


    // #TODO: remove this and connect API for filter
    const settlementOptions = mockSettlements
        .filter(settlement => settlement.regionId === formData.regionId)
        .map(settlement => ({
            value: settlement.id,
            label: settlement.name
        }));

    const pickupPointOptions = mockPickupPoints
        .filter(point =>
            point.settlementId === formData.settlementId &&
            point.deliveryProviderId === formData.deliveryProviderId
        )
        .map(point => ({
            value: point.id,
            label: point.address
        }));

    return (
        <form className={styles.form} onSubmit={handleSubmit} id="checkout-form">
            <section className={styles.section}>
                <div className={styles.row}>
                    <FormField
                        label="First name"
                        htmlFor="firstName"
                        required
                        error={errors.firstName}
                    >
                        <Input
                            type="text"
                            id="firstName"
                            placeholder="enter your first name"
                            value={formData.firstName}
                            onChange={handleChange("firstName")}
                            error={!!errors.firstName}
                        />
                    </FormField>

                    <FormField
                        label="Last name"
                        htmlFor="lastName"
                        required
                        error={errors.lastName}
                    >
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

                <div className={styles.row}>
                    <FormField
                        label="Middle name"
                        htmlFor="middleName"
                        required
                        error={errors.middleName}
                    >
                        <Input
                            type="text"
                            id="middleName"
                            placeholder="enter your middle name"
                            value={formData.middleName}
                            onChange={handleChange("middleName")}
                            error={!!errors.middleName}
                        />
                    </FormField>

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
                </div>

                <FormField
                    label="Phone number"
                    htmlFor="phoneNumber"
                    required
                    error={errors.phoneNumber}
                >
                    <Input
                        type="tel"
                        placeholder="enter your phone number (+380671234567)"
                        id="phoneNumber"
                        onChange={handleChange("phoneNumber")}
                        error={!!errors.phoneNumber}
                    />
                </FormField>

                <FormField
                    label="Comment to order"
                    htmlFor="comment"
                    error={errors.comment}
                >
                    <Textarea
                        id="comment"
                        placeholder="Additional wishes or information..."
                        value={formData.comment}
                        onChange={handleChange("comment")}
                        error={!!errors.comment}
                    />
                </FormField>
            </section>

            <section className={styles.section}>
                <h2 className={styles.sectionTitle}>Delivery</h2>

                <div className={styles.radioGroup}>
                    {mockDeliveryProviders.map((provider) => (
                        <RadioOption
                            key={provider.id}
                            id={`delivery-${provider.id}`}
                            name="deliveryProvider"
                            value={provider.id}
                            iconBgColor="#FFFFFF"
                            checked={formData.deliveryProviderId === provider.id}
                            onChange={handleDeliveryProviderChange}
                            icon={
                                <img
                                    src={provider.iconUrl}
                                    alt={provider.name}
                                    style={{ width: '30px', height: '30px', objectFit: 'contain' }}
                                />
                            }
                            label={provider.name}
                        />
                    ))}
                </div>
                {errors.deliveryProviderId && (
                    <div className={styles.error}>{errors.deliveryProviderId}</div>
                )}

                <div className={styles.selectRow}>
                    <FormField
                        label="Region"
                        htmlFor="region"
                        required
                        error={errors.regionId}
                    >
                        <Select
                            inputId="region"
                            options={regionOptions}
                            value={regionOptions.find(opt => opt.value === formData.regionId) || null}
                            onChange={(option) => handleRegionChange((option as any)?.value || "")}
                            placeholder="Pick region"
                            error={!!errors.regionId}
                            isSearchable
                        />
                    </FormField>

                    <FormField
                        label="Settlement"
                        htmlFor="settlement"
                        required
                        error={errors.settlementId}
                    >
                        <Select
                            inputId="settlement"
                            options={settlementOptions}
                            value={settlementOptions.find(opt => opt.value === formData.settlementId) || null}
                            onChange={(option) => handleSettlementChange((option as any)?.value || "")}
                            placeholder="Pick settlement"
                            error={!!errors.settlementId}
                            isDisabled={!formData.regionId}
                            isSearchable
                        />
                    </FormField>

                    <FormField
                        label="Pickup point"
                        htmlFor="pickupPoint"
                        required
                        error={errors.pickupPointId}
                    >
                        <Select
                            inputId="pickupPoint"
                            options={pickupPointOptions}
                            value={pickupPointOptions.find(opt => opt.value === formData.pickupPointId) || null}
                            onChange={(option) => handlePickupPointChange((option as any)?.value || "")}
                            placeholder="Pick pickup point"
                            error={!!errors.pickupPointId}
                            isDisabled={!formData.settlementId || !formData.deliveryProviderId}
                            isSearchable
                        />
                    </FormField>
                </div>
            </section>

            <section className={styles.section}>
                <h2 className={styles.sectionTitle}>Payment method</h2>

                <div className={styles.radioGroup}>
                    <RadioOption
                        id="payment-card"
                        name="paymentMethod"
                        value="Card"
                        checked={formData.paymentMethod === "Card"}
                        onChange={handlePaymentMethodChange}
                        icon={<CreditCard />}
                        iconBgColor="#2F71F0"
                        iconColor="#FFFFFF"
                        label="Credit Card"
                        description="Visa, Mastercard, Приват24"
                    />
                    <RadioOption
                        id="payment-crypto"
                        name="paymentMethod"
                        value="Crypto"
                        checked={formData.paymentMethod === "Crypto"}
                        onChange={handlePaymentMethodChange}
                        icon={<Bitcoin />}
                        iconBgColor="#EF990D"
                        iconColor="#FFFFFF"
                        label="Cryptocurrency"
                        description="Bitcoin, Ethereum, USDT"
                    />
                </div>
                {errors.paymentMethod && (
                    <div className={styles.error}>{errors.paymentMethod}</div>
                )}
            </section>
        </form>
    );
};

export default CheckoutForm;