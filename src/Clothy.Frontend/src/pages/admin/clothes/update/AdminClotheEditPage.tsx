import { useParams, useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";
import { catalogApi } from "../../../../app/api/catalogApi";
import Loader from "../../../../shared/ui/Loader/Loader";
import Container from "../../../../shared/layout/Container/Container.tsx";
import ClotheUpdateForm from "../../../../features/forms/clotheUpdateForm/ClotheUpdateForm.tsx";
import type { IClotheByIdDTO } from "../../../../entities/catalogService/interfaces/clothe/IClotheDetailDTO.ts";
import { toast } from "sonner";
import { getErrorMessage } from "../../../../shared/lib/errorHandler.ts";

const AdminClotheEditPage = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const [clothe, setClothe] = useState<IClotheByIdDTO | null>(null);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        if (!id) return;
        setIsLoading(true);
        catalogApi.getClotheByIdAsync(id)
            .then(setClothe)
            .catch((err) => toast.error(getErrorMessage(err)))
            .finally(() => setIsLoading(false));
    }, [id]);

    if (isLoading || !clothe) return <Loader />;

    return (
        <Container paddingY={30} paddingX={10}>
            <h1 style={{marginTop: 0}}>Edit: {clothe.name}</h1>
            <ClotheUpdateForm clothe={clothe} onSuccess={() => navigate("/admin/clothes")} />
        </Container>
    );
};

export default AdminClotheEditPage;