using Bogus;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.ValueObjects;

namespace Clothy.ReviewService.UnitTests.Helpers;

public static class QuestionFakes
{
    private static Faker faker = new();
 
    public static UserInfo CreateUserInfo(Guid? userId = null) => new(
        userId ?? Guid.NewGuid(),
        faker.Name.FirstName(),
        faker.Name.LastName(),
        faker.Internet.Avatar()
    );
 
    public static ClotheInfo CreateClotheInfo(Guid? clotheId = null) => new(
        clotheId ?? Guid.NewGuid(),
        faker.Commerce.ProductName(),
        faker.Internet.Avatar()
    );
 
    public static Question CreateQuestion(Guid? userId = null, Guid? clotheId = null)
    {
        UserInfo userInfo = CreateUserInfo(userId);
        ClotheInfo clotheInfo = CreateClotheInfo(clotheId);
        return new Question(clotheInfo, userInfo, faker.Lorem.Sentence());
    }
    
    public static (Question question, Answer answer) CreateQuestionWithAnswer(Guid? questionUserId = null, Guid? answerUserId = null)
    {
        Question question = CreateQuestion(questionUserId);
        Answer answer = CreateAnswer(answerUserId);
        
        question.Answers.Add(answer);
        return (question, answer);
    }
 
    public static Answer CreateAnswer(Guid? userId = null)
    {
        UserInfo userInfo = CreateUserInfo(userId);
        return new Answer(userInfo, faker.Lorem.Sentence());
    }
}