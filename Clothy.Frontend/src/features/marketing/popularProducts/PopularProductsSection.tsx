import {useEffect} from 'react';
import styles from "./PopularProductsSection.module.css";
import {catalogApi} from "../../../app/api/catalogApi.ts";
import ProductList from '../../catalog/productList/ProductList.tsx';
import {toast} from "sonner";
import {getErrorMessage} from "../../../shared/lib/errorHandler.ts";
import {useQuery} from "@tanstack/react-query";
import {Link} from "react-router-dom";
import {ArrowRight} from "lucide-react";
import ProductGridSkeleton from "../../catalog/skeleton/gridSkeleton/ProductGridSkeleton.tsx";

const PopularProductsSection = () => {
    const {data: clotheItems = [], isLoading, error} = useQuery({
        queryKey: ["clothe-top8"],
        queryFn: () => catalogApi.getTop8MostSaleAsync(),
    });

    useEffect(() => {
        if (error) toast.error(getErrorMessage(error));
    }, [error]);

    if (isLoading) return <ProductGridSkeleton />;

    return (
        <section className={styles.productsSection}>
            <div className={styles.container}>
                <div className={styles.sectionHeader}>
                    <div>
                        <p className={styles.titleLabel}>Trending</p>
                        <h2 className={styles.title}>Most Popular</h2>
                    </div>
                    <Link to="/catalog" className={styles.viewAll}>
                        VIEW ALL
                        <ArrowRight size={14} strokeWidth={1.5}/>
                    </Link>
                </div>
                <ProductList products={clotheItems} className={styles.popularGrid}/>
            </div>
        </section>
    );
};

export default PopularProductsSection;