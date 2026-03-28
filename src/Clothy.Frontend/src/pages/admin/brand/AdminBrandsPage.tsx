import { Tag } from "lucide-react";
import { catalogApi } from "../../../app/api/catalogApi.ts";
import BrandForm from "../../../features/forms/brandForm/BrandForm.tsx";
import type { IBrandReadDTO } from "../../../entities/catalogService/interfaces/brand/IBrandReadDTO.ts";
import CrudAdminPage from "../crud/CrudAdminPage.tsx";
import { useQueryClient } from "@tanstack/react-query";

const AdminBrandsPage = () => {
    const queryClient = useQueryClient();

    const removeBrand = async (id: string) => {
        await catalogApi.deleteBrandAsync(id);

        await queryClient.invalidateQueries({
            queryKey: ["brands"],
        });
    };

    return (
        <CrudAdminPage<IBrandReadDTO>
            title="Brands"
            description="Manage catalog brands"
            entityName="Brand"
            entityNamePlural="Brands"
            icon={<Tag size={28} color="#6B6B6B" />}
            getAll={catalogApi.getAllBrandsAsync}
            getById={catalogApi.getBrandByIdAsync}
            create={catalogApi.createBrandAsync}
            update={catalogApi.updateBrandAsync}
            remove={removeBrand}
            FormComponent={BrandForm}
        />
    );
};

export default AdminBrandsPage;