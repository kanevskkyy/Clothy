import { Layers } from "lucide-react";
import { catalogApi } from "../../../app/api/catalogApi.ts";
import CollectionForm from "../../../features/forms/collectionForm/CollectionForm.tsx";
import type { ICollectionReadDTO } from "../../../entities/catalogService/interfaces/collection/ICollectionReadDTO.ts";
import CrudAdminPage from "../crud/CrudAdminPage.tsx";

const AdminCollectionsPage = () => {
    return (
        <CrudAdminPage<ICollectionReadDTO>
            title="Collections"
            description="Manage catalog collections"
            entityName="Collection"
            entityNamePlural="Collections"
            icon={<Layers size={28} color="#6B6B6B" />}
            getAll={catalogApi.getAllCollectionsAsync}
            getById={catalogApi.getCollectionByIdAsync}
            create={catalogApi.createCollectionAsync}
            update={catalogApi.updateCollectionAsync}
            remove={catalogApi.deleteCollectionAsync}
            FormComponent={CollectionForm}
        />
    );
};

export default AdminCollectionsPage;