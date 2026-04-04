using Clothy.ReviewService.gRPC.Client.Services.Interfaces;
using Moq;

namespace Clothy.ReviewService.IntegrationTests.Infrastructure;

public static class MockExtensions
{
    public static void SetupValidClotheItem(this Mock<IClotheItemIdValidatorGrpcClient> mock, Guid clotheId)
    {
        mock.Setup(x => x.ValidateClotheItemIdAsync(It.Is<ClotheItemIdToValidate>(r => r.ClotheId == clotheId.ToString())))
            .ReturnsAsync(new ClotheItemResponse { IsValid = true });
    }

    public static void SetupInvalidClotheItem(this Mock<IClotheItemIdValidatorGrpcClient> mock, Guid clotheId)
    {
        mock.Setup(x => x.ValidateClotheItemIdAsync(It.Is<ClotheItemIdToValidate>(r => r.ClotheId == clotheId.ToString())))
            .ReturnsAsync(new ClotheItemResponse { IsValid = false, ErrorMessage = "Not found" });
    }

    public static void SetupUserPurchased(this Mock<ICheckUserPurchasedClotheGrpcClient> mock, Guid userId, Guid clotheId)
    {
        mock.Setup(x => x.CheckUserPurchasedAsync(It.Is<CheckUserPurchasedRequest>(r =>
                r.UserId == userId.ToString() && r.ClotheId == clotheId.ToString())))
            .ReturnsAsync(new CheckUserPurchasedResponse
            {
                Purchased = true,
                ClotheName = "Test Clothe",
                ClothePhotoURL = "https://photo.test/img.jpg"
            });
    }

    public static void SetupUserNotPurchased(this Mock<ICheckUserPurchasedClotheGrpcClient> mock, Guid userId, Guid clotheId)
    {
        mock.Setup(x => x.CheckUserPurchasedAsync(It.Is<CheckUserPurchasedRequest>(r =>
                r.UserId == userId.ToString() && r.ClotheId == clotheId.ToString())))
            .ReturnsAsync(new CheckUserPurchasedResponse { Purchased = false });
    }
}