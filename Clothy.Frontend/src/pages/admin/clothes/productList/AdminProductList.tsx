import type { IClotheSummaryDTO } from "../../../../entities/catalogService/interfaces/clothe/IClotheSummaryDTO";
import styles from "./AdminProductList.module.css";
import AdminClotheCard from "../card/AdminClotheCard.tsx";

interface AdminProductListProps {
    products: IClotheSummaryDTO[];
}

const AdminProductList = ({ products }: AdminProductListProps) => {
    return (
        <div className={styles.grid}>
            {products.map((product) => (
                <AdminClotheCard key={product.id} product={product} />
            ))}
        </div>
    );
};

export default AdminProductList;