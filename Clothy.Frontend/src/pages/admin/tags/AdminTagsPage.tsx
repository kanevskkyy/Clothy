import { Tag } from "lucide-react";
import type { ITagReadDTO } from "../../../entities/catalogService/interfaces/tag/ITagReadDTO";
import CrudAdminPage from "../crud/CrudAdminPage";
import { catalogApi } from "../../../app/api/catalogApi";
import TagForm from "../../../features/forms/tagForm/TagForm";

const AdminTagsPage = () => {
    return (
        <CrudAdminPage<ITagReadDTO>
            title="Tags"
            description="Manage catalog tags"
            entityName="Tag"
            entityNamePlural="Tags"
            icon={<Tag size={28} color="#6B6B6B" />}
            getAll={catalogApi.getAllTagsAsync}
            getById={catalogApi.getTagByIdAsync}
            create={catalogApi.createTagAsync}
            update={catalogApi.updateTagAsync}
            remove={catalogApi.deleteTagAsync}
            FormComponent={TagForm}
        />
    );
};

export default AdminTagsPage;