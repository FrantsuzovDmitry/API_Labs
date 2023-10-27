I this project iplemented Grpc Service which receives messages from client and works with local database.

Used by Microsoft.EntityFramework:
   - Data folder contains DbClass
   - Models folder contains entities of buisness logic

Used by Grpc:
   - File pizza.proto contains объявление API
   - Services folder contains Service which contains API logic