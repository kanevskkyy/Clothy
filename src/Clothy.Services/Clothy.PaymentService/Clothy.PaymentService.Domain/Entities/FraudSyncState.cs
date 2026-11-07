namespace Clothy.PaymentService.Domain.Entities
{
    /// <summary>
    /// What has been sent to FraudService for this payment. Stored with the payment so that /labels is sent
    /// exactly once and only after the /score attempt, whatever order the Stripe webhooks arrive in.
    /// Values are stored as int: append new ones at the end.
    /// </summary>
    public enum FraudSyncState
    {
        /// <summary>The /score attempt has not finished yet (or the payment is not paid).</summary>
        None,
        /// <summary>The /score attempt finished (scored or not); no label sent.</summary>
        Scored,
        /// <summary>The fraud label (dispute) was sent.</summary>
        Labeled,
        /// <summary>The fraud label was removed after a won dispute.</summary>
        Unlabeled
    }
}
