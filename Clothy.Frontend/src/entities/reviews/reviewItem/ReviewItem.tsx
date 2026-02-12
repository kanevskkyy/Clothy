import type { IReviewAggregatedReadDTO } from "../IReviewAggregatedReadDTO.ts";
import styles from "./ReviewItem.module.css";
import {formatDate} from "../../../shared/formatDate.ts";

interface ReviewItemProps {
    review: IReviewAggregatedReadDTO;
}

const ReviewItem: React.FC<ReviewItemProps> = ({ review }) => {
    const renderStars = (rating: number) => {
        return Array.from({ length: 5 }, (_, index) => (
            <span key={index} className={`${styles.star} ${index < rating ? styles.filled : ''}`}>
                ★
            </span>
        ));
    };

    return (
        <div className={styles.reviewItem}>
            <div className={styles.reviewHeader}>
                <img
                    src={review.user.photoUrl}
                    className={styles.reviewAvatar}
                    alt={`${review.user.firstName} ${review.user.lastName}`}
                />
                <div className={styles.reviewInfo}>
                    <p className={styles.reviewAuthor}>{review.user.firstName} {review.user.lastName}</p>
                    <div className={styles.reviewStars}>
                        {renderStars(review.rating)}
                    </div>
                </div>
                <span className={styles.reviewDate}>{formatDate(review.createdAt)}</span>
            </div>
            <p className={styles.reviewText}>{review.comment}</p>
        </div>
    );
};

export default ReviewItem;
