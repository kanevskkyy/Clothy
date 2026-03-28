import { useNavigate } from "react-router-dom";
import Container from "../../../../shared/layout/Container/Container";
import ClotheCreateForm from "../../../../features/forms/clotheCreateForm/ClotheCreateForm.tsx";

const AdminClotheCreatePage = () => {
    const navigate = useNavigate();
    return (
        <Container paddingY={30} paddingX={10}>
            <h1 style={{marginTop: 0}}>Create Clothe</h1>
            <ClotheCreateForm onSuccess={() => navigate("/admin/clothes")} />
        </Container>
    );
};

export default AdminClotheCreatePage;