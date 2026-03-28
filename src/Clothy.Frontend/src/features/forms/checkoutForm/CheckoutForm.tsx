import React, { useCallback, useEffect, useRef, useState } from 'react';
import { CreditCard, Bitcoin } from 'lucide-react';
import styles from './CheckoutForm.module.css';
import type { IRegionReadDTO } from '../../../entities/ordersService/interfaces/IRegionReadDTO.ts';
import type { ISettlementReadDTO } from '../../../entities/ordersService/interfaces/ISettlementReadDTO.ts';
import type { IPickupPointReadDTO } from '../../../entities/ordersService/interfaces/IPickupPointReadDTO.ts';
import type { PagedList } from '../../../shared/lib/pagedList.ts';
import { checkoutFormSchema, type CheckoutFormData } from '../../../app/schemas/checkoutFormSchema.ts';
import FormField from '../../../shared/form/FormField/FormField.tsx';
import RadioOption from '../../../shared/ui/RadioOption/RadioOption.tsx';
import Textarea from '../../../shared/ui/Textarea/Textarea.tsx';
import Input from '../../../shared/ui/Input/Input.tsx';
import Select from '../../../shared/ui/Select/Select.tsx';
import Loader from '../../../shared/ui/Loader/Loader.tsx';
import { ordersApi } from '../../../app/api/ordersApi.ts';
import { toast } from 'sonner';
import { getErrorMessage } from '../../../shared/lib/errorHandler.ts';
import { useAuthStore } from '../../../app/stores/authStore.ts';
import { getZodFieldErrors } from '../../../shared/lib/getZodFieldErrors.ts';
import { useQuery } from '@tanstack/react-query';

interface CheckoutFormProps {
    onValidSubmit: (data: CheckoutFormData) => void;
}

type SelectOption = { value: string; label: string };

function useDebounce<T>(value: T, delay: number): T {
    const [debounced, setDebounced] = useState(value);
    useEffect(() => {
        const t = setTimeout(() => setDebounced(value), delay);
        return () => clearTimeout(t);
    }, [value, delay]);
    return debounced;
}

const defaultPagedList = <T,>(): PagedList<T> => ({
    items: [],
    currentPage: 1,
    totalPages: 1,
    pageSize: 50,
    totalCount: 0,
    hasPrevious: false,
    hasNext: false,
});

