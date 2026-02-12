import type { IReviewReadDTO } from "../IReviewReadDTO";
import type { IReviewAggregatedReadDTO } from "../IReviewAggregatedReadDTO";
import styles from "./ReviewCard.module.css";
import ReviewItem from "../reviewItem/ReviewItem.tsx";

interface ReviewCardProps {
    review: IReviewReadDTO;
}

const ReviewCard: React.FC<ReviewCardProps> = ({ review, }) => {

    const aggregatedReview: IReviewAggregatedReadDTO = {
        id: review.id,
        user: review.user,
        rating: review.rating,
        comment: review.comment,
        createdAt: review.createdAt,
    };

    return (
        <>
            <div className={styles.card}>
                <div className={styles.clotheInfo}>
                    <img
                        src={review.clotheInfo.clothePhotoURL}
                        alt={review.clotheInfo.clotheName}
                        className={styles.clotheImage}
                    />
                    <p className={styles.clotheName}>{review.clotheInfo.clotheName}</p>
                </div>

                <div className={styles.reviewContent}>
                    <ReviewItem review={aggregatedReview} />
                </div>
            </div>
        </>
    );
};

export default ReviewCard;