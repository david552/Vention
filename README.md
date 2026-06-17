Task 2.4

1)Before running the application, you need to create a `.env` file in the root directory (next to `Vention.sln`) to store local secrets
Create a file named `.env` and add the following content: PASSWORD_PEPPER=12345

2)Build and Start the Application - docker compose up -d --build

3) Verify Environment Variables - docker exec my-vention-api-compose printenv

4) Stop the Application - docker compose down