import {useEffect} from 'react';
import styles from "./PopularProductsSection.module.css";
import {catalogApi} from "../../../app/api/catalogApi.ts";
import ProductList from '../../catalog/productList/ProductList.tsx';
import Loader from '../../../shared/ui/Loader/Loader.tsx';
import {toast} from "sonner";
import {getErrorMessage} from "../../../shared/lib/errorHandler.ts";
import {useQuery} from "@tanstack/react-query";

const PopularProductsSection = () => {
   const { data: clotheItems = [], isLoading, error } = useQuery({
       queryKey: ["clothe-top8"],
       queryFn: () => catalogApi.getTop8MostSaleAsync(),
   });

    useEffect(() => {
        if (error) toast.error(getErrorMessage(error));
    }, [error]);

    if (isLoading) return <Loader />;

    return (
        <section className={styles.productsSection}>
            <div className={styles.container}>
                <p className={styles.titleLabel}>Trending</p>
                <h2 className={styles.title}>Most Popular</h2>
                <ProductList products={clotheItems} />
            </div>
        </section>
    );
};

export default PopularProductsSection;