import Hero from "../../../features/marketing/hero/Hero.tsx";
import BenefitsList from "../../../features/marketing/benefits/BenefitsList.tsx";
import BrandsCarousel from "../../../features/marketing/carousel/BrandsCarousel.tsx";
import SaleBanner from "../../../features/marketing/saleBanner/SaleBanner.tsx";
import {Helmet} from 'react-helmet';
import Container from "../../../shared/layout/Container/Container.tsx";
import PopularProductsSection from "../../../features/marketing/popularProducts/PopularProductsSection.tsx";

export interface IBenefitItem {
    title: string;
    description: string;
}

const benefits: IBenefitItem[] = [
    {
        title: "Free shipping",
        description: "Enjoy complimentary delivery on orders over ₴1500.",
    },
    {
        title: "Quality guarantee",
        description: "Carefully selected materials and verified authenticity.",
    },
    {
        title: "Welcome discount",
        description: "Get 10% off your first purchase.",
    },
];

const HomePage = () => {
    return (
        <div>
            <Helmet>
                <title>Clothy — stylish clothing for your day</title>
            </Helmet>
            <Hero/>
            <BrandsCarousel/>
            <Container>
                <PopularProductsSection/>
                <SaleBanner/>
                <BenefitsList benefits={benefits}/>
            </Container>
        </div>
    );
};

export default HomePage;