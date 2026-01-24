import {BrowserRouter, Route, Routes} from 'react-router-dom';
import HomePage from '../../pages/HomePage/HomePage';
import AboutUsPage from '../../pages/AboutUsPage/AboutUsPage.tsx';
import Header from '../../features/header/Header';
import Footer from '../../features/footer/Footer';
import styles from './AppRouter.module.css';
import DeliveryInfoPage from "../../pages/DeliveryInfoPage/DeliveryInfoPage.tsx";
import ScrollToTop from "../scroll/ScrollToTop.tsx";


export const AppRouter = () => {
    return (
        <BrowserRouter>
            <ScrollToTop />
            <div className={styles.appWrapper}>
                <Header/>

                <main className={styles.mainContent}>
                    <Routes>
                        <Route path="/" element={<HomePage/>}/>
                        <Route path="/about-us" element={<AboutUsPage/>}/>
                        <Route path="/delivery-info" element={<DeliveryInfoPage/>}/>
                    </Routes>
                </main>

                <Footer/>
            </div>
        </BrowserRouter>
    );
};