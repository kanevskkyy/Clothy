import { catalogApi } from "../../../app/api/catalogApi.ts";
import type { ISizeReadDTO } from "../../../entities/catalogService/interfaces/size/ISizeReadDTO.ts";
import CrudAdminPage from "../crud/CrudAdminPage.tsx";
import {Layers} from "lucide-react";
import SizeForm from "../../../features/forms/sizeForm/SizeForm.tsx";

const AdminSizesPage = () => {
    return (
        <CrudAdminPage<ISizeReadDTO>
            title="Sizes"
            description="Manage catalog sizes"
            entityName="Size"
            entityNamePlural="Sizes"
            icon={<Layers size={28} color="#6B6B6B" />}
            getAll={catalogApi.getAllSizesAsync}
            getById={catalogApi.getSizeByIdAsync}
            create={catalogApi.createSizeAsync}
            update={catalogApi.updateSizeAsync}
            remove={catalogApi.deleteSizeAsync}
            FormComponent={SizeForm}
        />
    );
};

export default AdminSizesPage;