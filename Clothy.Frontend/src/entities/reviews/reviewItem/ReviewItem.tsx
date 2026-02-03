import type { IReviewReadDTO } from "../interfaces/IReviewReadDTO";
import styles from "./ReviewItem.module.css";

interface ReviewItemProps {
    review: IReviewReadDTO;
}

const ReviewItem: React.FC<ReviewItemProps> = ({ review }) => {
    const formatDate = (dateString: string) => {
        const date = new Date(dateString);
        return date.toLocaleDateString('uk-UA');
    };

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
