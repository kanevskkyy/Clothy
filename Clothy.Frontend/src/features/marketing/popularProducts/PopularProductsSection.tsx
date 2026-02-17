import {useEffect, useState} from 'react';
import styles from "./PopularProductsSection.module.css";
import type { IClotheSummaryDTO } from '../../../entities/catalogService/clotheItem/IClotheSummaryDTO';
import {catalogApi} from "../../../app/api/catalogApi.ts";
import ProductList from '../../catalog/productList/ProductList.tsx';
import Loader from '../../../shared/Loader/Loader.tsx';
import {toast} from "sonner";
import {getErrorMessage} from "../../../shared/utils/errorHandler.ts";

const PopularProductsSection = () => {
    const [loading, setLoading] = useState(true);
    const [clotheItems, setClotheItems] = useState<IClotheSummaryDTO[]>([]);

    useEffect(() => {
        const fetchTop8MostPopularClothes = async () => {
            try {
                const response = await catalogApi.getTop8MostSaleAsync();
                setClotheItems(response);
            }
            catch (error) {
                toast.error(getErrorMessage(error));
            }
            finally {
                setLoading(false);
            }
        }

        fetchTop8MostPopularClothes();
    }, [])

    if(loading){
        return <Loader />;
    }

    return (
        <section className={styles.productsSection}>
            <div className={styles.container}>
                <h2 className={styles.title}>Popular Products Among Visitors</h2>
                <ProductList products={clotheItems} />
            </div>
        </section>
    );
};

export default PopularProductsSection;