const CheckoutForm = ({ onValidSubmit }: CheckoutFormProps) => {
    const user = useAuthStore(state => state.user);

    const { data: deliveryProviders = [], isLoading: providersLoading } = useQuery({
        queryKey: ["delivery-providers"],
        queryFn: () => ordersApi.getDeliveryProvidersAsync(),
        staleTime: Infinity,
        throwOnError: (error) => {
            toast.error(getErrorMessage(error));
            return false;
        }
    });

    const { data: regions = [], isLoading: regionsLoading } = useQuery({
        queryKey: ["regions"],
        queryFn: () => ordersApi.getAllRegionsAsync(),
        staleTime: Infinity,
        throwOnError: (error) => {
            toast.error(getErrorMessage(error));
            return false;
        }
    });

    const isInitialLoading = providersLoading || regionsLoading;

    const [settlements, setSettlements] = useState<PagedList<ISettlementReadDTO>>(defaultPagedList());
    const [pickupPoints, setPickupPoints] = useState<PagedList<IPickupPointReadDTO>>(defaultPagedList());

    const [settlementsLoading, setSettlementsLoading] = useState(false);
    const [pickupPointsLoading, setPickupPointsLoading] = useState(false);

    const [settlementSearch, setSettlementSearch] = useState('');
    const [pickupSearch, setPickupSearch] = useState('');
    const debouncedSettlementSearch = useDebounce(settlementSearch, 400);
    const debouncedPickupSearch = useDebounce(pickupSearch, 400);

    const regionIdRef = useRef('');
    const settlementIdRef = useRef('');
    const deliveryProviderIdRef = useRef('');

    const [formData, setFormData] = useState<CheckoutFormData>({
        lastName: user?.lastName ?? '',
        firstName: user?.firstName ?? '',
        email: user?.email ?? '',
        phoneNumber: user?.phoneNumber ?? '+380',
        comment: '',
        deliveryProviderId: '',
        regionId: '',
        settlementId: '',
        pickupPointId: '',
        paymentMethod: 'Card',
    });

    useEffect(() => {
        if (deliveryProviders.length > 0 && !formData.deliveryProviderId) {
            deliveryProviderIdRef.current = deliveryProviders[0].id;
            setFormData(prev => ({ ...prev, deliveryProviderId: deliveryProviders[0].id }));
        }
    }, [deliveryProviders]);

    const [errors, setErrors] = useState<Partial<Record<keyof CheckoutFormData, string>>>({});

    const fetchSettlements = useCallback(async (
        regionId: string,
        name: string,
        page: number,
        append: boolean
    ) => {
        if (!regionId) return;
        setSettlementsLoading(true);
        try {
            const result = await ordersApi.getSettlementsAsync({ regionId, name: name || undefined, pageNumber: page });
            setSettlements(prev => ({
                ...result,
                items: append ? [...prev.items, ...result.items] : result.items,
            }));
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setSettlementsLoading(false);
        }
    }, []);

    const fetchPickupPoints = useCallback(async (
        settlementId: string,
        deliveryProviderId: string,
        address: string,
        page: number,
        append: boolean
    ) => {
        if (!settlementId || !deliveryProviderId) return;
        setPickupPointsLoading(true);
        try {
            const result = await ordersApi.getPickupPointsAsync({ settlementId, deliveryProviderId, address: address || undefined, pageNumber: page });
            setPickupPoints(prev => ({
                ...result,
                items: append ? [...prev.items, ...result.items] : result.items,
            }));
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setPickupPointsLoading(false);
        }
    }, []);

    useEffect(() => {
        if (!regionIdRef.current) return;
        fetchSettlements(regionIdRef.current, debouncedSettlementSearch, 1, false);
    }, [debouncedSettlementSearch, fetchSettlements]);

    useEffect(() => {
        if (!settlementIdRef.current || !deliveryProviderIdRef.current) return;
        fetchPickupPoints(settlementIdRef.current, deliveryProviderIdRef.current, debouncedPickupSearch, 1, false);
    }, [debouncedPickupSearch, fetchPickupPoints]);

    const handleSettlementsScrollToBottom = () => {
        if (settlements.hasNext && !settlementsLoading) {
            fetchSettlements(regionIdRef.current, debouncedSettlementSearch, settlements.currentPage + 1, true);
        }
    };

    const handlePickupScrollToBottom = () => {
        if (pickupPoints.hasNext && !pickupPointsLoading) {
            fetchPickupPoints(settlementIdRef.current, deliveryProviderIdRef.current, debouncedPickupSearch, pickupPoints.currentPage + 1, true);
        }
    };

    const handleChange = (field: keyof CheckoutFormData) =>
        (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
            setFormData(prev => ({ ...prev, [field]: e.target.value }));
            if (errors[field]) setErrors(prev => ({ ...prev, [field]: undefined }));
        };

    const handleDeliveryProviderChange = (value: string) => {
        deliveryProviderIdRef.current = value;
        setFormData(prev => ({ ...prev, deliveryProviderId: value, pickupPointId: '' }));
        if (errors.deliveryProviderId) setErrors(prev => ({ ...prev, deliveryProviderId: undefined }));
        if (settlementIdRef.current) {
            fetchPickupPoints(settlementIdRef.current, value, '', 1, false);
        }
    };

    const handleRegionChange = (value: string) => {
        regionIdRef.current = value;
        settlementIdRef.current = '';
        setFormData(prev => ({ ...prev, regionId: value, settlementId: '', pickupPointId: '' }));
        setSettlementSearch('');
        setPickupSearch('');
        setSettlements(defaultPagedList());
        setPickupPoints(defaultPagedList());
        if (errors.regionId) setErrors(prev => ({ ...prev, regionId: undefined }));
        if (value) fetchSettlements(value, '', 1, false);
    };

    const handleSettlementChange = (value: string) => {
        settlementIdRef.current = value;
        setFormData(prev => ({ ...prev, settlementId: value, pickupPointId: '' }));
        setPickupSearch('');
        setPickupPoints(defaultPagedList());
        if (errors.settlementId) setErrors(prev => ({ ...prev, settlementId: undefined }));
        if (value && deliveryProviderIdRef.current) {
            fetchPickupPoints(value, deliveryProviderIdRef.current, '', 1, false);
        }
    };

    const handlePickupPointChange = (value: string) => {
        setFormData(prev => ({ ...prev, pickupPointId: value }));
        if (errors.pickupPointId) setErrors(prev => ({ ...prev, pickupPointId: undefined }));
    };

    const handlePaymentMethodChange = (value: string) => {
        setFormData(prev => ({ ...prev, paymentMethod: value as 'Card' | 'Crypto' }));
        if (errors.paymentMethod) setErrors(prev => ({ ...prev, paymentMethod: undefined }));
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        const result = checkoutFormSchema.safeParse(formData);
        if (!result.success) {
            setErrors(getZodFieldErrors(result.error));
            return;
        }
        onValidSubmit(result.data);
    };

    if (!user) return null;

    const regionOptions: SelectOption[] = regions.map((r: IRegionReadDTO) => ({ value: r.id, label: r.name }));
    const settlementOptions: SelectOption[] = settlements.items.map((s: ISettlementReadDTO) => ({ value: s.id, label: s.name }));
    const pickupPointOptions: SelectOption[] = pickupPoints.items.map((p: IPickupPointReadDTO) => ({ value: p.id, label: p.address }));

    if (isInitialLoading) return <Loader />;

    return (
        <form className={styles.form} onSubmit={handleSubmit} id="checkout-form">
            <section className={styles.section}>
                <h2 className={styles.sectionTitle}>Personal information</h2>

                <div className={styles.row}>
                    <FormField label="First name" htmlFor="firstName" required error={errors.firstName}>
                        <Input
                            type="text"
                            id="firstName"
                            placeholder="enter your first name"
                            value={formData.firstName}
                            onChange={handleChange('firstName')}
                            error={!!errors.firstName}
                        />
                    </FormField>

                    <FormField label="Last name" htmlFor="lastName" required error={errors.lastName}>
                        <Input
                            type="text"
                            id="lastName"
                            placeholder="enter your last name"
                            value={formData.lastName}
                            onChange={handleChange('lastName')}
                            error={!!errors.lastName}
                        />
                    </FormField>
                </div>

                <div className={styles.row}>
                    <FormField label="Email" htmlFor="email" required error={errors.email}>
                        <Input
                            type="email"
                            id="email"
                            placeholder="enter your email"
                            value={formData.email}
                            onChange={handleChange('email')}
                            error={!!errors.email}
                        />
                    </FormField>

                    <FormField label="Phone number" htmlFor="phoneNumber" required error={errors.phoneNumber}>
                        <Input
                            type="tel"
                            id="phoneNumber"
                            placeholder="enter your phone number (+380671234567)"
                            value={formData.phoneNumber}
                            onChange={handleChange('phoneNumber')}
                            error={!!errors.phoneNumber}
                        />
                    </FormField>
                </div>

                <FormField label="Comment to order" htmlFor="comment" error={errors.comment}>
                    <Textarea
                        id="comment"
                        placeholder="Additional wishes or information..."
                        value={formData.comment}
                        onChange={handleChange('comment')}
                        error={!!errors.comment}
                    />
                </FormField>
            </section>

            <section className={styles.section}>
                <h2 className={styles.sectionTitle}>Delivery</h2>

                <div className={styles.radioGroup}>
                    {deliveryProviders.map(provider => (
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
                    <FormField label="Region" htmlFor="region" required error={errors.regionId}>
                        <Select
                            inputId="region"
                            options={regionOptions}
                            value={regionOptions.find(o => o.value === formData.regionId) || null}
                            onChange={option => handleRegionChange((option as SelectOption)?.value || '')}
                            placeholder="Pick region"
                            error={!!errors.regionId}
                            isSearchable
                        />
                    </FormField>

                    <FormField label="Settlement" htmlFor="settlement" required error={errors.settlementId}>
                        <Select
                            inputId="settlement"
                            options={settlementOptions}
                            value={settlementOptions.find(o => o.value === formData.settlementId) || null}
                            onChange={option => handleSettlementChange((option as SelectOption)?.value || '')}
                            onInputChange={(value, { action }) => {
                                if (action === 'input-change') setSettlementSearch(value);
                            }}
                            onMenuScrollToBottom={handleSettlementsScrollToBottom}
                            placeholder="Pick settlement"
                            error={!!errors.settlementId}
                            isDisabled={!formData.regionId}
                            isLoading={settlementsLoading}
                            isSearchable
                            filterOption={null}
                        />
                    </FormField>

                    <FormField label="Pickup point" htmlFor="pickupPoint" required error={errors.pickupPointId}>
                        <Select
                            inputId="pickupPoint"
                            options={pickupPointOptions}
                            value={pickupPointOptions.find(o => o.value === formData.pickupPointId) || null}
                            onChange={option => handlePickupPointChange((option as SelectOption)?.value || '')}
                            onInputChange={(value, { action }) => {
                                if (action === 'input-change') setPickupSearch(value);
                            }}
                            onMenuScrollToBottom={handlePickupScrollToBottom}
                            placeholder="Pick pickup point"
                            error={!!errors.pickupPointId}
                            isDisabled={!formData.settlementId || !formData.deliveryProviderId}
                            isLoading={pickupPointsLoading}
                            isSearchable
                            filterOption={null}
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
                        checked={formData.paymentMethod === 'Card'}
                        onChange={handlePaymentMethodChange}
                        icon={<CreditCard />}
                        iconBgColor="#2F71F0"
                        iconColor="#FFFFFF"
                        label="Credit Card"
                        description="Visa, Mastercard"
                    />
                    <RadioOption
                        id="payment-crypto"
                        name="paymentMethod"
                        value="Crypto"
                        checked={formData.paymentMethod === 'Crypto'}
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