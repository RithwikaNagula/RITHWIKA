// Tracks claim resolution stages: Submitted → UnderReview → DocumentVerification → Approved → Rejected → Settled.
namespace Domain.Enums
{
    public enum ClaimStatus
    {
        Submitted,         // Initial state when a customer files a claim
        UnderReview,       // Claims officer has begun examining the claim
        DocumentVerification, // Officer is validating the uploaded supporting documents
        Approved,          // Claim passes review; awaiting settlement payout
        Rejected,          // Claim denied by the officer with a reason
        Settled            // Settlement amount has been paid out and the claim is closed
    }
}
