
# Carton Caps Referral API Specification

## Overview

This API powers the referral feature of the Carton Caps app, enabling users to invite friends via email  
or SMS using shareable deferred deep links (powered by Branch.io). New users installing via a referral link will  
trigger referral redemption and reward workflows.

---

## Assumptions

- User authentication is already implemented.
- User profile includes referral code.
- New user registration & referral redemption is handled.
- If just one workflow is needed to invite , use the invite end-point , if the UI just wants a link use the GET end point and just get the URL and notification service can be implemented separately. 

---

## Anti-Abuse Measures

- **Rate-Limiting:** Users can only send up to 5 invites per hour.
- **No Self-Referral:** A user cannot refer their own email/phone.
- **If I was going to implement the whole notification system would have added a lot more checks and validations there**
- **For the email, phone number, and notification delivery.**

These protections are enforced inside the `ReferralService`.

---

## Endpoints Summary

| Method | Endpoint                                       | Description                        |
| ------ | ---------------------------------------------- | ---------------------------------- |
| POST   | `/api/referrals/invite`                        | Invite a friend via SMS or Email   |
| POST   | `/api/referrals/referalslist?userId=100`       | View all referrals sent by a user  |
| POST   | `/api/user/referral/update-redeemed`           | Update referral status to redeemed |
| GET    | `/api/referrals/get-referral-link`             | Get referral link for a user       |

---

## POST `/api/referrals/invite`

**Description:** Invites a friend using the current user's referral code. Supports SMS or Email.

### Request

```json
{
  "referrerUserId": 100,
  "emailOrPhone": "friend@example.com",
  "channel": "email"
}
```

### Response

```json
{
  "status": "invitation to friend@example.com was successfully sent."
}
```

### Errors

| Code | Message                                          |
| ---- | ------------------------------------------------ |
| 400  | "Invalid request data"                           |
| 400  | "Cannot invite yourself"                         |
| 400  | "Invite limit exceeded. Please try again later." |
| 404  | "Referral code not found for user"               |
| 500  | "Referral saved, but notification failed"        |

---

## POST `/api/referrals/referalslist?userId=100`

**Description:** Returns a list of referrals initiated by the user.

### Response

```json
[
  {
    "referralcode": "ACD345",
    "emailOrPhone": "friend@example.com",
    "status": "pending",
    "createdat": "2025-06-20T12:34:00Z"
  }
]
```

### Errors

| Code | Message                                          |
| ---- | ------------------------------------------------ |
| 400  | "Invalid userId. It must be a positive integer." |
| 404  | "No referrals found for user ID: 100"            |

---

## POST `/api/user/referral/update-redeemed`

**Description:** Updates a user’s referrals to redeemed after a new user registers using their referral code.

### Request

```json
{
  "referralCode": "ALICE123",
  "userId": 10
}
```

### Response

```json
{
  "message": "Updated 1 referral(s) to redeemed for your referred person who used the code. ALICE123"
}
```

### Errors

| Code | Message                                       |
| ---- | --------------------------------------------- |
| 400  | "Referral code is required."                  |
| 404  | "No user found with the given referral code." |

---

## GET `/api/referrals/get-referral-link`

**Description:** Retrieves a shareable referral link for a given user and communication channel.

### Query Parameters

| Parameter | Type   | Required | Description                |
| --------- | ------ | -------- | -------------------------- |
| userId    | int    | Yes      | The ID of the referrer     |
| channel   | string | Yes      | Either `email` or `sms`    |

### Response

```json
{
  "referralLink": "https://app.cartoncaps.com/invite?ref=ABC123"
}
```

### Errors

| Code | Message                                  |
| ---- | ---------------------------------------- |
| 400  | "UserId and channel are required."       |
| 500  | "Failed to retrieve referral link."      |

---

## Status Code Reference

| Code | Meaning                                  |
| ---- | ---------------------------------------- |
| 200  | OK                                       |
| 400  | Bad Request – validation error           |
| 404  | Not Found – invalid referral/user/code   |
| 500  | Internal Server Error – unexpected issue |

---

## Notes

- Endpoints are mocked and tested.
- Deep link generation is integrated through the referral service but may use mock responses, implementation would be done through Branch.Io.
- All inputs are validated at both controller and service layers.

---
