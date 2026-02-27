import { Truck } from "lucide-react";
import { ordersApi } from "../../../app/api/ordersApi.ts";
import DeliveryProviderForm from "../../../features/forms/deliveryProviderForm/DeliveryProviderForm.tsx";
import type { IDeliveryProviderReadDTO } from "../../../entities/ordersService/interfaces/IDeliveryProviderReadDTO.ts";
import CrudAdminPage from "../crud/CrudAdminPage.tsx";

const AdminDeliveryProvidersPage = () => {
    return (
        <CrudAdminPage<IDeliveryProviderReadDTO>
            title="Delivery Providers"
            description="Manage delivery providers"
            entityName="Delivery Provider"
            entityNamePlural="Delivery Providers"
            icon={<Truck size={28} color="#6B6B6B" />}
            getAll={ordersApi.getDeliveryProvidersAsync}
            getById={ordersApi.getDeliveryProviderByIdAsync}
            create={ordersApi.createDeliveryProviderAsync}
            update={ordersApi.updateDeliveryProviderAsync}
            remove={ordersApi.deleteDeliveryProviderAsync}
            FormComponent={DeliveryProviderForm}
        />
    );
};

export default AdminDeliveryProvidersPage;