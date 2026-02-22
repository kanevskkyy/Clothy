import styles from "./ProductList.module.css";
import ClotheSummaryCard from "../../../entities/catalogService/clotheItem/clotheSummary/ClotheSummaryCard.tsx";
import type {IClotheSummaryDTO} from "../../../entities/catalogService/clotheItem/IClotheSummaryDTO.ts";

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