import styles from "./AccountReviewsPage.module.css";
import type { IReviewReadDTO } from "../../entities/reviewsService/reviews/IReviewReadDTO";
import ReviewCard from "../../entities/reviewsService/reviews/reviewCard/ReviewCard.tsx";
import Pagination from "../../shared/Pagination/Pagination.tsx";
import { Helmet } from "react-helmet";
import { useSearchParams } from "react-router-dom";
import type { PagedList } from "../../shared/utils/pagedList.ts";
import {getCurrentPage, handlePageChange} from "../../shared/utils/paginationUtils.ts";
import {useEffect, useState} from "react";
import {reviewApi} from "../../app/api/reviewApi.ts";
import {decodeJwt} from "../../shared/utils/decodeJwt.ts";
import {useAuthStore} from "../../app/api/stores/authStore.ts";
import {toast} from "sonner";
import {getErrorMessage} from "../../shared/utils/errorHandler.ts";
import Loader from "../../shared/Loader/Loader.tsx";

const AccountReviewsPage = () => {
    const [searchParams, setSearchParams] = useSearchParams();

    const [isLoading, setIsLoading] = useState<boolean>(false);

    const currentPage = getCurrentPage(searchParams);
    const [reviews, setReviews] = useState<PagedList<IReviewReadDTO>>();
    const accessToken = useAuthStore.getState().accessToken;

    useEffect(() => {
        const fetchUsersReviews = async () => {
            try{
                setIsLoading(true);
                const decodeJwtData = decodeJwt(accessToken!);
                const userId = decodeJwtData.sub;

                const data = await reviewApi.getReviewsAsync({ pageNumber: currentPage, userId: userId });
                setReviews(data);
            }
            catch(error){
                toast.error(getErrorMessage(error));
            }
            finally {
                setIsLoading(false);
            }
        }

        fetchUsersReviews();
    }, [currentPage]);

    if (isLoading) {
        return <Loader />;
    }

    const onPageChange = (page: number) => {
        handlePageChange(page, searchParams, setSearchParams);
    };

    return (
        <div className={styles.container}>
            <Helmet>
                <title>My Reviews | Account</title>
                <meta
                    name="description"
                    content="View and manage your product reviews in your account."
                />
            </Helmet>

            <h1 className={styles.title}>My Reviews</h1>

            {!reviews || reviews.items.length === 0 ? (
                <p className={styles.empty}>You haven't written any reviews yet.</p>
            ) : (
                <>
                    <div className={styles.reviewsList}>
                        {reviews.items.map((review) => (
                            <ReviewCard key={review.id} review={review} />
                        ))}
                    </div>

                    <Pagination
                        currentPage={reviews.currentPage}
                        totalPages={reviews.totalPages}
                        onPageChange={onPageChange}
                    />
                </>
            )}
        </div>
    );
};

export default AccountReviewsPage;