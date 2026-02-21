import { useState } from "react";
import type { PagedList } from "../../../../shared/lib/pagedList.ts";
import type { IReviewStatistic } from "../IReviewStatistic.ts";
import type { IQuestionReadDTO } from "../../questions/IQuestionReadDTO.ts";
import styles from "./ReviewsSection.module.css";
import ReviewItem from "../reviewItem/ReviewItem.tsx";
import QuestionItem from "../../questions/questionItem/QuestionItem.tsx";
import Pagination from "../../../../shared/ui/Pagination/Pagination.tsx";
import { reviewApi } from "../../../../app/api/reviewApi.ts";
import { questionApi } from "../../../../app/api/questionApi.ts";
import Loader from "../../../../shared/ui/Loader/Loader.tsx";
import type { IReviewReadDTO } from "../IReviewReadDTO.ts";
import ReviewForm from "../../../../features/forms/reviewForm/ReviewForm.tsx";
import Modal from "../../../../shared/layout/Modal/Modal.tsx";
import Button from "../../../../shared/ui/Button/Button.tsx";
import QuestionCreateForm from "../../../../features/forms/questionCreateForm/QuestionCreateForm.tsx";
import { useQueryClient } from "@tanstack/react-query";

interface ReviewsSectionProps {
    clotheId: string;
    slug: string;
    initialReviews: PagedList<IReviewReadDTO>;
    statistics: IReviewStatistic;
    initialQuestions: PagedList<IQuestionReadDTO>;
}

