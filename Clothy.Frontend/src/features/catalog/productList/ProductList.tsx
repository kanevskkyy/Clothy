import styles from "./ProductList.module.css";
import ClotheSummaryCard from "../../../entities/catalogService/clotheSummary/ClotheSummaryCard.tsx";
import type {IClotheSummaryDTO} from "../../../entities/catalogService/interfaces/clothe/IClotheSummaryDTO.ts";

interface ProductListProps {
    products: IClotheSummaryDTO[];
    className?: string;
}

const ProductList = ({products, className}: ProductListProps) => {
    return (
        <div className={`${styles.grid} ${className ?? ''}`}>
            {products.map((product) => (
                <ClotheSummaryCard key={product.id} product={product} />
            ))}
        </div>
    );
};

export default ProductList;