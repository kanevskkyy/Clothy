import { MessageSquare, Trash2 } from "lucide-react";
import EmptyState from "../../../shared/ui/EmptyState/EmptyState.tsx";
import Pagination from "../../../shared/ui/Pagination/Pagination.tsx";
import Loader from "../../../shared/ui/Loader/Loader.tsx";
import { Helmet } from "react-helmet";
import { useOutletContext, useSearchParams } from "react-router-dom";
import { useEffect, useState } from "react";
import { questionApi } from "../../../app/api/questionApi.ts";
import { toast } from "sonner";
import { getErrorMessage } from "../../../shared/lib/errorHandler.ts";
import { getCurrentPage, handlePageChange } from "../../../shared/lib/paginationUtils.ts";
import type { IQuestionReadDTO } from "../../../entities/reviewsService/interfaces/IQuestionReadDTO.ts";
import type { PagedList } from "../../../shared/lib/pagedList.ts";
import type { AdminLayoutContext } from "../../../features/auth/admin/adminLayout/AdminLayout.tsx";
import { useQueryClient } from "@tanstack/react-query";
import QuestionItem from "../../../entities/reviewsService/questionItem/QuestionItem.tsx";
import SortSelect from "../../../features/catalog/sortSelect/SortSelect.tsx";
import styles from "./AdminQuestionsPage.module.css";

type FilterType = "all" | "unanswered";

const FILTER_OPTIONS = [
    { value: "unanswered", label: "Unanswered" },
    { value: "all", label: "All" },
];

const AdminQuestionsPage = () => {
    const [searchParams, setSearchParams] = useSearchParams();
    const currentPage = getCurrentPage(searchParams);
    const { setPageHeader } = useOutletContext<AdminLayoutContext>();
    const queryClient = useQueryClient();

    const [questions, setQuestions] = useState<PagedList<IQuestionReadDTO> | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [deletingId, setDeletingId] = useState<string | null>(null);
    const [deletingAnswerId, setDeletingAnswerId] = useState<string | null>(null);
    const [filter, setFilter] = useState<FilterType>("unanswered");

    useEffect(() => {
        setPageHeader({ title: "Questions", description: "Customer questions" });
    }, []);

    const fetchQuestions = async () => {
        try {
            setIsLoading(true);
            const data = await questionApi.getQuestionsAsync({
                pageNumber: currentPage,
                withoutAnswer: filter === "unanswered" ? true : undefined,
            });
            setQuestions(data);
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        fetchQuestions();
    }, [currentPage, filter]);

    const invalidate = () => {
        queryClient.invalidateQueries({ queryKey: ["clothe"] });
        fetchQuestions();
    };

    const handleDelete = async (id: string) => {
        try {
            setDeletingId(id);
            await questionApi.deleteQuestionAsync(id);
            toast.success("Question deleted.");
            invalidate();
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setDeletingId(null);
        }
    };

    const handleDeleteAnswer = async (questionId: string, answerId: string) => {
        try {
            setDeletingAnswerId(answerId);
            await questionApi.deleteAnswerAsync(questionId, answerId);
            toast.success("Answer deleted.");
            invalidate();
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setDeletingAnswerId(null);
        }
    };

    const handleFilterChange = (newFilter: FilterType) => {
        setFilter(newFilter);
        handlePageChange(1, searchParams, setSearchParams);
    };

    const onPageChange = (page: number) => {
        handlePageChange(page, searchParams, setSearchParams);
    };

    const renderContent = () => {
        if (isLoading) return <Loader />;

        if (!questions || questions.items.length === 0) {
            return (
                <EmptyState
                    icon={<MessageSquare size={28} color="#6B6B6B" />}
                    title="No questions found"
                    description={filter === "unanswered" ? "All questions have been answered." : "No questions yet."}
                    buttons={[]}
                />
            );
        }

        return (
            <>
                <div className={styles.list}>
                    {questions.items.map((question) => (
                        <div key={question.id} className={styles.questionCard}>
                            {question.clotheInfo && (
                                <div className={styles.clotheInfo}>
                                    <img
                                        src={question.clotheInfo.clothePhotoURL}
                                        alt={question.clotheInfo.clotheName}
                                        className={styles.clothePhoto}
                                    />
                                    <span className={styles.clotheName}>{question.clotheInfo.clotheName}</span>
                                </div>
                            )}
                            <div className={styles.questionBody}>
                                <div className={styles.questionItemWrapper}>
                                    <QuestionItem
                                        question={question}
                                        onInvalidate={invalidate}
                                        onDeleteAnswer={(answerId) => handleDeleteAnswer(question.id, answerId)}
                                        deletingAnswerId={deletingAnswerId}
                                    />
                                </div>
                                <div className={styles.actions}>
                                    <button
                                        className={styles.deleteBtn}
                                        disabled={deletingId === question.id}
                                        onClick={() => handleDelete(question.id)}
                                    >
                                        <Trash2 size={15} />
                                        Delete
                                    </button>
                                </div>
                            </div>
                        </div>
                    ))}
                </div>

                {questions.totalPages > 1 && (
                    <Pagination
                        currentPage={questions.currentPage}
                        totalPages={questions.totalPages}
                        onPageChange={onPageChange}
                    />
                )}
            </>
        );
    };

    return (
        <div>
            <Helmet>
                <title>Admin — Questions</title>
                <meta name="description" content="Manage customer questions." />
            </Helmet>

            <div className={styles.header}>
                <SortSelect
                    value={filter}
                    options={FILTER_OPTIONS}
                    onChange={(value) => handleFilterChange(value as FilterType)}
                    label="Show:"
                />
            </div>

            {renderContent()}
        </div>
    );
};

export default AdminQuestionsPage;