const ReviewsSection: React.FC<ReviewsSectionProps> = ({ clotheId, slug, initialReviews, statistics, initialQuestions }) => {
    const queryClient = useQueryClient();
    const invalidate = () => queryClient.invalidateQueries({ queryKey: ["clothe", slug] });

    const [activeTab, setActiveTab] = useState<'reviews' | 'questions'>('reviews');
    const [reviews, setReviews] = useState(initialReviews);
    const [questions, setQuestions] = useState(initialQuestions);
    const [reviewsLoading, setReviewsLoading] = useState(false);
    const [questionsLoading, setQuestionsLoading] = useState(false);
    const [isReviewFormOpen, setIsReviewFormOpen] = useState(false);
    const [isQuestionFormOpen, setIsQuestionFormOpen] = useState(false);

    const calculatePercentage = (count: number) => {
        if (statistics.totalReviews === 0) return 0;
        return (count / statistics.totalReviews) * 100;
    };

    const handleReviewPageChange = async (page: number) => {
        setReviewsLoading(true);
        try {
            const newReviews = await reviewApi.getReviewsAsync({ pageNumber: page, clotheItemId: clotheId });
            setReviews(prev => ({
                ...prev,
                currentPage: newReviews.currentPage,
                items: newReviews.items,
            }));
        } catch (error) {
            console.error('Error fetching reviews:', error);
        } finally {
            setReviewsLoading(false);
        }
    };

    const handleQuestionPageChange = async (page: number) => {
        setQuestionsLoading(true);
        try {
            const newQuestionsItems = await questionApi.getQuestionsAsync(page, clotheId);
            setQuestions(prev => ({
                ...prev,
                currentPage: page,
                items: newQuestionsItems
            }));
        } catch (error) {
            console.error('Error fetching questions:', error);
        } finally {
            setQuestionsLoading(false);
        }
    };

    return (
        <div className={styles.reviewsWrapper}>
            <div className={styles.reviewsSection}>
                <div className={styles.reviewsTabs}>
                    <button
                        className={`${styles.tabBtn} ${activeTab === 'reviews' ? styles.activeTab : ''}`}
                        onClick={() => setActiveTab('reviews')}
                    >
                        Reviews {statistics.totalReviews > 0 && `(${statistics.totalReviews})`}
                    </button>
                    <button
                        className={`${styles.tabBtn} ${activeTab === 'questions' ? styles.activeTab : ''}`}
                        onClick={() => setActiveTab('questions')}
                    >
                        Questions {questions.totalCount > 0 && `(${questions.totalCount})`}
                    </button>
                </div>

                {activeTab === 'reviews' && (
                    <>
                        <div className={styles.reviewPromptWrapper}>
                            <div className={styles.reviewPrompt}>
                                We'd love to hear what you think! <br />
                                Your review helps us improve and helps other shoppers choose with confidence. <br />
                                Thanks for your time!
                            </div>

                            <Button variant="outline" size="md" onClick={() => setIsReviewFormOpen(true)}>
                                Leave a review
                            </Button>
                        </div>

                        {isReviewFormOpen && (
                            <Modal title="Leave a review" onClose={() => setIsReviewFormOpen(false)}>
                                <ReviewForm
                                    clotheId={clotheId}
                                    onSuccess={() => {
                                        invalidate();
                                        setIsReviewFormOpen(false);
                                    }}
                                />
                            </Modal>
                        )}

                        {statistics.totalReviews > 0 ? (
                            <>
                                <div className={styles.ratingSummary}>
                                    <div className={styles.ratingNumber}>
                                        <h2>{statistics.averageRating}</h2>
                                        <span>({statistics.totalReviews} reviews)</span>
                                    </div>

                                    <div className={styles.ratingBars}>
                                        <div className={styles.ratingBarItem}>
                                            <span className={styles.ratingLabel}>5 <span className={styles.star}>★</span></span>
                                            <div className={styles.barContainer}>
                                                <div className={styles.barFill} style={{ width: `${calculatePercentage(statistics.fiveStars)}%` }}></div>
                                            </div>
                                            <span className={styles.ratingCount}>{statistics.fiveStars}</span>
                                        </div>
                                        <div className={styles.ratingBarItem}>
                                            <span className={styles.ratingLabel}>4 <span className={styles.star}>★</span></span>
                                            <div className={styles.barContainer}>
                                                <div className={styles.barFill} style={{ width: `${calculatePercentage(statistics.fourStars)}%` }}></div>
                                            </div>
                                            <span className={styles.ratingCount}>{statistics.fourStars}</span>
                                        </div>
                                        <div className={styles.ratingBarItem}>
                                            <span className={styles.ratingLabel}>3 <span className={styles.star}>★</span></span>
                                            <div className={styles.barContainer}>
                                                <div className={styles.barFill} style={{ width: `${calculatePercentage(statistics.threeStars)}%` }}></div>
                                            </div>
                                            <span className={styles.ratingCount}>{statistics.threeStars}</span>
                                        </div>
                                        <div className={styles.ratingBarItem}>
                                            <span className={styles.ratingLabel}>2 <span className={styles.star}>★</span></span>
                                            <div className={styles.barContainer}>
                                                <div className={styles.barFill} style={{ width: `${calculatePercentage(statistics.twoStars)}%` }}></div>
                                            </div>
                                            <span className={styles.ratingCount}>{statistics.twoStars}</span>
                                        </div>
                                        <div className={styles.ratingBarItem}>
                                            <span className={styles.ratingLabel}>1 <span className={styles.star}>★</span></span>
                                            <div className={styles.barContainer}>
                                                <div className={styles.barFill} style={{ width: `${calculatePercentage(statistics.oneStars)}%` }}></div>
                                            </div>
                                            <span className={styles.ratingCount}>{statistics.oneStars}</span>
                                        </div>
                                    </div>
                                </div>

                                {reviewsLoading ? (
                                    <Loader />
                                ) : (
                                    <>
                                        {reviews.items.map(review => (
                                            <ReviewItem key={review.id} review={review} />
                                        ))}

                                        {reviews.totalPages > 1 && (
                                            <Pagination
                                                currentPage={reviews.currentPage}
                                                totalPages={reviews.totalPages}
                                                onPageChange={handleReviewPageChange}
                                            />
                                        )}
                                    </>
                                )}
                            </>
                        ) : (
                            <div className={styles.emptyState}>
                                <p>No reviews yet. Be the first to review this product!</p>
                            </div>
                        )}
                    </>
                )}

                {activeTab === 'questions' && (
                    <>
                        <div className={styles.reviewPromptWrapper}>
                            <div className={styles.reviewPrompt}>
                                Have a question about this product? <br />
                                Ask anything and we'll get back to you as soon as possible!
                            </div>

                            <Button variant="outline" size="md" onClick={() => setIsQuestionFormOpen(true)}>
                                Ask a question
                            </Button>
                        </div>

                        {isQuestionFormOpen && (
                            <Modal title="Ask a question" onClose={() => setIsQuestionFormOpen(false)}>
                                <QuestionCreateForm
                                    clotheId={clotheId}
                                    onSuccess={(newQuestion) => {
                                        invalidate();
                                        setQuestions(prev => ({ ...prev, totalCount: prev.totalCount + 1, items: [newQuestion, ...prev.items] }));
                                        setIsQuestionFormOpen(false);
                                    }}
                                />
                            </Modal>
                        )}

                        {questions.items.length > 0 ? (
                            <>
                                {questionsLoading ? (
                                    <Loader />
                                ) : (
                                    <>
                                        {questions.items.map(question => (
                                            <QuestionItem
                                                key={question.id}
                                                question={question}
                                                onInvalidate={invalidate}
                                            />
                                        ))}

                                        {questions.totalPages > 1 && (
                                            <Pagination
                                                currentPage={questions.currentPage}
                                                totalPages={questions.totalPages}
                                                onPageChange={handleQuestionPageChange}
                                            />
                                        )}
                                    </>
                                )}
                            </>
                        ) : (
                            <div className={styles.emptyState}>
                                <p>No questions yet. Be the first to ask a question!</p>
                            </div>
                        )}
                    </>
                )}
            </div>
        </div>
    );
};

export default ReviewsSection;