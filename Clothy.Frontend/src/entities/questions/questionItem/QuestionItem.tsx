import type { IQuestionAggregatedReadDTO } from "../IQuestionAggregatedReadDTO.ts";
import styles from "./QuestionItem.module.css";
import {formatDate} from "../../../shared/formatDate.ts";

interface QuestionItemProps {
    question: IQuestionAggregatedReadDTO;
}

const QuestionItem: React.FC<QuestionItemProps> = ({ question }) => {
    return (
        <div className={styles.questionItem}>
            <div className={styles.questionSection}>
                <div className={styles.questionWrapper}>
                    <img
                        src={question.user.photoUrl}
                        alt={`${question.user.firstName} ${question.user.lastName}`}
                        className={styles.questionAvatar}
                    />
                    <div className={styles.questionInfo}>
                        <p className={styles.questionAuthor}>{question.user.firstName} {question.user.lastName}</p>
                        <p className={styles.questionText}>{question.questionText}</p>
                    </div>
                </div>
                <div className={styles.questionDate}>{formatDate(question.createdAt)}</div>
            </div>
            {question.answers.map(answer => (
                <div key={answer.id} className={styles.answerSection}>
                    <div className={styles.answerWrapper}>
                        <img
                            src={answer.user.photoUrl}
                            alt={`${answer.user.firstName} ${answer.user.lastName}`}
                            className={styles.answerAvatar}
                        />
                        <div className={styles.questionInfo}>
                            <p className={styles.answerAuthor}>{answer.user.firstName} {answer.user.lastName}</p>
                            <p className={styles.answerText}>{answer.answerText}</p>
                        </div>
                    </div>
                    <div className={styles.answerDate}>{formatDate(answer.createdAt)}</div>
                </div>
            ))}
        </div>
    );
};

export default QuestionItem;
