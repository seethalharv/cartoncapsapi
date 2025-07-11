# CartonCaps Referral API (Mock Service)

This is a .NET Core-based mock API service implementing the referral system for the Carton Caps mobile app. 
The API enables users to invite friends, track referral status, and simulate referral redemptions using shareable deep links.
All solutions were created to be compatible with macOS and .NET 8 SDK.
However I have used only windows to develop this, so there might be some issues with macOS. 
So if you find any issues please feel free to reach out to me and I can try my best to fix it.

---

## Features

- Invite friends via email or SMS
- Generate and use referral codes
- Track referral status
- Mocked integration with Branch.io
- Fully testable mock API for front-end development

---

## Project Structure

- `Controllers/` - API endpoints (`ReferralsController`, `UserController`)
- `Models/` - Domain models and DTOs
- `Services/` - Business logic layer
- `Repositories/` - Data access layer (mocked with in-memory collections)
- `DB/` - Mock `AppDbContext`

---

## Prerequisites

- [.NET 8 SDK]
- Git
- macOS-compatible runtime

---

## 🛠️ How to Run


```checkout the code from ,
https://github.com/CartonCaps/CartonCaps.ReferralApi
The repo was created as public so you should not have any issues cloning it.
This is integarated with swagger and the spec is avialble but I have added a separate file for the api spec to pass on to UI folks.
```


Navigate to: `https://localhost:[your port]/swagger` to explore the Swagger UI.

---

## API Documentation

Detailed API specs are available in [`referral_api_spec.md`](referral_api_spec.md).
And also some important diagrms are enclosed as well. 

---

## Running Tests

```There are MS Test unit tests available in the project which covers all major scenarios. 
Also there is a postman collection available in the repo to test the endpoints. Download that and import it to postman.
In postman please keep the Auth as No Auth and make sure it is https and that your local is running and modify port number according to your settings. 

```



---

##  Endpoints Overview

| Method | URL                                       | Description                      |
|--------|-------------------------------------------|----------------------------------|
| POST   | `/api/referrals/invite`                  | Invite a friend (SMS/Email)     |
| POST   | `/api/referrals/referalslist?userId=100` | Get referrals by user           |
| POST   | `/api/user/referrals/update-redeemed`     | Update status after redemption  |
|GET     | `/api/user/referrals/get-referral-link`   |Just get the URL. rest will need to handled by UI|

See [`api-spec.md`](./api-spec.md) for full details and sample payloads.

---

## Notes

- This is a mock service. Data is not persisted.
- Branch.io calls are simulated for testability.
- Safe for frontend integration development.

---

## Contact

Built by [Seethal Harvey]. For questions, open an issue or connect via GitHub.
I will probably not respond to issues here, as I am not checking this repo regularly and since it was exclusively built for the CartonCaps project.
But for real please just email me at seethalmd@gmail.com
