import Hero from "../../features/marketing/hero/Hero.tsx";
import BenefitsList from "../../features/marketing/benefits/BenefitsList.tsx";
import BrandsCarousel from "../../features/marketing/carousel/BrandsCarousel.tsx";
import SaleBanner from "../../features/marketing/saleBanner/SaleBanner.tsx";
import { Helmet } from 'react-helmet';
import Container from "../../shared/Container/Container.tsx";
import PopularProductsSection from "../../features/marketing/popularProducts/PopularProductsSection.tsx";

const HomePage = () => {
    return (
        <div>
            <Helmet>
                <title>Clothy — stylish clothing for your day</title>
            </Helmet>
            <Hero />
            <Container>
                <BenefitsList />
                <BrandsCarousel />
                <PopularProductsSection />
                <SaleBanner />
            </Container>
        </div>
    );
};

export default HomePage;