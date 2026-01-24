import Hero from "../../features/hero/Hero.tsx";
import BenefitsList from "../../features/benefits/BenefitsList.tsx";
import BrandsCarousel from "../../features/carousel/BrandsCarousel.tsx";
import styles from "./HomePage.module.css";
import ProductList from "../../features/productList/ProductList.tsx";
import SaleBanner from "../../features/saleBanner/SaleBanner.tsx";
import type {IClotheSummaryDTO} from "../../entities/clotheItem/clothe.ts";
import Container from "../../shared/Container/Container.tsx";

const mockProducts: IClotheSummaryDTO[] = [
    {
        id: "1692ee7a-e0c5-4fb7-9dd7-e20e362a6713",
        name: "Футболка базова",
        slug: "futbolka-bazova",
        price: 499,
        oldPrice: 699,
        discountPercent: 29,
        brand: {
            id: "645a040e-3c19-459d-b2d6-25fcd9209913",
            name: "Nike",
            slug: "nike",
            photoURL: "../../../src/assets/images/mockBrands/nikeLogo.png",
            createdAt: "2024-01-01",
            updatedAt: "2024-01-01"
        },
        colors: [
            {
                id: "8f83347a-de5b-4c6c-9087-6fcdc09863f8",
                mainPhotoURL: "https://res.cloudinary.com/dkdljnfja/image/upload/v1769171770/clotheThirdSecond_mljouz.jpg",
                colorId: "6a1dd9e3-b170-47fc-ad0b-c86ffd699f15",
                hexCode: "#000000"
            },
            {
                id: "5dbab66c-402b-4039-a158-a7caa5bf53af",
                mainPhotoURL: "https://res.cloudinary.com/dkdljnfja/image/upload/v1769171770/clotheThirdSecond_mljouz.jpg",
                colorId: "9790c7d4-9402-47a3-801b-7510c35ce76a",
                hexCode: "#FFFFFF"
            }
        ],
        isAvailable: true
    },
    {
        id: "43c93660-c281-4abf-9769-43d5f1a7fb38",
        name: "Джинси класичні",
        slug: "djynsy-klasychni",
        price: 1299,
        brand: {
            id: "54978cf4-6a43-4129-b45c-82e76e362c93",
            name: "Levi's",
            slug: "levis",
            photoURL: "../../../src/assets/images/mockBrands/levisLogo.png",
            createdAt: "2024-01-01",
            updatedAt: "2024-01-01"
        },
        colors: [
            {
                id: "775afb72-67f0-49c4-abb4-659e6de8df8c",
                mainPhotoURL: "https://res.cloudinary.com/dkdljnfja/image/upload/v1769171770/clotheThirdSecond_mljouz.jpg",
                colorId: "6c6116e4-b01b-46a4-a8da-0ecb66a372e1",
                hexCode: "#1E3A8A"
            }
        ],
        isAvailable: true
    },
    {
        id: "1789d9ef-ba23-4992-a5eb-9d0eeb0ece61",
        name: "Светр в'язаний",
        slug: "svetr-vyazanyj",
        price: 899,
        oldPrice: 1199,
        discountPercent: 25,
        brand: {
            id: "3029fc53-0da1-4871-9c9d-6e7a783920ab",
            name: "H&M",
            slug: "hm",
            photoURL: "https://res.cloudinary.com/dkdljnfja/image/upload/e_background_removal/f_png/v1769175662/nikeLogo_yi2s4v.png",
            createdAt: "2024-01-01",
            updatedAt: "2024-01-01"
        },
        colors: [
            {
                id: "2befbb5f-0d8f-405e-aaab-8c8dc2cbd087",
                mainPhotoURL: "https://res.cloudinary.com/dkdljnfja/image/upload/v1769171770/clotheThirdSecond_mljouz.jpg",
                colorId: "f2a7d8ec-fc9c-46e2-ab23-6862b931b5f7",
                hexCode: "#D4C5B9"
            },
            {
                id: "9c505311-9104-49b4-9b82-a98a5c7cfc50",
                mainPhotoURL: "https://res.cloudinary.com/dkdljnfja/image/upload/v1765717386/clothes/rdf6ew5pirs5le3baz2o.jpg",
                colorId: "46716456-7be5-47c1-9d40-86c1f6f5d3b3",
                hexCode: "#6B7280"
            }
        ],
        isAvailable: false
    },
    {
        id: "fd616855-8371-43d6-9cc0-890809403f22",
        name: "Куртка шкіряна",
        slug: "kurtka-shkiryana",
        price: 3499,
        brand: {
            id: "dbf2e5b7-1287-4011-99ff-9ef57470718d",
            name: "Zara",
            slug: "zara",
            photoURL: "../../../src/assets/images/mockBrands/zaraLogo.png",
            createdAt: "2024-01-01",
            updatedAt: "2024-01-01"
        },
        colors: [
            {
                id: "a912f610-ddcb-4c64-b2ef-2f719769f857",
                mainPhotoURL: "https://res.cloudinary.com/dkdljnfja/image/upload/v1769171770/clotheThirdSecond_mljouz.jpg",
                colorId: "black",
                hexCode: "#000000"
            }
        ],
        isAvailable: true
    },
    {
        id: "802fdb77-0cd6-489b-8e7c-8c6d0033657e",
        name: "Спортивні штани",
        slug: "sportyvni-shtany",
        price: 799,
        oldPrice: 999,
        discountPercent: 20,
        brand: {
            id: "3b24e2ba-4d7e-4e09-9b39-121657c2cee9",
            name: "Adidas",
            slug: "adidas",
            photoURL: "../../../src/assets/images/mockBrands/adidasLogo.png",
            createdAt: "2024-01-01",
            updatedAt: "2024-01-01"
        },
        colors: [
            {
                id: "16bf6ab7-47fd-40d9-94c5-04c1cef79fd1",
                mainPhotoURL: "https://res.cloudinary.com/dkdljnfja/image/upload/v1769171770/clotheThirdSecond_mljouz.jpg",
                colorId: "black",
                hexCode: "#000000"
            },
            {
                id: "5fd5a7c7-3153-465b-a8d6-07bc9553903d",
                mainPhotoURL: "https://res.cloudinary.com/dkdljnfja/image/upload/v1769172116/OMBB085F25FLE00L_1019_0_tyhswt.webp",
                colorId: "navy",
                hexCode: "#1E3A8A"
            }
        ],
        isAvailable: true
    },
    {
        id: "c0c8c621-052c-4381-b657-05c0e0f75ddf",
        name: "Сукня вечірня",
        slug: "suknya-vechirnya",
        price: 2199,
        brand: {
            id: "b1963fe4-5ac6-4274-bf24-a916f127355a",
            name: "Mango",
            slug: "mango",
            photoURL: "../../../src/assets/images/mockBrands/mangoLogo.png",
            createdAt: "2024-01-01",
            updatedAt: "2024-01-01"
        },
        colors: [
            {
                id: "05cde1b5-8fa7-48c2-a10c-c04b78b54d49",
                mainPhotoURL: "https://res.cloudinary.com/dkdljnfja/image/upload/v1769171770/clotheThirdSecond_mljouz.jpg",
                colorId: "red",
                hexCode: "#DC2626"
            }
        ],
        isAvailable: true
    },
    {
        id: "a49f999b-a8be-442f-8471-d0f9b1e7629f",
        name: "Худі оверсайз",
        slug: "hudi-oversajz",
        price: 1099,
        oldPrice: 1399,
        discountPercent: 21,
        brand: {
            id: "81e9f853-6287-48d9-8897-84b5abc1179f",
            name: "Pull&Bear",
            slug: "pullbear",
            photoURL: "../../../src/assets/images/mockBrands/tommyHilfigerLogo.png",
            createdAt: "2024-01-01",
            updatedAt: "2024-01-01"
        },
        colors: [
            {
                id: "7fcd0426-d642-41eb-be7d-4fd0b1830cd9",
                mainPhotoURL: "https://res.cloudinary.com/dkdljnfja/image/upload/v1769171770/clotheThirdSecond_mljouz.jpg",
                colorId: "gray",
                hexCode: "#6B7280"
            },
            {
                id: "bdc9b754-a7e1-4809-95e2-d32404759f42",
                mainPhotoURL: "https://res.cloudinary.com/dkdljnfja/image/upload/v1769171770/clotheThirdSecond_mljouz.jpg",
                colorId: "black",
                hexCode: "#000000"
            }
        ],
        isAvailable: false
    },
    {
        id: "b4d1e5a6-c7f2-4f82-9d0a-abcdef123456",
        name: "Шорти карго",
        slug: "shorty-kargo",
        price: 649,
        brand: {
            id: "b8d1f5a6-c7f2-4f82-9d0a-abcdef123457",
            name: "Bershka",
            slug: "bershka",
            photoURL: "../../../src/assets/images/mockBrands/bershkaLogo.png",
            createdAt: "2024-01-01",
            updatedAt: "2024-01-01"
        },
        colors: [
            {
                id: "c12d1e5a6-c7f2-4f82-9d0a-abcdef123460",
                mainPhotoURL: "https://res.cloudinary.com/dkdljnfja/image/upload/v1769171770/clotheThirdSecond_mljouz.jpg",
                colorId: "khaki",
                hexCode: "#78716C"
            }
        ],
        isAvailable: true
    }
];

const HomePage = () => {
    return (
        <div>
            <Hero/>
            <Container>
                <BenefitsList/>
                <BrandsCarousel/>

                <section className={styles.productsSection}>
                    <div className={styles.container}>
                        <h2 className={styles.title}>Популярні товари серед відвідувачів</h2>
                        <ProductList products={mockProducts}/>
                    </div>
                </section>
                <SaleBanner/>
            </Container>
        </div>
    );
};

export default HomePage;