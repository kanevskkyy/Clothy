import styles from "./StarRating.module.css";

interface StarRatingProps {
    rating: number;
    max?: number;
}

const StarRating: React.FC<StarRatingProps> = ({ rating, max = 5 }) => {
    return (
        <div className={styles.stars}>
            {Array.from({ length: max }, (_, index) => (
                <span key={index} className={`${styles.star} ${index < rating ? styles.filled : ''}`}>
                    ★
                </span>
            ))}
        </div>
    );
};

export default StarRating;