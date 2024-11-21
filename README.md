Library management System
This Library Management System helps users organize and manage the books in their library. This application consists of a backend built with ASP.NET Core and a frontend built with React.

Prerequisites
.NET 8 SDK
Node.js (for the React frontend)
Docker (optional, for running the application in containers)
Getting Started
Cloning the Repository
Clone the repository:

git clone https://github.com/malindichathumi/Library-Management-System.git
Navigate to the project directory:

cd Backend
Running the Application Locally
Backend (ASP.NET Core)
Navigate to the backend project directory:

cd Backend
Restore the dependencies:

dotnet restore
Update the database:

dotnet ef database update
Run the backend application:

dotnet run
The backend API will be available at http://localhost:5275.

Frontend (React)
Navigate to the client directory:

cd client
Create a .env.local file with the following content:

VITE_SERVER_URL = 'http://localhost:5275'
Install the dependencies:

npm install
Run the frontend application:

npm start
The frontend application will be available at http://localhost:5173.

Running with Docker
Build and run the Docker containers:

docker-compose up --build
This will start both the backend and frontend services.

Features
User authentication: Users can sign up and log in to access the application.
Create a new book record: Users can add new books to their library.
View a list of existing book records: Users can view the books they have added.
Update an existing book record: Users can update the details of their books.
Delete a book record: Users can delete books from their library.
Contributing
Contributions are welcome! Please open an issue or submit a pull request for any improvements or bug fixes.
