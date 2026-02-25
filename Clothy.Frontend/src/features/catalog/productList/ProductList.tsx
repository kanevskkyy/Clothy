import styles from "./ProductList.module.css";
import ClotheSummaryCard from "../../../entities/catalogService/clotheSummary/ClotheSummaryCard.tsx";
import type {IClotheSummaryDTO} from "../../../entities/catalogService/interfaces/clothe/IClotheSummaryDTO.ts";

interface ProductListProps {
    products: IClotheSummaryDTO[];
}

const ProductList = ({products}: ProductListProps) => {
    return (
        <div className={styles.grid}>
            {products.map((product) => (
                <ClotheSummaryCard key={product.id} product={product} />
            ))}
        </div>
    );
};

export default ProductList;